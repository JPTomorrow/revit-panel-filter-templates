

using System.Linq;
using JPMorrow.Revit.Documents;
using JPMorrow.Tools.Diagnostics;

namespace JPMorrow.Revit.Versioning {

    public class RevitVersion {

        public RevitVersion(ModelInfo info) {
            
            Version = info.DOC.Application.VersionName;
        }

        public string Version { get; private set; }

        public bool IsVersionBelowYear(int year) {

            var year_str = Version.Split(' ').Last();
            var year_int = int.Parse(year_str);
            return year_int < year;
        }
    }
}
