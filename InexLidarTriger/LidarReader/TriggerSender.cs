using System;
using System.Text;
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

        // ZAP parameters
        private UInt64 _msgID = 1;
        private string _zver = "4.0";
        private int _senID = 1;
        private int _tarID = 999;
        private string _zmode = "off";
        private string _zsrc = "";
        private UInt64 _trID= 1;

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
            _zsrc = "lidar_" + _senID;
            _socket = null;
        }

        public string getEventChannelMsg()
        {
            DateTime t = DateTime.Now;
            string rts = t.Year + "-" + t.Month + "-" + t.Day + "T" + t.Hour + ":" + t.Minute + ":" + t.Second + "." + t.Millisecond + "+00:00";
            UInt64 reqid = _msgID + 1000;
            string s = "";
            s = "<ZapPacket Type=\"MSG\" Id=\"" + _msgID + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + " TargetId=\"" + _tarID + "><SetEventChannel RequestId=\"" + reqid + "\"><RequestTimeStamp>" + rts + "</RequestTimeStamp><Mode>" + _zmode + "</Mode></SetEventChannel></ZapPacket>";
            _msgID++;
            return s;
        }

        public string getTriggerMsg()
        {
            DateTime t = DateTime.Now;
            string rts = t.Year + "-" + t.Month + "-" + t.Day + "T" + t.Hour + ":" + t.Minute + ":" + t.Second + "." + t.Millisecond + "+00:00";
            UInt64 reqid = _msgID + 1000;
            string s = "";
            s = "<ZapPacket Type=\"MSG\" Id=\"" + _msgID + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + " TargetId=\"" + _tarID + "><SetEventChannel RequestId=\"" + reqid + "\"><RequestTimeStamp>" + rts + "</RequestTimeStamp><TriggerId>" + _trID + "</TriggerId><Source>" + _zsrc + "</Source></Trigger></ZapPacket>";
            _msgID++;
            _trID++;
            return s;
        }
        
        public bool connectToCamera()
        {
            try 
            {
                _socket = new IntegratedSocket(_cameraIP, (ushort) _cameraPort);
                // Connects to the socket
                _socket.Connect();
                // Set up event channel to not send events back 
                byte[] zap = Encoding.UTF8.GetBytes(getEventChannelMsg());
                byte[] zapmsg = new byte[zap.Length + 2]; // zap message with STX and ETX
                zapmsg.CopyTo(zap, 1);
                zapmsg[0] = 2; //STX
                zapmsg[zapmsg.Length - 1] = 3; // ETX
                _socket.SendBinary(zapmsg);
                // read ack 
                byte[] ack = new byte[1024];
                int bytes_read = 0;
                while (_socket.IsConnected || _socket.BytesAvailable > 0)
                {
                    byte[] ack_read = new byte[_socket.BytesAvailable];
                    ack_read = _socket.ReceiveBinary((int) _socket.BytesAvailable);
                    Array.Copy(ack_read, 0, ack, bytes_read, ack_read.Length);
                    bytes_read += ack_read.Length;
                }
                if ( ack.Length != 0 && (ack[0] != 2 || ack[ack.Length-1] != 3) )
                {
                    if (_socket != null)
                        _socket.Close();
                    _socket = null;
                    return false;
                }
            }
            catch(Exception ex)
            {
                if (_socket != null)
                    _socket.Close();
                _socket = null;
                return false;
            }

            return false;
        }
        public bool sendTrigger()
        {
            return false;
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
                // try to connect to ALPR camera if IP trigger is enabled and there is no connection established
                if (_cameraTrigger == true && (_socket == null || _socket.IsConnected == false))
                {
                   bool ret =  connectToCamera();
                }
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
                    if (_cameraTrigger == true && _socket != null && _socket.IsConnected == true)
                    {
                        // send IP trigger
                    }
                }
            }
        }
    }
}
