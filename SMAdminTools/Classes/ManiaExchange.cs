using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using LitJson;

namespace AyoController.Classes
{
    public class result
    {
        public List<MapInfo> results;
    }
    public partial class MapInfo
    {
        public int TrackID;
        public int UserID;
        public string Username;
        public string Name;
        public int AwardCount;

        public string cdEnviro = "tm";
    }

    public partial class MapArgument
    {
        public string argEnviro = "tm";
        public string argAuthor = "";
        public int ID;
        public string uID;
    }

    public partial class ManiaExchangeAyoErrorCode
    {
        public bool Error;
        public int ErrorCode;
        public string ErrorString;
    }

    public enum MXAPIType
    {
        Site = 1,
        Api = 2
    }

    public partial class ManiaExchange
    {
        public MapArgument CurrentSearch = new MapArgument();
        public string Request(MXAPIType _T, MapArgument Param)
        {
            CurrentSearch = Param;
            string url = "https://" + Param.argEnviro + ".mania-exchange.com/tracksearch2/search?api=on&format=json&anyauthor=" + Param.argAuthor + "";
            if (_T == MXAPIType.Api)
            {
                url = "https://api.mania-exchange.com/" + CurrentSearch.argEnviro + "/maps/" + CurrentSearch.uID;
            }
            // Create a request for the URL. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            request.UserAgent = "Mozilla/5.0";
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            return responseFromServer;
        }

        public MapInfo[] ToResults(string _result)
        {
            result TempResult = new result();
            TempResult = JsonMapper.ToObject<result>(_result);
            List<MapInfo> MapResult = new List<MapInfo>();
            foreach (var res in TempResult.results)
            {
                MapResult.Add(res);
            }
            return MapResult.ToArray();
        }

        public MapInfo GetMapInformation(string _enviro, string _uID)
        {
            MapInfo TempMapInfo = new MapInfo();
            if (_uID != "")
            {
                if (Request(MXAPIType.Api, new MapArgument { uID = _uID }) ==
                    "[]") return TempMapInfo;
                TempMapInfo = JsonMapper.ToObject<MapInfo[]>(Request(MXAPIType.Api, new MapArgument { uID = _uID }))[0];
            }
            return TempMapInfo;
        }

        /// <summary>
        /// Add a map to the server.
        /// WARNING! The environnement for the map will be selected using the last Request function :
        /// If you used SM as a request, then it will add a SM map and not TM map.
        /// </summary>
        /// <param name="MapID"></param>
        /// <returns></returns>
        public ManiaExchangeAyoErrorCode AddMap(int MapID)
        {
            ManiaExchangeAyoErrorCode Error = new ManiaExchangeAyoErrorCode();
            // Create a request for the URL. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
              "https://" + CurrentSearch.argEnviro + ".mania-exchange.com/tracks/download/" + MapID);
            // If required by the server, set the credentials.
            request.UserAgent = "Mozilla/5.0";
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream, Encoding.Default);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Clean up the streams and the response.
            reader.Close();
            response.Close();
            /* ------------------- */
            /// Create the map
            ServerManager.CreateNewFile("mx", MapID + ".Map.Gbx", responseFromServer, DoNothing);

            return Error;
        }

        void DoNothing()
        {

        }
    }
}
