using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using Codaxy.WkHtmlToPdf;
using System.Text;

namespace UberDespatch
{
	public class Label
	{
		// Labels List:
		public static Dictionary<string, Label> Labels = new Dictionary<string, Label> ();

		public string Name = ""; // Name of this label (used to reference via rules json, case insensitive.
		public string Type = "Label"; // The type of label, by default it is a standard label but it can also be a Group which is a collection of several label entries.
		public string PrinterProfile = Printer.DefaultPrinterName; // Name (full name as set in the config or selected via the dropdown) of the printer to use, leave blank for default.
		public string URL = ""; // Path and filename or url to html to use for printing the label (append with http or https for a url).
		public bool Shared = false; // If true, this label is generic (not order specific) and only needs one copy.
		public string[] LabelNames; // Used for the Group type, contains a list of label names to use.

		public string Copies = "1"; // How many copies of this label to print.
		public string Zoom = "1"; // The Zoom to scale pdfs by, usually 1, 1.33 is good for labels.
		public string Orientation = "Portrait"; // Can be Portrait or Landscape.
		public string Options = ""; // Here additional wkhtmltopdf arguments can be passed.

		public string PDF = ""; // The path to this label's PDF, used if this label should be cached.


		// ========== Label JSON ==========
		public class LabelJSON
		{
			public string name;
			public string type;
			public string printer = Printer.DefaultPrinterName;
			public string url;
			public bool shared = false;
			public string[] labelNames;
			public string copies = "1";
			public string zoom = "1";
			public string orientation = "Portrait";
			public string options = "";
		}


		// ========== Constructor ==========
		public Label ()
		{
			// Contains a printer (full name) to use as well as a path and filename or url to the html to print, this is reletive to the templates directory as defined via the config.
		}


		// ========== Load Labels ==========
		/** Loads all labels from JSON data. This is normally called by Rule.LoadLabels(). **/
		public static void LoadLabels () {
			Program.Log ("Rules", "Loading rule labels...");
			Labels = new Dictionary<string, Label> ();

			// Get JSON:
			string jsonData = "";
			string labelsPath = Program.configGlobal.localLabelsPath + Program.configGlobal.localLabelsFilename;
			string labelsURL = Program.configGlobal.remoteLabelsPath;
			bool labelsLoaded = false;

			if (labelsURL != "") {
				WebClient web = new WebClient();
				try {
					Stream stream = web.OpenRead(labelsURL);
					using (StreamReader reader = new StreamReader(stream)) {
						jsonData = reader.ReadToEnd();
					}
					labelsLoaded = true;
				}
				catch (Exception e) {
					Program.LogWarning ("Rules", "Unable to connect to " + labelsURL);
					Program.LogException (e);
					Program.LogAlert ("Rules", "Searching for local labels instead...");
				}
			}
			else
				Program.Log ("Rules", "No remote labels url set, reading from local file...");

			if (!labelsLoaded) {
				if (File.Exists (labelsPath)) {
					jsonData = File.ReadAllText (labelsPath);
					labelsLoaded = true;
				} else {
					Program.LogWarning ("Rules", "Warning: No remote labels url set and no local file found at " + labelsPath + ".");
					Program.LogError ("Rules", "No labels loaded.");
					return;
				}
			}

			// Read From JSON:
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			try {
				LabelJSON[] labelJSONs = serializer.Deserialize<LabelJSON[]> (jsonData);
				foreach(LabelJSON labelJSON in labelJSONs) {
					Label label = new Label();
					label.Name = labelJSON.name;
					label.Type = labelJSON.type;
					label.PrinterProfile = labelJSON.printer;
					label.URL = labelJSON.url;
					label.Shared = labelJSON.shared;
					label.LabelNames = labelJSON.labelNames;
					label.Copies = labelJSON.copies;
					label.Zoom = labelJSON.zoom;
					label.Orientation = labelJSON.orientation;
					label.Options = labelJSON.options;
					Labels.Add (label.Name, label);
				}
			} catch (Exception e) {
				Program.LogError ("Rules", "Invalid labels JSON format:");
				Program.LogException (e);
				Program.LogError ("Rules", "No labels loaded.");
				return;
			}

			Program.LogSuccess ("Rules", Labels.Count + " labels(s) loaded.");
		}


		// ========== Get Label ==========
		/** Gets a label by name. **/
		public static Label GetLabel (string name = "")
		{
			if (name == "")
				return null;
			if (Labels.ContainsKey (name))
				return Labels[name];
			return null;
		}


		// ========== Is URL ==========
		/** Returns true if the html for this label is located via url rather than a file path. **/
		public bool IsURL()
		{
			return this.URL.StartsWith("http://") || this.URL.StartsWith("https://");
		}


