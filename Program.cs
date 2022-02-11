using Exceptionless;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OVMS
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ExceptionlessClient.Default.Startup("WA0Y7kBPrfI4yXwYPzSmB4NQrycRj9ooFh1Y5sKB");
                ExceptionlessClient.Default.Configuration.ServerUrl = ApplicationSettings.Default.ExceptionlessServerUrl;
                ExceptionlessClient.Default.Configuration.SetVersion(Assembly.GetExecutingAssembly().GetName().Version);

                ExceptionlessClient.Default.SubmitLog("Start " + Assembly.GetExecutingAssembly().GetName().Version);


                var dt = DBHelper.GetAllOVMSCars();

                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        int dbcarid = (Int32)dr["id"];
                        string name = dr["tesla_name"].ToString();
                        string password = dr["tesla_password"].ToString();
                        string CarId = dr["tesla_token"].ToString().Substring(5);

                        var c = new Car(dbcarid, name, password, CarId);
                    }
                    catch (Exception ex)
                    {
                        ex.ToExceptionless().Submit();
                    }
                }

                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                TeslaLogger.Logfile.Log(ex.ToString());
            }
        }
    }
}
