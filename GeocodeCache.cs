﻿using System;
using System.Data;
using Exceptionless;

namespace TeslaLogger
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Pending>")]
    internal class GeocodeCache
    {
        private DataTable dt = new DataTable("cache");
        private static GeocodeCache _instance;
        static readonly string FilenameGeocodeCache = "GeocodeCache.xml";

        public static GeocodeCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GeocodeCache();
                }

                return _instance;
            }
        }

        public GeocodeCache()
        {
            DataColumn lat = dt.Columns.Add("lat", typeof(double));
            DataColumn lng = dt.Columns.Add("lng", typeof(double));
            dt.Columns.Add("Value");

            dt.PrimaryKey = new DataColumn[] { lat, lng };

            try
            {
                if (System.IO.File.Exists(FilenameGeocodeCache))
                {
#pragma warning disable CA3075 // Unsichere DTD-Verarbeitung in XML
                    _ = dt.ReadXml(FilenameGeocodeCache);
#pragma warning restore CA3075 // Unsichere DTD-Verarbeitung in XML
                    Logfile.Log("GeocodeCache Items: " + dt.Rows.Count);
                }
                else
                {
                    Logfile.Log(FilenameGeocodeCache + " Not found!");
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Logfile.ExceptionWriter(ex, "");
            }
        }

        public string Search(double lat, double lng)
        {
            DataRow dr = dt.Rows.Find(new object[] { lat, lng });
            return dr?["Value"].ToString();
        }


        public void Insert(double lat, double lng, string value)
        {
            try
            {
                DataRow dr = dt.NewRow();
                dr["lat"] = lat;
                dr["lng"] = lng;
                dr["value"] = value;
                dt.Rows.Add(dr);

                Logfile.Log("GeocodeCache:Insert");
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Logfile.Log(ex.Message);
            }
        }

        public void Write()
        {
            try
            {
                dt.WriteXml(FilenameGeocodeCache);
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Logfile.Log(ex.Message);
            }
        }

        internal void ClearCache()
        {
            dt.Clear();
        }

    }
}
