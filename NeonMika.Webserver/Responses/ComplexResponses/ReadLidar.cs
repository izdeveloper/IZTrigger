using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;
using DistanceValue;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class ReadLidar:NeonMika.Webserver.Responses.JSONResponse
    {
        private ReadLidarResponse _readLidarResponse;

        public ReadLidar(string indexPage)
            : base(indexPage, (JSONResponseMethodObject) null)

        {
            _readLidarResponse = new ReadLidarResponse();
            setResponseMethodObject(_readLidarResponse.readLidarRequestMethod);
        }

        public void testReadLidarRequest(Request e, JsonObject results)
        {
            Debug.Print(e.GetArguments.ToString());
        }

        public void setDistanceValue(DistanceValue.DistanceValueClass dv)
        {
            _readLidarResponse.setDistanceValue(dv);
        }
    }

    class ReadLidarResponse
    {
        private DistanceValue.DistanceValueClass _distanceValue;

        public ReadLidarResponse()
        {
            Debug.Print("ReadLidarResponse constructor");
        }

        public void setDistanceValue(DistanceValue.DistanceValueClass dv)
        {
            _distanceValue = dv;
        }
        public void readLidarRequestMethod(Request e, JsonObject results)
        {
            Debug.Print(e.GetArguments.ToString());
            Hashtable ht = e.GetArguments;
            if (ht.Contains("text"))
            {
                string s = ht["text"].ToString();
                results.Add("distance",_distanceValue.DistanceValue.ToString());
                results.Add("text", "Hello World!");
            }
        }
    }
}