		// ========== Get Template ==========
		/** Returns the label template html as a string. **/
		public string GetTemplate(Order order)
		{
			if (this.URL == null || this.URL == "")
				return null;

			// URL:
			if (this.IsURL())
			{
				WebClient web = new WebClient();
				web.Encoding = Encoding.UTF8;
				try
				{
					web.Headers[HttpRequestHeader.ContentType] = "application/json";
					web.Headers.Add("Accept-Charset", "utf-8");
					string json = "";
					if (order != null)
						json = order.GetJSON();
					string template = web.UploadString (new Uri(this.URL), "POST", json);
					if (!template.Contains("<base href=")) {
						string head = "<head>";
						if (template.Contains(head)) {
							int headIndex = template.IndexOf(head) + head.Length;
							template.Insert(headIndex, Environment.NewLine + "<base href=\"" + this.URL + "\" />");
						}
					}
					return template;
				}
				catch (Exception e)
				{
					Program.LogWarning("Labels", "Unable to connect to label url: " + this.URL);
					Program.LogException(e);
					return null;
				}
			}

			// Local:
			string templatePath = Program.configGlobal.localLabelsTemplatePath + this.URL;
			if (File.Exists(templatePath))
			{
				string template = File.ReadAllText(templatePath);
				if (!template.Contains("<base href="))
				{
					string head = "<head>";
					if (template.Contains(head))
					{
						int headIndex = template.IndexOf(head) + head.Length;
						template.Insert(headIndex, Environment.NewLine + "<base href=\"" + templatePath + "\" />");
					}
				}
				return template;
			}

			// Not Found:	
			Program.LogError("Labels", "Unable to locate the template for label: " + this.Name + " from: " + templatePath);
			return null;
		}


		// ========== Print Order ==========
		/**
		 * Prints this label as an order label, all order data will be inserted into the template using markers such as {OrderNumber} to insert order information.
		 * Markers match each variable that an order object has by exact name but in PascalCase such as order.shippingCost becoming {ShippingCost}.
		**/
		public void PrintOrder(Order order)
		{
			// Group Type:
			if (this.Type != null && this.Type.ToLower() == "group") {
				if (this.LabelNames == null || this.LabelNames.Length == 0) {
					Program.LogWarning("Labels", "The label group: " + this.Name + " has no labels that can be found in it, please ensure that the 'labelNames' key is present and populated with an array of names.");
					return;
				}
				foreach (string labelName in this.LabelNames) {
					if (!Labels.ContainsKey(labelName)) {
						Program.LogWarning("Labels", "The label: " + labelName + " in the group: " + this.Name + " could not be found.");
						return;
					}
					Label label = Labels[labelName];
					if (label == null)
						continue;
					label.PrintOrder (order);
				}
				return;
			}

			// Label Type:
			string filepath = this.PDF;

			if (!this.Shared || filepath == null || filepath == "" || !File.Exists(filepath))  {

				// Get Label Filepath:
				if (!this.Shared) {
					filepath = Program.configGlobal.archivePath + Path.DirectorySeparatorChar + order.Carrier.Name + Path.DirectorySeparatorChar + order.OrderNumber + "-" + this.Name + ".pdf";
					Program.LogAlert("Labels", "Generating " + this.Name + " label, please wait...");
				}
				else {
					filepath = Program.configGlobal.archivePath + Path.DirectorySeparatorChar + this.Name + ".pdf";
					this.PDF = filepath;
					Program.LogAlert("Labels", "Generating " + this.Name + " shared label, this only needs to be done once per session or reload, please wait...");
				}

				// Get Label Template:
				string template = this.GetTemplate(order);
				if (template == null || template == "") {
					Program.LogError("Labels", this.Name + " label template could not be found or is blank, unable to print.");
					return;
				}

				// Populate Label Template Order Data:
				template = this.PopulateOrderTemplate(template, order);
				
				// Convert Label To PDF:
				try {
					if (File.Exists (filepath))
						File.Delete (filepath);
					Program.htmlConverter.ConvertToPDF(template, filepath, this.Copies, this.Zoom, this.Orientation, this.Options);
				}
				catch (Exception e) {
					Program.LogError("Labels", this.Name + " failed to convert to PDF.");
					Program.LogException(e);
					return;
				}
			}

			// Print PDF:
			try {
				Program.printer.PrintPDF(filepath, this.PrinterProfile);
			}
			catch (Exception e) {
				Program.LogError("Labels", this.Name + " failed to print PDF.");
				Program.LogException(e);
				return;
			}

			/*/ Remove PDF:
			if (!this.Cache && File.Exists (filepath)) {
				File.Delete (filepath);
			}*/


			Program.LogSuccess("Labels", this.Name + " printed successfully.");
		}


		// ========== Populate Order Template ==========
		/** Returns a template with markers such as {OrderNumber} replaced with data from the provided order object. **/
		public string PopulateOrderTemplate(string template, Order order)
		{
			template = template.Replace ("{OrderNumber}", order.OrderNumber);
			template = template.Replace ("{OrderDate}", order.OrderDate);
			template = template.Replace ("{OrderWeight}", order.OrderWeight.ToString ());
			template = template.Replace ("{OrderCost}", order.OrderCost.ToString());
			template = template.Replace ("{ShippingCost}", order.ShippingCost.ToString());
			template = template.Replace ("{TaxAmount}", order.TaxAmount.ToString());
			template = template.Replace ("{ItemAmount}", order.ItemAmount.ToString());

			template = template.Replace ("{CustomerName}", order.CustomerName);
			template = template.Replace ("{CustomerPhone}", order.CustomerPhone);
			template = template.Replace ("{CustomerMobile}", order.CustomerMobile);
			template = template.Replace ("{CustomerEmail}", order.CustomerEmail);

			template = template.Replace ("{Postcode}", order.Postcode);
			template = template.Replace ("{Country}", order.Country);
			template = template.Replace ("{City}", order.City);
			template = template.Replace ("{Street}", order.Street);
			template = template.Replace ("{StreetExtra}", order.Locality);
			
			template = template.Replace("{OrderDescription}", order.OrderNumber);

			return template;
		}
	}
}

