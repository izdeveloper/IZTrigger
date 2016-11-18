using System;
using Microsoft.SPOT;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using NeonMika.Util;
using NeonMika.Webserver.Responses;
using System.Collections;

namespace NeonMika.Webserver.POST
{
    /// <summary>
    /// Saves a POST-request at Setting.POST_TEMP_PATH
    /// Sends back "OK" on success
    /// </summary>
    class PostToSdWriter : IDisposable
    {
        private byte[] _buffer;
        private Request _e;

        public PostToSdWriter(Request e)
        {
            _e = e;
        }

        /// <summary>
        /// Saves content to Setting.POST_TEMP_PATH
        /// </summary>
        /// <param name="e">The request which should be handeld</param>
        /// <returns>True if 200_OK was sent, otherwise false</returns>
        public bool ReceiveAndSaveData(byte [] buffer_in, int hdr_length)
        {
            Debug.Print(Debug.GC(true).ToString());
            Debug.Print(Debug.GC(true).ToString());

            // DEBUG
            int data_position = buffer_in.Length - hdr_length;
            byte[] content_start_buff = new byte[0];
            Debug.Print("->" + new String(System.Text.Encoding.UTF8.GetChars(buffer_in)) + "<--");

            // trim zeros from buffer
            // populate foo
            int k = buffer_in.Length - 1;
            while (buffer_in[k] == 0)
                --k;
            // now buffer[i] is the last non-zero byte
            byte[] buffer = new byte[k + 1];
            Array.Copy(buffer_in, buffer, k + 1);

            if (buffer.Length > hdr_length) // copy the beginning of the content from previuos buffer
            {
                content_start_buff = new byte[buffer.Length - hdr_length];
                for (int i = 0; i < buffer.Length - hdr_length; i++)
                    content_start_buff[i] = buffer[hdr_length + i];
            }
 
            String strLine = "";
       //     if (data_position > 1)
       //     {
       //         strLine = new String(System.Text.Encoding.UTF8.GetChars(buffer, data_position, buffer.Length));
       //        Debug.Print("-->"+strLine);
       //     }

            Hashtable hdrs = _e.Headers;
            int content_bytes_length = 0;
            int content_bytes_toread = 0;
            if (hdrs.Contains("Content-Length"))
            {
                content_bytes_length = Convert.ToInt32(_e.Headers["Content-Length"].ToString().TrimEnd('\r'));
                content_bytes_toread = content_bytes_length;
            }
            else
            {
                Debug.Print("ERROR-> Content-Length header is not found");
                return false; // ERROR
            }

            String boundary = "";
            if (hdrs.Contains("Content-Type"))
            {
                string[] sub_hdr = _e.Headers["Content-Type"].ToString().Split('=');
                boundary = sub_hdr[1];
                Debug.Print("Boundary  = " + boundary);
            }
            else
            {
                Debug.Print("ERROR-> Content-Type header is not found");
                return false; // ERROR
            }

            //convert a boundary string into byte array , "--" is added to pre and post of the boundary 
            byte[] byte_boundary = System.Text.Encoding.UTF8.GetBytes("--"+boundary+"--");


            
            int newAvBytes = 0;
            int double_cr = 0;
            byte[] _buffer = new byte[0];
            Hashtable file_hdr;
            int content_start_buff_length = content_start_buff.Length;
            String file_name = "";

            // find boundary and POST file name 
            do
            {
                Thread.Sleep(15);

                for (int r = 0; r < 3; r++)
                {
                    newAvBytes = _e.Client.Available;
                    if (newAvBytes > 0)
                    {
                        // TODO if the newAvbytes is > than Settings.MAX_REQUESTSIZE case
                        int to_read = newAvBytes > Settings.MAX_REQUESTSIZE ? Settings.MAX_REQUESTSIZE : newAvBytes;
                        _buffer = new byte[to_read];
                        _e.Client.Receive(_buffer, _buffer.Length, SocketFlags.None);
                    }
                }

                if (content_start_buff_length > 0)
                {
                    Debug.Print("content_start_buff_length: " + content_start_buff_length);
                    _buffer = Combine(content_start_buff, _buffer);
                    content_start_buff_length = 0;
                }

                //Debug.Print("->" + new String(System.Text.Encoding.UTF8.GetChars(_buffer)) + "<--");

                double_cr = FindDoubleCR(_buffer); //returns position number of the first byte after double cr
                Debug.Print("->" + new String(System.Text.Encoding.UTF8.GetChars(_buffer, 0, double_cr)) + "<--");

                if (double_cr != 0)
                {
                    strLine = strLine + new String(System.Text.Encoding.UTF8.GetChars(_buffer, 0, double_cr));
                   // Debug.Print("->"+strLine);
                    string[] lines = strLine.Split('\n');
                    using (NeonMika.Util.Converter c = new NeonMika.Util.Converter())
                    {
                        file_hdr = c.ToHashtable(lines, ": ", 0);
                    }

                    
                    if (file_hdr.Contains("Content-Disposition"))
                    {
                        String[] ct = file_hdr["Content-Disposition"].ToString().Split(';');
                        using (NeonMika.Util.Converter c = new NeonMika.Util.Converter())
                        {
                            Hashtable boundary_hdr = c.ToHashtable(ct, "=");
                            if (boundary_hdr.Contains("filename"))
                            {
                                file_name = boundary_hdr["filename"].ToString().Trim('"');
                            }
                            else
                                file_name = ""; // error
                        }
                    }
                    else
                        file_name = ""; // error
                    if (file_name == "")
                    {
                        Debug.Print("Missing file name in the header");
                        return false;
                    }
                    else
                        Debug.Print("file name: " + file_name + " <--");

                    break;
                }

            } while (true);

            Debug.Print("double_cr: " + double_cr + " buffer size:" + _buffer.Length);


            // Open the file system and start writing the file 
            try
            {
                Debug.Print("Writing file: " + Settings.FIRMWARE_PATH + file_name);
                FileStream fs = new FileStream(Settings.FIRMWARE_PATH + file_name, FileMode.Create, FileAccess.Write);
                Debug.GC(true).ToString();
                Debug.GC(true).ToString();

                // write the beginning of the file
                int total_bytes = 0;
                // check if we have read the file already
                int boundary_position = checkEndOfContent(_buffer, byte_boundary);
                if (boundary_position > 0)
                    content_bytes_toread = 0; // we found end of file , nothing to read more 
                else
                    content_bytes_toread -= double_cr; // double_cr points to the last cr, the content start after that

                // check if we have read the file already
                
                total_bytes += _buffer.Length - double_cr;

               //  Debug.Print("content_bytes_toread: " + (content_bytes_toread - byte_boundary.Length - 2));
                Debug.Print("content_bytes_toread: " + (content_bytes_toread ));
                // write the first buffer
                if (boundary_position != 0)
                    fs.Write(_buffer, double_cr, _buffer.Length - double_cr - byte_boundary.Length - 2);
                else
                    fs.Write(_buffer, double_cr, _buffer.Length - double_cr);

                // String str_d = new String(System.Text.Encoding.UTF8.GetChars(_buffer, double_cr, _buffer.Length - double_cr));
                // Debug.Print("===>" + str_d);

                while (content_bytes_toread > 0)
                {
                    // check if we have end boundary in the buffer already (very small files
                    boundary_position = checkEndOfContent(_buffer, byte_boundary);
                    if (boundary_position != 0) // we have fully read the file
                    {
                        Debug.Print("Found boundary position: " + boundary_position);
                        Debug.Print("total_bytes: " + (total_bytes - 2 - byte_boundary.Length - 2));
                        // write the last block of data
                        fs.Write(_buffer, 0, boundary_position);
                        break;
                    }

                    Thread.Sleep(15);
                    int nb = _e.Client.Available;
                    if (nb == 0)
                    {
                        Debug.Print("0 Available");
                        continue;
                    }
                    int to_read = nb > Settings.MAX_REQUESTSIZE ? Settings.MAX_REQUESTSIZE : nb;

                    Debug.Print("to read: " + to_read);

                    _buffer = new byte[to_read];

                    Debug.Print("_buffer.length: " + _buffer.Length);

                    total_bytes += to_read;

                    _e.Client.Receive(_buffer, _buffer.Length, SocketFlags.None);

                    // String str = new String(System.Text.Encoding.UTF8.GetChars(_buffer, 0, _buffer.Length));

                    // Debug.Print(str);

                    content_bytes_toread -= _buffer.Length;

                    boundary_position = checkEndOfContent(_buffer, byte_boundary);
                    if (boundary_position != 0)
                    {
                        Debug.Print("Found boundary position: " + boundary_position);
                        Debug.Print("total_bytes: " + (total_bytes - 2 - byte_boundary.Length - 2));
                        // write the last block of data
                        fs.Write(_buffer, 0, boundary_position);

                        break;
                    }
                    // write buffer to disk
                    fs.Write(_buffer, 0, _buffer.Length);

                    Debug.Print("content_bytes_toread left: " + content_bytes_toread);
                }
                fs.Flush();
                fs.Close();

                Debug.Print(File.Exists(Settings.FIRMWARE_PATH + file_name) ? "File exists:" + Settings.FIRMWARE_PATH+file_name : "File does not exist.");
                
               // testReadFile(Settings.FIRMWARE_PATH + file_name);
            }
            catch (Exception ex)
            {
                Debug.Print("Error writing POST-data: "+ex.ToString());
                return false;
            }
            return true;
        }

