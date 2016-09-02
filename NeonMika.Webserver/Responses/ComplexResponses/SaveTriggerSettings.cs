using System;
using System.IO;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Net.NetworkInformation;
using Microsoft.SPOT.Hardware;
using System.Net.Sockets;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class SaveTriggerSettings : NeonMika.Webserver.Responses.JSONResponse
    {
        private SaveTriggerSettingsResponse _saveTriggerSettingsResponse;
        public SaveTriggerSettings(string indexPage)
            : base(indexPage, (JSONResponseMethodObject)null)
        {
            _saveTriggerSettingsResponse = new SaveTriggerSettingsResponse();
            setResponseMethodObject(_saveTriggerSettingsResponse.SaveJsonFile);
        }

        class SaveTriggerSettingsResponse
        {
            public void SaveJsonFile(Request e, JsonObject result)
            {
                string filePath = "\\SD\\trigger.txt";
                Hashtable reqOnSave = e.GetArguments;
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        Thread.Sleep(250);
                    }
                    using (var fl = File.Create(filePath))
                    {
                        using (StreamWriter jsonFile = new StreamWriter(fl))
                        {
                            foreach (DictionaryEntry entry in reqOnSave)
                            {
                                var str = entry.Key.ToString() + "=" + entry.Value.ToString();
                                Debug.Print(str);
                                jsonFile.WriteLine(str);
                            }
                        }
                    }
                    // Add some delay to let the Flash close the file , otherwise it gets corrupted with the next Open 
                    Thread.Sleep(250);
                    Debug.Print("Trigger Saved");
                }
                catch (Exception ex)
                {
                    // log error, turn status LED red and reboot
                    Debug.Print("Failed to save: " + ex.ToString());
                    // reboot
                    PowerState.RebootDevice(false);
                }
            }
        }
    }
}
