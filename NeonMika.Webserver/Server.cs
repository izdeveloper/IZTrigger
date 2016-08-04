using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using NeonMika.Webserver.Responses;
using NeonMika.Util;
using NeonMika.Webserver.Responses.ComplexResponses;
using NeonMika.Webserver.POST;
using System.IO;
using LidarReader;
using DistanceValue;

namespace NeonMika.Webserver
{
    /// <summary>
    /// XML Expansion methods have to be in this form
    /// </summary>
    /// <param name="e">Access to GET or POST arguments,...</param>
    /// <param name="results">This hashtable gets converted into xml on response</param>       
    public delegate void XMLResponseMethod(Request e, Hashtable results);

    /// <summary>
    /// JSON Expansion methods have to be in this form
    /// </summary>
    /// <param name="e">Access to GET or POST arguments,...</param>
    /// <param name="results">This JsonArray gets converted into JSON on response</param>
    /// <returns>True if URL refers to this method, otherwise false (false = SendRequest should not be executed) </returns>        
    public delegate void JSONResponseMethod(Request e, JsonArray results);

    public delegate void JSONResponseMethodObject(Request e, JsonObject results);

    /// <summary>
    /// Main class of NeonMika.Webserver
    /// </summary>
    public class Server
    {
        public int Port { get; private set; }

        private Socket listeningSocket = null;
        private Hashtable responses = new Hashtable();

        private InterruptPort _inport;

        private DistanceValue.DistanceValueClass _distanceValue;

        // private OutputPort led;


        /// <summary>
        /// Creates an NeonMika.Webserver instance running in a seperate thread
        /// </summary>
        /// <param name="portNumber">The port to listen for incoming requests</param>
        public Server()
        {

        }

        public void setDistanceValue(DistanceValue.DistanceValueClass dv)
        {
            _distanceValue = dv;
        }

        public void setInterruptPort(InterruptPort inport)
        {
            _inport = inport;
        }

        public void Start(int port = 80, bool DhcpEnable = true, string ipAddress = "", string subnetMask = "", string gatewayAddress = "", string networkName = "NETDUINOPLUS")
        {
            Debug.Print("THANKS FOR USING INEX LIDAR TRIGGER");

            this.Port = port;

            NetworkSetup(DhcpEnable, ipAddress, subnetMask, gatewayAddress, networkName);
            // StartLedThread(ledPort);
            ResponseListInitialize();
            SocketSetup();

            var webserverThread = new Thread(WaitingForRequest);
            webserverThread.Start();

            Debug.Print("\n\n---------------------------");
            Debug.Print("Webserver is now up and running");

        }

        /// <summary>
        /// Creates the socket that will listen for incoming requests
        /// </summary>
        private void SocketSetup()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            listeningSocket.Listen(5);
        }

        /// <summary>
        /// Starts a loop that lets the selected led blink all 2 seconds
        /// </summary>
        /// <param name="ledPort"></param>
        /// 
        /*
        private void StartLedThread(OutputPort ledPort)
        {
            led = ledPort;

            Thread t = new Thread(
                new ThreadStart(
                    delegate()
                    {
                        while (true)
                        {
                            ledPort.Write(true);
                            Thread.Sleep(30);
                            ledPort.Write(false);
                            Thread.Sleep(2000);
                        }
                    }
                    ));
            t.Start();
        }
        */


        /// <summary>
        /// Sets the ip adress and the networkname
        /// </summary>
        /// <param name="DhcpEnable">If true, ip will be received from router via DHCP</param>
        /// <param name="ipAddress"></param>
        /// <param name="subnetMask"></param>
        /// <param name="gatewayAddress"></param>
        /// <param name="networkName">Instead of using the ip, this name can be used in the browser to connect to the device</param>
        private void NetworkSetup(bool DhcpEnable, string ipAddress, string subnetMask, string gatewayAddress, string networkName)
        {
            var interf = NetworkInterface.GetAllNetworkInterfaces()[0];

            if (DhcpEnable)
            {
                //Dynamic IP
                interf.EnableDhcp();
                interf.RenewDhcpLease();
            }
            else
            {
                //Static IP
                interf.EnableStaticIP(ipAddress, subnetMask, gatewayAddress);
            }

            NameService nameService = new NameService();
            nameService.AddName(networkName, NameService.NameType.Unique, NameService.MsSuffix.Default);

            Debug.Print("\n\n---------------------------");
            Debug.Print("Network is set up!\nIP: " + interf.IPAddress + " (DHCP: " + interf.IsDhcpEnabled + ")");
            Debug.Print("You can also reach your Netduino with the following network name: " + networkName);
            Debug.Print("---------------------------");
        }

