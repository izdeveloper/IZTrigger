using System;
using Microsoft.SPOT;

namespace NeonMika.Util
{
    public class ExtensionMethods :IDisposable
    {
        public string[] EasySplit(string s, string seperator)
        {
            int pos = s.IndexOf(seperator);
            if (pos != -1)
                return new string[] { s.Substring(0, pos).Trim(new char[] { ' ', '\n', '\r' }), s.Substring(pos + seperator.Length, s.Length - pos - seperator.Length).Trim(new char[] { ' ', '\n', '\r' }) };
            else
                return new string[] { s.Trim(new char[] { ' ', '\n', '\r' }) };
        }

        public  bool StartsWith(string s, string start)
        {
            for (int i = 0; i < start.Length; i++)
                if (s[i] != start[i])
                    return false;

            return true;
        }

        public  string Replace(string s, char replaceThis, char replaceWith)
        {
            string temp = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == replaceThis)
                    temp += replaceWith;
                else
                    temp += s[i];
            }
            return temp;
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }
}
