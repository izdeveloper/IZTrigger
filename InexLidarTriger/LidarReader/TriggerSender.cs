using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using StatusLED;
using XMLParser;



namespace LidarReader
{
    class TriggerSender
    {
        public AutoResetEvent autoEvent = new AutoResetEvent(false);
        public Queue triggersQ = new Queue();

        private string _cameraIP;
        private Int32 _cameraPort;
        private bool _cameraTriggerDisabled;
        private double _ttlLength;
        private bool _ttlTriggerDisabled;
        private bool _stopTriggerDisabled;
        private Socket _socket = null;
        private bool _closed = true;

        private LidarReader _inport;
        private StatusLED.StatusLED _statusLED = null;

        // ZAP parameters
        private UInt64 _msgID = 1;
        private string _zver = "4.0";
        private int _senID = 1;
        private int _tarID = 999;
        private string _zmode = "off";
        private string _zsrc = "";
        private UInt64 _trID= 1;

        // Reading thread
        Thread _readThread = null;
        bool _threadIsAlive = false;
        bool _threadDead = true;

        private OutputPort _ttlInterupt = new OutputPort(Pins.GPIO_PIN_D11, false);

        public TriggerSender( string cameraIP, Int32 cameraPort, bool cameraTriggerDisabled, double ttlLength, bool ttlTriggerDisabled, bool stopTriggerDisabled)
        {
            Debug.Print("TriggerSender constructor");
            _cameraIP = cameraIP;
            _cameraTriggerDisabled = cameraTriggerDisabled;
            _ttlLength = ttlLength;
            _ttlTriggerDisabled = ttlTriggerDisabled;
            _stopTriggerDisabled = stopTriggerDisabled;
            _cameraPort = cameraPort;
            _zsrc = "lidar_" + _senID;
            _socket = null;
        }

        public void setInterruptPort(LidarReader inport)
        {
            _inport = inport;
        }

        public void setStatusLED(StatusLED.StatusLED statusLED)
        {
            _statusLED = statusLED;
        }

        public string getEventChannelMsg()
        {
            DateTime t = DateTime.Now;
            string rts = t.Year + "-" + t.Month + "-" + t.Day + "T" + t.Hour + ":" + t.Minute + ":" + t.Second + "." + t.Millisecond + "+00:00";
            UInt64 reqid = _msgID + 1000;
            string s = "";
            s = "<ZapPacket Type=\"MSG\" Id=\"" + _msgID + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + "\" TargetId=\"" + _tarID + "\"><SetEventChannel RequestId=\"" + reqid + "\"><RequestTimeStamp>" + rts + "</RequestTimeStamp><Mode>" + _zmode + "</Mode>"
                +"<MemberFilter>all</MemberFilter><ConfidenceFilter>0</ConfidenceFilter><Expiration>300</Expiration><StartTime>2009-02-19T15:39:14.203+02:00</StartTime>" + "</SetEventChannel></ZapPacket>";
            Debug.Print(s);
            _msgID++;
            return s;
        }

        public string getTriggerMsg()
        {
            DateTime t = DateTime.Now;
            string rts = t.Year + "-" + t.Month + "-" + t.Day + "T" + t.Hour + ":" + t.Minute + ":" + t.Second + "." + t.Millisecond + "+00:00";
            UInt64 reqid = _msgID + 1000;
            string s = "";
            // s = "<ZapPacket Type=\"MSG\" Id=\"" + _msgID + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + "\" TargetId=\"" + _tarID + "\"><Trigger RequestId=\""+_trID+"\">" +"<RequestTimeStamp>"+rts+"</RequestTimeStamp>" +"<TriggerId>" + _trID + "</TriggerId>"+"<Source>" + _zsrc + "</Source></Trigger></ZapPacket>";
            s = "<ZapPacket Type=\"MSG\" Id=\"" + _msgID + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + "\" TargetId=\"" + _tarID + "\"><Trigger RequestId=\"" + _trID + "\"><TriggerId>" + _trID + "</TriggerId>" + "<Source>" + _zsrc + "</Source></Trigger></ZapPacket>";
            _msgID++;
            _trID++;
            return s;
        }
        
