using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using Microsoft.SPOT.Hardware;


namespace NetworkConfig
{
    public class NetworkConfig
    {
        private string _networConfig = "";

        private string _webPort;

        Hashtable _defaults;
        public NetworkConfig(string networkConfig)
        {
            if (networkConfig == null || networkConfig != "")
                _networConfig = networkConfig;
            else
                _networConfig = @"\\SD\\config.txt";

            _defaults = new Hashtable();
            _defaults.Add("static_ip","192.168.5.200");
            _defaults.Add("network_mask","255.255.255.0");
            _defaults.Add("gateway","192.168.5.1");
            _defaults.Add("ntp","0.0.0.0");
            _defaults.Add("dhcp","0");
            _defaults.Add("timezone","15");
            _defaults.Add("timezone_val","-5");
            _defaults.Add("web_port","80");
            _defaults.Add("username","root");
            _defaults.Add("password","root");
        }

        public string getWebPort()
        {
            return _webPort;
        }


        public bool configNetworkSystem()
        {
            string line;
            if (!File.Exists(_networConfig))
            {

                using (var fl = File.Create(_networConfig))
                {
                    using (StreamWriter jsonFile = new StreamWriter(fl))
                    {
                        foreach (DictionaryEntry entry in _defaults)
                        {
                            var str = entry.Key.ToString() + "=" + entry.Value.ToString();
                            jsonFile.WriteLine(str);
                        }
                    }
                }
                // Add some delay to let the Flash close the file , otherwise it gets corrapted with with the next Open 
                Thread.Sleep(250);
            }

            using (var fl = File.OpenRead(_networConfig))
            {
                using (var reader = new StreamReader(fl))
                {
                    Hashtable hashtable = new Hashtable();
                    try
                    {
                        while (null != (line = reader.ReadLine()))
                        {
                            string[] keyvalue = line.Split('=');
                            hashtable[keyvalue[0]] = keyvalue[1];
                        }
                    }
                    catch (Exception ex )
                    {
                        Debug.Print("Exeption ex="+ex.ToString());
                    }

                    if ((string)hashtable["web_port"] == null)
                        _webPort = (string)_defaults["web_port"];
                    else
                        _webPort = (string)hashtable["web_port"];

                    if ((string) hashtable["dhcp"] == "1")
                    {
                        NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();
                        int maxAttemps = 5;
                        while ((NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress == "0.0.0.0") && (maxAttemps-- > 0))
                            System.Threading.Thread.Sleep(1000);
                        if (NetworkInterface.GetAllNetworkInterfaces()[0].IPAddress == "0.0.0.0")
                            return false;
                    }
                    else
                    {
                        string sip;
                        string nm;
                        string gw;
                        if (hashtable["static_ip"] == null)
                            sip = (string)_defaults["static_ip"]; 
                        else
                            sip = (string)hashtable["static_ip"];

                        if (hashtable["network_mask"] == null)
                            nm = (string)_defaults["network_mask"]; 
                        else
                            nm = (string)hashtable["network_mask"];

                        if (hashtable["gateway"] == null)
                            gw = (string)_defaults["gateway"]; 
                        else
                            gw = (string)hashtable["gateway"];

                        NetworkInterface.GetAllNetworkInterfaces()[0].EnableStaticIP(sip, nm, gw);
                    }
                    return true;
                }
            }
        }
    }
}
