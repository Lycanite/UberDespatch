using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Text;
using System.Globalization;
using System.Linq;

namespace UberDespatch {
	public class CarrierRMDMO : Carrier {
		public ConfigCarrierRMDMO config;
		public bool orderSent = false;

		// ========== Config Class ==========
		public class ConfigCarrierRMDMO : ConfigBase<ConfigCarrierRMDMO> {
			public string inputPath = Program.ExecutableFolder + Path.DirectorySeparatorChar + "RMDMO" + Path.DirectorySeparatorChar + "Input";
			public string inputFilename = "Data.txt";
			public string outputPath = Program.ExecutableFolder + Path.DirectorySeparatorChar + "RMDMO" + Path.DirectorySeparatorChar + "Output";
			public string outputFilename = "Result.txt";
			public string template = @"﻿ADD ""UberDespatchLocalised""
""1"",""{ServiceType}"",""{ServiceFormat}"",""{CustomerName}"",""{Country}"",""{City}"",""{Region}"",""{Street}"",""{Locality}"",""{Postcode}"",""{OrderNumber}"",""{ItemAmount}"",""{OrderWeight}"",""{LocalisedCustomerName}"",""{LocalisedCity}"",""{LocalisedRegion}"",""{LocalisedStreet}"",""{LocalisedLocality}"",""{LocalisedPostcode}""";

			public static ConfigCarrierRMDMO LoadFile() {
				return Load ("config" + Path.DirectorySeparatorChar + "carrierRMDMO.json");
			}

			public void SaveFile() {
				Save ("config" + Path.DirectorySeparatorChar + "carrierRMDMO.json");
			}
		}


		// ========== Constructor ==========
		public CarrierRMDMO () {
			this.name = "RoyalMail";

			// Icon:
			string iconPath = Program.ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar;
			string iconDir = "svg" + System.IO.Path.DirectorySeparatorChar;
			string iconExtension = ".svg";
			try {
				this.icon = new Gdk.Pixbuf (iconPath + iconDir + this.name + iconExtension);
			} catch (Exception e) {
				iconDir = "png" + System.IO.Path.DirectorySeparatorChar;
				iconExtension = ".png";
				this.icon = new Gdk.Pixbuf (iconPath + iconDir + this.name + iconExtension);
			}

			// Config:
			this.config = ConfigCarrierRMDMO.LoadFile ();
			if (this.GetConfigValue ("inputPath") != "" && !Directory.Exists (this.GetConfigValue ("inputPath")))
				Directory.CreateDirectory (this.GetConfigValue ("inputPath"));
			if (this.GetConfigValue ("outputPath") != "" && !Directory.Exists (this.GetConfigValue ("outputPath")))
				Directory.CreateDirectory (this.GetConfigValue ("outputPath"));
		}


		// ========== Config ==========
		/** Use to set a config value via key. This should be overriden and handled as neccesary. **/
		public override void SetConfigValue(string key, string value) {
			if (key == "inputPath")
				this.config.inputPath = value;
			else if (key == "inputFilename")
				this.config.inputFilename = value;
			else if (key == "outputPath")
				this.config.outputPath = value;
			else if (key == "outputFilename")
				this.config.outputFilename = value;
			else if (key == "template")
				this.config.template = value;
		}

		/** Use to get a config value via key. This should be overriden and handled as neccesary. **/
		public override string GetConfigValue(string key) {
			if (key == "inputPath")
				return this.config.inputPath;
			else if (key == "inputFilename")
				return this.config.inputFilename;
			else if (key == "outputPath")
				return this.config.outputPath;
			else if (key == "outputFilename")
				return this.config.outputFilename;
			else if (key == "template")
				return this.config.template;
			return "";
		}

		/** Saves the config. **/
		public override void SaveConfig() {
			this.config.SaveFile ();
		}


		// ========== Menu Action Activated ==========
		// This is called by the UI to open up a specific settings window.
		public override void OpenSettingsWindow () {
			CarrierRMDMOWindow settingsWindow = new CarrierRMDMOWindow (this);
			settingsWindow.Show ();
		}


		// ========== Validate ==========
		/** Returns true if the order is valid for processing, if the order is invalid (missing some essential fields) then false is returned where the OrderChecker will wait for the user to either edit the order and try again or skip the order and send it straight to the archive. **/
		public override bool ValidateOrder (Order order) {
			if (order.Format == "")
				return false;
			if (order.OrderWeight <= 0) {
				order.OrderWeight = 1;
				Program.LogWarning (this.name, "Warning, the order had a weight of 0 or below, this has been changed to 1.");
			}
			if (Regex.IsMatch(order.CustomerPhone, @"^\d+$")) {
				Program.LogWarning (this.name, "Warning, the order phone number had non-numeric characters, these will be stripped.");
			}
			order.CustomerPhone = Regex.Replace (order.CustomerPhone, "[^0-9]", "");
			if (Regex.IsMatch(order.CustomerMobile, @"^\d+$")) {
				Program.LogWarning (this.name, "Warning, the order mobile number had non-numeric characters, these will be stripped.");
			}
			order.CustomerMobile = Regex.Replace (order.CustomerMobile, "[^0-9]", "");
			return true;
		}


		// ========== Send To Carrier ==========
		// Sends an Order object to the carrier service. This is invoked on a new thread while the main thread waits until WaitForCarrier() returns true.
		public override void SendToCarrier(Order order) {
			// Clear Data:
			string inputPath = this.GetConfigValue ("inputPath") + Path.DirectorySeparatorChar + this.GetConfigValue ("inputFilename");
			string outputPath = this.GetConfigValue ("outputPath") + Path.DirectorySeparatorChar + this.GetConfigValue ("outputFilename");
			string lockPath = this.GetConfigValue ("inputPath") + Path.DirectorySeparatorChar + "Lock.txt";
			if(File.Exists (inputPath))
				File.Delete (inputPath);
			if(File.Exists (outputPath))
				File.Delete (outputPath);

			// Send:
			Program.Log (this.name, "Sending input to RMDMO...");
			string input = this.GetConfigValue ("template");
			input = input.Replace ("{ServiceType}",  CleanInput (order, order.Service));
			input = input.Replace("{ServiceFormat}", CleanInput(order, order.Format));

			input = input.Replace ("{CustomerName}",  CleanInput (order, order.CustomerName));
			input = input.Replace ("{Street}",  CleanInput (order, order.Street));
			string locality = String.IsNullOrEmpty (order.Locality) ? order.Company : order.Locality;
			input = input.Replace ("{Locality}",  CleanInput (order, locality)); // If locality is empty use Company.
			input = input.Replace ("{Company}",  CleanInput (order, order.Company));
			input = input.Replace ("{Postcode}",  CleanInput (order, order.Postcode));
			input = input.Replace ("{City}",  CleanInput (order, order.City));
			input = input.Replace ("{Country}",  CleanInput (order, order.Country));
			input = input.Replace ("{Region}", CleanInput(order, order.Region));
			input = input.Replace ("{OrderNumber}",  CleanInput (order, order.OrderNumber));
			input = input.Replace ("{ItemAmount}",  CleanInput (order, order.ItemAmount.ToString ()));
			input = input.Replace ("{OrderWeight}",  CleanInput (order, order.OrderWeight.ToString ()));
			
			input = input.Replace("{LocalisedCustomerName}", order.CustomerName);
			input = input.Replace("{LocalisedStreet}", order.Street);
			string localityLocal = String.IsNullOrEmpty(order.Locality) ? order.Company : order.Locality;
			input = input.Replace("{LocalisedLocality}", localityLocal); // If locality is empty use Company.
			input = input.Replace("{LocalisedCompany}", order.Company);
			input = input.Replace("{LocalisedPostcode}", order.Postcode);
			input = input.Replace("{LocalisedCity}", order.City);
			input = input.Replace("{LocalisedCountry}", order.Country);
			input = input.Replace("{LocalisedRegion}", order.Region);

			File.WriteAllText (inputPath, input);

			// Create Lock File:
			File.WriteAllText (lockPath, "");

			// Wait For Responce:
			Program.Log (this.name, "Waiting for RMDMO output...");
			while (!this.CheckFile (outputPath) || File.Exists (lockPath)) {
				if (order.Cancelled || order.Error || this.timedOut) {
					File.Delete (inputPath);
					return;
				}
				Thread.Sleep (1);
			}
			Thread.Sleep (1000);

			// Read Responce:
			bool outputValid = false;
			for (int attempt = 0; attempt < 3; attempt++) {
				if (attempt > 0) {
					Thread.Sleep (3000);
					Program.Log (this.name, "Reading output attempt " + (attempt + 1) + "...");
				}
				if (this.ReadOutput (order, outputPath)) {
					outputValid = true;
					break;
				}
			}
			File.Delete (inputPath);
			File.Delete (outputPath);

			// Tracking Number Validation:
			if (string.IsNullOrEmpty (order.TrackingNumber))
				Program.LogWarning (this.name, "No tracking number was returned.");
			else
				Program.LogSuccess (this.name, "Tracking number received: " + order.TrackingNumber);

			// Update Order:
			if (outputValid) {
				if (!order.Error)
					order.Processed = true;
			} else {
				order.Error = true;
			}
		}


		// ========== Read Output ==========
		// Read the carrier output file, returns false if the output should be re-read after a short delay, this is useful for handling output files that might not be written to all at once.
		public bool ReadOutput(Order order, string outputPath) {
			string output = File.ReadAllText(outputPath);
			string[] outputDetails = Regex.Split(output, @"\n|\r\n?");
			if (outputDetails.Length > 2) {

				// 0 Success:
				if (outputDetails [0] == "0") {
					order.TrackingNumber = outputDetails [1];
					return true;
				}

				// 1 Success - No Printer:
				else if (outputDetails [0] == "1") {
					order.TrackingNumber = outputDetails [1];
					Program.LogWarning (this.name, "Warning: RMDMO was unable to print the label, check the RMDMO printer settings and then reprint via RMDMO.");
					return true;
				}

				// Error:
				else {
					Program.LogError (this.name, "RMDMO has returned an error:");
					Program.LogException (string.Join ("\n", outputDetails));
					order.Error = true;
					return true;
				}
			}

			// Invalid:
			Program.LogError (this.name, "Invalid result file received from RMDMO.");
			return false;
		}


		// ========== On Wait ==========
		// Called each second when waiting for an order to dispatch, can be used to print waiting messages, etc.
		public override void OnWait(long waitTime)
		{
			if (waitTime == 12) {
				Program.LogAlert(this.name, this.name + " is taking a while to respond, please make sure that it is open and running and that the internet connection is not slow.");
			}
		}


		// ========== Get Icon ==========
		// Returns an image to display in the main interface when sending an order to this carrier, if null, the default sending image is used instead.
		public override Gdk.Pixbuf GetIcon() {
			return this.icon;
		}
	}
}