        private int FindDoubleCR(byte [] buffer)
        {
            int doublecr = 0;

            for (int count = 0; count < buffer.Length - 3; count++)
            {
                if (buffer[count] == '\r' && buffer[count + 1] == '\n' && buffer[count + 2] == '\r' && buffer[count + 3] == '\n')
                {
                    doublecr = count + 4;               
                    break;
                }
            }
            return doublecr;
        }

        private int checkEndOfContent(byte [] buffer, byte [] boundary)
        {
            int cr_start = 0;
            byte [] single_cr = new byte[] {(byte) '\r', (byte) '\n'};

            if (buffer[buffer.Length-2] == '\r' && buffer[buffer.Length-1] == '\n' )
            {
                cr_start = buffer.Length - 2;
            }
            
            Debug.Print("checking for dbl cr: " + cr_start);
            if ( cr_start == 0)
                return 0; // not the end of content 
            if (cr_start != buffer.Length - 2)
                return 0; // not the end of the content
           
            // if we got here , we have a cr at the end of the buffer 
            // we have to check if if there is a boundary message that would indicate the end of the content 
            byte[] _may_be_boundary = new byte[boundary.Length];
             
            for (int i = 0; i < boundary.Length; i++)
            {
                _may_be_boundary[i] = buffer[buffer.Length - boundary.Length - 2 + i];
            }

            String str = new String(System.Text.Encoding.UTF8.GetChars(_may_be_boundary, 0, _may_be_boundary.Length));

            if (ByteArrayCompare(_may_be_boundary, boundary))
                return buffer.Length - 2 - boundary.Length - 2; // return the end of the content (2 /r/n in front of the boundary, boiundary length and /r/n after the boundary 
            else
                return 0;
        }

        private bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }

        public void testReadFile(String filename)
        {
            using (FileStream inputStream = new FileStream(filename, FileMode.Open))
            {

                byte[] readBuffer = new byte[Settings.FILE_BUFFERSIZE];
                int sentBytes = 0;

                while (sentBytes < inputStream.Length)
                {
                    int bytesRead = inputStream.Read(readBuffer, 0, readBuffer.Length);
                    Debug.Print("Read: "+ bytesRead);
                    sentBytes += bytesRead;
                    if (bytesRead > 0)
                    {
                        String str = new String(System.Text.Encoding.UTF8.GetChars(readBuffer, 0, bytesRead));
                        Debug.Print(str);
                    }
                }
            }

        }

        public byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            for (int i = 0; i < first.Length; i++)
                ret[i] = first[i];
            for (int i = first.Length, j = 0; i < ret.Length; i++, j++)
                ret[i] = second[j];
            return ret;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _buffer = new byte[0];            
        }

        #endregion
    }
}
