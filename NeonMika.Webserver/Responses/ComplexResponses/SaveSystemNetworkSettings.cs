using System;
using System.IO;
using System.Collections;
using Microsoft.SPOT;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Hardware;
using System.Net.Sockets;
using NetworkConfig; 


namespace NeonMika.Webserver.Responses.ComplexResponses
{
    public class SaveSystemNetworkSettings : NeonMika.Webserver.Responses.JSONResponse
    {
        private SaveSystemNetworkSettingsResponse _saveSystemNetworkSettingsResponse;
        public SaveSystemNetworkSettings(string indexPage)
            : base(indexPage, (JSONResponseMethodObject)null)
        {
            _saveSystemNetworkSettingsResponse = new SaveSystemNetworkSettingsResponse();
            setResponseMethodObject(_saveSystemNetworkSettingsResponse.SaveJsonFile);
        }

        class SaveSystemNetworkSettingsResponse
        {

            public void SaveJsonFile(Request e, JsonObject result)
            {
                string filePath = "\\SD\\config.txt";
                // string fileTmp = "\\SD\\tmp.txt";
                Hashtable reqOnSave = e.GetArguments;

                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    using (var fl = File.Create(filePath))
                    {
                        using (StreamWriter jsonFile = new StreamWriter(fl))
                        {
                            foreach (DictionaryEntry entry in reqOnSave)
                            {
                                var str = entry.Key.ToString() + "=" + entry.Value.ToString();
                                jsonFile.WriteLine(str);
                            }
                        }
                    }
                    Debug.Print("Network Saved");
                    // Configure Network Settings
                    NetworkConfig.NetworkConfig nc = new NetworkConfig.NetworkConfig(filePath);
                    nc.configNetworkSystem();
                }
                catch (Exception ex)
                {
                    // log error, turn status LED red and reboot
                    Debug.Print("Failed to save: "+ex.ToString());
                    // reboot
                    PowerState.RebootDevice(false);
                }
            }
        }
    }
}