        public string getACKMsg(string id)
        {
            UInt64 reqid = _msgID + 1000;
            string s = "";
            s = "<ZapPacket Type=\"ACK\" Id=\"" + id + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + "\" SenderName=\"Trigger 1\" SenderSysType=\"ThirdParty\" SenderVersion=\"1\"></ZapPacket>";
            _msgID++;
            return s;
        }

        public string getStatusMsg(string requestid)
        {
            DateTime t = DateTime.Now;
            string rts = t.Year + "-" + t.Month + "-" + t.Day + "T" + t.Hour + ":" + t.Minute + ":" + t.Second + "." + t.Millisecond + "+00:00";
            UInt64 reqid = _msgID + 1000;
            string s = "";
            s = "<ZapPacket Type=\"MSG\" Id=\"" + _msgID + "\" Version=\"" + _zver + "\" SenderId=\"" + _senID + "\"><Status RequestId=\"" + requestid + "\" Severity=\"information\"><GetStatus RequestId=\"" + requestid + "\"><RequestTimeStamp>" + rts + "</RequestTimeStamp></GetStatus><TimeStamp>" + rts + "</TimeStamp></Status></ZapPacket>";
            _msgID++;
            _trID++;
            return s;
        }

        public bool openSocket()
        {
            try
            {
                this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Resolves the hostname to an IP address
                IPHostEntry address = Dns.GetHostEntry(this._cameraIP);
                // Creates the new IP end point
                EndPoint Destination = new IPEndPoint(address.AddressList[0], (int)this._cameraPort);
                Debug.Print("Destination=" + Destination.ToString());
                // Set socket to non-blocking mode
                Type sType = Type.GetType("System.Net.Sockets.Socket");
                FieldInfo blockingInfo = sType.GetField("m_fBlocking", BindingFlags.NonPublic | BindingFlags.Instance);
                blockingInfo.SetValue(_socket, false);

                // Start reading thread
                _readThread = new Thread(new ThreadStart(this.msgReceiver));
                _threadIsAlive = true;
                _threadDead = false;
                _readThread .Start();

                // Connects to the socket
                try
                {
                    _statusLED.redLED();
                    this._socket.Connect(Destination);
                    _statusLED.greenLED();
                    _closed = false;
                }
                catch (Exception ex)
                {
                    //we should ignore it as it takes some time to connect
                    _closed = true;
                    for (int i=0; i < 8; i++)
                    {
                        if (_socket.Poll(100, SelectMode.SelectWrite))
                        {
                            _statusLED.greenLED();
                            Debug.Print("Connect Exception, ignoring="+i);
                            _closed = false;
                            break;
                        }
                        else
                        {
                            Debug.Print("Poll returned false");
                            Thread.Sleep(500);
                        }
                    }
                    if (_closed )
                    {
                        socketDispose();
                        Debug.Print("Connect Failed");
                        return false;
                    }
                }
            }
            catch
            {
                socketDispose();
                return false;
            }
            return true;
        }

        public bool sendMessage(string s)
        {
            
            try
            {
                byte[] zap = Encoding.UTF8.GetBytes(s);
                byte[] zapmsg = new byte[zap.Length + 2]; // zap message with STX and ETX
                zap.CopyTo(zapmsg, 1);
                zapmsg[0] = 2; //STX
                zapmsg[zapmsg.Length - 1] = 3; // ETX

                if (_socket != null && _closed == false)
                    _socket.Send(zapmsg);
                // if we send too fast we may run out of buffers, so lets wait
                if (zapmsg.Length < 32) Thread.Sleep(zapmsg.Length * 10);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Failed to Send a message ...");
                return false;
            }
            
        }


        public void socketDispose()
        {
            // Closes down the socket
            if (_socket != null)
                _socket.Close();
            _socket = null;
            _closed = true;
            _threadIsAlive = false;
            while(_threadDead)
            {
                Thread.Sleep(50);
            }
        }

