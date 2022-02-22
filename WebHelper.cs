using Exceptionless;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaLogger;

namespace OVMS
{
    class WebHelper
    {
        internal HttpClient httpclientOVMSAPI = null;
        internal object httpClientLock = new object();
        internal HttpClient httpClientForAuthentification;
        HttpClient httpClientCurrentJSON = new HttpClient();
        CookieContainer tokenCookieContainer;
        string Username;
        string Password;
        string CarId;
        Car car;

        string lastChargingTimestamp = "";
        string lastPosTimestamp = "";

        static object lock_auth = new object();
        private static int NominatimCount = 0;
        internal string OVMSVersion;
        private string VIN;
        internal string Cartype;
        GPSMovementDetector gPSMovementDetector = null;

        public WebHelper(Car car, string Username, string Password, string CarId)
        {
            this.Username = Username;
            this.Password = Password;
            this.CarId = CarId;

            //Damit Mono keine Zertifikatfehler wirft :-(
#pragma warning disable CA5359 // Deaktivieren Sie die Zertifikatüberprüfung nicht
            ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;
#pragma warning restore CA5359 // Deaktivieren Sie die Zertifikatüberprüfung nicht
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            this.car = car;
        }

        internal void Auth()
        {
            lock (lock_auth)
            {
                car.Log("Get Token");
                HttpClient client = GetDefaultHttpClientForAuthentification();
                var url = "https://ovms.dexters-web.de:6869/api/cookie?username=" + Username + "&password=" + Password;
                var result = client.GetAsync(url).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;

                result.Headers.TryGetValues("Set-Cookie", out var setCookie);
                // PrintCookieContainer();

                var cookieKey = setCookie.First().Split('=')[0];
                var cookieValue = setCookie.First().Split('=')[1];

                // tokenCookieContainer.Add(new Cookie(cookieKey, cookieValue, "/api/vehicles", "ovms.dexters-web.de"));
                tokenCookieContainer.Add(new Cookie(cookieKey, cookieValue, "/", "ovms.dexters-web.de"));

                // PrintCookieContainer();

                car.Log("Auth Result: " + resultContent);
                string c = setCookie.First();
                // client.DefaultRequestHeaders.Add("Cookie", c);


                var r2 = client.GetAsync("https://ovms.dexters-web.de:6869/api/vehicles").Result;
                var json = r2.Content.ReadAsStringAsync().Result;

                car.Log("Vehicles: " + json);

                dynamic j = JsonConvert.DeserializeObject(json);

                foreach (dynamic a in j)
                {
                    string id = a["id"];
                    System.Diagnostics.Debug.WriteLine("ID:" + id);

                    if (id == CarId)
                    {
                        car.Log("Car found in account!");
                        break;
                    }
                }
            }
        }

        void PrintCookieContainer()
        {
            var cookies = new List<Cookie>();

            Hashtable table = (Hashtable)tokenCookieContainer.GetType().InvokeMember(
                "m_domainTable",
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.Instance,
                null,
                tokenCookieContainer,
                new object[] { }
            );

            foreach (string key in table.Keys)
            {
                var item = table[key];
                var items = (ICollection)item.GetType().GetProperty("Values").GetGetMethod().Invoke(item, null);
                foreach (CookieCollection cc in items)
                {
                    foreach (Cookie cookie in cc)
                    {
                        cookies.Add(cookie);

                        System.Diagnostics.Debug.WriteLine(cookie.ToString());
                    }
                }
            }
        }

        HttpClient GetDefaultHttpClientForAuthentification()
        {
            lock (httpClientLock)
            {
                if (httpClientForAuthentification == null)
                {
                    tokenCookieContainer = new CookieContainer();

                    HttpClientHandler handler = new HttpClientHandler()
                    {
                        CookieContainer = tokenCookieContainer,
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        AllowAutoRedirect = false,
                        UseCookies = true
                    };

                    httpClientForAuthentification = new HttpClient(handler);
                    httpClientForAuthentification.Timeout = TimeSpan.FromSeconds(30);
                    httpClientForAuthentification.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.82 Safari/537.36");
                    httpClientForAuthentification.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                    httpClientForAuthentification.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    httpClientForAuthentification.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    // client.DefaultRequestHeaders.ConnectionClose = true;
                    httpClientForAuthentification.BaseAddress = new Uri("https://ovms.dexters-web.de:6869");
                }
            }

            return httpClientForAuthentification;
        }

