using System;
using System.Text;
using System.IO;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    public class FilesList : NeonMika.Webserver.Responses.JSONResponse
    {
        private FilesListResponse _filesListResponse;
        public FilesList(string indexPage)
            : base(indexPage, (JSONResponseMethodObject)null)
        {
            _filesListResponse = new FilesListResponse();
            setResponseMethodObject(_filesListResponse.readFilesList);
        }
        /*
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
         * */
        /*
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
         * */

        class FilesListResponse
        {
            public FilesListResponse()
            {
                // constructor
            }
            public void readFilesList(Request e, JsonObject results)
            {
                string filePath = "\\SD";
                string file = "file";
                int i = 0;

                foreach (string f in Directory.GetFiles(filePath))
                {
                    results.Add(file + i, f);
                    i++;
                }
            }
        }
    }
}

