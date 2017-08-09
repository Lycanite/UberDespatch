using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Text;
using System.Globalization;
using System.Linq;

namespace UberDespatch
{
	public class CarrierInterlink : Carrier
	{
		public ConfigCarrierInterlink config;
		public bool orderSent = false;

		// ========== Config Class ==========
		public class ConfigCarrierInterlink : ConfigBase<ConfigCarrierInterlink>
		{
			public string inputPath = Program.ExecutableFolder + Path.DirectorySeparatorChar + "Interlink" + Path.DirectorySeparatorChar + "Input";
			public string inputFilename = "Input_{ID}.csv";
			public string outputPath = Program.ExecutableFolder + Path.DirectorySeparatorChar + "Interlink" + Path.DirectorySeparatorChar + "Output";
			public string outputFilename = "Input_{ID}.csv";
			public string template = @"{CustomerName},{CustomerPhone},{CustomerMobile},{CustomerEmail},{OrganisationName},{Street},{Locality},{City},{Region},{Country},{Postcode},{OrderNumber},{ItemAmount},{OrderWeight},{ServiceType}";

			public static ConfigCarrierInterlink LoadFile() {
				return Load ("config" + Path.DirectorySeparatorChar + "carrierInterlink.json");
			}

			public void SaveFile() {
				Save ("config" + Path.DirectorySeparatorChar + "carrierInterlink.json");
			}
		}


		// ========== Constructor ==========
		public CarrierInterlink ()
		{
			this.Name = "Interlink";
			string iconPath = Program.ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar;
			string iconDir = "svg" + System.IO.Path.DirectorySeparatorChar;
			string iconExtension = ".svg";
			try {
				this.Icon = new Gdk.Pixbuf (iconPath + iconDir + this.Name + iconExtension);
			} catch (Exception e) {
				iconDir = "png" + System.IO.Path.DirectorySeparatorChar;
				iconExtension = ".png";
				this.Icon = new Gdk.Pixbuf (iconPath + iconDir + this.Name + iconExtension);
			}

			// Config:
			this.config = ConfigCarrierInterlink.LoadFile ();
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
			CarrierInterlinkWindow settingsWindow = new CarrierInterlinkWindow (this);
			settingsWindow.Show ();
		}


		// ========== Validate ==========
		/** Returns true if the order is valid for processing, if the order is invalid (missing some essential fields) then false is returned where the OrderChecker will wait for the user to either edit the order and try again or skip the order and send it straight to the archive. **/
		public override bool ValidateOrder (Order order) {
			if (order.OrderWeight <= 0) {
				order.OrderWeight = 1;
				Program.LogWarning (this.Name, "Warning, the order had a weight of 0 or below, this has been changed to 1.");
			}
			if (Regex.IsMatch(order.CustomerPhone, @"^\d+$")) {
				Program.LogWarning (this.Name, "Warning, the order phone number had non-numeric characters, these will be stripped.");
			}
			order.CustomerPhone = Regex.Replace (order.CustomerPhone, "[^0-9]", "");
			if (Regex.IsMatch(order.CustomerMobile, @"^\d+$")) {
				Program.LogWarning (this.Name, "Warning, the order mobile number had non-numeric characters, these will be stripped.");
			}
			order.CustomerMobile = Regex.Replace (order.CustomerMobile, "[^0-9]", "");
			return true;
		}


