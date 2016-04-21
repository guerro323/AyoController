using System;
using System.Configuration;

namespace AyoController.Classes
{
	public class Config
	{

		public string ShootManiaIp = "";
		public int ShootManiaXmlRpcPort = 0;
		public int ShootManiaReconnectTimeout = 0;
		public string ShootManiaSuperAdminLogin = string.Empty;
		public string ShootManiaSuperAdminPassword = string.Empty;
	    public int XmlRpcType = 0;
		public bool AyoControllerDebug = false;

		public Boolean ParseFromConfigFile ()
		{

			ShootManiaIp = ConfigurationManager.AppSettings ["IP"];
		    XmlRpcType = int.Parse(ConfigurationManager.AppSettings["UseXMLRPC"]);

			if (ShootManiaIp == string.Empty) {
				Console.WriteLine ("Invalid ShootMania IP !");
				return false;
			}
				
			if (!int.TryParse(ConfigurationManager.AppSettings ["XMLRPC Port"], out ShootManiaXmlRpcPort)) {
				Console.WriteLine ("Invalid ShootMania XML-RPC Port !");
				return false;
			}

			if (ShootManiaXmlRpcPort == 0) {
				Console.WriteLine ("Invalid ShootMania XML-RPC Port !");
				return false;
			}

			if (!int.TryParse (ConfigurationManager.AppSettings ["Reconnect TimeOut"], out ShootManiaReconnectTimeout)) {
				Console.WriteLine ("Invalid ShootMania Reconnect Timeout !");
				return false;
			}

			if (ShootManiaReconnectTimeout == 0) {
				Console.WriteLine ("Invalid ShootMania Reconnect Timeout !");
				return false;
			}

			ShootManiaSuperAdminLogin = ConfigurationManager.AppSettings ["SuperAdmin Login"];

			if (ShootManiaSuperAdminLogin == string.Empty) {
				Console.WriteLine ("Invalid ShootMania SuperAdmin login !");
				return false;
			}

			ShootManiaSuperAdminPassword = ConfigurationManager.AppSettings ["SuperAdmin Password"];

			if (ShootManiaSuperAdminPassword == string.Empty) {
				Console.WriteLine ("Invalid ShootMania SuperAdmin password !");
				return false;
			}

			//int mAyoControllerDebug = ini.GetValue("AyoController", "Debug", 0);

			/*if (mAyoControllerDebug == 1)
                AyoControllerDebug = true;*/
		
			return true;

		}

	}
}

