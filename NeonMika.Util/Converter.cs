using System;
using Microsoft.SPOT;
using System.Collections;
using System.IO;

namespace NeonMika.Util
{
    public class Converter : IDisposable
    {
        public Hashtable ToHashtable(string[] lines, string seperator, int startAtLine = 0)
        {
            Hashtable toReturn = new Hashtable( );
            string[] line;
            for ( int i = startAtLine; i < lines.Length; i++ )
            {
                using (ExtensionMethods em = new ExtensionMethods())
                {
                    line = em.EasySplit(lines[i], seperator);
                    Debug.Print("line[0]->" + line[0]);
                    if (line.Length > 1)
                    {
                        toReturn.Add(line[0], line[1]);
                        Debug.Print("line[1]->" + line[1]);
                    }
                }

            }
            return toReturn;
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }
}
