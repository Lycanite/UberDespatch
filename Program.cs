using System;
using Gtk;
using System.Threading;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace UberDespatch
{
	class Program
	{
		public static string version = "1.12.7";
		public static string versionName = "Transcendent Ginkgo";
		public static ConfigGlobal configGlobal;
		public static string Language = "GB"; // TODO: Localise UberDespatch in the future.

		public static MainWindow mainWindow;
		public static OrderChecker orderChecker;
		public static WMS wms;
		public static Printer printer;
		public static HTMLConverter htmlConverter;
		public static bool shutdown = false;
		public static Translator Translator;

		//public static Thread animationThread;
		public static Thread mainThread;
		public static Thread apiThread;

		public static String _ExecutableFolder = "";
		public static String ExecutableFolder {
			get {
				if (_ExecutableFolder == "")
					_ExecutableFolder = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
				return _ExecutableFolder;
			}
		}

		public static string mainState;

		public static string LogToken = "";
		public static string ErrorWebURL = "";


		// ========== Global Config ==========
		public class ConfigGlobal : ConfigBase<ConfigGlobal> {
			public string downloadsPath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Orders";
			public string archivePath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Archive";
			public string pluginsPath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Plugins";

			public string localRulesPath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar;
			public string localRulesFilename = "rules.json";
			public string remoteRulesPath = "";

			public string localZonesPath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar;
			public string localZonesFilename = "zones.json";
			public string remoteZonesPath = "";

			public string localAlertsPath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar;
			public string localAlertsFilename = "alerts.json";
			public string remoteAlertsPath = "";

			public string localLabelsPath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar;
			public string localLabelsFilename = "labels.json";
			public string remoteLabelsPath = "";
			public string localLabelsTemplatePath = ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Labels" + System.IO.Path.DirectorySeparatorChar;

			public string logURL = "";
			public string logUsername = "";
			public string logPassword = "";

			public string despatchURL = "";

			public bool ignoreReferences = false;

			public static ConfigGlobal LoadFile() {
				return Load ();
			}

			public void SaveFile() {
				Save ();
			}
		}


		// ========== Web Log JSON ==========
		public class WebLogJSON {
			public string AccessToken;
			public string Username;
			public string Password;
			public string Version;
			public string LogType;
			public string LogCategory;
			public string LogMessage;
			public string OrderNumber;
			public string Object;
		}


		// ========== Main ==========
		public static void Main (string[] args)
		{
			// Main:
			Application.Init ();

			// Load Config:
			configGlobal = ConfigGlobal.LoadFile ();

			// Classes:
			printer = new Printer ();
			Translator = new Translator ();
			htmlConverter = new HTMLConverter ();
			orderChecker = new OrderChecker ();
			wms = WMS.GetDefaultWMS ();
			mainWindow = new MainWindow ();
			mainWindow.Title = "UberDespatch " + version + " " + versionName;
			mainWindow.Show ();

			// Start Threads:
			//animationThread = new Thread (new ThreadStart(AnimationLoop));
			//animationThread.Start ();
			mainThread = new Thread (new ThreadStart(Startup));
			mainThread.Start ();
			apiThread = new Thread (new ThreadStart(wms.Startup));
			apiThread.Start ();

			// Run:
			Application.Run ();

			shutdown = true;
		}


		// ========== Startup ==========
		public static void Startup() {
			Program.Log ("Main", "UberDespatch is firing up...! OS: " + System.Environment.OSVersion.ToString());
			Carrier.LoadCarriers ();
			Rule.LoadRules ();
			Program.LogSuccess ("Main", "UberDespatch is loaded up and ready to rock!");
			UpdateState ("idle");

			try {
				MainLoop();
			}
			catch (Exception e) {
				LogError ("Main", "A fatal error occured. UberDespatch will need to be restarted.");
				LogException (e);
			}
		}


		// ========== Reload ==========
		public static void Reload() {
			Log ("Main", "Reloading...");
			orderChecker.SkipOrder ();
			if (orderChecker.autoCheck)
				mainWindow.SetAutoCheckButton(orderChecker.ToggleAutoCheck ());
			UpdateState ("reload");
			try {
				Carrier.ReloadCarriers ();
				Rule.LoadRules ();
			}
			catch (Exception e) {
				LogError ("Main", "An error occured when reloading.");
				LogException (e);
			}
			Log ("Main", "Reloaded.");
			UpdateState ("idle");
		}


		// ========== Main Loop ==========
		public static void MainLoop() {
			if (!orderChecker.orderScheduled) {
				if (orderChecker.manualCheckScheduled)
					orderChecker.ManualCheck ();
				else if (orderChecker.autoCheck)
					orderChecker.CheckForOrder ();
				UpdateState (orderChecker.autoCheck ? "order-check" : "idle");
			}

			Thread.Sleep (1000);

			if(!shutdown)
				MainLoop ();
		}


		// ========== Animation Loop ==========
		public static void AnimationLoop() {
			Application.Invoke (delegate {
				mainWindow.UpdateAnimation ();
			});

			Thread.Sleep (10);
			while (mainWindow.animationTickActive) {
				Thread.Sleep (10);
			}
			if(!shutdown)
				AnimationLoop ();
		}


		// ========== Logging ==========
		public static void Log (string category, string message, bool webLog = true,  string dataObject = null) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog (category, message);
			});
			if(webLog)
				WebLog ("Log", category, message, dataObject);
		}

		public static void LogAlert (string category, string message, bool webLog = true, string dataObject = null) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog (category, message, "alert");
			});
			if(webLog)
				WebLog ("Alert", category, message, dataObject);
		}

		public static void LogSuccess (string category, string message, bool webLog = true, string dataObject = null) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog (category, message, "success");
			});
			if(webLog)
				WebLog ("Success", category, message, dataObject);
		}

		public static void LogWarning (string category, string message, bool webLog = true, string dataObject = null) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog (category, message, "warning");
			});
			if(webLog)
				WebLog ("Warning", category, message, dataObject);
		}

		public static void LogError (string category, string message, bool webLog = true, string dataObject = null) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog (category, message, "error");
			});
			if(webLog)
				WebLog ("Error", category, message, dataObject);
		}

		public static void LogException (Exception e, bool webLog = true) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog ("", e.ToString (), "exception");
			});
			if(webLog)
				WebLog ("Exception", "Exception", e.ToString ().Split (new char[] { '\r', '\n' })[0], e.ToString());
		}

		public static void LogException (string exceptionMessage, bool webLog = true) {
			Application.Invoke (delegate {
				mainWindow.UpdateLog ("", exceptionMessage, "exception");
			});
			if(webLog)
				WebLog ("Exception", "Exception", exceptionMessage.Split(new char[] { '\r', '\n' })[0], exceptionMessage);
		}



		// ========== Web Logging ==========
		/** Sends a log message to the web server if web logging is enabled. This will also append the active order ID if available. **/
		public static void WebLog (string type, string category, string message, string dataObject = null) {
			Thread webLogThread = new Thread (new ThreadStart(delegate {
				if (configGlobal.logURL == "")
					return;

				// Gather Data:
				WebLogJSON logJSON = new WebLogJSON ();
				logJSON.AccessToken = LogToken;
				logJSON.Username = configGlobal.logUsername;
				logJSON.Password = configGlobal.logPassword;
				logJSON.Version = version;
				logJSON.LogType = type;
				logJSON.LogCategory = category;
				logJSON.LogMessage = message;
				if(orderChecker != null && orderChecker.activeOrder != null)
					logJSON.OrderNumber = orderChecker.activeOrder.OrderNumber;

				// Data Object:
				bool jsonObject = false;
				if (dataObject != null) {
					if (dataObject[0] == '{' && dataObject[dataObject.Length - 1] == '}') {
						logJSON.Object = "{JSONObject}";
						jsonObject = true;
					}
					else
						logJSON.Object = dataObject;
				}
				
				try {
					// Send:
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(configGlobal.logURL);
					request.ContentType = "application/json; charset=utf-8";
					request.Method = "POST";
					JavaScriptSerializer serializer = new JavaScriptSerializer();
					using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream())) {
						string jsonData = serializer.Serialize(logJSON);
						if (jsonObject)
							jsonData = jsonData.Replace("\"{JSONObject}\"", dataObject);
						streamWriter.Write(jsonData);
						streamWriter.Flush();
					}

					// Receive:
					HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
					using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream())) {
						string jsonData = streamReader.ReadToEnd();
						WebLogJSON outputJSON = serializer.Deserialize<WebLogJSON> (jsonData);
						if(outputJSON != null)
							if(!string.IsNullOrEmpty(outputJSON.AccessToken))
								LogToken = outputJSON.AccessToken;
					}
				} catch (Exception e) {
					if(ErrorWebURL != configGlobal.logURL)
						return;
					LogError ("Web Logging", "Unable to upload logs to the web logging URL:", false);
					LogException (e, false);
					if(e is WebException) {
						using (StreamReader streamReader = new StreamReader(((WebException) e).Response.GetResponseStream ())) {
							LogException (streamReader.ReadToEnd(), false);
						}
					}
					ErrorWebURL = configGlobal.logURL;
					return;
				}
				ErrorWebURL = "";
			}));
			webLogThread.Start ();
		}

		/** Parses the provided order and sends it to the web logging service. **/
		public static void WebLogOrder (Order order) {
			WebLog ("Order", "Order", order.ToString (), order.GetJSON ());
		}

		/** Clear any current web logging token, used if the current one is no longer valid or the logging connection details are changed. **/
		public static void ClearLogToken () {
			LogToken = "";
		}


		// ========== Update State ==========
		public static void UpdateState (string state) {
			mainState = state;
			Application.Invoke (delegate {
				Program.mainWindow.UpdateMainState (state);
			});
		}


		// ========== Display Order ==========
		public static void DisplayOrder (Order order) {
			Application.Invoke (delegate {
				mainWindow.DisplayOrder (order);
			});
		}


		// ========== Change WMS ==========
		public static void ChangeWMS (string newWMSName) {
			WMS newWMS = WMS.GetWMS (newWMSName);
			if (newWMS == null)
				return;
			apiThread.Abort ();
			wms = newWMS;
			apiThread = new Thread (new ThreadStart(wms.Startup));
			apiThread.Start ();
		}


		// ========== Can Translate ==========
		public static bool CanTranslate(string country)
		{
			if (Translator == null)
				return false;
			return Translator.CanTranslate(country);
		}


		// ========== Quit ==========
		public static void Quit () {
			Log ("UberDespatch", "Closing down...");
			orderChecker.OnQuit ();
			Application.Quit ();
		}
	}
}
