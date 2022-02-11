using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

                var dt = DBHelper.GetAllOVMSCars();

                foreach (DataRow dr in dt.Rows)
                {
                    int dbcarid = (Int32)dr["id"];
                    string name = dr["tesla_name"].ToString();
                    string password = dr["tesla_password"].ToString();
                    string CarId = dr["tesla_token"].ToString().Substring(5);

                    var c = new Car(dbcarid, name, password, CarId);
                }

                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                TeslaLogger.Logfile.Log(ex.ToString());
            }
        }
    }
}
