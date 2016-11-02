using System;
using System.Collections;
using Microsoft.SPOT;

namespace XMLParser
{
    public class XmlParser
    {
        private string mstrInput;
        private string mstrName   ;
        private int mintCurrentPosition;
        private System.Collections.Stack mobjStack = new System.Collections.Stack();
        public XmlParser(string Input)
        {
            mstrInput = Input;
            mintCurrentPosition = 0;
        }
        public bool Read()
        {
            // find the next <element> 			
            int intElementSpace = 0;
            int intElementNameEnd = 0;
            bool blnElementFound = false;
            while (!(blnElementFound || mintCurrentPosition >= mstrInput.Length))
            {
                switch (mstrInput.Substring(mintCurrentPosition, 1))
                {
                    case "<":						// element start (or end)						
                        if (mstrInput.Substring(mintCurrentPosition, 2) == "</")
                        {
                            // element end							
                            mstrName = mobjStack.Peek().ToString();
                            if (mstrInput.Substring(mintCurrentPosition, mstrName.Length + 3) == "</" + mstrName + ">")
                            {
                                // long-form element close, pop the current stack item								
                                mobjStack.Pop();
                            }
                            mintCurrentPosition = mintCurrentPosition + (mstrName.Length + 3);
                        }
                        else
                        {
                            // element start							
                            if (mstrInput.Substring(mintCurrentPosition, 2) == "<?")
                            {
                                // xml or other declaration, skip								
                                intElementNameEnd = mstrInput.Substring(mintCurrentPosition).IndexOf(">");
                                mintCurrentPosition = mintCurrentPosition + intElementNameEnd;
                            }
                            else
                            {
                                intElementNameEnd = mstrInput.Substring(mintCurrentPosition).IndexOf(">");
                                intElementSpace = mstrInput.Substring(mintCurrentPosition).IndexOf(" ");
                                if (intElementSpace > 0 && intElementSpace < intElementNameEnd)
                                {
                                    mstrName = mstrInput.Substring(mintCurrentPosition + 1, intElementSpace - 1);
                                    mintCurrentPosition = mintCurrentPosition + intElementSpace + 1;
                                }
                                else
                                {
                                    mstrName = mstrInput.Substring(mintCurrentPosition + 1, intElementNameEnd - 1);
                                    mintCurrentPosition = mintCurrentPosition + intElementNameEnd + 1;
                                }
                                mobjStack.Push(mstrName);
                                blnElementFound = true;
                            }
                        }
                        break;
                    case "/":						// short-form element close						
                        if (mstrInput.Substring(mintCurrentPosition, 2) == "/>")
                        {
                            //  pop the current stack item							
                            mobjStack.Pop();
                            mstrName = mobjStack.Peek().ToString();
                            mintCurrentPosition = mintCurrentPosition + 2;
                        }
                        else
                        {
                            mintCurrentPosition = mintCurrentPosition + 1;
                        }
                        break;
                    default:
                        mintCurrentPosition = mintCurrentPosition + 1;
                        break;
                }
            }
            return !(mobjStack.Count == 0) & !(mintCurrentPosition >= mstrInput.Length);
        }

        public string Name
        {
            get { return mstrName; }
        }

        public string GetAttribute(string Name)
        {
            string strDataLine = null;
            int intElementEnd = 0;
            string[] strAttributes = null;
            string[] strSplit = null;
            string strName = null;
            string strValue = null;
            intElementEnd = mstrInput.Substring(mintCurrentPosition).IndexOfAny(new char[] { '>', '<', '/' });
            strDataLine = mstrInput.Substring(mintCurrentPosition, intElementEnd);
            strAttributes = strDataLine.Split(' ');
            foreach (string strAttribute in strAttributes)
            {
                if (strAttribute != string.Empty)
                {
                    strSplit = strAttribute.Split('=');
                    if (strSplit.Length == 2)
                    {
                        strName = strSplit[0];
                        strValue = strSplit[1];
                        strValue.Trim('"');
                        if (strName == Name)
                        {
                            return strValue;
                        }
                    }
                }
            }
            return "";
        }

        public System.Collections.DictionaryEntry[] GetAttributes()
        {
            string strDataLine = null;
            int intElementEnd = 0;
            string[] strAttributes = null;
            string[] strSplit = null;
            string strName = null;
            string strValue = null;
            System.Collections.ArrayList objAttributes = new System.Collections.ArrayList();
            intElementEnd = mstrInput.Substring(mintCurrentPosition).IndexOfAny(new char[] { '>', '<', '/' });
            strDataLine = mstrInput.Substring(mintCurrentPosition, intElementEnd);
            strAttributes = strDataLine.Split(' ');
            foreach (string strAttribute in strAttributes)
            {
                if (strAttribute != string.Empty)
                {
                    strSplit = strAttribute.Split('=');
                    if (strSplit.Length == 2)
                    {
                        strName = strSplit[0];
                        strValue = strSplit[1];
                        // remove quotes						
                        strValue.Trim('"');
                        objAttributes.Add(new System.Collections.DictionaryEntry(strName, strValue));
                    }
                }
            }
            return (System.Collections.DictionaryEntry[])objAttributes.ToArray(typeof(System.Collections.DictionaryEntry));
        }
    }
}
