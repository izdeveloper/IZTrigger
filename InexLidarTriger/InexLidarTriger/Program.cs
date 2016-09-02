using System;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.SPOT;
using NeonMika.Webserver;
using NeonMika.Webserver.Responses;
using DistanceValue;

namespace InexLidar
{
    public class Program
    {
        public System.Reflection.Assembly WebServerAssm;

        public static void Main()
        {

            /*

            using (FileStream json = new FileStream("\\SD\\JSONLib.pe", FileMode.Open, FileAccess.Read, FileShare.None),
                             util = new FileStream("\\SD\\NeonMika.Util.pe", FileMode.Open, FileAccess.Read, FileShare.None),                        
                             webserver = new FileStream("\\SD\\NeonMika.Webserver.pe", FileMode.Open, FileAccess.Read, FileShare.None))
            {
                byte[] jsonbytes = new byte[json.Length];
                byte[] utilbytes = new byte[util.Length];                
                byte[] webserverbytes = new byte[webserver.Length];

                json.Read(jsonbytes, 0, (int) json.Length);
                util.Read(utilbytes, 0, (int) util.Length);                
                webserver.Read(webserverbytes, 0, (int) webserver.Length);

                var assmjson = Assembly.Load(jsonbytes);
                var assmutil = Assembly.Load(utilbytes);                
                var assmwebserver = Assembly.Load(webserverbytes);

                var objwebserver = AppDomain.CurrentDomain.CreateInstanceAndUnwrap("NeonMika.Webserver, Version=1.0.0.0", "NeonMika.Webserver.Server");
                                
                var typewebserver = assmwebserver.GetType("NeonMika.Webserver.Server");

 
                MethodInfo mi = typewebserver.GetMethod("Start");  

                object[] param = new object[] {(int) 80, (bool) false, "192.168.1.185", "255.255.255.0", "192.168.1.1", "NETDUINOPLUS"};

                // Start web server 
                mi.Invoke(objwebserver, param);

            }
            
        }
        */

            

            Server WebServer = new Server();
            WebServer.Start(80, false, "192.168.2.185", "255.255.255.0", "192.168.2.1", "NETDUINOPLUS");

            Debug.Print("freemem: " + Debug.GC(true));

           // WebServer.AddResponse(new NeonMika.Webserver.Responses.ComplexResponses.Abc("abc"));
           // WebServer.AddResponse(new XMLResponse("wave", new XMLResponseMethod(WebserverXMLMethods.Wave)));
        }
    }
}
