using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaLogger;

namespace OVMS
{
    class GPSData
    {
        public DateTime d;
        public double lat, lng;
    }

    class GPSMovementDetector
    {
        Queue<GPSData> queue = new Queue<GPSData>();
        Car car;
        DateTime lastGPS = DateTime.MinValue;
        bool lastIsDriving = false;

        public GPSMovementDetector(Car car)
        {
            this.car = car;
        }

        
        
        public bool InsertGPSData(DateTime d, double lat, double lng)
        {
            if (lat == 0 | lng == 0)
                return lastIsDriving;

            var t = new GPSData();
            t.d = d;
            t.lat = lat;
            t.lng = lng;

            if (lastGPS != d)
            {
                lastGPS = d;
                queue.Enqueue(t);
            }

            GPSData o = queue.Peek();
            TimeSpan ts = d - o.d;
            if (ts.TotalMinutes > 2)
                queue.Dequeue();

            double distance = Geofence.GetDistance(t.lng, t.lat, o.lng, o.lat);
            // System.Diagnostics.Debug.WriteLine("Distance: " + distance);

            if (ts.TotalSeconds > 0)
                car.Log("Distance: " + distance + " Timespan: " + ts.TotalSeconds + "s");

            if (distance > 200)
            {
                if (!lastIsDriving)
                {
                    car.Log("Ioniq is driving now!");
                    lastIsDriving = true;
                }
                return true;
            }

            if (lastIsDriving)
            {
                car.Log("Ioniq is parking now!");
                lastIsDriving = false;
            }

            return false;
        }
    }
}
