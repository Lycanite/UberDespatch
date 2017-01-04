using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;

namespace UberDespatch
{
	public class Alert
	{
		// Tags List:
		public static Dictionary<string, Alert> Alerts = new Dictionary<string, Alert> ();

		public string Name;
		public string Title;
		public string Message;

		// ========== Constructor ==========
		public Alert()
		{
		}


		// ========== Alert JSON ==========
		public class AlertJSON
		{
			public string name;
			public string title;
			public string message;
		}


		// ========== Load Alerts ==========
		/** Loads all alerts from JSON data. This is normally called by Rule.LoadRules. **/
		public static void LoadAlerts()
		{
			Program.Log("Rules", "Loading alert messages...");
			Alerts = new Dictionary<string, Alert>();

			// Get JSON:
			string jsonData = "";
			string alertsPath = Program.configGlobal.localAlertsPath + Program.configGlobal.localAlertsFilename;
			string alertsURL = Program.configGlobal.remoteAlertsPath;
			bool alertsLoaded = false;

			if (alertsURL != "")
			{
				WebClient web = new WebClient();
				try
				{
					Stream stream = web.OpenRead(alertsURL);
					using (StreamReader reader = new StreamReader(stream))
					{
						jsonData = reader.ReadToEnd();
					}
					alertsLoaded = true;
				}
				catch (Exception e)
				{
					Program.LogWarning("Rules", "Unable to connect to " + alertsURL);
					Program.LogException(e);
					Program.LogAlert("Rules", "Searching for local alert messages instead...");
				}
			}
			else
				Program.Log("Rules", "No remote alert messages url set, reading from local file...");

			if (!alertsLoaded)
			{
				if (File.Exists(alertsPath))
				{
					jsonData = File.ReadAllText(alertsPath);
					alertsLoaded = true;
				}
				else {
					Program.LogWarning("Rules", "Warning: No remote alert messages url set and no local file found at " + alertsPath + ".");
					Program.LogError("Rules", "No alert messages loaded.");
					return;
				}
			}

			// Read From JSON:
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			try
			{
				AlertJSON[] alertJSONs = serializer.Deserialize<AlertJSON[]>(jsonData);
				foreach (AlertJSON alertJSON in alertJSONs)
				{
					Alert alert = new Alert();
					alert.Name = alertJSON.name;
					alert.Title = alertJSON.title;
					alert.Message = alertJSON.message;

					Alerts.Add(alert.Name, alert);
				}
			}
			catch (Exception e)
			{
				Program.LogError("Rules", "Invalid alert messages JSON format:");
				Program.LogException(e);
				Program.LogError("Rules", "No alert messages loaded.");
				return;
			}

			Program.LogSuccess("Rules", Alerts.Count + " alert message(s) loaded.");
		}


		// ========== Get Alert ==========
		/** Gets an alert by name or order tag. **/
		public static Alert GetAlert(string alertName = "")
		{
			if (alertName == "")
				return null;
			if (Alerts.ContainsKey(alertName))
				return Alerts[alertName];
			return null;
		}
	}
}
