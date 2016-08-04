using System;
using System.Text;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;
using DistanceValue;



namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class JsonTest : NeonMika.Webserver.Responses.JSONResponse
    {
        public JsonTest(string indexPage)
            : base(indexPage, new testJsonResponse().testJsonRequestMethod)
        { }

        public void testJsonRequest(Request e, JsonObject results)
        {
            Debug.Print(e.GetArguments.ToString());
        }

        public void setDistanceValue(DistanceValue.DistanceValueClass dv)
        {

        }
    }

    class testJsonResponse
    {
        public testJsonResponse ()
        {
            Debug.Print("in constructor");
        }
        public void testJsonRequestMethod(Request e, JsonObject results)
        {
            Debug.Print(e.GetArguments.ToString());
            Hashtable ht = e.GetArguments;
            if (ht.Contains("text"))
            {
                string s = ht["text"].ToString();
                results.Add("text",s);
                results.Add("parama1", "one");
                results.Add("param2","two");
            }
        }
    }

}
