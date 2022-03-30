using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaLogger;
using Exceptionless;

namespace OVMS
{
    class DBHelper
    {
        Car car;
        private DateTime lastChargingInsert = DateTime.Today;
        internal static string DBConnectionstring => GetDBConnectionstring();
        private static Dictionary<string, int> mothershipCommands = new Dictionary<string, int>();

        internal double start_charging_soc = 0;

        public DBHelper(Car car)
        {
            this.car = car;
        }

        public static void AddMothershipDataToDB(string command, DateTime start, int httpcode)
        {
            DateTime end = DateTime.UtcNow;
            TimeSpan ts = end - start;
            double duration = ts.TotalSeconds;
            AddMothershipDataToDB(command, duration, httpcode);
        }

        private static void AddCommandToDB(string command)
        {
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("insert mothershipcommands (command) values (@command)", con))
                {
                    cmd.Parameters.AddWithValue("@command", command);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void GetMothershipCommandsFromDB()
        {
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT id, command FROM mothershipcommands", con))
                {
                    MySqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        int id = Convert.ToInt32(dr["id"], Tools.ciDeDE);
                        string command = dr[1].ToString();
                        if (!mothershipCommands.ContainsKey(command))
                        {
                            mothershipCommands.Add(command, id);
                        }
                    }
                }
            }
        }

        public static void AddMothershipDataToDB(string command, double duration, int httpcode)
        {

            if (!mothershipCommands.ContainsKey(command))
            {
                AddCommandToDB(command);
                GetMothershipCommandsFromDB();
            }
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"
INSERT
    mothership(
        ts,
        commandid,
        duration,
        httpcode
    )
VALUES(
    @ts,
    @commandid,
    @duration,
    @httpcode
)", con))
                {
                    cmd.Parameters.AddWithValue("@ts", DateTime.Now);
                    cmd.Parameters.AddWithValue("@commandid", mothershipCommands[command]);
                    cmd.Parameters.AddWithValue("@duration", duration);
                    cmd.Parameters.AddWithValue("@httpcode", httpcode);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal static string GetDBConnectionstring(bool obfuscate = false)
        {
            if (Tools.IsDocker())
                return "Server=database;Database=teslalogger;Uid=root;Password=teslalogger;CharSet=utf8mb4;";

            return "Server=127.0.0.1;Database=teslalogger;Uid=root;Password=teslalogger;CharSet=utf8mb4;";
        }

        public static DataTable GetAllOVMSCars()
        {
            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter("select * from cars where tesla_token like 'OVMS:%'", DBConnectionstring);
            da.Fill(dt);
            return dt;
        }


        public static DateTime UnixToDateTime(long t)
        {
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dt = dt.AddMilliseconds(t);
            dt = dt.ToLocalTime();
            return dt;

        }

        private void CloseChargingState(int openChargingState)
        {
            object meter_vehicle_kwh_end = DBNull.Value;
            object meter_utility_kwh_end = DBNull.Value;

            try
            {
                car.Log($"CloseChargingState id:{openChargingState}");

                int chargeID = GetMaxChargeid(out DateTime chargeEnd, out _);
                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(@"
UPDATE
    chargingstate
SET
    EndDate = @EndDate,
    EndChargingID = @EndChargingID
WHERE
    id = @ChargingStateID
    AND CarID = @CarID", con))
                    {
                        cmd.Parameters.AddWithValue("@EndDate", chargeEnd);
                        cmd.Parameters.AddWithValue("@EndChargingID", chargeID);
                        cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                        cmd.Parameters.AddWithValue("@ChargingStateID", openChargingState);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                car.CreateExceptionlessClient(ex).Submit();
                Logfile.ExceptionWriter(ex, "Exception during CloseChargingState()");
            }
        }

        private int GetMaxChargingstateId(out double lat, out double lng, out DateTime UnplugDate, out DateTime EndDate)
        {
            UnplugDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"
SELECT
    chargingstate.id,
    lat,
    lng,
    UnplugDate,
    EndDate
FROM
    chargingstate
JOIN
    pos
ON
    chargingstate.pos = pos.id
WHERE
    chargingstate.id IN(
    SELECT
        MAX(id)
    FROM
        chargingstate
    WHERE
        carid = @CarID
)", con))
                {
                    cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read() && dr[0] != DBNull.Value && dr[1] != DBNull.Value && dr[2] != DBNull.Value)
                    {
                        if (!double.TryParse(dr[1].ToString(), out lat)) { lat = double.NaN; }
                        if (!double.TryParse(dr[2].ToString(), out lng)) { lng = double.NaN; }
                        if (!DateTime.TryParse(dr[3].ToString(), out UnplugDate)) { UnplugDate = DateTime.MinValue; }
                        if (!DateTime.TryParse(dr[4].ToString(), out EndDate)) { EndDate = DateTime.MinValue; }
                        return Convert.ToInt32(dr[0], Tools.ciEnUS);
                    }
                }
            }
            lat = double.NaN;
            lng = double.NaN;
            return 0;
        }

        private int GetMaxChargeid(out DateTime chargeStart, out double battery_level)
        {
            battery_level = 0;
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"
SELECT
    id,
    datum,
    battery_level
FROM
    charging
WHERE
    CarID = @CarID
ORDER BY
    datum DESC
LIMIT 1", con))
                {
                    cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read() && dr[0] != DBNull.Value && dr[1] != DBNull.Value)
                    {
                        if (!DateTime.TryParse(dr[1].ToString(), out chargeStart))
                        {
                            chargeStart = DateTime.Now;
                        }

                        battery_level = Convert.ToDouble(dr["battery_level"].ToString(), Tools.ciEnUS);

                        return Convert.ToInt32(dr[0], Tools.ciEnUS);
                    }
                }
            }
            chargeStart = DateTime.Now;
            return 0;
        }

