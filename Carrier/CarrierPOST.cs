using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.IO.Compression;
using Gtk;
using System.Collections.Generic;

namespace UberDespatch
{
	public class CarrierPOST : Carrier
	{
		public CarrierGroup Group;
		public CarrierPOSTGroup.CarrierEntry CarrierEntry;
		public bool OrderSent = false;
		public string CarrierResult = "";
		public Exception CarrierError = null;


		// ========== Response JSON Class ==========
		public class ResponseJSON
		{
			public string LabelData;
			public string LabelDataType;
			public string TrackingId;
			public string Error;
			public string ErrorObject;
		}


		// ========== Constructor ==========
		public CarrierPOST (string name, string description)
		{
			this.Name = name;
			this.Description = description;
			this.Timeout = 180;

			// Icon:
			string iconPath = Program.ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar;
			string iconDir = "svg" + System.IO.Path.DirectorySeparatorChar;
			string iconExtension = ".svg";
			try {
				this.Icon = new Gdk.Pixbuf (iconPath + iconDir + this.Name + iconExtension);
			} catch (Exception e) {
				iconDir = "png" + System.IO.Path.DirectorySeparatorChar;
				iconExtension = ".png";
				try {
					this.Icon = new Gdk.Pixbuf (iconPath + iconDir + this.Name + iconExtension);
				} catch (Exception e2) {
					Program.LogWarning (this.Name, "Unable to load an icon for this carrier. (" + iconPath + iconDir + this.Name + iconExtension + ")");
				}
			}
		}


		// ========== Config ==========
		/** Use to set a config value via key. This should be overriden and handled as neccesary. **/
		public override void SetConfigValue(string key, string value) {
			if (key == "name") {
				this.CarrierEntry.Name = value;
				this.Group.RenameCarrier (this.Name, value);
			}
			else if (key == "url")
				this.CarrierEntry.URL = value;
			else if (key == "printerProfile")
				this.CarrierEntry.PrinterProfile = value;
			else if (key == "additionalPOST")
				this.CarrierEntry.AdditionalPOST = value;
		}

		/** Use to get a config value via key. This should be overriden and handled as neccesary. **/
		public override string GetConfigValue(string key) {
			if (key == "name")
				return this.CarrierEntry.Name;
			else if (key == "url")
				return this.CarrierEntry.URL;
			else if (key == "printerProfile")
				return this.CarrierEntry.PrinterProfile;
			else if (key == "additionalPOST")
				return this.CarrierEntry.AdditionalPOST;
			return "";
		}

		/** Saves the config. **/
		public override void SaveConfig() {
			this.Group.SaveCarriers ();
		}


