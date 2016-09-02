using System;
using Microsoft.SPOT;
using System.Text;
using NeonMika.Webserver; 

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    public class FileUpload : NeonMika.Webserver.Responses.Response
    {
               /// <summary>
        /// Page on which indexPage should be displayed
        /// </summary>
        public FileUpload(string indexPage)
            : base(indexPage)
        { }

        /// <summary>
        /// Execute this to check if SendResponse should be executed
        /// </summary>
        /// <param name="e">The request that should be handled</param>
        /// <returns>True if URL refers to this method, otherwise false (false = SendRequest should not be exicuted) </returns>
        public override bool ConditionsCheckAndDataFill(Request e)
        {
            if (e.URL == "upload")
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
            string page = @"
<html>
	<header>
		<title>Upload Assembly File</title>
	</header>
	<body>
		<h2>Please fill in the file-upload form below</h2>
		<form method=""POST"" enctype=""multipart/form-data"" action=""upload"">File to upload: <input type=""file"" name=""upfile"">
		    <br>
		    <br>
		    <input type=""submit"" value=""Press""> to upload the file!
		</form>
	</body>
</html>
";

            Send200_OK("text/html", page.Length, e.Client);
            SendData(e.Client, Encoding.UTF8.GetBytes(page));
            page = null;
            return true;
        }
    }
}
