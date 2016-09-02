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
    class ShowSystemNetworkSettings : NeonMika.Webserver.Responses.JSONResponse
    {
        private ShowSystemNetworkSettingsResponse _showSystemNetworkSettingsResponse;
        public ShowSystemNetworkSettings(string indexPage)
            : base(indexPage, (JSONResponseMethodObject)null)
        {
            _showSystemNetworkSettingsResponse = new ShowSystemNetworkSettingsResponse();
            setResponseMethodObject(_showSystemNetworkSettingsResponse.showJson);
        }
    }
    class ShowSystemNetworkSettingsResponse
    {
        public ShowSystemNetworkSettingsResponse()
        {
            // constructor
        }
        public void showJson(Request e, JsonObject results)
        {
            string filePath = "\\SD\\config.txt";
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
                    catch(Exception ex)
                    {
                        Debug.Print("ShowSystem Exception ex:" + ex.ToString());
                    }
                }
            }
        }
    }
}
