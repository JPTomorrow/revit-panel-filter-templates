using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using JPMorrow.Revit.Documents;
using JPMorrow.Revit.Versioning;
using JPMorrow.Tools.Diagnostics;

namespace JPMorrow.Revit.Panels
{
    using SelFunc = Func<ModelInfo, IEnumerable<ViewFilter>, IEnumerable<ViewFilter>>; 
    using SelSwapDict = System.Collections.Generic.Dictionary<FilterGroupSelection, Func<ModelInfo, IEnumerable<ViewFilter>, IEnumerable<ViewFilter>>>;
    
    ///
    /// THIS CODE IS NOT SAFE FOR REUSE WITH A UI DRIVEN APP FOR REVIT.
    /// IT DOES NOT IMPLEMENT EXTERNAL EVENTS FOR COMMUNICATION
    ///

    public enum FilterGroupSelection {
        InvalidGenerated,
        PanelGenerated,
        All,
        PanelAll,
        PanelNonGenerated,
    }
    
    public class ProjectFilterGroup {

        private ProjectFilterGroup(
            IEnumerable<ViewFilter> v_filters,
            FilterGroupSelection selection) {
            
            master_filter_list = v_filters.ToList();
            GroupSelection = selection;
        }

        private List<ViewFilter> master_filter_list = new List<ViewFilter>();

        public IEnumerable<ViewFilter> Filters { get => master_filter_list; }
        public FilterGroupSelection GroupSelection { get; private set; }
        public string GroupSelectionName { get => Enum.GetName(typeof(FilterGroupSelection), GroupSelection); }

        private static SelSwapDict SelectionSwap = new SelSwapDict() {
            
            { FilterGroupSelection.All, null },
            { FilterGroupSelection.PanelAll, GetPanelViewFilters },
            { FilterGroupSelection.InvalidGenerated, GetInvalidAutoGeneratedViewFilters },
            { FilterGroupSelection.PanelGenerated, GetGeneratedPanelViewFilters },
            { FilterGroupSelection.PanelNonGenerated, GetNonGeneratedPanelViewFilters },
        };

        private static IEnumerable<ViewFilter> GetGroupSelection(
            ModelInfo info, IEnumerable<ViewFilter> filters,
            FilterGroupSelection selection) {
            
            bool s = SelectionSwap.TryGetValue(selection, out var func);

            if(!s || func == null)
                throw new Exception("No function exists for the selection group: " +
                                    Enum.GetName(typeof(FilterGroupSelection), selection));

            return func(info, filters);
        }

        public override string ToString() {
            
            var o = string.Join("\n", Filters.Select(x => x.ToString()));
            o += "\n-----------------\n";
            return o;
        }

        // create a structure containing all of a revit projects filters
        public static ProjectFilterGroup CreateProjectFilterGroup(
            ModelInfo info, FilterGroupSelection selection) {

            
            var coll = new FilteredElementCollector(info.DOC);
            var filter_ids = coll.OfClass(typeof(ParameterFilterElement)).ToElementIds();
            var filters = ViewFilter.CreateViewFilters(info, filter_ids);

            filters = GetGroupSelection(info, filters, selection);
            
            return new ProjectFilterGroup(filters, selection);
        }

        // get all filters associated with a panel name from a list of filters
        private static IEnumerable<ViewFilter> GetPanelViewFilters(
            ModelInfo info, IEnumerable<ViewFilter> source_filters) {
            
            var coll = new FilteredElementCollector(info.DOC);
            var panel_names = GetPanelNames(coll);

            var filters = source_filters.Where(x => panel_names.Any(y => x.FilterName.Contains(y)));
            return filters;
        }

        // get all filters prefixed with (AUTO)
        // that are not associated with a panel name
        // these are invalid filters generated by our program
        private static IEnumerable<ViewFilter> GetInvalidAutoGeneratedViewFilters(
            ModelInfo info, IEnumerable<ViewFilter> source_filters) {

            var coll = new FilteredElementCollector(info.DOC);
            var panel_names = GetPanelNames(coll);

            var filters = source_filters
                .Where( x => !panel_names.Any(y => x.FilterName.Contains(y)) && x.IsGenerated);
            
            return filters;
        }

        // get all filters prefixed with (AUTO)
        // that are associated with a panel name
        private static IEnumerable<ViewFilter> GetGeneratedPanelViewFilters(
            ModelInfo info, IEnumerable<ViewFilter> source_filters) {

            var coll = new FilteredElementCollector(info.DOC);
            var panel_names = GetPanelNames(coll);

            var filters = source_filters
                .Where( x => panel_names.Any(y => x.FilterName.Contains(y)) && x.IsGenerated);
            
            return filters;
        }

