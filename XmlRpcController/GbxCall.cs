
//---------------------------------------------------//
//
//			Credits : Flo
//
//---------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections;
using System.Web;
using System.Net.Sockets;

namespace ShootManiaXMLRPC.XmlRpc
{
    public enum MessageTypes
    {
        None,
        Response,
        Request,
        Callback
    }

    public class GbxCall
    {
        private int _mHandle;
        private readonly string _mXml;
        private readonly ArrayList _mParams = new ArrayList();
        private readonly bool _mError = false;
        private readonly string _mErrorString;
        private readonly int _mErrorCode;
        private readonly string _mMethodName;
        private readonly MessageTypes _mType;

        /// <summary>
        /// Parses an incoming message.
        /// Xml to object.
        /// </summary>
        /// <param name="in_handle"></param>
        /// <param name="in_data"></param>
        public GbxCall(int inHandle, byte[] inData)
        {
            _mType = MessageTypes.None;
            _mHandle = inHandle;
            _mXml = Encoding.UTF8.GetString(inData);
            _mErrorCode = 0;
            _mErrorString = "";

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(_mXml);
            XmlElement methodParams = null;

            // message is of type request ...
            if (xmlDoc["methodCall"] != null)
            {
                // check message type ...
                if (inHandle > 0)
                    _mType = MessageTypes.Callback;
                else
                    _mType = MessageTypes.Request;

                // try to get the method name ...
                if (xmlDoc["methodCall"]["methodName"] != null)
                {
                    _mMethodName = xmlDoc["methodCall"]["methodName"].InnerText;
                }
                else
                    _mError = true;

                // try to get the mehtod's parameters ...
                if (xmlDoc["methodCall"]["params"] != null)
                {
                    _mError = false;
                    methodParams = xmlDoc["methodCall"]["params"];
                }
                else
                    _mError = true;
            }
            else if (xmlDoc["methodResponse"] != null) // message is of type response ...
            {
                // check message type ...
                _mType = MessageTypes.Response;

                if (xmlDoc["methodResponse"]["fault"] != null)
                {
                    Hashtable errStruct = (Hashtable)ParseXml(xmlDoc["methodResponse"]["fault"]);
                    _mErrorCode = (int)errStruct["faultCode"];
                    _mErrorString = (string)errStruct["faultString"];
                    _mError = true;
                }
                else if (xmlDoc["methodResponse"]["params"] != null)
                {
                    _mError = false;
                    methodParams = xmlDoc["methodResponse"]["params"];
                }
                else
                {
                    _mError = true;
                }
            }
            else
            {
                _mError = true;
            }

            // parse each parameter of the message, if there are any ...
            if (methodParams != null)
            {
                foreach (XmlElement param in methodParams)
                {
                    _mParams.Add(ParseXml(param));
                }
            }
        }

        /// <summary>
        /// Parses a response message.
        /// Object to xml.
        /// </summary>
        /// <param name="in_params"></param>
        public GbxCall(object[] inParams)
        {
            _mXml = "<?xml version=\"1.0\" ?>\n";
            _mXml += "<methodResponse>\n";
            _mXml += "<params>\n";
            foreach (object param in inParams)
            {
                _mXml += "<param>" + ParseObject(param) + "</param>\n";
            }
            _mXml += "</params>";
            _mXml += "</methodResponse>";
        }

        /// <summary>
        /// Parses a request message.
        /// Object to xml.
        /// </summary>
        /// <param name="in_method_name"></param>
        /// <param name="in_params"></param>
        public GbxCall(string inMethodName, object[] inParams)
        {
            _mXml = "<?xml version=\"1.0\" ?>\n";
            _mXml += "<methodCall>\n";
            _mXml += "<methodName>" + inMethodName + "</methodName>\n";
            _mXml += "<params>\n";
            foreach (object param in inParams)
            {
                _mXml += "<param>" + ParseObject(param) + "</param>\n";
            }
            _mXml += "</params>";
            _mXml += "</methodCall>";
        }

