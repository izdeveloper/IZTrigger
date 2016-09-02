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

            System.IO.DirectoryInfo di = new DirectoryInfo(filePath);

            foreach (FileInfo file in di.GetFiles())
            {                
                foreach(string item in reqOnDelete.Values)
                {                    
                    if(file.Name.Equals(item))               
                    {
                        Debug.Print("Deleting file: " + file.FullName.ToString());
                        file.Delete();
                    }
                }
            }

        }
    }
}
