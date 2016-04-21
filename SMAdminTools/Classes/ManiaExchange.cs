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
    public class Result
    {
        public List<MapInfo> results;
    }
    public partial struct MapInfo
    {
        public int TrackID;
        public int UserID;
        public string Username;
        public string Name;
        public int AwardCount;

        public string CdEnviro;
    }

    public partial class MapArgument
    {
        public string ArgEnviro = "tm";
        public string ArgAuthor = "";
        public int Id;
        public string UId;
    }

    public partial class ManiaExchangeAyoErrorCode
    {
        public bool Error;
        public int ErrorCode;
        public string ErrorString;
    }

    public enum MxapiType
    {
        Site = 1,
        Api = 2
    }

    public partial class ManiaExchange
    {
        public MapArgument CurrentSearch = new MapArgument();
        public string Request(MxapiType t, MapArgument param)
        {
            CurrentSearch = param;
            string url = "https://" + param.ArgEnviro + ".mania-exchange.com/tracksearch2/search?api=on&format=json&anyauthor=" + param.ArgAuthor + "";
            if (t == MxapiType.Api)
            {
                url = "https://api.mania-exchange.com/" + CurrentSearch.ArgEnviro + "/maps/" + CurrentSearch.UId;
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

        public MapInfo[] ToResults(string result)
        {
            Result tempResult = new Result();
            tempResult = JsonMapper.ToObject<Result>(result);
            List<MapInfo> mapResult = new List<MapInfo>();
            foreach (var res in tempResult.results)
            {
                mapResult.Add(res);
            }
            return mapResult.ToArray();
        }

        public MapInfo GetMapInformation(string enviro, string uId)
        {
            MapInfo tempMapInfo = new MapInfo();
            if (uId != "")
            {
                if (Request(MxapiType.Api, new MapArgument { UId = uId }) ==
                    "[]") return tempMapInfo;
                tempMapInfo = JsonMapper.ToObject<MapInfo[]>(Request(MxapiType.Api, new MapArgument { UId = uId }))[0];
            }
            return tempMapInfo;
        }

        /// <summary>
        /// Add a map to the server.
        /// WARNING! The environnement for the map will be selected using the last Request function :
        /// If you used SM as a request, then it will add a SM map and not TM map.
        /// </summary>
        /// <param name="mapId"></param>
        /// <returns></returns>
        public ManiaExchangeAyoErrorCode AddMap(int mapId)
        {
            ManiaExchangeAyoErrorCode error = new ManiaExchangeAyoErrorCode();
            // Create a request for the URL. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
              "https://" + CurrentSearch.ArgEnviro + ".mania-exchange.com/tracks/download/" + mapId);
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
            ServerManager.CreateNewFile("mx", mapId + ".Map.Gbx", responseFromServer, DoNothing);

            return error;
        }

        void DoNothing()
        {

        }
    }
}