        internal void CloseChargingstates()
        {
            int id = GetMaxChargingstateId(out double lat, out double lng, out DateTime UnplugDate, out DateTime EndDate);
            if (id > 0)
                CloseChargingState(id);
        }

        public void InsertPos(string timestamp, double latitude, double longitude, int speed, decimal power, double odometer, double idealBatteryRangeKm, double batteryRangeKm, int batteryLevel, double? outsideTemp, string altitude)
        {
            double? inside_temp = null;
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(@"
INSERT
    pos(
        CarID,
        Datum,
        lat,
        lng,
        speed,
        POWER,
        odometer,
        ideal_battery_range_km,
        battery_range_km,
        outside_temp,
        altitude,
        battery_level,
        inside_temp,
        battery_heater,
        is_preconditioning,
        sentry_mode
    )
VALUES(
    @CarID,
    @Datum,
    @lat,
    @lng,
    @speed,
    @power,
    @odometer,
    @ideal_battery_range_km,
    @battery_range_km,
    @outside_temp,
    @altitude,
    @battery_level,
    @inside_temp,
    @battery_heater,
    @is_preconditioning,
    @sentry_mode
)", con))
                {
                    cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                    cmd.Parameters.AddWithValue("@Datum", timestamp);
                    cmd.Parameters.AddWithValue("@lat", latitude);
                    cmd.Parameters.AddWithValue("@lng", longitude);
                    cmd.Parameters.AddWithValue("@speed", speed);
                    cmd.Parameters.AddWithValue("@power", Convert.ToInt32(power * 1.35962M));
                    cmd.Parameters.AddWithValue("@odometer", odometer);

                    if (idealBatteryRangeKm == -1)
                    {
                        cmd.Parameters.AddWithValue("@ideal_battery_range_km", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ideal_battery_range_km", idealBatteryRangeKm);
                    }

                    if (batteryRangeKm == -1)
                    {
                        cmd.Parameters.AddWithValue("@battery_range_km", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@battery_range_km", batteryRangeKm);
                    }

                    if (outsideTemp == null)
                    {
                        cmd.Parameters.AddWithValue("@outside_temp", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@outside_temp", (double)outsideTemp);
                    }

                    if (altitude != null && altitude.Length == 0)
                    {
                        cmd.Parameters.AddWithValue("@altitude", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@altitude", altitude);
                    }

                    if (batteryLevel == -1)
                    {
                        cmd.Parameters.AddWithValue("@battery_level", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@battery_level", batteryLevel);
                    }

                    if (inside_temp == null)
                    {
                        cmd.Parameters.AddWithValue("@inside_temp", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@inside_temp", (double)inside_temp);
                    }

                    cmd.Parameters.AddWithValue("@battery_heater",  0);
                    cmd.Parameters.AddWithValue("@is_preconditioning",  0);
                    cmd.Parameters.AddWithValue("@sentry_mode",  0);
                    cmd.ExecuteNonQuery();

                    
                }
            }
        }

        internal void InsertCan(string timestamp, int id, double value)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();

                    using (MySqlCommand cmd = new MySqlCommand("insert into can (datum, id, val, carid) values (@datum, @id, @val, @carid)", con))
                    {
                        cmd.Parameters.AddWithValue("@datum", timestamp);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@val", value);
                        cmd.Parameters.AddWithValue("@carid", car.CarInDB);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (MySqlException mex)
            {
                if (mex.ErrorCode == -2147467259) // e.g. Duplicate entry '2022-02-10 23:08:20-2' for key 'PRIMARY'
                    car.Log("Can row already exists");
                else
                {
                    car.Log(mex.ToString());
                    car.SendException2Exceptionless(mex);
                }
            }
            catch (Exception ex)
            {
                car.SendException2Exceptionless(ex);
                car.Log(ex.ToString());
            }
        }

        public void StartDriveState(DateTime now)
        {
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("insert drivestate (StartDate, StartPos, CarID) values (@StartDate, @Pos, @CarID)", con))
                {
                    cmd.Parameters.AddWithValue("@StartDate", now);
                    cmd.Parameters.AddWithValue("@Pos", GetMaxPosid());
                    cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CloseDriveState(DateTime EndDate)
        {
            int StartPos = 0;
            int MaxPosId = GetMaxPosid();

            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"
SELECT
    StartPos
FROM
    drivestate
WHERE
    EndDate IS NULL
    AND CarID = @carid", con))
                {
                    cmd.Parameters.AddWithValue("@carid", car.CarInDB);
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        StartPos = Convert.ToInt32(dr[0], Tools.ciEnUS);
                    }
                    dr.Close();
                }
            }

            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"
UPDATE
    drivestate
SET
    EndDate = @EndDate,
    EndPos = @Pos
WHERE
    EndDate IS NULL
    AND CarID = @CarID", con))
                {
                    cmd.Parameters.AddWithValue("@EndDate", EndDate);
                    cmd.Parameters.AddWithValue("@Pos", MaxPosId);
                    cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                    cmd.ExecuteNonQuery();
                }
            }

            if (StartPos != 0)
            {
                UpdateDriveStatistics(StartPos, MaxPosId);
            }

            /*
            _ = Task.Factory.StartNew(() =>
            {
                if (StartPos > 0)
                {
                    UpdateTripElevation(StartPos, MaxPosId, car, " (Task)");

                    StaticMapService.GetSingleton().Enqueue(car.CarInDB, StartPos, MaxPosId, 0, 0, StaticMapProvider.MapMode.Dark, StaticMapProvider.MapSpecial.None);
                    StaticMapService.GetSingleton().CreateParkingMapFromPosid(StartPos);
                    StaticMapService.GetSingleton().CreateParkingMapFromPosid(MaxPosId);
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            */
        }

        private void UpdateDriveStatistics(int startPos, int endPos, bool logging = false)
        {
            try
            {
                if (logging)
                {
                    car.Log("UpdateDriveStatistics");
                }

                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT avg(outside_temp) as outside_temp_avg, max(speed) as speed_max, max(power) as power_max, min(power) as power_min, avg(power) as power_avg FROM pos where id between @startpos and @endpos and CarID=@CarID", con))
                    {
                        cmd.Parameters.AddWithValue("@startpos", startPos);
                        cmd.Parameters.AddWithValue("@endpos", endPos);
                        cmd.Parameters.AddWithValue("@CarID", car.CarInDB);

                        MySqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            using (MySqlConnection con2 = new MySqlConnection(DBConnectionstring))
                            {
                                con2.Open();
                                using (MySqlCommand cmd2 = new MySqlCommand("update drivestate set outside_temp_avg=@outside_temp_avg, speed_max=@speed_max, power_max=@power_max, power_min=@power_min, power_avg=@power_avg where StartPos=@StartPos and EndPos=@EndPos  ", con2))
                                {
                                    cmd2.Parameters.AddWithValue("@StartPos", startPos);
                                    cmd2.Parameters.AddWithValue("@EndPos", endPos);

                                    cmd2.Parameters.AddWithValue("@outside_temp_avg", dr["outside_temp_avg"]);
                                    cmd2.Parameters.AddWithValue("@speed_max", dr["speed_max"]);
                                    cmd2.Parameters.AddWithValue("@power_max", dr["power_max"]);
                                    cmd2.Parameters.AddWithValue("@power_min", dr["power_min"]);
                                    cmd2.Parameters.AddWithValue("@power_avg", dr["power_avg"]);

                                    cmd2.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }

                // If Startpos doesn't have an "ideal_battery_rage_km", it will be updated from the first valid dataset
                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM pos where id = @startpos", con))
                    {
                        cmd.Parameters.AddWithValue("@startpos", startPos);

                        MySqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            if (dr["ideal_battery_range_km"] == DBNull.Value)
                            {
                                DateTime dt1 = (DateTime)dr["Datum"];
                                dr.Close();

                                using (MySqlCommand cmd2 = new MySqlCommand("SELECT * FROM pos where id > @startPos and ideal_battery_range_km is not null and battery_level is not null and CarID=@CarID order by id asc limit 1", con))
                                {
                                    cmd2.Parameters.AddWithValue("@startPos", startPos);
                                    cmd2.Parameters.AddWithValue("@CarID", car.CarInDB);
                                    dr = cmd2.ExecuteReader();

                                    if (dr.Read())
                                    {
                                        DateTime dt2 = (DateTime)dr["Datum"];
                                        TimeSpan ts = dt2 - dt1;

                                        object ideal_battery_range_km = dr["ideal_battery_range_km"];
                                        object battery_level = dr["battery_level"];

                                        if (ts.TotalSeconds < 120)
                                        {
                                            dr.Close();

                                            using (var cmd3 = new MySqlCommand("update pos set ideal_battery_range_km = @ideal_battery_range_km, battery_level = @battery_level where id = @startPos", con))
                                            {
                                                cmd3.Parameters.AddWithValue("@startPos", startPos);
                                                cmd3.Parameters.AddWithValue("@ideal_battery_range_km", ideal_battery_range_km.ToString());
                                                cmd3.Parameters.AddWithValue("@battery_level", battery_level.ToString());
                                                cmd3.ExecuteNonQuery();

                                                car.Log($"Trip from {dt1} ideal_battery_range_km updated!");
                                            }
                                        }
                                        else
                                        {
                                            // Logfile.Log($"Trip from {dt1} ideal_battery_range_km is NULL, but last valid data is too old: {dt2}!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                // If Endpos doesn't have an "ideal_battery_rage_km", it will be updated from the last valid dataset
                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT * FROM pos where id = @endpos", con))
                    {
                        cmd.Parameters.AddWithValue("@endpos", endPos);

                        MySqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            if (dr["ideal_battery_range_km"] == DBNull.Value)
                            {
                                DateTime dt1 = (DateTime)dr["Datum"];
                                dr.Close();

                                using (var cmd2 = new MySqlCommand("SELECT * FROM pos where id < @endpos and ideal_battery_range_km is not null and battery_level is not null and CarID=@CarID order by id desc limit 1", con))
                                {
                                    cmd2.Parameters.AddWithValue("@endpos", endPos);
                                    cmd2.Parameters.AddWithValue("@CarID", car.CarInDB);
                                    dr = cmd2.ExecuteReader();

                                    if (dr.Read())
                                    {
                                        DateTime dt2 = (DateTime)dr["Datum"];
                                        TimeSpan ts = dt1 - dt2;

                                        object ideal_battery_range_km = dr["ideal_battery_range_km"];
                                        object battery_level = dr["battery_level"];

                                        if (ts.TotalSeconds < 120)
                                        {
                                            dr.Close();

                                            using (var cmd3 = new MySqlCommand("update pos set ideal_battery_range_km = @ideal_battery_range_km, battery_level = @battery_level where id = @endpos", con))
                                            {
                                                cmd3.Parameters.AddWithValue("@endpos", endPos);
                                                cmd3.Parameters.AddWithValue("@ideal_battery_range_km", ideal_battery_range_km.ToString());
                                                cmd3.Parameters.AddWithValue("@battery_level", battery_level.ToString());
                                                cmd3.ExecuteNonQuery();

                                                car.Log($"Trip from {dt1} ideal_battery_range_km updated!");
                                            }
                                        }
                                        else
                                        {
                                            // Logfile.Log($"Trip from {dt1} ideal_battery_range_km is NULL, but last valid data is too old: {dt2}!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                car.CreateExceptionlessClient(ex).Submit();
                car.Log(ex.ToString());
            }
        }

        internal void InsertCharging(string timestamp, string battery_level, string charge_energy_added, string charger_power, double ideal_battery_range, double battery_range, string charger_voltage, string charger_phases, string charger_actual_current, double? outside_temp, bool forceinsert, string charger_pilot_current, string charge_current_request)
        {
            Tools.SetThreadEnUS();

            if (charger_phases.Length == 0)
            {
                charger_phases = "1";
            }

            double kmIdeal_Battery_Range = ideal_battery_range;
            double kmBattery_Range = battery_range;

            double powerkW = Convert.ToDouble(charger_power, Tools.ciEnUS);

            // default waitbetween2pointsdb
            double waitbetween2pointsdb = 1000.0 / powerkW;

            double deltaSeconds = (DateTime.Now - lastChargingInsert).TotalSeconds;

            // if (forceinsert || deltaSeconds > waitbetween2pointsdb)
            {
                lastChargingInsert = DateTime.Now;

                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(@"
INSERT
    charging(
        CarID,
        Datum,
        battery_level,
        charge_energy_added,
        charger_power,
        ideal_battery_range_km,
        battery_range_km,
        charger_voltage,
        charger_phases,
        charger_actual_current,
        outside_temp,
        charger_pilot_current,
        charge_current_request,
        battery_heater
    )
VALUES(
    @CarID,
    @Datum,
    @battery_level,
    @charge_energy_added,
    @charger_power,
    @ideal_battery_range_km,
    @battery_range_km,
    @charger_voltage,
    @charger_phases,
    @charger_actual_current,
    @outside_temp,
    @charger_pilot_current,
    @charge_current_request,
    @battery_heater
)", con))
                    {
                        cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                        cmd.Parameters.AddWithValue("@Datum", timestamp);
                        cmd.Parameters.AddWithValue("@battery_level", battery_level);
                        cmd.Parameters.AddWithValue("@charge_energy_added", charge_energy_added);
                        cmd.Parameters.AddWithValue("@charger_power", charger_power);
                        cmd.Parameters.AddWithValue("@ideal_battery_range_km", kmIdeal_Battery_Range);
                        cmd.Parameters.AddWithValue("@battery_range_km", kmBattery_Range);
                        cmd.Parameters.AddWithValue("@charger_voltage", int.Parse(charger_voltage, Tools.ciEnUS));
                        cmd.Parameters.AddWithValue("@charger_phases", charger_phases);
                        cmd.Parameters.AddWithValue("@charger_actual_current", charger_actual_current);
                        cmd.Parameters.AddWithValue("@battery_heater", 0);

                        if (charger_pilot_current != null && int.TryParse(charger_pilot_current, out int i))
                        {
                            cmd.Parameters.AddWithValue("@charger_pilot_current", i);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@charger_pilot_current", DBNull.Value);
                        }

                        if (charge_current_request != null && int.TryParse(charge_current_request, out i))
                        {
                            cmd.Parameters.AddWithValue("@charge_current_request", i);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@charge_current_request", DBNull.Value);
                        }

                        if (outside_temp == null)
                        {
                            cmd.Parameters.AddWithValue("@outside_temp", DBNull.Value);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@outside_temp", (double)outside_temp);
                        }
                        cmd.ExecuteNonQuery();

                        car.Log("Insert Charging");
                    }
                }
            }

        }

        public static void UpdateAddress(Car c, int posid)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("select lat, lng from pos where id = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", posid);
                        MySqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            double lat = Convert.ToDouble(dr[0], Tools.ciEnUS);
                            double lng = Convert.ToDouble(dr[1], Tools.ciEnUS);
                            dr.Close();

                            WebHelper.ReverseGecocodingAsync(c, lat, lng).ContinueWith(task =>
                            {
                                try
                                {
                                    using (MySqlConnection con2 = new MySqlConnection(DBConnectionstring))
                                    {
                                        con2.Open();
                                        using (MySqlCommand cmd2 = new MySqlCommand(@"
UPDATE
  pos
SET
  address = @adress
WHERE
  id = @id", con2))
                                        {
                                            cmd2.Parameters.AddWithValue("@id", posid);
                                            cmd2.Parameters.AddWithValue("@adress", task.Result);
                                            cmd2.ExecuteNonQuery();

                                            GeocodeCache.Instance.Write();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.ToExceptionless().Submit();
                                    Logfile.Log(ex.ToString());
                                }
                            }, TaskScheduler.Default);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                Logfile.Log(ex.ToString());
            }
        }

        public int GetMaxPosid(bool withReverseGeocoding = true)
        {
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(@"
SELECT
    MAX(id)
FROM
    pos
WHERE
    CarID = @CarID", con))
                {
                    cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read() && dr[0] != DBNull.Value)
                    {
                        int pos = Convert.ToInt32(dr[0], Tools.ciEnUS);
                        if (withReverseGeocoding)
                        {
                            UpdateAddress(car, pos);
                        }

                        return pos;
                    }
                }
            }

            return 0;
        }

        public void StartChargingState(WebHelper wh)
        {
            
            int chargeID = GetMaxChargeid(out DateTime chargeStart, out double start_charging_soc);
            int posId = GetMaxPosid();
            UpdateAddress(car, posId);
            this.start_charging_soc = start_charging_soc;

            int chargingstateid = 0;
            if (wh != null)
            {
                using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(@"
INSERT
    chargingstate(
        CarID,
        StartDate,
        Pos,
        StartChargingID,
        fast_charger_brand,
        fast_charger_type,
        conn_charge_cable,
        fast_charger_present,
        meter_vehicle_kwh_start,
        meter_utility_kwh_start
    )
VALUES(
    @CarID,
    @StartDate,
    @Pos,
    @StartChargingID,
    @fast_charger_brand,
    @fast_charger_type,
    @conn_charge_cable,
    @fast_charger_present,
    @meter_vehicle_kwh_start,
    @meter_utility_kwh_start
)", con))
                    {
                        cmd.Parameters.AddWithValue("@CarID", car.CarInDB);
                        cmd.Parameters.AddWithValue("@StartDate", chargeStart);
                        cmd.Parameters.AddWithValue("@Pos", posId);
                        cmd.Parameters.AddWithValue("@StartChargingID", chargeID);
                        cmd.Parameters.AddWithValue("@fast_charger_brand", "");
                        cmd.Parameters.AddWithValue("@fast_charger_type", "");
                        cmd.Parameters.AddWithValue("@conn_charge_cable", "");
                        cmd.Parameters.AddWithValue("@fast_charger_present", DBNull.Value);
                        cmd.Parameters.AddWithValue("@meter_vehicle_kwh_start", DBNull.Value);
                        cmd.Parameters.AddWithValue("@meter_utility_kwh_start", DBNull.Value);
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "SELECT LAST_INSERT_ID();";
                        chargingstateid = Convert.ToInt32(cmd.ExecuteScalar(), Tools.ciEnUS);
                    }
                }
            }
        }

        public static string GetVersion()
        {
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("SELECT @@version", con))
                {
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        return dr[0].ToString();
                    }
                }
            }

            return "NULL";
        }

        public static int CountPos()
        {
            using (MySqlConnection con = new MySqlConnection(DBConnectionstring))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand("Select count(*) from pos", con))
                {
                    MySqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        return Convert.ToInt32(dr[0], Tools.ciEnUS);
                    }
                }
            }

            return 0;
        }
    }
}
