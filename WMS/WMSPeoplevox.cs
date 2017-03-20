using System;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace UberDespatch
{
	public class WMSPeoplevox : WMS
	{
		public ConfigWMSPeoplevox config; // Main config, should be integrated using the SetConfig and GetConfig methods.
		protected PvxApi.IntegrationServiceV4 serviceProxy; // Peoplevox API proxy class.


		// ========== Config Class ==========
		public class ConfigWMSPeoplevox : ConfigBase<ConfigWMSPeoplevox>
		{
			public string url = "http://peoplevox.net/{clientID}/resources/integrationservicev4.asmx";
			public string clientID = "";
			public string username = "";
			public string password = "";
			public bool debug = false;

			public static ConfigWMSPeoplevox LoadFile() {
				return Load ("config" + System.IO.Path.DirectorySeparatorChar + "wmsPeoplevox.json");
			}

			public void SaveFile() {
				Save ("config" + System.IO.Path.DirectorySeparatorChar + "wmsPeoplevox.json");
			}
		}


		// ========== Constructor ==========
		public WMSPeoplevox ()
		{
			// Peoplevox Warehouse Management System
			this.name = "Peoplevox";
			this.config = ConfigWMSPeoplevox.LoadFile ();
			this.FileFilters = new string[] {"*.pvx", "*.xml"};
		}


		// ========== Config ==========
		/** Use to set a config value via key. This should be overriden and handled as neccesary. **/
		public override void SetConfigValue (string key, string value) {
			if (key == "url")
				this.config.url = value;
			else if (key == "clientID")
				this.config.clientID = value;
			else if (key == "username")
				this.config.username = value;
			else if (key == "password")
				this.config.password = value;
		}

		/** Use to get a config value via key. This should be overriden and handled as neccesar.y **/
		public override string GetConfigValue (string key) {
			if (key == "url")
				return this.config.url;
			else if (key == "clientID")
				return this.config.clientID;
			else if (key == "username")
				return this.config.username;
			else if (key == "password")
				return this.config.password;
			return "";
		}

		/** Saves the config. **/
		public override void SaveConfig() {
			this.config.SaveFile ();
		}


		// ========== Main Update ==========
		/** The main update lgoic of this WMS, called by MainLoop(). **/
		public override void MainUpdate () {
			// Optional looping logic goes here.
		}


		// ========== Connect ==========
		/** Connects to the WMS API, will refresh the connection if already connected. Returns true for success and false for failure. Calls UpdateState() for UI feedback. **/
		public override bool Connect ()
		{
			this.UpdateState (1);
			Program.Log (this.name, "Connecting to " + this.name + "...");
			this.serviceProxy = this.GetServiceProxy ();

			// Failure:
			if (this.serviceProxy == null) {
				this.UpdateState (0);
				Program.LogError (this.name, "Unable to connect to " + this.name + ":");
				Program.LogException (this.lastError);
				return false;
			}

			// Success:
			this.UpdateState (2);
			Program.LogSuccess (this.name, "Connected to " + this.name + " successfully.");
			return true;
		}


		// ========== Get Service Proxy ==========
		/** Connects to Peoplevox returning and return a Service proxy if successful or null if failed populating the lastError string of this class.. **/
		public PvxApi.IntegrationServiceV4 GetServiceProxy ()
		{
			PvxApi.IntegrationServiceV4 serviceProxy = new PvxApi.IntegrationServiceV4();
			try 
			{	        
				// Connect to PVX API
				serviceProxy.Url = this.GetConfigValue ("url").Replace ("{clientID}", this.GetConfigValue ("clientID"));
				var response = serviceProxy.Authenticate(
					this.config.clientID,
					this.config.username,
					Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(this.config.password)));

				// Set SessionId
				if (response.ResponseId != 0)
				{
					serviceProxy = null;
					this.lastError = response.Detail;
				}
				else
				{
					// Split the response and get ClientId and Session string
					var csvValues = response.Detail.Split(",".ToCharArray());
					serviceProxy.UserSessionCredentialsValue = new PvxApi.UserSessionCredentials()
					{
						ClientId = csvValues[0], //Client Id
						SessionId = csvValues[1] //Session
					};
				}
			}
			catch (Exception e)
			{
				serviceProxy = null;
				this.lastError = e.ToString ();
			}
			return serviceProxy;
		}


		// ========== Is Connected ==========
		/** Returns if UberDespatch is currently connected to the WMS and updates the state to reflect it via the GUI. **/
		public override bool IsConnected () {
			if (this.serviceProxy == null) {
				this.UpdateState (0);
				return false;
			} else {
				this.UpdateState (2);
				return true;
			}
		}


		// ========== Get File Search ==========
		// Returns a file search syntax such as '*.xml'.
		public override String GetFileSearch () {
			return "*.pvx";
		}


		// ========== Create Order ==========
		/** Returns an Order object created from the provided file. Returns null if the file is invalid. **/
		public override Order CreateOrder (FileInfo orderFile) {
			Order order = new Order ();
			order.FileInfo = orderFile;
			XDocument xmlDoc = new XDocument ();
			try {
				xmlDoc = XDocument.Parse (File.ReadAllText(orderFile.FullName));
			}
			catch (Exception e) {
				Program.LogError (this.name, "The order is not a valid pvx file:");
				Program.LogException (e);
				return null;
			}

			XElement despatch = xmlDoc.Element ("Despatch");
			//XElement packages = despatch.Element ("Packages");
			//XElement despatchPackage = packages.Element ("DespatchPackage");

			order.Channel = despatch.Element ("ChannelName").Value;
			try {
				order.OnHold = despatch.Element ("OnHold").Value == "True";
			}
			catch (Exception e) {
				order.OnHold = false;
				Program.LogWarning(this.name, "The OnHold property is missing from the order data.");
			}

			try {
				order.CarrierType = despatch.Element ("CarrierName").Value;
			}
			catch (Exception e) {
				order.OnHold = false;
				Program.LogWarning(this.name, "The CarrierName property is missing from the order data.");
			}

			order.OrderStatus = despatch.Element ("OrderStatus").Value;
			order.OrderNumber = despatch.Element ("SalesOrderNumber").Value;
			order.OrderDate = despatch.Element ("CreatedDate").Value;
			order.OrderWeight = 1; // TODO Loop through all items and add the weights together.


			// Costs:
			try {
				order.OrderCost = Convert.ToDouble(Regex.Replace(despatch.Element("TotalSale").Value, "[^0-9.]", ""));
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a bad number format for TotalSale: " + despatch.Element("TotalSale").Value + " and will be set to 0.");
			}

			try {
				order.ShippingCost = Convert.ToDouble(Regex.Replace(despatch.Element ("ShippingCost").Value, "[^0-9.]", ""));
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a bad number format for ShippingCost: " + despatch.Element("ShippingCost").Value + " and will be set to 0.");
			}

			try {
				order.TaxAmount = Convert.ToDouble(Regex.Replace(despatch.Element ("TaxPaid").Value, "[^0-9.]", ""));
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a bad number format for TaxPaid: " + despatch.Element("TaxPaid").Value + " and will be set to 0.");
			}

			try {
				order.ItemAmount = Convert.ToDouble(Regex.Replace(despatch.Element ("NumberOfPackages").Value, "[^0-9.]", ""));
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a bad number format for NumberOfPackages: " + despatch.Element("NumberOfPackages").Value + " and will be set to 0.");
			}


			// Custom Contact:
			try {
				order.CustomerName = despatch.Element("CustomerName").Value;
				//order.CustomerName = despatchPackage.Element("CustomerName").Value;
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a missing an Customer Name. This will need to be manually inserted. The Despatch Package Customer Name is used.");
			}

			bool badPhone = false;
			try {
				order.CustomerPhone = despatch.Element("CustomerPhone").Value;
				order.CustomerMobile = despatch.Element("CustomerPhone").Value; // Same as Phone
			}
			catch (Exception e) {
				badPhone = true;
			}
			if (order.CustomerPhone == null || order.CustomerPhone == "")
				badPhone = true;
			if (badPhone) {
				Program.LogWarning(this.name, "The order has a missing phone number. 00000000000 will be used.");
				order.CustomerPhone = "00000000000";
				order.CustomerMobile = "00000000000";
			}

			try {
				order.CustomerEmail = despatch.Element ("CustomerEmail").Value;
			}
			catch (Exception e)
			{
				Program.LogWarning(this.name, "The order has a missing an Email. An empty entry will be used.");
				order.CustomerEmail = "";
			}


			// Address Details:
			try {
				order.Postcode = despatch.Element("ShippingAddressPostCode").Value;
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a missing a Postcode. An empty entry will be used.");
				order.Postcode = "";
			}

			try {
				order.SetCountry(despatch.Element("ShippingAddressCountry").Value);
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a missing a Country. An empty entry will be used.");
			}

			try {
				order.City = despatch.Element ("ShippingAddressTownCity").Value;
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a missing a City. An empty entry will be used.");
				order.City = "";
			}

			try
			{
				order.Region = despatch.Element("ShippingAddressRegion").Value;
			}
			catch (Exception e)
			{
				Program.LogWarning(this.name, "The order has a missing a Region. An empty entry will be used.");
				order.Region = "";
			}

			try {
				order.Street = despatch.Element ("ShippingAddressLine1").Value;
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a missing street line 1. An empty entry will be used.");
				order.Street = "";
			}

			try {
				order.Locality = despatch.Element("ShippingAddressLine2").Value;
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "The order has a missing street line 2. An empty entry will be used.");
				order.Locality = "";
			}

			try {
				order.Company = despatch.Element ("Attribute1").Value; // Was ShippingAddressReference
			}
			catch (Exception e) {
				Program.LogWarning(this.name, "Custom Peoplevox entry: Attribute1 has not be provided by the order, this should be the Company Name or an empty string.");
				order.Company = "";
			}


			// Tags:
			try {
				string tagsValue = despatch.Element("Attribute2").Value;
				if (tagsValue != null && tagsValue.Trim() != "") {
					order.Tags = tagsValue.Trim().Replace(", ", ",").Split(',');
				}
				foreach (string tag in order.Tags) {
					Alert alert = Alert.GetAlert(tag);
					if (alert != null)
						order.OrderAlerts.Add(alert);
				}
			}
			catch (Exception e) {
				order.Tags = new string[0];
			}


			// Items:
			XElement items = despatch.Element("Items");
			List<Order.OrderItem> orderItems = new List<Order.OrderItem> ();
			foreach (XElement item in items.Elements("Item")) {
				try {
					Order.OrderItem orderItem = new Order.OrderItem();
					orderItem.Name = item.Element("Name").Value;
					orderItem.Description = item.Element("Description").Value;
					orderItem.Barcode = item.Element("Barcode").Value;
					orderItem.Weight = Convert.ToDouble(Regex.Replace(item.Element("Weight").Value, "[^0-9.]", ""));
					orderItem.Price = Convert.ToDouble(Regex.Replace(item.Element("SalePrice").Value, "[^0-9.]", ""));
					orderItem.Quantity = item.Element("QuantityOrdered").Value;
					orderItems.Add (orderItem);
				}
				catch (Exception e) {
					Program.LogWarning(this.name, "Unable to add item to order, labels may be missing information.");
					Program.LogException(e);
				}
			}
			order.Items = orderItems.ToArray ();

			order.Translate();
			return order;
		}


		// ========== Update Order ==========
		// Updates the WMS with the provided order changes, this is primarily used for sending back tracking information.
		public override void UpdateOrder(Order order) {

			// Check For Invalid Tracking Numbers:
			if (order.TrackingNumber == null || order.TrackingNumber == "") {
				Program.LogWarning (this.name, "The order does not have a tracking number set, this will not be sent.");
				return;
			}
			if (order.TrackingNumber.Substring (0, 3) == "AAA" && Program.configGlobal.ignoreReferences) {
				Program.LogAlert (this.name, "The order only has a reference number rather than a tracking number, this will not be sent.");
				return;
			}

			// Send Tracking Number
			Program.Log (this.name, "Sending tracking informations...");
			var saveRequest = new PvxApi.SaveRequest();
			string input = "SalesOrderNumber,DespatchTrackingNumber,CarrierName\n" + order.OrderNumber + "," + order.TrackingNumber + "," + order.CarrierType;
			if (this.config.debug) {
				Program.Log(this.name, "Tracking Input: " + input);
			}
			saveRequest.CsvData = "\n" + input;
			saveRequest.TemplateName = "Sales orders";

			bool connected = this.IsConnected ();
			if (!connected)
				connected = this.Connect ();

			if (connected) {
				var responce = serviceProxy.SaveData(saveRequest);
				if (responce.ResponseId == 0) {
					Program.LogSuccess(this.name, "Tracking information was sent successfully.");
					if (this.config.debug) {
						try {
							XmlSerializer x = new XmlSerializer(responce.GetType());
							StringWriter writer = new StringWriter();
							x.Serialize(writer, responce);
							Program.LogException(writer.ToString());
						}
						catch (Exception e) { }
					}
				}
				else {
					Program.LogError (this.name, "Tracking information failed to send.");
					Program.LogException(responce.Detail);
				}
			} else {
				Program.LogError (this.name, "Unable to connect, tracking information was not sent.");
			}
		}
	}
}

