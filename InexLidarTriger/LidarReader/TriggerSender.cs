using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using Toolbox.NETMF.NET;


namespace LidarReader
{
    class TriggerSender
    {
        public AutoResetEvent autoEvent = new AutoResetEvent(false);
        public Queue triggersQ = new Queue();

        private string _cameraIP;
        private Int32 _cameraPort;
        private bool _cameraTrigger;
        private double _ttlLength;
        private bool _ttlTrigger;
        private bool _stopTrigger;
        private SimpleSocket _socket = null;

        private OutputPort _ttlInterupt = new OutputPort(Pins.GPIO_PIN_D11, false);

        public TriggerSender( string cameraIP, Int32 cameraPort, bool cameraTrigger, double ttlLength, bool ttlTrigger, bool stopTrigger)
        {
            Debug.Print("TriggerSender constructor");
            _cameraIP = cameraIP;
            _cameraTrigger = cameraTrigger;
            _ttlLength = ttlLength;
            _ttlTrigger = ttlTrigger;
            _stopTrigger = stopTrigger;
            _cameraPort = cameraPort;
        }

        public bool connectToCamera()
        {
            try 
            {
                _socket = new IntegratedSocket(_cameraIP, (ushort) _cameraPort);
                // Connects to the socket
                _socket.Connect();
                return true;
            }
            catch(Exception ex)
            {
                if (_socket != null)
                    _socket.Close();
                _socket = null;
                return false;
            }
        }

        public void socketDispose()
        {
            // Closes down the socket
            if (_socket != null)
                _socket.Close();
        }

        public void SendTrigger()
        {
            while (true)
            {
                autoEvent.WaitOne();
                foreach (Object obj in triggersQ)
                {
                    Debug.Print("From Q=" + triggersQ.Dequeue().ToString());
                    // check if we have to  send a HW trigger and the HW trigger length 
                    if (_ttlTrigger == true )
                    {
                        _ttlInterupt.Write(true);
                        Thread.Sleep((int) _ttlLength);
                        _ttlInterupt.Write(false);
                    }
                }
            }
        }
    }
}
