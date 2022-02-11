using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaLogger;

namespace OVMS
{
    class UpdateTeslalogger
    {
        public static void Chmod(string filename, int chmod, bool logging = true)
        {
            try
            {
                if (!Tools.IsMono())
                {
                    return;
                }

                if (logging)
                {
                    Logfile.Log("chmod " + chmod + " " + filename);
                }

                using (System.Diagnostics.Process proc = new System.Diagnostics.Process
                {
                    EnableRaisingEvents = false
                })
                {
                    proc.StartInfo.FileName = "chmod";
                    proc.StartInfo.Arguments = chmod + " " + filename;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                // ex.ToExceptionless().Submit();
                Logfile.Log("chmod " + filename + " " + ex.Message);
            }
        }
    }
}
