using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    }
}
