﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UberDespatch
{
	public abstract class Carrier
	{
		public static Dictionary<string, Carrier> Carriers = new Dictionary<string, Carrier> ();
		public static Dictionary<string, CarrierGroup> CarrierGroups = new Dictionary<string, CarrierGroup> ();
		public static string LastCarrierAdded;

		public string Name = "Carrier";
		public string Description = "";
		public Gdk.Pixbuf Icon = null;
		public long Timeout = 60;
		public bool TimedOut = false; // Set to true on timeout.


		// ========== Reload Carriers ==========
		public static void ReloadCarriers ()
		{
			/*foreach(Carrier carrier in Carrier.carriers.Values) {
				carrier.Unload ();
			}*/
			Carriers.Clear ();
			CarrierGroups.Clear ();
			LoadCarriers ();
		}


		// ========== Load Carriers ==========
		public static void LoadCarriers ()
		{
			// None:
			Carrier carrierManual = new CarrierManual ();
			Carriers.Add (carrierManual.Name, carrierManual);
			LastCarrierAdded = carrierManual.Name;

			// RMDMO:
			Carrier carrierRMDMO = new CarrierRMDMO ();
			Carriers.Add (carrierRMDMO.Name, carrierRMDMO);

			// DPD:
			Carrier carrierDPD = new CarrierDPD ();
			Carriers.Add (carrierDPD.Name, carrierDPD);

			// Interlink:
			Carrier carrierInterlink = new CarrierInterlink ();
			Carriers.Add (carrierInterlink.Name, carrierInterlink);

			// POST Group:
			CarrierGroup carrierPOST = new CarrierPOSTGroup ();
			CarrierGroups.Add (carrierPOST.Name, carrierPOST);

			/*/ Remote - Amazon:
			string carrierHiveDesc = "This is a Remote powered carrier for Amazon Fulfilled/Prime. In the details box below you can add additional POST parameters, if required. The service and format entries in the rules can be sent over if additional info is needed.";
			Carrier carrierHive = new CarrierHive("Amazon", carrierHiveDesc);
			carriers.Add(carrierHive.name, carrierHive);

			// Remote - DHL:
			carrierHiveDesc = "This is a Remote powered carrier for DHL. In the details box below you can add additional POST parameters, if required. The service and format entries in the rules can be sent over if additional info is needed.";
			carrierHive = new CarrierHive("DHL", carrierHiveDesc);
			carriers.Add(carrierHive.name, carrierHive);*/

			Program.LogSuccess ("Carriers", Carriers.Count + " carriers loaded: " + GetCarrierNames ());
		}


		// ========== Get Carrier ==========
		/** Returns the first carrier found by the provided name, this will also search each carrier group, the first occurance will be returned. GroupName:CarrierName can be used to target carriers from specific groups. **/
		public static Carrier GetCarrier (string carrierName = "")
		{
			if (carrierName == "")
				return Carriers [LastCarrierAdded];
			if (Carriers.ContainsKey (carrierName))
				return Carriers[carrierName];
			foreach (CarrierGroup carrierGroup in CarrierGroups.Values) {
				Carrier childCarrier = carrierGroup.GetCarrier (carrierName);
				if (childCarrier != null)
					return childCarrier;
			}
			return null;
		}


		// ========== Get Carrier Group ==========
		/** Returns the first carrier group by the provided name. **/
		public static CarrierGroup GetCarrierGroup (string carrierGroupName = "")
		{
			if (carrierGroupName == "")
				return CarrierGroups [LastCarrierAdded];
			if (CarrierGroups.ContainsKey (carrierGroupName))
				return CarrierGroups[carrierGroupName];
			return null;
		}


		// ========== Get Carrier Names ==========
		/** Returns a comma and space seperated list of all available carriers by case sensitive name. **/
		public static string GetCarrierNames ()
		{
			if (Carriers == null || Carriers.Count == 0)
				return "No carriers available.";
			string carrierNames = "";
			foreach (Carrier carrier in Carriers.Values)
				carrierNames += ", " + carrier.Name;
			foreach (CarrierGroup carrierGroup in CarrierGroups.Values) {
				if (carrierGroup.Carriers.Count > 0) {
					carrierNames += ", " + carrierGroup.Name + " Carriers: ";
					foreach (Carrier carrier in carrierGroup.Carriers.Values)
						carrierNames += ", " + carrier.Name;
				}
			}
			return carrierNames.Substring (2);
		}


		// ========== Constructor ==========
		public Carrier ()
		{
			// Carrier objects should be extended, they process orders and update the Order object so that it can be return to the WMS system.
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


		// ========== Validate ==========
		/** Returns true if the order is valid for processing, if the order is invalid (missing some essential fields) then false is returned where the OrderChecker will wait for the user to either edit the order and try again or skip the order and send it straight to the archive. **/
		public virtual bool ValidateOrder (Order order) {
			return false; // Override this method.
		}


		// ========== Process Order ==========
		// Sends an Order object to the carrier service for despatch, edits the order (adding a tracking number, etc) and then returns the order for the WMS to finish with.
		public bool ProcessOrder(Order order) {
			order.CarrierName = this.Name;

			// Send and Wait:
			this.TimedOut = false;
			Thread carrierThread = new Thread (new ThreadStart(delegate {
				try {
					this.SendToCarrier (order);
					if (order.Processed) {
						order.OnPostProcess();
					}
				}
				catch (Exception e) {
					Program.LogError(this.Name, "A runtime exception occured:");
					Program.LogException (e);
				}
			}));
			Program.Log (this.Name, "Sending order...");
			carrierThread.Start ();
			long waitTime = 0;
			while ((this.Timeout <= 0 || waitTime < this.Timeout) && !order.Processed && !order.Cancelled && !order.Error) {
				this.OnWait(waitTime);
				waitTime++;
				Thread.Sleep (1000);
			}

			// Not Processed:
			if (!order.Processed) {
				// Cancelled:
				if (order.Cancelled) {
					Program.LogAlert (this.Name, "Processing cancelled, the order was not sent.");
					Program.UpdateState ("order-skip");
				}
				// Error:
				else if (order.Error) {
					Program.LogError (this.Name, "Processing encounted an error, the order was not sent.");
					Program.UpdateState ("order-fail");
				}
				// Timeout:
				else {
					this.TimedOut = true;
					Program.LogError (this.Name, "Processing timed out, the order was not sent.");
					Program.UpdateState ("order-fail");
					order.Error = true;
				}
			}

			// Processed:
			else {
				Program.LogSuccess (this.Name, "Order successfully processed.");
			}

			return order.Processed;
		}


		// ========== On Wait ==========
		// Called each second when waiting for an order to dispatch, can be used to print waiting messages, etc.
		public virtual void OnWait(long waitTime)
		{
			return; // Override this method.
		}


		// ========== Send To Carrier ==========
		// Sends an Order object to the carrier service for despatch. This is invoked on a new thread while the main thread checks on the Order object (processed and cancelled booleans).
		public virtual void SendToCarrier(Order order) {
			return; // Override this method.
		}


		// ========== Check Output ==========
		// Returns true if the provided file path leads to a file that exists and is readable.
		public virtual bool CheckFile(string filePath) {
			if (!File.Exists (filePath))
				return false;
			bool isWritableAndFree = false;
			try {
				using (new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) {
					isWritableAndFree = true;
				}
			} catch { }
			return isWritableAndFree;
		}


		// ========== Menu Action Activated ==========
		// This is called by the UI to open up a specific settings window.
		public virtual void OpenSettingsWindow () {
			// Override this method.
		}


		// ========== Get Icon ==========
		// Returns an image to display in the main interface when sending an order to this carrier, if null, the default sending image is used instead.
		public virtual Gdk.Pixbuf GetIcon() {
			return null; // Override this method.
		}


		// ========== Clean Input ==========
		/** Returns a cleaned version of the provided string, replacing accented characters with suitable non-accented characters and remove all invalid characters. **/
		public static string CleanInput(Order order, string text)  {
			if (order.Translations != null && order.Translations.ContainsKey(text))
				text = order.Translations[text];

			string normalizedString = text.Normalize(NormalizationForm.FormD);
			StringBuilder stringBuilder = new StringBuilder();

			foreach (char c in normalizedString) {
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
				if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
					stringBuilder.Append(c);
				}
			}

			string filteredInput = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
			filteredInput = filteredInput.Replace ("ß", "ss");
			filteredInput = filteredInput.Replace ("ø", "oe");
			filteredInput = filteredInput.Replace("Ø", "Oe");
			return Regex.Replace(filteredInput, @"[^\w\s\.\@\\\-\/]", string.Empty);
		}
	}
}