        // get all filters that are not prefixed with (AUTO)
        // that associated with a panel name
        private static IEnumerable<ViewFilter> GetNonGeneratedPanelViewFilters(
            ModelInfo info, IEnumerable<ViewFilter> source_filters) {

            var coll = new FilteredElementCollector(info.DOC);
            var panel_names = GetPanelNames(coll);

            var filters = source_filters
                .Where( x => panel_names.Any(y => x.FilterName.Contains(y)) && !x.IsGenerated);
            
            return filters;
        }

        // delete all of the view filters in the current structure
        public void DeleteFilters(ModelInfo info) {
            
            using var tx = new Transaction(info.DOC, "Deleting Filters");
            tx.Start();

            foreach(var f in Filters)
                info.DOC.Delete(f.FilterId);

            tx.Commit();
            master_filter_list.Clear();
        }

        // convert filters in this group that are not prefixed with (AUTO)
        // to be have the appropriate prefix and settings
        public void ConvertNonGeneratedToGenerated(ModelInfo info) {

            var non_gen = Filters.Where(x => !x.IsGenerated);
            RevitVersion ver = new RevitVersion(info);

            var bics = new[] {
                new ElementId(BuiltInCategory.OST_Conduit),
                new ElementId(BuiltInCategory.OST_ConduitFitting)
            };

            var coll = new FilteredElementCollector(info.DOC);
            var from_id = GetParameterId(coll, "From");
            var to_id = GetParameterId(coll, "To");
            var wsize_id = GetParameterId(coll, "Wire Size");
            var workset_id = GetParameterId(coll, "Workset");

            using var tx = new Transaction(info.DOC, "Converting Non Generated Filters");
            tx.Start();

            foreach(var f in non_gen) {
                var idx = Filters.ToList().FindIndex(x => x.FilterName.Contains(f.FilterName) && x.IsGenerated);
                var filter = info.DOC.GetElement(f.FilterId) as ParameterFilterElement;
                
                if(idx != -1) info.DOC.Delete(Filters.ToList()[idx].FilterId);
                filter.SetCategories(bics);
                
                // make rules for new filter
                var from_rule = ParameterFilterRuleFactory.CreateEqualsRule(from_id, f.FilterName, true);
                var to_ws_rule = ParameterFilterRuleFactory.CreateHasValueParameterRule(to_id);
                var 
                var rule_arr = new[] { from_rule };
                var el_f = new ElementParameterFilter(rule_arr);
                
                if(ver.IsVersionBelowYear(2019)) {
                    SetFilter(filter, rule_arr);
                }
                else {
                    SetFilter(filter, el_f);
                }

                filter.Name = ViewFilter.FilterPrefix + " " + filter.Name;
            } 

            tx.Commit();

            coll = new FilteredElementCollector(info.DOC);
            var filter_ids = coll.OfClass(typeof(FilterElement)).ToElementIds();
            var filters = ViewFilter.CreateViewFilters(info, filter_ids);

            master_filter_list = GetGroupSelection(info, filters, GroupSelection).ToList();
        }

        // generate panel filters for all panels that don't
        // have a corresponding generated or non generated filter already
        // returns: names of panels that it failed to create a filter for because of duplicate naming errors
        public IEnumerable<string> GenerateMissingPanelFilters(ModelInfo info) {

            var failed_panel_names = new List<string>();
            
            if(GroupSelection != FilterGroupSelection.PanelAll)
                throw new Exception("Group filter must be set to PanelAll in order to generate missing panel filters");
            
            var bics = new[] {
                new ElementId(BuiltInCategory.OST_Conduit),
                new ElementId(BuiltInCategory.OST_ConduitFitting),
                new ElementId(BuiltInCategory.OST_ElectricalFixtures)
            };

            var coll = new FilteredElementCollector(info.DOC);
            
            var from_id = GetParameterId(coll, "From");
            var to_id = GetParameterId(coll, "To");
            var wsize_id = GetParameterId(coll, "Wire Size");
            var workset_id = GetParameterId(coll, "Workset");
            
            coll = new FilteredElementCollector(info.DOC);
            var panel_names = GetPanelNames(coll);

            using var tx = new Transaction(info.DOC, "Generating Missing Panel Filters");
            tx.Start();

            foreach(var name in panel_names) {

                var idx = Filters.ToList().FindIndex(x => x.FilterName.Contains(name));

                if(idx == -1) {
                    // make rules for new filter
                    var from_rule = ParameterFilterRuleFactory.CreateEqualsRule(from_id, name, true);
                    var rule_arr = new[] { from_rule };
                    var el_f = new ElementParameterFilter(rule_arr);

                    try {
                        
                        ParameterFilterElement.Create(info.DOC, ViewFilter.FilterPrefix + " " + name, bics, el_f);
                    }
                    catch {
                        failed_panel_names.Add(name);
                    }
                }
            }

            tx.Commit();

            coll = new FilteredElementCollector(info.DOC);
            var filter_ids = coll.OfClass(typeof(FilterElement)).ToElementIds();
            var filters = ViewFilter.CreateViewFilters(info, filter_ids);

            master_filter_list = GetGroupSelection(info, filters, GroupSelection).ToList();
            failed_panel_names.Add("-------------------");
            return failed_panel_names;
        }

