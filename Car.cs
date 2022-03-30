using Exceptionless;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeslaLogger;

namespace OVMS
{
    class Car
    {
        internal CarState _currentState = CarState.Start;
        internal WebHelper wh;
        internal DBHelper DbHelper;
        internal int CarInDB;
        internal CurrentJSON currentJSON;


        internal enum CarState
        {
            Start,
            Drive,
            Park,
            Charge,
            Sleep,
            WaitForSleep,
            Online,
            GoSleep
        }

        Thread thread;
        bool run = true;
        readonly string carId;

        public Car(int CarInDB, string name, string password, string carId)
        {
            this.CarInDB = CarInDB;
            this.carId = carId;


            wh = new WebHelper(this, name, password, carId);
            DbHelper = new DBHelper(this);
            currentJSON = new CurrentJSON(this);

            thread = new Thread(Loop)
            {
                Name = "Car" + CarInDB
            };
            thread.Start();
        }

        private void Loop()
        {
            try
            {
                wh.Auth();
                wh.isDriving(true);
                wh.PrintProtocol();

                while (run)
                {
                    try
                    {
                        switch (_currentState)
                        {
                            case CarState.Start:
                                HandleState_Start();
                                break;

                            case CarState.Online:
                                HandleState_Online();
                                break;

                            case CarState.Charge:
                                HandleState_Charge();
                                break;

                            case CarState.Sleep:
                                HandleState_Sleep();
                                break;

                            case CarState.Drive:
                                HandleState_Drive();
                                break;

                            case CarState.GoSleep:
                                HandleState_GoSleep();
                                break;

                            case CarState.Park:
                                // this state is currently unused
                                Thread.Sleep(5000);
                                break;

                            case CarState.WaitForSleep:
                                // this state is currently unused
                                Thread.Sleep(5000);
                                break;

                            default:
                                Log("Main loop default reached with state: " + _currentState.ToString());
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        SendException2Exceptionless(ex);
                        Log(ex.ToString());

                        Logfile.ExceptionWriter(ex, "#" + CarInDB + ": main loop");
                        Thread.Sleep(10000);
                    }
                }
            }
            catch (Exception ex)
            {
                string temp = ex.ToString();

                if (!temp.Contains("ThreadAbortException"))
                {
                    SendException2Exceptionless(ex);
                    Log(temp);
                }
            }
            finally
            {
                Log("*** Exit Loop !!!");
            }
        }

        private void HandleState_Drive()
        {
            if (!wh.isDriving())
            {
                Log("Driving stop");
                _currentState = CarState.Start;
                DbHelper.CloseDriveState(DateTime.Now);
            }
            else
            {
                System.Threading.Thread.Sleep(10000);
            }
        }

        private void HandleState_GoSleep()
        {
            throw new NotImplementedException();
        }

        private void HandleState_Sleep()
        {
            throw new NotImplementedException();
        }

        private void HandleState_Charge()
        {
            if (!wh.isCharging())
            {
                Log("Charging stop");
                _currentState = CarState.Start;
            }
            else
            {
                System.Threading.Thread.Sleep(30000);
            }
        }

        private void HandleState_Online()
        {
            if (wh.isDriving())
            {
                Log("Start Driving");
                _currentState = CarState.Drive;
                DbHelper.StartDriveState(DateTime.Now);

            }
            else if (wh.isCharging())
            {
                Log("Start Charging");
                _currentState = CarState.Charge;
                wh.isDriving(true);
                DbHelper.StartChargingState(wh);
            }
            else
            {
                System.Threading.Thread.Sleep(15000);
            }
        }

        private void HandleState_Start()
        {
            currentJSON.current_driving = false;
            currentJSON.current_charging = false;
            currentJSON.CreateCurrentJSON();

            DbHelper.CloseChargingstates();

            Log("Start Online");
            _currentState = CarState.Online;            
        }

        public void Log(string text)
        {
            string temp = "#" + CarInDB + ": " + text;
            Logfile.Log(temp);
            System.Diagnostics.Debug.WriteLine(text);
        }

        internal void SendException2Exceptionless(Exception ex)
        {
            CreateExceptionlessClient(ex).Submit();
        }

        internal EventBuilder CreateExceptionlessClient(Exception ex)
        {
            EventBuilder b = ex.ToExceptionless()
                .SetUserIdentity(this.carId)
                .AddObject(wh.Cartype, "Cartype")
                .AddObject(wh.OVMSVersion, "OVMSVersion");

            return b;
        }
    }
}
