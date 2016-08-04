using System;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using DistanceValue;

namespace LidarReader
{
    public class LidarReader
    {
        internal bool LidarInitialized = false;

        internal double LastDistance;
        internal delegate void DistanceHandler(double value);
        internal event DistanceHandler OnDistanceChanged;

        private double _mLidarStartTime;
        private double _sensitivy;
        private double _minValue;
        private double _lasttriggertime = 0;
        private double _lastTriggerDistance = 0;
        private double _triggerSensetivity = 0;
        private double _stoppedTimeInterval = 0;
        private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        private bool _stoppedVehicleHappened;

        private DistanceValue.DistanceValueClass _distanceValueLocation;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        OutputPort _oport;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        public InterruptPort _inport;

        // TriggerSender thread objects
        private TriggerSender _ts;

        public LidarReader(double minValue, double sensitivy, double trigger_sensetivity, double stopped_time_interval)
        {
            Debug.Print("== LidarReader constructor");
            _sensitivy = sensitivy;
            _minValue = minValue;
            _mLidarStartTime = 0;
            LidarInitialized = true;
            _lastTriggerDistance = 0;
            _triggerSensetivity = trigger_sensetivity;
            _stoppedTimeInterval = stopped_time_interval;
            _stoppedVehicleHappened = false;
            _distanceValueLocation = null;

            // assign port , but disable interrupts , will be enabled in the start function
            _oport = new OutputPort(Pins.GPIO_PIN_D2, true);
            _inport = new InterruptPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            _inport.DisableInterrupt();
            // start trigger sender thread
            _ts = new TriggerSender();
            Thread oThread = new Thread(new ThreadStart(_ts.SendTrigger));
            oThread.Start();
        }

        public void Start ()
        {
            _inport.OnInterrupt += inport_OnInterrupt;
            _oport.Write(false);
            _inport.EnableInterrupt();

        }

        public InterruptPort getInterruptPort()
        {
            return _inport;
        }

        public void setDistanceValueLocation(DistanceValue.DistanceValueClass dv)
        {
            _distanceValueLocation = dv;
        }

        public double getDistance()
        {
            return LastDistance;
        }
        public  void  inport_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            //_inport.Dispose();
            
            try
            {
                _inport.DisableInterrupt();
            }
            catch (Exception ex)
            {
                Debug.Print("ERROR => LidarRead exception");
                _inport.Dispose();
                _inport = new InterruptPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            }
            
            

       //     _inport.Dispose();

            // Debug.GC(true);
            try
            {
                // Debug.Print("data2: " + data2+" data1"+data1);
                if (data2 == 0)
                {
                    LastDistance = (time.Ticks - _mLidarStartTime) / 100.0;
                    _distanceValueLocation.DistanceValue = LastDistance;


                   // if (_distanceValueLocation != null)
                   //     _distanceValueLocation.currentDistance = LastDistance;

                    // Debug.Print("Distance: " + LastDistance);

                    // If last measured distance less than the distance considered to be a vehicle in FOV
                    // record it and decide if a trigger has to be sent
                    if (LastDistance < _minValue + _sensitivy)
                    {

                        // if the new distance almost the same as the old one then don't consider it as a new trigger 
                        if ((_lastTriggerDistance < LastDistance + _triggerSensetivity)
                            &&
                             (_lastTriggerDistance > LastDistance - _triggerSensetivity))
                        {
                            // Debug.Print("OLD Trigger: " + LastDistance);

                            // if time when trigger happend first is greater than N then consider this as a stop event 
                            if (time.Ticks - _lasttriggertime > _stoppedTimeInterval)
                            {
                                if (!_stoppedVehicleHappened)
                                {
                                    Debug.Print("Stopped Vehicle Trigger: " + LastDistance);
                                    // Sent notification to the trigger thread that vehicle stopped 
                                    _ts.triggersQ.Enqueue("S");
                                    _ts.autoEvent.Set();
                                }
                                _stoppedVehicleHappened = true;
                                // _lasttriggertime = time.Ticks;
                            }
                        }
                        // if mesuared distance is significanlty diffewrent from the previuos measured
                        // distance, then consider it as a new trigger event 
                        else
                        {
                            _lasttriggertime = time.Ticks;
                            _stoppedVehicleHappened = false;
                            Debug.Print("New Trigger: " + LastDistance);

                            led.Write(true);
                            Thread.Sleep(50);
                            led.Write(false);

                            // Sent notification to the trigger thread
                            _ts.triggersQ.Enqueue("T");
                            _ts.autoEvent.Set();
                        }

                        if (OnDistanceChanged != null)
                        {
                            Debug.Print("Distance: " + LastDistance);
                            // OnDistanceChanged(LastDistance);
                        }
                    }

                    _lastTriggerDistance = LastDistance;
                }

                else
                {
                    // Set time when PMW starts it cycle (rising edge)
                    _mLidarStartTime = time.Ticks;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            Debug.GC(true);
          //  _inport = new InterruptPort(Pins.GPIO_PIN_D3, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            _inport.EnableInterrupt();
        }

        public void Dispose()
        {
            Debug.Print("LidarReader Dispose");
            if (_oport != null)
                _oport.Dispose();
            if (_inport != null)
                _inport.Dispose();
        }
    }
}
