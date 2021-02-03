using Autodesk.Revit.DB;
using System.Linq;
using System;
using System.Collections.Generic;
using JPMorrow.Revit.Documents;

namespace JPMorrow.Revit.Worksets
{
	public static class WorksetManager
	{
		public static Workset CreateWorkset(Document doc, string set_name)
		{
			// Worksets can only be created in a document with worksharing enabled
			Workset ws = null;
			if (doc.IsWorkshared && !WorksetExists(doc, set_name, false))
			{
				using (Transaction tx = new Transaction(doc, "Creating " + set_name + "workset"))
				{
					tx.Start();
					ws = Workset.Create(doc, set_name);
					tx.Commit();
				}
				return ws;
			}
			else {
				ws = GetWorksetByName(doc, set_name, false);
				return ws;
			}
		}

		public static WorksetId GetWorksetId(Document doc, string set_name)
		{
			if (!doc.IsWorkshared)
				throw new Exception("This document is not workshared. Please make it a workshared document and restart this application to create the appropriate worksets.");

			FilteredWorksetCollector ws_coll = new FilteredWorksetCollector(doc);
			var ws_id = ws_coll.Where(x => x.Name.Equals(set_name)).FirstOrDefault().Id;

			if (ws_id == null)
				throw new ArgumentNullException("The specified workset does not exist. The program should create this workset in a workshared model when you launch it.");

			return ws_id;
		}

		public static bool WorksetExists(Document doc, string set_name, bool case_sensitive) {
			var wss = GetProjectWorksets(doc);
			var sn = case_sensitive ? set_name : set_name.ToLower();
			return wss.Any(x => (case_sensitive ? x.Name.Equals(sn) : x.Name.ToLower().Equals(sn)));
		}

		public static Workset GetWorksetByName(Document doc, string set_name, bool case_sensitive) {

			var wss = GetProjectWorksets(doc);
			var sn = case_sensitive ? set_name : set_name.ToLower();
			var ws = wss.FirstOrDefault(x => (case_sensitive ? x.Name.Equals(sn) : x.Name.ToLower().Equals(sn)));
			return ws;
		}

		public static IEnumerable<Workset> GetProjectWorksets(Document doc) {

			FilteredWorksetCollector coll = new FilteredWorksetCollector(doc);
			var ret_ws = new List<Workset>();

            foreach(var ws in coll)
                ret_ws.Add(ws);

			return ret_ws;
		}
	}
}