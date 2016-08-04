using System;
using Microsoft.SPOT;
using System.Text;
using NeonMika.Webserver;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    public class Abc : NeonMika.Webserver.Responses.Response
    {
        /// <summary>
        /// Page on which indexPage should be displayed
        /// </summary>
        public Abc(string indexPage)
            : base(indexPage)
        { }

        /// <summary>
        /// Execute this to check if SendResponse shoul be executed
        /// </summary>
        /// <param name="e">The request that should be handled</param>
        /// <returns>True if URL refers to this method, otherwise false (false = SendRequest should not be exicuted) </returns>
        public override bool ConditionsCheckAndDataFill(Request e)
        {
            if (e.URL == "abc")
                return true;
            else
                return false;
        }

        /// <summary>
        /// Sends infotext to client
        /// </summary>
        /// <param name="e">The request which should be handled</param>
        /// <returns>True if 200_OK was sent, otherwise false</returns>
        public override bool SendResponse(Request e)
        {
            Debug.Print(Debug.GC(true).ToString());
            string index = @"<!DOCTYPE html><html>
                                <script src=""https://ajax.googleapis.com/ajax/libs/jquery/2.2.0/jquery.min.js""></script>
                                <body>
                                    <button type=""button"" onclick=sendAjax()>Click Me!</button>
                                    <div id=""response""></div>
                                
                                <script>
                                      $(window).load(function(e) 
                                            {
                                                $(""#response"").text(""clear"");
	                                        }
                                        )
		                                function sendAjax()
		                                {
			                                var request = $.ajax({
			                                 url: ""readlidar"",
			                                type: ""GET"",
			                                data: {text : ""testtext""},
			                                success: function (event)
			                                         {
				                                        var e = event;
				                                        $(""#response"").text(event.distance);
			                                        },
			                                error: function(event)
			                                {
			                                var e = event;
			   
			                                }
                                          })
			                            };
		
	                               </script>
                                </body>
                            </html>";
            Send200_OK("text/html", index.Length, e.Client);
            SendData(e.Client, Encoding.UTF8.GetBytes(index));
            index = null;
            return true;
        }
    }
}
