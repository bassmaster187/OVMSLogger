using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVMS
{
    class FileManager
    {
        public static string GetExecutingPath()
        {
            //System.IO.Directory.GetCurrentDirectory() is not returning the current path of the assembly

            System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            string executingPath = executingAssembly.Location;

            executingPath = executingPath.Replace(executingAssembly.ManifestModule.Name, string.Empty);

            return executingPath;
        }

        public static string GeofenceFilename
        {
            get {return "../geofence.csv";}
        }

        public static string GeofencePrivateFilename
        {
            get { return "../geofence-private.csv"; }
        }
    }
}
