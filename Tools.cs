using Exceptionless;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaLogger;

namespace OVMS
{
    class Tools
    {
        public static readonly System.Globalization.CultureInfo ciEnUS = new System.Globalization.CultureInfo("en-US");
        public static readonly System.Globalization.CultureInfo ciDeDE = new System.Globalization.CultureInfo("de-DE");

        public static void SetThreadEnUS()
        {
            Thread.CurrentThread.CurrentCulture = ciEnUS;
        }

        public static bool IsPropertyExist(dynamic settings, string name)
        {
            if (settings == null)
                return false;

            if (!(settings is IDictionary<string, object>))
                return false;

            return settings is IDictionary<string, object> dictionary && dictionary.ContainsKey(name);
        }

        public static bool IsMono()
        {
            return GetMonoRuntimeVersion() != "NULL";
        }

        public static string GetMonoRuntimeVersion()
        {
            Type type = Type.GetType("Mono.Runtime");
            if (type != null)
            {
                MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                {
                    return displayName.Invoke(null, null).ToString();
                }
            }

            return "NULL";
        }

        public static bool IsDocker()
        {
            try
            {
                string filename = "/tmp/teslalogger-DOCKER";

                if (File.Exists(filename))
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                ex.ToExceptionless().FirstCarUserID().Submit();
                Logfile.ExceptionWriter(ex, "IsDocker");
            }

            return false;
        }
    }

    public static class EventBuilderExtension
    {
        static String lastFirstCar;
        public static EventBuilder FirstCarUserID(this EventBuilder v)
        {
            try
            {
                if (lastFirstCar != null)
                    return v.SetUserIdentity(lastFirstCar);

                using (MySqlConnection con = new MySqlConnection(DBHelper.DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT tesla_token FROM cars where length(tesla_token) >= 8 limit 1", con))
                    {
                        object o = cmd.ExecuteScalar()?.ToString();
                        if (o is String && o.ToString().Length >= 8)
                        {
                            lastFirstCar = o.ToString();
                            return v.SetUserIdentity(o.ToString());
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logfile.Log(ex.ToString());
            }

            return v;
        }
    }
}
