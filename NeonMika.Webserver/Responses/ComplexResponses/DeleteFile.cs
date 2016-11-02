using System;
using System.Text;
using System.IO;
using System.Collections;
using Microsoft.SPOT;
using NeonMika.Webserver;
using FastloadMedia.NETMF.Http;

namespace NeonMika.Webserver.Responses.ComplexResponses
{
    class DeleteFile: NeonMika.Webserver.Responses.JSONResponse
    {
        private DeleteFileResponse _deleteFileResponse;

        public DeleteFile(string indexPage): base(indexPage, (JSONResponseMethodObject) null)
        {
            _deleteFileResponse = new DeleteFileResponse();
            setResponseMethodObject(_deleteFileResponse.deleteFile);
        }
    }

    class DeleteFileResponse
    {
        public void deleteFile(Request e, JsonObject results)
        {
            string filePath = "\\SD";
            Hashtable reqOnDelete = e.GetArguments;

            string filename = (string) reqOnDelete["param"];



            if (filename != null)
            {
                string fullname = filePath+"\\"+filename;
                if (File.Exists(fullname))
                {
                    Debug.Print("Deleting file: " + fullname);
                    File.Delete(fullname);
                }
            }
        }
    }
}
