using System;
using Gtk;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UberDespatch
{
	public abstract class WMS
	{
		public static Dictionary<string, WMS> wmsList = new Dictionary<string, WMS> ();

		public byte connection = 0; // 0 = Disconnected, 1 = Connecting, 2 = Connected
		public string name = "WMS"; // The name of this WMS.
		public string lastError { get; protected set; } // The last error message set via the WMS.
		public string[] FileFilters = new string[0]; // A list of file extension filters to used when searching for an order file to open. Eg: "*.xml"


		// ========== Get Default WMS ==========
		// Returns the default WMS to use and saves all available WMS to a dictionary. In the future, new WMS will be available via plugins and set via the settings menu.
		public static WMS GetDefaultWMS() {
			WMS defaultWMS = new WMSPeoplevox ();
			wmsList.Add (defaultWMS.name, defaultWMS); // TODO Move this to the plugins loader when implemented later.
			return defaultWMS;
		}


		// ========== Get WMS ==========
		// Returns a WMS via name (case sensitive).
		public static WMS GetWMS(string wmsName) {
			if(wmsList.ContainsKey (wmsName))
				return wmsList[wmsName];
			return null;
		}


		// ========== Constructor ==========
		public WMS ()
		{
			// Warehouse Management System Base - Should not be instantiated.
		}


		// ========== Config ==========
		/** Use to set a config value via key. This should be overriden and handled as neccesary. **/
		public virtual void SetConfigValue(string key, string value) {
			// Use custom config class and respond to valid keys here.
		}

		/** Use to get a config value via key. This should be overriden and handled as neccesary. **/
		public virtual string GetConfigValue(string key) {
			// Use custom config class and respond to valid keys here.
			return "";
		}

		/** Saves the config. **/
		public virtual void SaveConfig() {
			// Override this method.
		}


		// ========== Startup ==========
		/** Dedicated thread loop for API logic. Override the Connect() method rather than this one. **/
		public void Startup() {
			this.Connect ();
			this.MainLoop ();
		}


		// ========== Main Loop ==========
		/** Dedicated thread loop for API logic. Override the MainUpdate() method rather than this one. **/
		public void MainLoop() {
			System.Threading.Thread.Sleep (1000);
			if (!Program.shutdown) {
				this.MainUpdate ();
				this.MainLoop ();
			}
		}


		// ========== Main Update ==========
		/** The main update lgoic of this WMS, called by MainLoop(). **/
		public virtual void MainUpdate() {
			// Optional looping logic goes here.
		}


		// ========== Connect ==========
		/** Connects to the WMS API, will refresh the connection if already connected. Returns true for success and false for failure. Calls UpdateState() for UI feedback. **/
		public virtual bool Connect ()
		{
			// Failure:
			this.UpdateState (0);
			Program.LogError (this.name, "The base WMS class cannot be used.");
			return false;
		}


		// ========== Is Connected ==========
		/** Returns if UberDespatch is currently connected to the WMS and updates the state to reflect it via the GUI. Should be overriden. **/
		public virtual bool IsConnected () {
			this.UpdateState (0);
			return false;
		}


		// ========== Update State ==========
		/** Updates the current connection state, mainly used for the GUI representation. 0 = Disconnected, 1 = Connecting, 2 = Connected **/
		public virtual void UpdateState (byte state) {
			this.connection = state;
			Application.Invoke(delegate {
				Program.mainWindow.UpdateAPIState (this.connection);
			});
		}


		// ========== Check For Order ==========
		// Called by the main OrderChecker instance. Checks for orders and returns a parsed Order object if found or null if none is found. Try to avoid overriding this and use the GetFileSearch() and CreateOrder() methods instead.
		public Order CheckForOrder() {
			FileInfo orderFile = null;
			try {
				DirectoryInfo directory = new DirectoryInfo(Program.configGlobal.downloadsPath);
				orderFile = directory.GetFiles(this.GetFileSearch ()).OrderByDescending(f => f.LastWriteTime).First();
				Program.Log (this.name, "Found order file: " + orderFile.Name);
				Program.UpdateState ("order-read");
			} catch {}
			if (orderFile != null) {
				Order order = null;
				try {
					order = this.CreateOrder (orderFile);
				} catch (Exception e) {
					Program.LogError (this.name, "Error reading order:");
					Program.LogException (e);
				}
				if (order == null) {
					Program.UpdateState ("order-none");
					Program.LogError (this.name, "The order data is invalid, skipped and sent to archive.");
				}
				return order;
			}
			return null;
		}


		// ========== Load Order ==========
		// Used when loading a specific order from a file. Try to avoid overriding this and use the GetFileSearch() and CreateOrder() methods instead.
		public Order LoadOrder(string filePath) {
			Order order = null;
			try {
				FileInfo orderFile = new FileInfo (filePath);
				order = this.CreateOrder(orderFile);
			}
			catch (Exception e) {
				Program.LogError(this.name, "Error reading order:");
				Program.LogException(e);
			}
			if (order == null) {
				Program.LogError(this.name, "The order data is invalid.");
			}
			return order;
		}


		// ========== Get File Search ==========
		// Returns a file search syntax such as '*.xml'.
		public virtual String GetFileSearch() {
			return "*.xml"; // Override this method.
		}


		// ========== Create Order ==========
		/** Returns an Order object created from the provided file. Returns null if the file is invalid. **/
		public virtual Order CreateOrder (FileInfo orderFile) {
			return null; // Override this method.
		}


		// ========== Update Order ==========
		// Updates the WMS with the provided order changes, this is primarily used for send back tracking numbers.
		public virtual void UpdateOrder(Order order) {
			// Override this metod.
		}
	}
}

