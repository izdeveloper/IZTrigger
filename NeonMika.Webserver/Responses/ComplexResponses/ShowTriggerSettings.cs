using System;
using System.Text;
using System.IO;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Net.NetworkInformation;
using System.Net.Sockets;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class ShowTriggerSettings: NeonMika.Webserver.Responses.JSONResponse
    {
        private ShowTriggerSettingsResponse _showTriggerSettingsResponse;
        public ShowTriggerSettings(string indexPage)
            : base(indexPage, (JSONResponseMethodObject)null)
        {
            _showTriggerSettingsResponse = new ShowTriggerSettingsResponse();
            setResponseMethodObject(_showTriggerSettingsResponse.showJson);
        }
    }

    class ShowTriggerSettingsResponse
    {
        public ShowTriggerSettingsResponse()
        {
            // constructor
        }
        public void showJson(Request e, JsonObject results)
        {
            string filePath = "\\SD\\trigger.txt";
            string line;
            using (var stream = File.OpenRead(filePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    try
                    {
                        while (null != (line = reader.ReadLine()))
                        {
                            string[] keyvalue = line.Split('=');
                            results.Add(keyvalue[0], keyvalue[1]);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("ShowTrigger Exception ex:" + ex.ToString());
                    }
                }
            }
        }
    }
}
