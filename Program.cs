using Exceptionless;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaLogger;

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

                Logfile.WriteToLogfile = true;
                Logfile.Logfilepath = new System.IO.FileInfo("../nohup.out").FullName;
                Logfile.Log("Start OVMSLogger V" + Assembly.GetExecutingAssembly().GetName().Version);


                InitConnectToDB();

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

        private static void InitConnectToDB()
        {
            for (int x = 1; x <= 30; x++) // try 30 times until DB is up and running
            {
                try
                {
                    Logfile.Log("DB Version: " + DBHelper.GetVersion());
                    Logfile.Log("Count Pos: " + DBHelper.CountPos()); // test the DBConnection
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Connection refused")
                        || ex.Message.Contains("Unable to connect to any of the specified MySQL hosts")
                        || ex.Message.Contains("Reading from the stream has failed."))
                    {
                        Logfile.Log($"Wait for DB ({x}/30): Connection refused.");
                    }
                    else
                    {
                        ex.ToExceptionless().Submit();
                        Logfile.Log("DBCONNECTION " + ex.Message);
                    }

                    Thread.Sleep(15000);
                }
            }
        }        
    }
}
