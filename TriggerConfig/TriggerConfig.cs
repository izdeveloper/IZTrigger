using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections;
using Microsoft.SPOT;

namespace TriggerConfig
{
    public class TriggerConfig
    {
        private string _triggerConfig;
        Hashtable _defaults;

        public double SetNoVehicle { get; set; }
        public string CameraIP { get; set; }
        public Int32 CameraPort { get; set; }
        public bool CameraTrigger { get; set; }
        public double TTLLength { get; set; }
        public bool TTLTrigger { get; set; }
        public double StopTime { get; set; }
        public bool StopTrigger { get; set; }

        public TriggerConfig(string triggerConfig)
        {
            if (triggerConfig == null || triggerConfig != "")
                _triggerConfig = triggerConfig;
            else
                _triggerConfig = "\\SD\\trigger.txt";
            _defaults = new Hashtable();
            _defaults.Add("set_no_vehicle", "0");
            _defaults.Add("camera_ip", "0.0.0.0");
            _defaults.Add("camera_port", "13001");
            _defaults.Add("camera_trigger", "1");
            _defaults.Add("ttl_length", "50");
            _defaults.Add("ttl_trigger", "1");
            _defaults.Add("stop_time", "20000000");
            _defaults.Add("stop_trigger", "0");
        }

        public void configTrigger()
        {
            string line;
            if (!File.Exists(_triggerConfig))
            {
                using (var fl = File.Create(_triggerConfig))
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

            using (var fl = File.OpenRead(_triggerConfig))
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
                    catch (Exception ex)
                    {
                        Debug.Print("Exeption ex=" + ex.ToString());
                    }
                    SetNoVehicle = Convert.ToDouble((string)hashtable["set_no_vehicle"]);
                    CameraIP = (string)hashtable["camera_ip"];
                    CameraPort = Int32.Parse((string)hashtable["camera_port"]);
                    CameraTrigger = (string)hashtable["camera_trigger"] == "0" ? false : true;
                    TTLLength = Convert.ToDouble((string)hashtable["ttl_length"]);
                    TTLTrigger = (string)hashtable["ttl_trigger"] == "0" ? false : true;
                    StopTime = Convert.ToDouble((string)hashtable["stop_time"]);
                    StopTrigger = (string)hashtable["stop_trigger"] == "0" ? false : true;
                }
            }
        }
    }
}
