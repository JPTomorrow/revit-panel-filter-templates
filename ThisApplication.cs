using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using JPMorrow.Revit.Documents;
using System.Diagnostics;
using System.Linq;
using JPMorrow.Tools.Diagnostics;
using forms = System.Windows.Forms;
using JPMorrow.Revit.Panels;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace MainApp
{
    /// <summary>
    /// Main Execution
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
	[Autodesk.Revit.DB.Macros.AddInId("9BBF529B-520A-4877-B63B-BEF1238B6B06")]
    public partial class ThisApplication : IExternalCommand
    {
		// public static View3D Hanger_View { get; set; } = null;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
			string[] dataDirectories = new string[] {
				"data" };

			// Set revit model info
			bool debugApp = false;
			ModelInfo info = ModelInfo.StoreDocuments(
				commandData, dataDirectories, debugApp);
			IntPtr main_rvt_wind = Process.GetCurrentProcess().MainWindowHandle;

            var coll = new FilteredElementCollector(info.DOC);

            var result = debugger.show_yesno(
                header:"View Panel Filters",
                err:"This program will create filters for every panel in a project.\n" +
                "This can take a few minutes to complete. Do not be alarmed.",
                continue_txt:"Do you want to continue?");

            if(result == forms.DialogResult.No)
                return Result.Succeeded;
            
            var views = coll
                .OfCategory(BuiltInCategory.OST_Views)
                .Where(x => (x as View).ViewType == ViewType.FloorPlan && !(x as View).IsTemplate)
                .Select(x => x as View).ToList();
                
            if(!views.Any()) {
                debugger.show(err:"There were no views to process.");
                return Result.Succeeded;
            }
            
            var filter_group = ProjectFilterGroup.CreateProjectFilterGroup(info, FilterGroupSelection.InvalidGenerated);
            filter_group.DeleteFilters(info);

            filter_group = ProjectFilterGroup.CreateProjectFilterGroup(info, FilterGroupSelection.PanelAll);
            
            debugger.show(
                header:"Panel Filters",
                err:"Unprocessed Panel Filters\n\n" +
                filter_group.ToString());

            var failed_panels = filter_group.GenerateMissingPanelFilters(info);
            filter_group.ConvertNonGeneratedToGenerated(info);
            
            debugger.show(
                header:"Panel Filters",
                err:"Failed Panel Filters\n\n" +
                string.Join("\n", failed_panels) + "\n" + 
                "Final Panel Filters\n\n" +
                filter_group.ToString());
            
            /* OLD METHOD
			try {
				ParentView pv = new ParentView(revit_info, main_rvt_wind);
				pv.Show();
			}
			catch(Exception ex) {
				debugger.show(err:ex.ToString());
			}
            */            

			return Result.Succeeded;
        }
    }

    public static class MAINLINQEXT {
        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int item_cnt) {

            if(source == null || !source.Any()) return new List<T>();
            int count_down = item_cnt;
            List<T> list_source = source.ToList();

            List<int> random_exclude_list = new List<int>();
            List<T> ret_list = new List<T>();
            Random rand = new Random();
            
            while(count_down != 0) {
                var idx = rand.Next(0, list_source.Count - 1);
                if(random_exclude_list.Any(x => x == idx) || list_source[idx] == null) continue;
                ret_list.Add(list_source[idx]);
                random_exclude_list.Add(idx);
                count_down--;
            }
            
            return ret_list;
        }
    }
}