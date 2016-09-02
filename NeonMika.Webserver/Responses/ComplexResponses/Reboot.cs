using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Hardware;


namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class Reboot : NeonMika.Webserver.Responses.JSONResponse
    {
        public Reboot(string indexPage)
            : base(indexPage, new rebootResponse().rebootNowMethod)
        { }
    }

    class rebootResponse
    {
        public rebootResponse()
        {
            
        }
        public void rebootNowMethod(Request e, JsonObject results)
        {
            // reboot the device to pick up the new settings
            PowerState.RebootDevice(false);
        }
    }
}