        /// <summary>
        /// Waiting for client to connect.
        /// When bytes were read they get wrapped to a "Reqeust"
        /// </summary>
        private void WaitingForRequest()
        {
            while (true)
            {
                try
                {
                   /* Socket clientSocket =null;
                    while (!listeningSocket.Poll(1000, SelectMode.SelectRead))
                 //   {
                        clientSocket = listeningSocket.Accept();
                        break;
                    }
                    **/
                     using (Socket clientSocket = listeningSocket.Accept())
                    {
                        _inport.DisableInterrupt();
                        //Wait to get the bytes in the sockets "available buffer"
                        int availableBytes = AwaitAvailableBytes(clientSocket);

                        if (availableBytes > 0)
                        {
                            byte[] buffer = new byte[availableBytes > Settings.MAX_REQUESTSIZE ? Settings.MAX_REQUESTSIZE : availableBytes];

                            byte[] header = FilterHeader(clientSocket, buffer);

                            // something wrong with request, ignore it 
                            if (header.Length == 0) continue;

                            //reqeust created, checking the response possibilities
                            using (Request tempRequest = new Request(Encoding.UTF8.GetChars(header), clientSocket))
                            {
                                Debug.Print("\n\nClient connected\nURL: " + tempRequest.URL + "\nFinal byte count: " + availableBytes + "\n");

                                if (tempRequest.Method == "POST")
                                {
                                    //POST was incoming, it will be saved to SD card at Settings.POST_TEMP_PATH
                                    // This file can later be handled in a normal response method by using PostFileReader
                                    PostToSdWriter post = new PostToSdWriter(tempRequest);
                                    post.ReceiveAndSaveData(buffer, header.Length);
                                }

                                //Let's check if we have to take some action or if it is a file-response 
                                SendResponse(tempRequest);
                            }

                            try
                            {
                                //Close client, otherwise the browser / client won't work properly
                                clientSocket.Close();
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.ToString());
                            }

                            Debug.Print("Request finished");
                            _inport.EnableInterrupt();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                }
            }
        }

        /// <summary>
        /// Reads in the data from the socket and seperates the header from the rest of the request.
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="buffer">Will get filled with the incoming data</param>
        /// <returns>The header</returns>
        private byte[] FilterHeader(Socket clientSocket, byte[] buffer)
        {
            byte[] header = new byte[0];
            int readByteCount = clientSocket.Receive(buffer, buffer.Length, SocketFlags.None);

            // DEBUG
            String strLine = new String(System.Text.Encoding.UTF8.GetChars(buffer, 0, buffer.Length));
            Debug.Print("+->" + strLine);

            for (int headerend = 0; headerend < buffer.Length - 3; headerend++)
            {
                if (buffer[headerend] == '\r' && buffer[headerend + 1] == '\n' && buffer[headerend + 2] == '\r' && buffer[headerend + 3] == '\n')
                {
                    header = new byte[headerend + 4];
                    Array.Copy(buffer, 0, header, 0, headerend + 4);
                    break;
                }
            }

            return header;
        }

        /// <summary>
        /// Returns the number of available bytes.
        /// Waits till all bytes from one request are received.
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <returns></returns>
        private int AwaitAvailableBytes(Socket clientSocket)
        {
            int availableBytes = 0;
            int newAvBytes;

            do
            {
                //Wait if bytes come in
                Thread.Sleep(15);
                newAvBytes = clientSocket.Available - availableBytes;

                // breaks the "always true loop" if no new bytes got available
                if (newAvBytes == 0)
                    break;

                availableBytes += newAvBytes;
                newAvBytes = 0;
            } while (true); //repeat as long as new bytes were received

            return availableBytes;
        }

        /// <summary>
        /// Checks what Response has to be executed.
        /// It compares the requested page URL with the URL set for the coded responses 
        /// </summary>
        /// <param name="e"></param>
        private void SendResponse(Request e)
        {
            Response response = null;


            if (responses.Contains(e.URL))
                response = (Response)responses[e.URL];
            else
                response = (Response)responses["FileResponse"];


            if (response != null)
            {
                using (response)
                {
                    if (response.ConditionsCheckAndDataFill(e))
                    {
                        if (!response.SendResponse(e))
                            Debug.Print("Sending response failed");
                    }
                    else
                    {
                        response.Send404_NotFound(e.Client);
                    }
                }
            }
        }

        //-------------------------------------------------------------
        //-------------------------------------------------------------
        //---------------Webserver expansion---------------------------
        //-------------------------------------------------------------
        //-------------------------------------------------------------
        //-------------------Basic methods-----------------------------

        /// <summary>
        /// Adds a Response
        /// </summary>
        /// <param name="response">XMLResponse that has to be added</param>
        public void AddResponse(Response response)
        {
            if (!responses.Contains(response.URL))
            {
                responses.Add(response.URL, response);
            }
        }

        /// <summary>
        /// Removes a Response
        /// </summary>
        /// <param name="ResponseName">XMLResponse that has to be deleted</param>
        public void RemoveResponse(String ResponseName)
        {
            if (responses.Contains(ResponseName))
            {
                responses.Remove(ResponseName);
            }
        }

        //-------------------------------------------------------------
        //-------------------------------------------------------------
        //-----------------------EXPAND this methods-------------------

        /// <summary>
        /// Initialize the basic functionalities of the webserver
        /// </summary>
        private void ResponseListInitialize()
        {
            AddResponse(new IndexResponse(""));
            AddResponse(new Abc("abc"));
            AddResponse(new FileUpload("upload"));
            AddResponse(new FileResponse());
            AddResponse(new DeleteAll("deleteall"));
            // Create Response and pass DistanceValue object 
            AddResponse(new ReadLidar("readlidar"));
            ReadLidar r = (ReadLidar) responses["readlidar"];
            r.setDistanceValue(_distanceValue);
        }

        //-------------------------------------------------------------
        //---------------------Expansion Methods-----------------------
        //-------------------------------------------------------------
        //----------Look at the echo method for xml example------------

        /// <summary>
        /// Example for webserver expand method
        /// Call via http://servername/echo?value='echovalue'
        /// Submit a 'value' GET parameter
        /// </summary>
        /// <param name="e"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private void Echo(Request e, Hashtable results)
        {
            if (e.GetArguments.Contains("value") == true)
                results.Add("echo", e.GetArguments["value"]);
            else
                results.Add("ERROR", "No 'value'-parameter transmitted to server");
        }

    }
}
