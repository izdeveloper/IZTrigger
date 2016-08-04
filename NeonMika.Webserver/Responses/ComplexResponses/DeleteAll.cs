using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using Microsoft.SPOT;
using System.Collections;
using Microsoft.SPOT.Net.NetworkInformation;
using NeonMika.Util;

namespace NeonMika.Webserver.Responses
{
    /// <summary>
    /// Standard response sending file to client
    /// If filename is a directory, a directory-overview will be displayed
    /// </summary>
    public class DeleteAll : NeonMika.Webserver.Responses.Response
    {
        public DeleteAll(String name = "DeleteAll")
            : base(name)
        { }

        /// <summary>
        /// Delete all files has no conditions
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool ConditionsCheckAndDataFill(Request e)
        {
            string filePath = "/";
            bool isDirectory = false;
            if (CheckFileDirectoryExist(ref filePath, out isDirectory) == true)
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Depending on the requested path, a file, a directory-overview or a 404-error will be returned
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool SendResponse(Request e)
        {
            // Delete all tyhe files and return diurectory listing
            string directory_path = "\\SD";
            DeleteAllFiles(ref directory_path);

            string filePath = "\\SD";
            
            ArrayList toReturn = new ArrayList();
            string send;
            var interf = NetworkInterface.GetAllNetworkInterfaces()[0];

            Send200_OK(MimeType(".html"), 0, e.Client);

            string uppath = ((filePath.LastIndexOf("\\") >= 0) ? interf.IPAddress + ((filePath[0] != '\\') ? "\\" : "") + filePath.Substring(0, filePath.LastIndexOf("\\")) : interf.IPAddress + ((filePath[0] != '\\') ? "\\" : "") + filePath);

            send = "<html><head><title>" + e.URL + "</title>" +
                    "<style type=\"text/css\">a.a1{background-color:#ADD8E6;margin:0;padding:0;font-weight:bold;}a.a2{background-color:#87CEEB;margin:0;padding:0;font-weight:bold;}</style>" +
                    "</head><body><a href=\"http:\\\\" + uppath + "\">One level up</a><br/>" +
                    "<h1>" + e.URL + "</h1><h2>Directories:</h2>";
            if (SendData(e.Client, Encoding.UTF8.GetBytes(send)) == 0)
                    return false;

            foreach (string d in Directory.GetDirectories(filePath))
            {
                    send = "<a href=\"http:\\\\" + interf.IPAddress + d + "\" class=\"a1\">" + d + "</a><br/>";
                    if (SendData(e.Client, Encoding.UTF8.GetBytes(send)) == 0)
                        return false;
            }

            SendData(e.Client, Encoding.UTF8.GetBytes("<h2>Files:</h2>"));

            foreach (string f in Directory.GetFiles(filePath))
            {
                    send = "<a href=\"http:\\\\" + interf.IPAddress + f + "\" class=\"a2\">" + f + "</a><br/>";
                    if (SendData(e.Client, Encoding.UTF8.GetBytes(send)) == 0)
                        return false;
            }

            send = "</body></html>";
            if (SendData(e.Client, Encoding.UTF8.GetBytes(send)) == 0)
                    return false;
            return true;
        }

        private bool DeleteAllFiles(ref string filePath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(filePath);

            foreach (FileInfo file in di.GetFiles())
            {
                Debug.Print("Deleting file: " + file.FullName.ToString());
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                Debug.Print("Deleting Directory: " + dir.FullName.ToString());
                dir.Delete(true);
            }

            return true;
        }


        private static bool CheckFileDirectoryExist(ref string filePath, out bool isDirectory)
        {
            isDirectory = false;

            using (NeonMika.Util.ExtensionMethods em = new NeonMika.Util.ExtensionMethods())
            {
                filePath = em.Replace(filePath, '/', '\\');
            }

            //File found check
            try
            {
                if (filePath == "")
                    return false;

                if (!File.Exists(filePath))
                    if (!Directory.Exists(filePath))
                    {
                        isDirectory = false;
                        return false;
                    }
                    else
                        isDirectory = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print("Error accessing file/directory");
                Debug.Print(ex.ToString());
                return false;
            }
        }
    }
}