        public void msgReceiver()
        {
            while (_threadIsAlive)
            {
                bool read_bytes = false;
                byte[] msg = new byte[1024];
                try
                {
                    do
                    {
                        if (_socket != null && _closed == false)
                        {                            
                            _socket.ReceiveTimeout = 300;
                            this._socket.Receive(msg);
                            if (msg[0] == 2 && (Array.IndexOf(msg, 3) > -1))
                            {
                                _inport.disableInterrupt("msgrecieve");
                                read_bytes = true;
                                //process message (ACK, NAC, GetStatus, SetChannel
                                processMsg(msg);
                                _inport.enableInterrupt("msgrecieve");
                            }
                        }
                        else
                        {
                            read_bytes = false;
                        }
                    } while (read_bytes);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10060) // SocketError.TimedOut
                    {
                        Debug.Print("Nothing more to receive, mem:"+Debug.GC(true) );
                    }
                    else
                    {
                        // reboot 
                        PowerState.RebootDevice(false);
                    }
                } 
            }
            _threadDead = true;
        }


        public void processMsg(byte[] msg)
        {
            String msgLine = new String(System.Text.Encoding.UTF8.GetChars(msg, 1, msg.Length-2));
            // XmlParser xml = new XmlParser(msgLine);
            /*
            if ( xml.Read() )
            {
                DictionaryEntry [] xml_attr= xml.GetAttributes();
            }
            */

            int ack = msgLine.IndexOf("ACK");
            int ec = msgLine.IndexOf("EventChannel");
            int gs = msgLine.IndexOf("<GetStatus");
            // check what is this message
            if (ack > 0 )
            {
                // do nothing
                Debug.Print("ACK");
            }

            if (ec > 0)
            {
                // send ack
                Debug.Print("EventChannel");
                int ps = msgLine.IndexOf("Id");
                int pe = msgLine.IndexOf(' ', ps);
                string id  = msgLine.Substring(ps,pe-ps).Split('=')[1].Trim('"');
                string ack_msg = getACKMsg(id);
                sendMessage(ack_msg);
            }

            // send status response 
            if (gs > 0)
            {
                Debug.Print("GetStatus");
                int p = msgLine.IndexOf(">", gs);
                if (p > -1)
                {
                    string xmlgs = msgLine.Substring(gs + 1, p - 1 - gs);
                    string[] param = xmlgs.Split(' ');
                    for (int i = 0; i < param.Length; i++)
                    {
                        int j = param[i].IndexOf("RequestId");
                        if (param[i].IndexOf("RequestId") > -1)
                        {
                            string id = param[i].Split('=')[1].Trim('"');
                            string get_stat_msg = getStatusMsg(id);
                            sendMessage(get_stat_msg);
                            break;
                        }
                    }
                }
            }

        }

        public void SendTrigger()
        {
            while (true)
            {
                // try to connect to ALPR camera if IP trigger is enabled and there is no connection established
                if (_cameraTriggerDisabled == false && (_socket == null || _closed == true))
                {
                    _inport.disableInterrupt("st open");
                    bool ret = openSocket();
                    if (ret == true)
                    {
                        // set ZAP channnel to not send event back to trigger
                        ret = sendMessage(this.getEventChannelMsg());
                    }
                    _inport.enableInterrupt("st open");
                }

                // Wait for trigger from LIDAR Interrupt routine 
                autoEvent.WaitOne();
                // disable interrupt for the time we are sending the triggers
                _inport.disableInterrupt("st");
                foreach (Object obj in triggersQ)
                {
                    string[] trigger_msg = triggersQ.Dequeue().ToString().Split(' ');
                    Debug.Print("From Q=" + trigger_msg[0]+" "+trigger_msg[1]);
                    // check if we have to  send a HW trigger and the HW trigger length 
                    if (_ttlTriggerDisabled == false && trigger_msg[0] == "T")
                    {
                        _ttlInterupt.Write(true);
                        Thread.Sleep((int) _ttlLength);
                        _ttlInterupt.Write(false);
                    }

                    // TODO: add stop trigger sending 
                    /* =====
                     * STOP TRIGGER HERE
                     * =====
                     */

                    // Send IP trigger if not disabled
                    if (_cameraTriggerDisabled == false && _socket != null && _closed == false && trigger_msg[0] == "T")
                    {
                        // send IP trigger
                        sendMessage(this.getTriggerMsg());
                    }
                }
                _inport.enableInterrupt("st");
            }
        }
    }
}