        // get the string names of all the panels in the current revit project
        private static IEnumerable<string> GetPanelNames(FilteredElementCollector coll) {
            return coll
                .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                .OfClass(typeof(FamilyInstance))
                .Where(x => (x as FamilyInstance).Symbol.FamilyName.ToLower().Contains("panelboard"))
                .Select(x => x.Name.Trim());
        }

        // Get an element id for the From parameter from a piece of conduit in the model
        private static ElementId GetParameterId(FilteredElementCollector coll, string param) {
            
            var conduit = coll
                .OfCategory(BuiltInCategory.OST_Conduit)
                .Where(x => x.LookupParameter(param) != null)?
                .First();

            if(conduit == null)
                throw new Exception(
                    "Please load the parameter called \"" + param +
                    "\" on conduit and conduit fittings.");
            
            return conduit.LookupParameter(param).Id;
        }

        // WARNING: KEEP THESE LOCKED AWAY INSIDE THIS FUNCTION OR ELSE IT WILL DESTROY THE PROGRAM... DONT ASK WHY
        private static void SetFilter(ParameterFilterElement filter, ElementParameterFilter rules) {
            filter.SetElementFilter(rules);
        }

        // WARNING: KEEP THESE LOCKED AWAY INSIDE THIS FUNCTION OR ELSE IT WILL DESTROY THE PROGRAM... DONT ASK WHY
        private static void SetFilter(ParameterFilterElement filter, FilterRule[] rules) {
            filter.SetRules(rules);
        }
    }

    // represents a view and its currently active filters in revit
    public class ViewFilterGroup {

        private ViewFilterGroup(
            string view_name, ElementId view_id,
            IEnumerable<ViewFilter> v_filters) {
            
            filters = v_filters.ToList();
            ViewName = view_name;
            ViewId = view_id;
        }
        
        public string ViewName { get; private set; }
        public ElementId ViewId { get; private set; }
        private List<ViewFilter> filters = new List<ViewFilter>();

        public IEnumerable<ViewFilter> Filters { get => filters; }
        
        public override string ToString() {
            var o = ViewName + ":\n";
            o += string.Join("\n", Filters.Select(x => x.ToString()));
            o += "\n-----------------\n";
            return o;
        }

        // print a group of panel filters
        public static string PrintFilterGroups(IEnumerable<ViewFilterGroup> groups) =>
            string.Join("\n", groups.Select(x => x.ToString()));

        // create a structure containing the view and all of its filters
        public static ViewFilterGroup CreateViewFilterGroup(ModelInfo info, View view) {

            var active_filters = view.GetFilters();
            var filters = ViewFilter.CreateViewFilters(info, active_filters);
            return new ViewFilterGroup(view.Name, view.Id, filters);
        }
    }

    // reprents a filter in a revit view
    public class ViewFilter {

        private ViewFilter(string name, ElementId filter_id) {
            FilterName = name.Trim();
            FilterId = filter_id;
        }
        
        public string FilterName { get; private set; }
        public ElementId FilterId { get; private set; }

        public static readonly string FilterPrefix = "(AUTO-PANEL)";

        public bool IsGenerated => FilterName.StartsWith(FilterPrefix);
        
        public override string ToString() {
            return FilterName + " - " + FilterId.IntegerValue.ToString();
        }

        // create many filter strutctures representing a filter in revit
        public static IEnumerable<ViewFilter> CreateViewFilters(
            ModelInfo info, IEnumerable<ElementId> filter_ids) {
            
            var filters = filter_ids.Select(id => CreateViewFilter(info, id)).ToList();
            return filters;
        }

        // create a filter structure representing a filter in revit
        public static ViewFilter CreateViewFilter(ModelInfo info, ElementId filter_id) {
            var filter = info.DOC.GetElement(filter_id);
            return new ViewFilter(filter.Name, filter_id); 
        }
    }

    
}

