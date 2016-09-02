using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;
using Microsoft.SPOT.Hardware;


namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class ShowDistance : NeonMika.Webserver.Responses.JSONResponse
    {
        public ShowDistance(string indexPage)
            : base(indexPage, new ShowDistanceResponse().ShowDistanceMethod)
        { }
    }


    class ShowDistanceResponse
    {

        public ShowDistanceResponse()
        {

        }
        public void ShowDistanceMethod(Request e, JsonObject results)
        {
            results.Add("no_vehicle", e.getLidarDistance().DistanceValue.ToString());
            results.Add("laser_status", e.getLaser().getStatus());
        }
    }
}