		// ========== Send To Carrier ==========
		// Sends an Order object to the carrier service. This is invoked on a new thread while the main thread waits until WaitForCarrier() returns true.
		public override void SendToCarrier(Order order) {
			// Clear Data:
			string inputPath = this.GetConfigValue ("inputPath") + Path.DirectorySeparatorChar + this.GetConfigValue ("inputFilename").Replace ("{ID}", order.OrderNumber);
			DirectoryInfo outputDir = new DirectoryInfo(this.GetConfigValue ("outputPath"));
			if(File.Exists (inputPath))
				File.Delete (inputPath);
			if(File.Exists (inputPath + ".BAD"))
				File.Delete (inputPath + ".BAD");
			foreach(System.IO.FileInfo file in outputDir.GetFiles())
				file.Delete();

			// Send:
			Program.Log (this.Name, "Sending input to Interlink...");
			string input = this.GetConfigValue ("template");

			input = input.Replace ("{CustomerName}",  CleanInput (order, order.CustomerName.Replace (",", "")));
			input = input.Replace ("{CustomerPhone}",  CleanInput (order, order.CustomerPhone.Replace (",", "")));
			input = input.Replace ("{CustomerMobile}",  CleanInput (order, order.CustomerMobile.Replace (",", ""))); // Customer SMS
			input = input.Replace ("{CustomerEmail}",  CleanInput (order, order.CustomerEmail.Replace (",", "")));

			string company = String.IsNullOrEmpty (order.Company) ? order.CustomerName : order.Company;
			input = input.Replace ("{OrganisationName}",  CleanInput (order, company.Replace (",", ""))); // Company, if blank, same as Customer Name
			input = input.Replace ("{Street}",  CleanInput (order, order.Street.Replace (",", ""))); // Line 1
			input = input.Replace ("{Locality}",  CleanInput (order, order.Locality.Replace (",", ""))); // line 2
			input = input.Replace("{City}", CleanInput(order, order.City.Replace(",", ""))); // Line 3
			input = input.Replace("{Region}", CleanInput(order, order.Region.Replace(",", ""))); // Line 4
			input = input.Replace ("{Company}",  CleanInput (order, order.Company.Replace (",", "")));
			input = input.Replace ("{Postcode}",  CleanInput (order, order.Postcode.Replace (",", "")));
			input = input.Replace ("{Country}",  CleanInput (order, order.Country.Replace (",", "")));

			input = input.Replace ("{OrderNumber}",  CleanInput (order, order.OrderNumber)); // Customer Ref 1
			input = input.Replace ("{ItemAmount}",  CleanInput (order, order.ItemAmount.ToString ()));
			input = input.Replace ("{OrderWeight}",  CleanInput (order, order.OrderWeight.ToString ()));

			input = input.Replace ("{ServiceType}",  CleanInput (order, order.Service));

			File.WriteAllText (inputPath, input);

			// Wait For Responce:
			Program.Log (this.Name, "Waiting for Interlink output...");
			while (outputDir.GetFiles().Length < 1 && !this.CheckFile (inputPath + ".BAD")) {
				if (order.Cancelled || order.Error || this.TimedOut) {
					File.Delete (inputPath);
					return;
				}
				Thread.Sleep (1);
			}
			Thread.Sleep (1000);
			File.Delete (inputPath);

			// Output File:
			if (outputDir.GetFiles ().Length > 0) {
				string outputPath = outputDir.GetFiles ().First ().FullName;
				string output = File.ReadAllText (outputPath);
				File.Delete (outputPath);
				string[] outputDetails = Regex.Split (output, "\r\n");

				// Tracking Number:
				if (outputDetails [0] != null && outputDetails [0] != "") {

					// 0 Success:
					string trackingNumber = Regex.Split (outputDetails [0], ",").First ();
					if (trackingNumber != "")
						order.TrackingNumber = trackingNumber;

					// Error:
					else {
						Program.LogError (this.Name, "Interlink has not returned a tracking number, it returned this:");
						Program.LogException (string.Join ("\n", outputDetails));
						order.Error = true;
						return;
					}
				}

				// Invalid:
				else {
					Program.LogError (this.Name, "Invalid output file received from Interlink.");
					order.Error = true;
					return;
				}
			}

			// Error File:
			else if (File.Exists (inputPath + ".BAD")) {
				Program.LogError (this.Name, "Error received from Interlink, the following values were not accepted:");
				Program.LogException (File.ReadAllText (inputPath + ".BAD").Replace (",", ", "));
				order.Error = true;
				File.Delete (inputPath + ".BAD");
				return;
			}

			// No Output:
			else {
				Program.LogError (this.Name, "No output file received from Interlink.");
				order.Error = true;
				return;
			}

			// Tracking Number Validation:
			if (string.IsNullOrEmpty (order.TrackingNumber))
				Program.LogWarning (this.Name, "No tracking number was returned.");
			else
				Program.LogSuccess (this.Name, "Tracking number received: " + order.TrackingNumber);
			order.Processed = true;
		}


		// ========== Get Icon ==========
		// Returns an image to display in the main interface when sending an order to this carrier, if null, the default sending image is used instead.
		public override Gdk.Pixbuf GetIcon() {
			return this.Icon;
		}
	}
}