        private string ParseObject(object inParam)
        {
            // open parameter ...
            string xml = "<value>";

            if (inParam.GetType() == typeof(string)) // parse type string ...
            {
                xml += "<string>" + HttpUtility.HtmlEncode((string)inParam) + "</string>";
            }
            else if (inParam.GetType() == typeof(int)) // parse type int32 ...
            {
                xml += "<int>" + (int)inParam + "</int>";
            }
            else if (inParam.GetType() == typeof(double)) // parse type double ...
            {
                xml += "<double>" + (double)inParam + "</double>";
            }
            else if (inParam.GetType() == typeof(bool))  // parse type bool ...
            {
                if ((bool)inParam)
                    xml += "<boolean>1</boolean>";
                else
                    xml += "<boolean>0</boolean>";
            }
            else if (inParam.GetType() == typeof(ArrayList)) // parse type array ...
            {
                xml += "<array><data>";
                foreach (object element in ((ArrayList)inParam))
                {
                    xml += ParseObject(element);
                }
                xml += "</data></array>";
            }
            else if (inParam.GetType() == typeof(Hashtable)) // parse type struct ...
            {
                xml += "<struct>";
                foreach (object key in ((Hashtable)inParam).Keys)
                {
                    xml += "<member>";
                    xml += "<name>" + key.ToString() + "</name>";
                    xml += ParseObject(((Hashtable)inParam)[key]);
                    xml += "</member>";
                }
                xml += "</struct>";
            }
            else if (inParam.GetType() == typeof(byte[])) // parse type of byte[] into base64
            {
                xml += "<base64>";
                xml += Convert.ToBase64String((byte[])inParam);
                xml += "</base64>";
            }

            // close parameter ...
            return xml + "</value>\n";
        }

        private object ParseXml(XmlElement inParam)
        {
            XmlElement val;
            if (inParam["value"] == null)
            {
                val = inParam;
            }
            else
            {
                val = inParam["value"];
            }

            if (val["string"] != null) // param of type string ...
            {
                return val["string"].InnerText;
            }
            else if (val["int"] != null) // param of type int32 ...
            {
                return Int32.Parse(val["int"].InnerText);
            }
            else if (val["i4"] != null) // param of type int32 (alternative) ...
            {
                return Int32.Parse(val["i4"].InnerText);
            }
            else if (val["double"] != null) // param of type double ...
            {
                return double.Parse(val["double"].InnerText);
            }
            else if (val["boolean"] != null) // param of type boolean ...
            {
                if (val["boolean"].InnerText == "1")
                    return true;
                else
                    return false;
            }
            else if (val["struct"] != null) // param of type struct ...
            {
                Hashtable structure = new Hashtable();
                foreach (XmlElement member in val["struct"])
                {
                    // parse each member ...
                    structure.Add(member["name"].InnerText, ParseXml(member));
                }
                return structure;
            }
            else if (val["array"] != null) // param of type array ...
            {
                ArrayList array = new ArrayList();
                foreach (XmlElement data in val["array"]["data"])
                {
                    // parse each data field ...
                    array.Add(ParseXml(data));
                }
                return array;
            }
            else if (val["base64"] != null) // param of type base64 ...
            {
                byte[] data = Convert.FromBase64String(val["base64"].InnerText);
                return data;
            }

            return null;
        }

        public string MethodName
        {
            get
            {
                return _mMethodName;
            }
        }

        public MessageTypes Type
        {
            get
            {
                return _mType;
            }
        }

        public string Xml
        {
            get
            {
                return _mXml;
            }
        }

        public ArrayList Params
        {
            get
            {
                return _mParams;
            }
        }

        public int Size
        {
            get
            {
                return _mXml.Length;
            }
        }

        public int Handle
        {
            get
            {
                return _mHandle;
            }
            set
            {
                _mHandle = value;
            }
        }

        public bool Error
        {
            get
            {
                return _mError;
            }
        }

        public string ErrorString
        {
            get
            {
                return _mErrorString;
            }
        }

        public int ErrorCode
        {
            get
            {
                return _mErrorCode;
            }
        }
    }
}
