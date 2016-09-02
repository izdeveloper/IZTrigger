using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Hardware;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class TurnLaser : NeonMika.Webserver.Responses.JSONResponse
    {
         public TurnLaser(string indexPage)
            : base(indexPage, new turnLaserResponse().TurnLaserMethod)
        { }
    }

    class turnLaserResponse
    {
       
        public turnLaserResponse()
        {

        }
        public void TurnLaserMethod(Request e, JsonObject results)
        {
            Hashtable turnCmd = e.GetArguments;
            if ((string) turnCmd["CMD"] == "on")
                e.getLaser().laserOn();
            if ( (string) turnCmd["CMD"] == "off")
                e.getLaser().laserOff(null);        
        }
    }
}
