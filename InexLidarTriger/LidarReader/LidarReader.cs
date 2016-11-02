using System;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Math = System.Math;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using DistanceValue;
using StatusLED;

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
        private OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);
        private bool _stoppedVehicleHappened;
        private double _noVehicleDistanceRange = 0;

        private DistanceValue.DistanceValueClass _distanceValueLocation;

        private string _cameraIP = "";
        private Int32 _cameraPort;
        private bool _cameraTrigger = false;
        private double _ttlLength = 50;
        private bool _ttlTrigger = true;
        private double _stopTime = 20000000;
        private bool _stopTrigger = false;
        private StatusLED.StatusLED _statusLED;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        OutputPort _oport;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        public InterruptPort _inport;

        // TriggerSender thread objects
        private TriggerSender _ts;

        private int _interruptDisableCount = 0;

        // I2C 
        private I2CDevice.Configuration _i2cConfig;
        private I2CDevice i2c;
        private int _i2cDistance;
        DateTime _currentDate;

        public LidarReader(double setNoVehicle, double sensitivy, double trigger_sensetivity)
        {
            Debug.Print("== LidarReader constructor");
            _sensitivy = sensitivy;
            _minValue = setNoVehicle * 0.7;
            _noVehicleDistanceRange = _minValue * 3.2; // distance with no vehicle , if we get more than that , we just ignore the value.
            _mLidarStartTime = 0;
            LidarInitialized = true;
            _lastTriggerDistance = 0;
            _triggerSensetivity = trigger_sensetivity;
            _stoppedVehicleHappened = false;
            _distanceValueLocation = null;
            _interruptDisableCount = 0;

            // assign port , but disable interrupts , will be enabled in the start function
            _oport = new OutputPort(Pins.GPIO_PIN_D2, true);
            _inport = new InterruptPort(Pins.GPIO_PIN_D1, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

            DateTime _currentDate = DateTime.Now;


        }

        public void disableInterrupt (string s)
        {
            lock (_inport)
            {
                _interruptDisableCount++;
           //     _inport.DisableInterrupt();
                // Debug.Print(s+" Disable: " + _interruptDisableCount);
            }
        }

        public void enableInterrupt(string s)
        {
            lock (_inport)
            {
                _interruptDisableCount--;
           //     if (_interruptDisableCount == 0)
           //         _inport.EnableInterrupt();
                // Debug.Print(s+ " Enable: " + _interruptDisableCount);
            }

        }

        public void setTTLTriger(double ttlLength, bool ttlTrigger)
        {
            _ttlLength = ttlLength;
            _ttlTrigger = ttlTrigger;
        }

        public void setIPTrigger(string cameraIP, Int32 cameraPort, bool cameraTrigger)
        {
            _cameraIP = cameraIP;
            _cameraTrigger = cameraTrigger;
            _cameraPort = cameraPort;
        }

        public void setStopTrigger(double stopTime, bool stopTrigger)
        {
            /*
             * Stopped time configuration is in milliseconds,
             * the time resolution is in microseconds,
             * so we have to multiply millisecond by 10000
             * 
             */
            _stopTime = stopTime * 10000;
            _stopTrigger = stopTrigger;

        }
        public void setStatusLED(StatusLED.StatusLED statusLED)
        {
            _statusLED = statusLED;
        }
        public void Start ()
        {
            // start trigger sender thread
            _ts = new TriggerSender(_cameraIP, _cameraPort,_cameraTrigger, _ttlLength, _ttlTrigger, _stopTrigger);
            _ts.setInterruptPort(this);
            _ts.setStatusLED(_statusLED);
            Thread oThread = new Thread(new ThreadStart(_ts.SendTrigger));

            // Init and start I2C
            initI2C();
            inport_OnInterrupt();

            // ======================  DISABLE for DEBUG =======================
            // oThread.Start();
            //_inport.OnInterrupt += inport_OnInterrupt;
           // _oport.Write(false);
           // _inport.EnableInterrupt();
           //   _inport.DisableInterrupt();
            
            // ======================  DISABLE for DEBUG =======================

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

        public void initI2C ()
        {
            _i2cConfig = new I2CDevice.Configuration(0x62, 100);
            i2c = new I2CDevice(_i2cConfig);

            int res = 0;

            // reset I2C
            byte[] command_i2c_reset = { 0x00, 0x00 };
            I2CDevice.I2CTransaction[] i2cReset = new I2CDevice.I2CTransaction[1];
            i2cReset[0] = I2CDevice.CreateWriteTransaction(command_i2c_reset);
            res = i2c.Execute(i2cReset, 1000);
            //    Debug.Print("reset res =" + res);
            Thread.Sleep(100);


        }

        public int readI2CDistance()
        {
            int res = 0;

            // Set I2C to read
            byte[] command_read_with_bias = { 0x00, 0x04 };
            //byte[] status_register = { 0x01 };
            //byte[] status_value = { 0x01, 0x00 };

            I2CDevice.I2CTransaction[] i2cReadDistance = new I2CDevice.I2CTransaction[1];
            i2cReadDistance[0] = I2CDevice.CreateWriteTransaction(command_read_with_bias);
            res = i2c.Execute(i2cReadDistance, 1000);
        //    Debug.Print("Set to read res =" + res);
            Thread.Sleep(20);

            // read Distance
            byte[] distance_addr = { 0x8f };
            byte[] distance1 = { 0, 0 };
            I2CDevice.I2CTransaction[] i2cRead = new I2CDevice.I2CTransaction[2];
            i2cRead[0] = I2CDevice.CreateWriteTransaction(distance_addr);
            i2cRead[1] = I2CDevice.CreateReadTransaction(distance1);
            res = i2c.Execute(i2cRead, 1000);
           // Debug.Print("distance read res =" + res);
            _i2cDistance = distance1[0] << 8 | distance1[1];
         //   Debug.Print("distance = " + _i2cDistance);
            Thread.Sleep(20);
            return _i2cDistance;
        }

        public void inport_OnInterrupt()
        {
            //_inport.Dispose();
            long trigger_time = 0;

            this.disableInterrupt("interrupt");

            while (true)
            {
                trigger_time = DateTime.Now.Ticks; 

                try
                {
                        LastDistance = readI2CDistance();
                        _distanceValueLocation.DistanceValue = LastDistance;

                        // If last measured distance less than the distance considered to be a vehicle in FOV
                        // record it and decide if a trigger has to be sent
                        if (LastDistance < _minValue + _sensitivy)
                        {

                            // if the new distance almost the same as the old one then don't consider it as a new trigger 
                            if ((_lastTriggerDistance < LastDistance + _triggerSensetivity)
                                &&
                                 (_lastTriggerDistance > LastDistance - _triggerSensetivity))
                            {

                                // if time when trigger happend first is greater than N then consider this as a stop event 
                                if (trigger_time - _lasttriggertime > _stopTime)
                                {
                                    if (!_stoppedVehicleHappened)
                                    {
                                        Debug.Print("Stopped Vehicle Trigger: " + LastDistance);
                                        // Sent notification to the trigger thread that vehicle stopped 
                                        _ts.triggersQ.Enqueue("S " + trigger_time);
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
                                _lasttriggertime = trigger_time;
                                _stoppedVehicleHappened = false;
                                Debug.Print("New Trigger: " + LastDistance + " " + trigger_time);

                                led.Write(true);
                                Thread.Sleep(30);
                                led.Write(false);

                                // Sent notification to the trigger thread
                                _ts.triggersQ.Enqueue("T " + trigger_time);
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
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }

                Debug.GC(true);
                this.enableInterrupt("interrupt");
            }
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
