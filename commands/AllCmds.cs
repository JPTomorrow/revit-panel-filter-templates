using System;
using System.Windows;
using JPMorrow.Tools.Diagnostics;
using Autodesk.Revit.DB;
using System.Linq;
using JPMorrow.Revit.Panels;
using System.Windows.Controls;
using forms = System.Windows.Forms;

namespace JPMorrow.UI.ViewModels
{
    public partial class ParentViewModel
    {
        /// <summary>
        /// prompt for save and exit
        /// </summary>
        public void RefreshPanels(Window window)
        {
            /*
            try {
                var coll = new FilteredElementCollector(Info.DOC, Info.UIDOC.ActiveView.Id);
                var views = coll
                    .OfCategory(BuiltInCategory.OST_Views)
                    .Where(x => (x as View).ViewType == ViewType.FloorPlan && !(x as View).IsTemplate)
                    .Select(x => x as View).ToList();
                
                if(!views.Any()) {
                    debugger.show(err:"There were no views to process.");
                    return;
                }

                var view_filter_groups = views
                    .Select(x => ViewFilterGroup.CreateViewFilterGroup(Info, x))
                    .ToList();

                View_Items.Clear();
                view_filter_groups.ForEach(x => View_Items.Add(new ViewFilterGroupPresenter(x)));
                Update("View_Items");
                Filter_Items.Clear();
                Update("Filter_Items");
            }
            catch(Exception ex) {
                debugger.show(err:ex.ToString());
            }
            */
        }

        public void SelectViews(DataGrid grid) {
            /*
            try {
                grid.SelectAll();
            }
            catch(Exception ex) {
                debugger.show(err:ex.ToString());
            }
            */
        }

        public void DeselectViews(DataGrid grid) {
            /*
            try {
                grid.UnselectAll();
            }
            catch(Exception ex) {
                debugger.show(err:ex.ToString());
            }
            */
        }

        public void FilterSelChanged(Window window) {
            /*
            try {
                var selected = Filter_Items
                    .Where(x => x.IsSelected)
                    .Select(x => x.Value.PanelId).ToList();

                if(!selected.Any()) return;
                Info.SEL.SetElementIds(selected);
            }
            catch(Exception ex) {
                debugger.show(err:ex.ToString());
            }
            */
        }

        public void ViewSelChanged(Window window) {
            /*
            try {
                var selected = View_Items
                    .Where(x => x.IsSelected)
                    .SelectMany(x => x.Value.PanelFilters).ToList();

                if(!selected.Any()) {
                    Filter_Items.Clear();
                    Update("Filter_Items");
                    return;
                }

                Filter_Items.Clear();
                selected.ForEach(x => Filter_Items.Add(new PanelFilterPresenter(x)));
                Update("Filter_Items");
            }
            catch(Exception ex) {
                debugger.show(err:ex.ToString());
            }
            */
        }
	}
}