		// ========== Menu Action Activated ==========
		// This is called by the UI to open up a specific settings window.
		public override void OpenSettingsWindow () {
			CarrierPOSTWindow settingsWindow = new CarrierPOSTWindow (this.Group);
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
			this.CarrierResult = "";
			this.CarrierError = null;

			// Send via POST:
			WebClient web = new WebClient();
			try
			{
				web.Headers["Content-Type"] = "application/x-www-form-urlencoded";
				web.UploadStringCompleted += new UploadStringCompletedEventHandler(OnCarrierResponse);
				web.UploadStringAsync(new Uri(this.GetConfigValue("url") + "/create"), "POST", this.GetPOSTData(order));
			}
			catch (Exception e)
			{
				Program.LogWarning(this.Name, "Unable to connect to " + this.GetConfigValue("url") + "/create");
				Program.LogException(e);
				order.Error = true;
				return;
			}

			// Wait For Responce:
			Program.Log (this.Name, "Waiting for " + this.Name + " output...");
			while (this.CarrierResult == "" && this.CarrierError == null) { // While no responce
				if (order.Cancelled || this.TimedOut) {
					web.CancelAsync ();
					return;
				}
				Thread.Sleep (1);
			}

			// Success Responce:
			if (this.CarrierResult != "") {
				// Parse JSON:
				JavaScriptSerializer serializer = new JavaScriptSerializer();
				ResponseJSON responseJSON;
				try {
					responseJSON = serializer.Deserialize<ResponseJSON>(this.CarrierResult);
				}
				catch (Exception e) {
					Program.LogError(this.Name, "Invalid response JSON format:");
					Program.LogException(e);
					order.Error = true;
					return;
				}

				// Error Check:
				if (responseJSON.Error != null) {
					Program.LogError(this.Name, "Hive has returned an error: " + responseJSON.Error, true, responseJSON.ErrorObject);
					order.Error = true;
					return;
				}

				// Tracking Number:
				order.TrackingNumber = responseJSON.TrackingId;

				// Decode Label:
				byte[] label = null;
				string fileType = "pdf";
				try {
					label = Decoder.Decompress(Convert.FromBase64String(responseJSON.LabelData));
					fileType = responseJSON.LabelDataType.Split('/')[1];
				}
				catch (Exception e) {
					Program.LogError (this.Name, "An error occured when trying to decompress the label.");
					Program.LogException (e);
				}

				if (label != null)
				{
					// Save Label:
					string saveDir = Program.configGlobal.archivePath + System.IO.Path.DirectorySeparatorChar + this.Name;
					string savePath = saveDir + System.IO.Path.DirectorySeparatorChar + order.FileInfo.Name + "." + fileType;
					try
					{
						System.IO.Directory.CreateDirectory(saveDir);
						System.IO.File.WriteAllBytes(savePath, label);
					}
					catch (Exception e)
					{
						Program.LogError(this.Name, "An error occured when trying to save the label or printing.");
						Program.LogException(e);
					}

					// Print Label:
					if (fileType == "pdf")
						Program.printer.PrintPDF(savePath, this.GetConfigValue("printerProfile"));
					else if (fileType == "png")
						Program.printer.PrintPNG(savePath, this.GetConfigValue("printerProfile"));
					else
						Program.LogError(this.Name, "Unable to print the filetype: " + responseJSON.LabelDataType);
				}
				else {
					Program.LogError(this.Name, "No carrier label was printed.");
				}
			}

			// Error Response:
			else if (this.CarrierError != null) {
				Program.LogError (this.Name, "Error received from " + this.Name + ":");
				Program.LogException (this.CarrierError);
				order.Error = true;
				return;
			}

			// No Response:
			else {
				Program.LogError (this.Name, "No response was received from " + this.Name + ".");
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


		// ========== On Wait ==========
		// Called each second when waiting for an order to dispatch, can be used to print waiting messages, etc.
		public override void OnWait(long waitTime)
		{
			if (waitTime == 12) {
				Program.LogAlert(this.Name, this.Name + " is taking a while to respond, it is likely being throttled due to too many requests, this may take a minute to complete...");
			}
		}


		// ========== On Carrier Response ==========
		// This is added to the WebClient's async upload event and is called when it is finished either in success or error.
		public void OnCarrierResponse(object sender, UploadStringCompletedEventArgs output)
		{
			try
			{
				this.CarrierResult = output.Result;
			}
			catch (Exception e)
			{
				this.CarrierError = e;
			}
		}


		// ========== Get POST Data ==========
		// Returns the POST data to be sent to Hive, this takes an Order object which is to be converted into POST data.
		public string GetPOSTData(Order order)
		{
			string postData = this.GetConfigValue ("additionalPOST").Trim ();
			if (postData.Length > 0)
				postData += "&";
			postData += "orderId=" + order.OrderNumber;
			postData += "&orderCost=" + order.OrderCost;
			postData += "&storeName=" + order.Service;
			postData += "&shippingService=" + order.Format;

			postData += "&customerName=" + order.CustomerName;
			postData += "&customerPhone=" + order.CustomerPhone;
			postData += "&customerEmail=" + order.CustomerEmail;
			postData += "&companyName=" + order.Company;

			postData += "&countryCode=" + order.Country;
			postData += "&countryName=" + order.CountryName;
			postData += "&postCode=" + order.Postcode;
			postData += "&street=" + order.Street;
			postData += "&locality=" + order.Locality;
			postData += "&city=" + order.City;
			postData += "&region=" + order.Region;

			postData += "&itemAmount=" + order.ItemAmount;
			postData += "&weight=" + order.OrderWeight;
			return postData;
		}


		// ========== Get Icon ==========
		// Returns an image to display in the main interface when sending an order to this carrier, if null, the default sending image is used instead.
		public override Gdk.Pixbuf GetIcon() {
			return this.Icon;
		}
	}
}
