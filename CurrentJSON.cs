﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OVMS
{
    class CurrentJSON
    {
        public bool current_charging = false;
        public bool current_driving = false;
        public bool current_online = false;
        public bool current_sleeping = false;
        public bool current_falling_asleep = false;
        private long timestamp = 0;

        public int current_speed = 0;
        public int current_power = 0;
        public double current_odometer = 0;
        public double current_ideal_battery_range_km = 0;
        public double current_battery_range_km = 0;
        public double current_outside_temperature = 0;
        public int current_battery_level = 0;

        public int current_charger_voltage = 0;
        public int current_charger_phases = 0;
        public int current_charger_actual_current = 0;
        public double current_charge_energy_added = 0;
        public int current_charger_power = 0;
        public double current_charge_rate_km = 0;
        public double current_time_to_full_charge = 0;
        public bool current_charge_port_door_open = false;

        public string current_car_version = "";

        public DateTime current_trip_start = DateTime.MinValue;
        public DateTime current_trip_end = DateTime.MinValue;
        public double current_trip_km_start = 0;
        public double current_trip_km_end = 0;
        public double current_trip_max_speed = 0;
        public double current_trip_max_power = 0;
        public double current_trip_start_range = 0;
        public double current_trip_end_range = 0;
        public double Wh_TR = 0.19;

        public int current_trip_duration_sec = 0;

        public double latitude = 0;
        public double longitude = 0;
        public int charge_limit_soc = 0;
        public int heading = 0;
        public double current_inside_temperature = 0;
        public bool current_battery_heater = false;
        public bool current_is_sentry_mode = false;
        public bool current_is_preconditioning = false;

        public string current_country_code = "";
        public string current_state = "";

        public DateTime lastScanMyTeslaReceived = DateTime.MinValue;
        public double? SMTCellTempAvg = null;
        public double? SMTCellMinV = null;
        public double? SMTCellAvgV = null;
        public double? SMTCellMaxV = null;
        public double? SMTCellImbalance = null;
        public double? SMTBMSmaxCharge = null;
        public double? SMTBMSmaxDischarge = null;
        public double? SMTACChargeTotal = null;
        public double? SMTDCChargeTotal = null;
        public double? SMTNominalFullPack = null;

        public double? SMTSpeed = null;
        public double? SMTBatteryPower = null;

        public string current_json = "";
        private DateTime lastJSONwrite = DateTime.MinValue;
        Car car;

        public CurrentJSON(Car car)
        {
            this.car = car;
        }

        public void CreateCurrentJSON()
        {
            try
            {
                lastJSONwrite = DateTime.UtcNow;

                int duration = 0;
                double distance = 0;
                double trip_kwh = 0.0;
                double trip_avg_wh = 0.0;

                try
                {
                    if (current_trip_end == DateTime.MinValue)
                    {
                        duration = (int)(DateTime.Now - current_trip_start).TotalSeconds;
                        distance = current_odometer - current_trip_km_start;
                        trip_kwh = (current_trip_start_range - current_ideal_battery_range_km) * Wh_TR;

                        if (distance > 0)
                        {
                            trip_avg_wh = trip_kwh / distance * 1000;
                        }
                    }
                    else
                    {
                        duration = (int)(current_trip_end - current_trip_start).TotalSeconds;
                        distance = current_trip_km_end - current_trip_km_start;
                        trip_kwh = (current_trip_start_range - current_trip_end_range) * Wh_TR;

                        if (distance > 0)
                        {
                            trip_avg_wh = trip_kwh / distance * 1000;
                        }
                    }
                }
                catch (Exception ex)
                {
                    car.CreateExceptionlessClient(ex).Submit();

                    car.Log(ex.ToString());
                    duration = 0;
                }
                if (duration < 0)
                {
                    duration = 0;
                }

                Dictionary<string, object> values = new Dictionary<string, object>
                {
                   { "charging", current_charging},
                   { "driving", current_driving },
                   { "online", current_online },
                   { "sleeping", current_sleeping },
                   { "falling_asleep", current_falling_asleep },
                   { "speed", current_speed},
                   { "power", current_power },
                   { "odometer", current_odometer },
                   { "ideal_battery_range_km", current_ideal_battery_range_km},
                   { "battery_range_km", current_battery_range_km},
                   { "outside_temp", current_outside_temperature},
                   { "battery_level", current_battery_level},
                   { "charger_voltage", current_charger_voltage},
                   { "charger_phases", current_charger_phases},
                   { "charger_actual_current", current_charger_actual_current},
                   { "charge_energy_added", current_charge_energy_added},
                   { "charger_power", current_charger_power},
                   { "charge_rate_km", current_charge_rate_km},
                   { "charge_port_door_open", current_charge_port_door_open },
                   { "time_to_full_charge", current_time_to_full_charge},
                   { "car_version", current_car_version },
                   { "trip_start", current_trip_start.ToString("t",Tools.ciDeDE) },
                   { "trip_start_dt", current_trip_start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", Tools.ciEnUS) },
                   { "trip_max_speed", current_trip_max_speed },
                   { "trip_max_power", current_trip_max_power },
                   { "trip_duration_sec", duration },
                   { "trip_kwh", trip_kwh },
                   { "trip_avg_kwh", trip_avg_wh },
                   { "trip_distance", distance },
                   { "ts", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", Tools.ciEnUS)},
                   { "latitude", latitude },
                   { "longitude", longitude },
                   { "charge_limit_soc", charge_limit_soc},
                   { "inside_temperature", current_inside_temperature },
                   { "battery_heater", current_battery_heater },
                   { "is_preconditioning", current_is_preconditioning },
                   { "sentry_mode", current_is_sentry_mode },
                   { "country_code", current_country_code },
                   { "state", current_state },
                   { "display_name", ""},
                   { "heading", heading}
                };



                current_json = JsonConvert.SerializeObject(values);
                car.wh.SendCurrentJSON(current_json);

            }
            catch (Exception ex)
            {
                if (ex is AggregateException && ex.InnerException is HttpRequestException hrex && hrex.InnerException is WebException wex)
                {
                    car.Log("SendCurrentJSON: " + wex.Message);
                }
                else
                {
                    car.CreateExceptionlessClient(ex).Submit();

                    car.Log(ex.ToString());
                    current_json = "";

                }
                current_json = "";
            }
        }

    }
}