        public void PrintProtocol()
        {
            try
            {
                var s = GetProtocol();
                dynamic j = JsonConvert.DeserializeObject(s);

                foreach (dynamic k in j)
                {
                    if (k["m_code"] == "F")
                    {
                        string m_msg = k["m_msg"];
                        var args = m_msg.Split(',');
                        OVMSVersion = args[0];
                        VIN = args[1];
                        Cartype = args[4];

                        car.Log("Version: " + OVMSVersion);
                        car.Log("VIN: " + VIN);
                        car.Log("Cartype: " + Cartype);
                        car.Log("Provider: " + args[5]);

                        
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                car.SendException2Exceptionless(ex);
                car.Log(ex.ToString());
            }
        }

        public static void UpdateAllPOIAddresses()
        {
            try
            {
                if (Geofence.GetInstance().RacingMode)
                {
                    return;
                }

                int t = Environment.TickCount;
                int count = 0;
                Logfile.Log("UpdateAllPOIAddresses start");

                using (MySqlConnection con = new MySqlConnection(DBHelper.DBConnectionstring))
                {
                    con.Open();

                    using (MySqlCommand cmdBucket = new MySqlCommand(@"SELECT distinct Pos FROM chargingstate union distinct SELECT StartPos FROM drivestate union distinct SELECT EndPos FROM drivestate order by Pos", con))
                    {
                        var bucketdr = cmdBucket.ExecuteReader();
                        var loop = true;

                        do
                        {
                            StringBuilder bucket = new StringBuilder();
                            for (int x = 0; x < 100; x++)
                            {
                                if (!bucketdr.Read())
                                {
                                    loop = false;
                                    break;
                                }

                                if (bucket.Length > 0)
                                    bucket.Append(",");

                                string posid = bucketdr[0].ToString();
                                bucket.Append(posid);
                            }

                            count = UpdateAllPOIAddresses(count, bucket.ToString());
                        }
                        while (loop);
                    }


                    t = Environment.TickCount - t;
                    Logfile.Log($"UpdateAllPOIAddresses end {t}ms count:{count}");
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Logfile.Log(ex.ToString());
            }
        }

        private static int UpdateAllPOIAddresses(int count, string bucket)
        {
            if (bucket.Length == 0)
                return count;

            using (MySqlConnection con = new MySqlConnection(DBHelper.DBConnectionstring))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"Select lat, lng, pos.id, address, fast_charger_brand, max_charger_power 
                        from pos    
                        left join chargingstate on pos.id = chargingstate.pos
                        where pos.id in (" + bucket + ")", con))
                {
                    MySqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        count = UpdatePOIAdress(count, dr);
                    }
                }
            }

            return count;
        }

        private static int UpdatePOIAdress(int count, MySqlDataReader dr)
        {
            try
            {
                Thread.Sleep(1);
                double lat = (double)dr["lat"];
                double lng = (double)dr["lng"];
                int id = (int)dr["id"];
                string brand = dr["fast_charger_brand"] as String ?? "";
                int max_power = dr["max_charger_power"] as int? ?? 0;

                Address a = Geofence.GetInstance().GetPOI(lat, lng, false, brand, max_power);
                if (a == null)
                {
                    if (dr[3] == DBNull.Value || dr[3].ToString().Length == 0)
                    {
                        DBHelper.UpdateAddress(null, id);
                    }
                    return count;
                }

                if (dr[3] == DBNull.Value || a.name != dr[3].ToString())
                {
                    using (MySqlConnection con2 = new MySqlConnection(DBHelper.DBConnectionstring))
                    {
                        con2.Open();
                        using (MySqlCommand cmd2 = new MySqlCommand("update pos set address=@address where id = @id", con2))
                        {
                            cmd2.Parameters.AddWithValue("@id", id);
                            cmd2.Parameters.AddWithValue("@address", a.name);
                            cmd2.ExecuteNonQuery();

                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Logfile.Log(" Exception in UpdateAllPOIAddresses: " + ex.Message);
            }

            return count;
        }

        internal string GetProtocol()
        {
            return GetDataFromServer("https://ovms.dexters-web.de:6869/api/protocol/" + CarId);
        }

        internal string GetCharge()
        {
            return GetDataFromServer("https://ovms.dexters-web.de:6869/api/charge/" + CarId);
        }

        internal string GetStatus()
        {
            return GetDataFromServer("https://ovms.dexters-web.de:6869/api/status/" + CarId);
        }

        internal string GetLocation()
        {
            return GetDataFromServer("https://ovms.dexters-web.de:6869/api/location/" + CarId);
        }

        string GetDataFromServer(string url, bool autoAuth = true)
        {
            var ret = httpClientForAuthentification.GetAsync(url).Result;
            if (ret.StatusCode == HttpStatusCode.Unauthorized || ret.StatusCode == HttpStatusCode.NotFound)
            {
                car.Log("url Error: " + ret.StatusCode.ToString());

                Auth();
                ret = httpClientForAuthentification.GetAsync(url).Result;
            }

            var resultContent = ret.Content.ReadAsStringAsync().Result;

            if (ApplicationSettings.Default.ProtocolLogging)
            {
                try
                {
                    string name = url.Substring(37).Replace('/', '-') + ".txt";
                    string path = "protocollogging/" + DateTime.Now.ToString("yyyyMMddmmssfff") + "-" + name;

                    System.IO.File.WriteAllText(path, resultContent);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }

            return resultContent;
        }

        bool checkCarTypeIsDriving(dynamic j)
        {
            /*
            if (isHyundaiVFL)
            {
                if (gPSMovementDetector == null)
                    gPSMovementDetector = new GPSMovementDetector(car);

                double latitude = Convert.ToDouble(j["latitude"], Tools.ciEnUS);
                double longitude = Convert.ToDouble(j["longitude"], Tools.ciEnUS);
                string timestamp = j["m_msgtime_l"];
                DateTime dt = DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", Tools.ciEnUS).ToLocalTime();

                return gPSMovementDetector.InsertGPSData(dt, latitude, longitude);
            } 
            else 
            */
            if (isSmartElectic)
            {
                return Convert.ToDouble(j["speed"], Tools.ciEnUS) > 1;
            }
            else
            {
                return j["drivemode"] == "1" || Convert.ToDouble(j["speed"], Tools.ciEnUS) > 1;
            }
        }

        public bool isDriving(bool forceInsert = false)
        {
            string resultContent = null;
            try
            {
                resultContent = GetLocation();

                if (resultContent.Length == 3)
                {
                    car.Log("Empty Location!");
                    return false;
                }

                dynamic j = JsonConvert.DeserializeObject(resultContent);
                bool driving = checkCarTypeIsDriving(j);

                if (driving || forceInsert)
                {
                    string timestamp = j["m_msgtime_l"];
                    DateTime dt = DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", Tools.ciEnUS).ToLocalTime();
                    timestamp = dt.ToString("yyyy-MM-dd HH:mm:ss", Tools.ciEnUS);

                    string altitude = j["altitude"];
                    string latitude = j["latitude"];
                    string longitude = j["longitude"];
                    int power = (int)Convert.ToDouble(j["power"], Tools.ciEnUS);
                    string speed = j["speed"];

                    if (lastPosTimestamp != timestamp)
                    {
                        lastPosTimestamp = timestamp;
                        forceInsert = true;
                    }

                    string statusResult = GetStatus();
                    dynamic j2 = JsonConvert.DeserializeObject(statusResult);

                    double odometer = Convert.ToDouble(j2["odometer"], Tools.ciEnUS) / 10;
                    int batteryLevel = (int)Convert.ToDouble(j2["soc"], Tools.ciEnUS);
                    double idealBatteryRangeKm = Convert.ToDouble(j2["estimatedrange"], Tools.ciEnUS);
                    double batteryRangeKm = Convert.ToDouble(j2["idealrange"], Tools.ciEnUS);

                    int iSpeed = (int)Convert.ToDouble(speed, Tools.ciEnUS);
                    car.currentJSON.current_speed = iSpeed;
                    car.currentJSON.current_odometer = odometer;
                    car.currentJSON.latitude = Convert.ToDouble(latitude, Tools.ciEnUS);
                    car.currentJSON.longitude = Convert.ToDouble(longitude, Tools.ciEnUS);
                    car.currentJSON.current_battery_level = batteryLevel;
                    car.currentJSON.current_battery_range_km = batteryRangeKm;
                    car.currentJSON.CreateCurrentJSON();

                    car.DbHelper.InsertPos(timestamp, car.currentJSON.latitude, car.currentJSON.longitude,
                        iSpeed,
                        Convert.ToDecimal(power, Tools.ciEnUS),
                        odometer, idealBatteryRangeKm, batteryRangeKm, batteryLevel, null, altitude);
                    
                    return driving;
                }
            }
            catch (Exception ex)
            {
                car.CreateExceptionlessClient(ex).AddObject(resultContent, "ResultContent").Submit();
                car.Log(ex.ToString());
                System.Threading.Thread.Sleep(30000);
            }

            return false;
        }

        public bool isCharging()
        {
            string resultContent = null;
            try
            {
                resultContent = GetCharge();

                dynamic j = JsonConvert.DeserializeObject(resultContent);
                if (j["chargestate"] == "charging")
                {
                    string timestamp = j["m_msgtime_d"];
                    DateTime dt = DateTime.ParseExact(timestamp, "yyyy-MM-dd HH:mm:ss", Tools.ciEnUS).ToLocalTime();
                    timestamp = dt.ToString("yyyy-MM-dd HH:mm:ss", Tools.ciEnUS);

                    string battery_level = j["soc"];
                    double dBattery_level = Convert.ToDouble(battery_level, Tools.ciEnUS);
                    double chargekwh = Convert.ToDouble(j["chargekwh"], Tools.ciEnUS) / 10;
                    double dCharge_energy_added = GetChargeEnergyAdded(chargekwh, dBattery_level);
                    string charge_energy_added = dCharge_energy_added.ToString(Tools.ciEnUS);
                    int charger_power = (int)Math.Round(Convert.ToDouble(j["chargepower"], Tools.ciEnUS));
                    string ideal_battery_range_km = j["estimatedrange"];
                    string battery_range_km = j["idealrange"];
                    string linevoltage = j["linevoltage"];
                    int temperature_outside = (int)Math.Round(Convert.ToDouble(j["temperature_ambient"], Tools.ciEnUS));
                    double temperature_battery = Convert.ToDouble(j["temperature_battery"], Tools.ciEnUS);

                    string chargecurrent = j["chargecurrent"];

                    bool forceInsert = false;

                    if (lastChargingTimestamp != timestamp)
                    {
                        lastChargingTimestamp = timestamp;
                        forceInsert = true;

                        car.DbHelper.InsertCharging(timestamp, battery_level, charge_energy_added, charger_power.ToString(),
                        Convert.ToDouble(ideal_battery_range_km, Tools.ciEnUS), Convert.ToDouble(battery_range_km, Tools.ciEnUS),
                        linevoltage, "0", chargecurrent, (double)temperature_outside, forceInsert, "", "");

                        car.DbHelper.InsertCan(timestamp, 2, temperature_battery);
                    }

                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                car.CreateExceptionlessClient(ex).AddObject(resultContent, "ResultContent").Submit();
                car.Log(ex.ToString());
            }

            return false;
        }

        bool isHyundaiVFL
        {
            get { return Cartype == "HIONVFL"; }
        }

        bool isSmartElectic
        {
            get { return Cartype == "SE"; }
        }

        private double GetChargeEnergyAdded(double chargekwh, double currentSOC)
        {
            if (isHyundaiVFL)
            {
                double start_charging_soc = car.DbHelper.start_charging_soc;
                if (start_charging_soc == 0)
                    return chargekwh;

                double socDiff = currentSOC - start_charging_soc;
                double kWhDiff = 28.0 / 100 * socDiff;
                return kWhDiff;
            }

            return chargekwh;
        }

        public static async Task<string> ReverseGecocodingAsync(Car c, double latitude, double longitude, bool forceGeocoding = false, bool insertGeocodecache = true)
        {
            string url = "";
            string resultContent = "";
            try
            {
                if (!forceGeocoding)
                {
                    Address a = null;
                    a = Geofence.GetInstance().GetPOI(latitude, longitude);
                    if (a != null)
                    {
                        Logfile.Log("Reverse geocoding by Geofence");
                        return a.name;
                    }

                    string value = GeocodeCache.Instance.Search(latitude, longitude);
                    if (value != null)
                    {
                        Logfile.Log("Reverse geocoding by GeocodeCache");
                        return value;
                    }
                }

                Tools.SetThreadEnUS();

                Thread.Sleep(5000); // Sleep to not get banned by Nominatim

                using (WebClient webClient = new WebClient())
                {

                    webClient.Headers.Add("User-Agent: TL 1.1");
                    webClient.Encoding = Encoding.UTF8;

                    url = "http://nominatim.openstreetmap.org/reverse";

                    url += "?format=jsonv2&lat=";
                    url += latitude.ToString();
                    url += "&lon=";
                    url += longitude.ToString();

                   
                    url += "&email=mail";
                    url += "@";
                    url += "teslalogger";
                    url += ".de";
                    

                    DateTime start = DateTime.UtcNow;
                    resultContent = await webClient.DownloadStringTaskAsync(new Uri(url));
                    DBHelper.AddMothershipDataToDB("ReverseGeocoding", start, 0);

                    dynamic jsonResult = JsonConvert.DeserializeObject(resultContent);
                    object r1 = ((Dictionary<string, object>)jsonResult)["address"];
                    Dictionary<string, object> r2 = (Dictionary<string, object>)r1;
                    string postcode = "";
                    if (r2.ContainsKey("postcode"))
                    {
                        postcode = r2["postcode"].ToString();
                    }

                    string country_code = r2["country_code"].ToString();

                    string road = "";
                    if (r2.ContainsKey("road"))
                    {
                        road = r2["road"].ToString();
                    }

                    string city = "";
                    if (r2.ContainsKey("city"))
                    {
                        city = r2["city"].ToString();
                    }
                    else if (r2.ContainsKey("town"))
                    {
                        city = r2["town"].ToString();
                    }
                    else if (r2.ContainsKey("village"))
                    {
                        city = r2["village"].ToString();
                    }

                    string house_number = "";
                    if (r2.ContainsKey("house_number"))
                    {
                        house_number = r2["house_number"].ToString();
                    }

                    string name = "";
                    if (r2.ContainsKey("name") && r2["name"] != null)
                    {
                        name = r2["name"].ToString();
                    }

                    string address29 = "";
                    if (r2.ContainsKey("address29") && r2["address29"] != null)
                    {
                        address29 = r2["address29"].ToString();
                    }

                    string adresse = "";

                    if (address29.Length > 0)
                    {
                        adresse += address29 + ", ";
                    }

                    if (country_code != "de")
                    {
                        adresse += country_code + "-";
                    }

                    adresse += postcode + " " + city + ", " + road + " " + house_number;

                    if (name.Length > 0)
                    {
                        adresse += " / " + name;
                    }

                    System.Diagnostics.Debug.WriteLine(url + "\r\n" + adresse);

                    if (insertGeocodecache)
                    {
                        GeocodeCache.Instance.Insert(latitude, longitude, adresse);
                    }


                    NominatimCount++;
                    Logfile.Log("Reverse geocoding by Nominatim" + NominatimCount);
                    

                    return adresse;
                }
            }
            catch (Exception ex)
            {
                // ex.ToExceptionless().AddObject(resultContent, "ResultContent").AddObject(url, "Url").Submit();

                if (url == null)
                {
                    url = "NULL";
                }

                if (resultContent == null)
                {
                    resultContent = "NULL";
                }

                Logfile.ExceptionWriter(ex, url + "\r\n" + resultContent);
            }

            return "";
        }

        public void SendCurrentJSON(string data)
        {
            var r = httpClientCurrentJSON.PostAsync("http://localhost:5000/setcurrentjson/" + car.CarInDB, new StringContent(data));
            var tt = r.Result;
        }
    }
}
