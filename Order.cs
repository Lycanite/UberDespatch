using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Threading;

namespace UberDespatch
{
	public class Order
	{
		// Order File:
		public FileInfo FileInfo;

		// Order States:
		public bool Processed = false; // If true, this order has been successfully processed.
		public bool Cancelled = false; // If true, this order has been cancelled and it should stop at the next step if possible.
		public bool Error = false; // If true, an error has occured with this order and it should stop at the next step if possible.
		public bool Edit = false; // If true, this order will prompt the user to be edited (this is done before the validation step).
		public bool DontArchive = false; // If true, this order will not be archived when cancelled, errored or processed and will remain in the new orders directory.

		// Status:
		public string Channel = "";
		public bool OnHold = false;

		// Order:
		public string CarrierType = ""; // Set via WMS, this field is used to affect the behaviour of rules, for instance an order can be set to manual edit.
		public string OrderStatus = "";
		public string OrderNumber = "";
		public string OrderDate = "";
		public double OrderWeight = 0;
		public double OrderCost = 0;
		public double ShippingCost = 0;
		public double TaxAmount = 0;
		public double ItemAmount = 0;

		// Customer:
		public string CustomerName = "";
		public string CustomerPhone = "";
		public string CustomerMobile = "";
		public string CustomerEmail = "";

		// Address:
		public string Postcode = "";
		public string Country = ""; // Should be the Country Code
		public string CountryName = ""; // Should be the full Country Name
		public string City = "";
		public string Region = "";
		public string Street = "";
		public string Locality = "";
		public string Company = "";

		// Items:
		public OrderItem[] Items;

		// Tags - Keywords added to the order for special rules.
		public string[] Tags;
		
		// Alerts - A list of messages to display when the order is about to be processed, such as gift wrapping, can be added by order tags and by rules.
		public List<Alert> OrderAlerts = new List<Alert> ();
		public List<Alert> RuleAlerts = new List<Alert>();

		// Carrier - Populated by Rules:
		public Carrier Carrier;
		public string Service = "";
		public string Enhancement = "";
		public string Format = "";
		public Zone Zone;

		// Tracking - Populated by Carrier:
		public string TrackingNumber = "";

		// Labels - Populated by Rules:
		public Label[] Labels;

		// Translations:
		public Dictionary<string, string> Translations = new Dictionary<string, string> ();


		// ========== Constructor ==========
		public Order ()
		{
			// Order objects contain all data for a read order. WMS objects should be able to create orders from APIs or input files and then Carrier objects should be able to take an Order and process it.
		}


		// ========== Set Country ==========
		/** Takes either a Country Code or a Counry Name and sets the country variable to the corrosponding Counry Code. **/
		public void SetCountry (string countryEntry)
		{
			this.CountryName = countryEntry;

			// Valid Code:
			if (countryEntry.Length == 2) {
				this.Country = countryEntry;
				return;
			}

			// Hard Fixes:
			if(countryEntry.Contains ("America"))
				countryEntry = "United States";
			if (countryEntry.Contains("Great Britain"))
				countryEntry = "United Kingdom";
			if (countryEntry.Contains("Russian Federation"))
				countryEntry = "Russia";

			// Get Code From Name:
			var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
			RegionInfo regionInfo = regions.FirstOrDefault (region => region.EnglishName.Contains (countryEntry));
			if (regionInfo != null) {
				this.Country = regionInfo.TwoLetterISORegionName;
				Program.LogAlert("Order", "This order uses the country name: " + countryEntry + " instead of a country code and has been converted to: " + this.Country + " please ensure that this is correct.");
			}
			else
				Program.LogWarning("Order", "Warning, unable to get a valid region for: " + countryEntry + " you will need to inspect this order and manually add the 2 digit Country Code.");
		}


		// ========== Has Packing Station ==========
		/** Returns true if this order uses a packing station. **/
		public bool HasPackingStation () {
			if (this.Street.ToLower ().Contains ("packstation") || this.Street.ToLower ().Contains ("pack station") || this.Street.ToLower ().Contains ("packing station"))
				return true;
			if (this.Locality.ToLower ().Contains ("packstation") || this.Locality.ToLower ().Contains ("pack station") || this.Locality.ToLower ().Contains ("packing station"))
				return true;
			return false;
		}


		// ========== Order JSON ==========
		public class OrderJSON {
			// File Info:
			public string Filename = "";

			// Order States:
			public bool Processed = false; // If true, this order has been successfully processed.
			public bool Cancelled = false; // If true, this order has been cancelled and it should stop at the next step if possible.
			public bool Error = false; // If true, an error has occured with this order and it should stop at the next step if possible.
			public bool Edit = false; // If true, this order will prompt the user to be edited (this is done before the validation step).
			public bool DontArchive = false; // If true, this order will not be archived when cancelled, errored or processed and will remain in the new orders directory.

			// Status:
			public string Channel = "";
			public bool OnHold = false;

			// Order:
			public string CarrierType = ""; // Set via WMS, this field is used to affect the behaviour of rules, for instance an order can be set to manual edit.
			public string OrderStatus = "";
			public string OrderNumber = "";
			public string OrderDate = "";
			public double OrderWeight = 0;
			public double OrderCost = 0;
			public double ShippingCost = 0;
			public double TaxAmount = 0;
			public double ItemAmount = 0;

			// Customer:
			public string CustomerName = "";
			public string CustomerPhone = "";
			public string CustomerMobile = "";
			public string CustomerEmail = "";

			// Address:
			public string Postcode = "";
			public string Country = ""; // Should be the Country Code
			public string CountryName = ""; // Should be the full Country Name
			public string City = "";
			public string Region = "";
			public string Street = "";
			public string Locality = "";
			public string Company = "";

			// Items:
			public OrderItem[] Items;

			// Tags - Keywords added to the order for special rules.
			public string[] Tags;

			// Alerts
			public string[] Alerts;

			// Carrier - Populated by Rules:
			public string Carrier;
			public string Service = "";
			public string Enhancement = "";
			public string Format = "";
			public string Zone;

			// Tracking - Populated by Carrier:
			public string TrackingNumber = "";

			// Labels - Populated by Rules:
			public string[] Labels;

			// Translations:
			public Dictionary<string, string> Translations = new Dictionary<string, string>();
		}


		// ========== Order Item ==========
		public class OrderItem
		{
			public string Name;
			public string Description;
			public string Barcode;
			public double Weight;
			public double Price;
			public string Quantity;
		}


		// ========== Get JSON ==========
		/** Returns this order in JSON format. **/
		public string GetJSON () {
			OrderJSON orderJSON = new OrderJSON ();

			if (this.FileInfo != null)
				orderJSON.Filename = this.FileInfo.Name;

			orderJSON.Processed = this.Processed;
			orderJSON.Cancelled = this.Cancelled;
			orderJSON.Error = this.Error;
			orderJSON.Edit = this.Edit;
			orderJSON.DontArchive = this.DontArchive;

			orderJSON.Channel = this.Channel;
			orderJSON.OnHold = this.OnHold;

			orderJSON.CarrierType = this.CarrierType;
			orderJSON.OrderStatus = this.OrderStatus;
			orderJSON.OrderNumber = this.OrderNumber;
			orderJSON.OrderDate = this.OrderDate;
			orderJSON.OrderWeight = this.OrderWeight;
			orderJSON.OrderCost = this.OrderCost;
			orderJSON.ShippingCost = this.ShippingCost;
			orderJSON.TaxAmount = this.TaxAmount;
			orderJSON.ItemAmount = this.ItemAmount;

			orderJSON.CustomerName = this.CustomerName;
			orderJSON.CustomerPhone = this.CustomerPhone;
			orderJSON.CustomerMobile = this.CustomerMobile;
			orderJSON.CustomerEmail = this.CustomerEmail;

			orderJSON.Postcode = this.Postcode;
			orderJSON.Country = this.Country;
			orderJSON.CountryName = this.CountryName;
			orderJSON.City = this.City;
			orderJSON.Region = this.Region;
			orderJSON.Street = this.Street;
			if (this.Street.Length > 35 && string.IsNullOrEmpty (this.Locality)) {
				int breakPoint = 34;
				for (breakPoint = 34; breakPoint >= 0; breakPoint--) {
					if (this.Street [breakPoint] == ' ')
						break;
				}
				if (breakPoint > 0) {
					this.Locality = this.Street.Substring (breakPoint + 1);
					this.Street = this.Street.Substring (0, breakPoint);
				}
			}
			orderJSON.Locality = this.Locality;

			orderJSON.Items = this.Items;

			orderJSON.Tags = this.Tags;

			var alertNames = new List<string>();
			if (this.OrderAlerts != null)
			{
				foreach (Alert alert in this.OrderAlerts)
				{
					alertNames.Add(alert.Name);
				}
			}
			if (this.RuleAlerts != null)
			{
				foreach (Alert alert in this.OrderAlerts)
				{
					alertNames.Add(alert.Name);
				}
			}
			if(alertNames.Count > 0)
				orderJSON.Alerts = alertNames.ToArray();

			orderJSON.Carrier = this.Carrier != null ? this.Carrier.name : "";
			orderJSON.Service = this.Service;
			orderJSON.Enhancement = this.Enhancement;
			orderJSON.Format = this.Format;
			orderJSON.Zone = this.Zone != null ? this.Zone.name : "";

			orderJSON.TrackingNumber = this.TrackingNumber;

			if (this.Labels != null) {
				List<string> labelNames = new List<string>();
				foreach (Label label in this.Labels)
				{
					labelNames.Add(label.Name);
				}
				orderJSON.Labels = labelNames.ToArray ();
			}

			orderJSON.Translations = this.Translations;

			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Serialize (orderJSON);
		}


		// ========== To String ==========
		/** Returns a summary of this order as a string. **/
		public override string ToString()
		{
			if(this.Carrier != null)
				return "Order " + this.OrderNumber + " for " + this.CustomerName + " using " + this.Carrier.name + ".";
			return "Order " + this.OrderNumber + " for " + this.CustomerName + " with no carrier set yet.";
		}


		// ========== To String ==========
		/** Translates any translatable fields (address fields) if a translation service is set. **/
		public void Translate()
		{
			if (!Program.CanTranslate(this.Country) || this.Country.ToUpper () == Program.Country)
				return;
			List<string> untranslated = new List<string> ();
			untranslated.Add (this.CountryName);
			untranslated.Add (this.City);
			untranslated.Add (this.Region);
			untranslated.Add (this.Street);
			untranslated.Add (this.Locality);
			untranslated.Add (this.Company);
			this.Translations = Program.Translator.Translate (this.Country.ToUpper(), untranslated.ToArray ());
		}


		// ========== On Pre-Process ==========
		/** Called when this order is about to be processed (successfull or not), additional labels are printed here as well as alerts such as for Gift Wrapping. **/
		public void OnPreProcess()
		{
			this.PrintLabels();
			if (this.OrderAlerts != null) {
				foreach (Alert alert in this.OrderAlerts) {
					Gtk.Application.Invoke(delegate {
						AlertDialog alertDialog = new AlertDialog(alert.Message);
						alertDialog.Title = alert.Title;
						alertDialog.Show();
					});
				}
			}
			if (this.RuleAlerts != null)
			{
				foreach (Alert alert in this.RuleAlerts)
				{
					Gtk.Application.Invoke(delegate
					{
						AlertDialog alertDialog = new AlertDialog(alert.Message);
						alertDialog.Title = alert.Title;
						alertDialog.Show();
					});
				}
			}
		}


		// ========== On Post-Process ==========
		/** Called after this order has been successfully processed, additional labels are printed here as well as alerts such as for Gift Wrapping. **/
		public void OnPostProcess()
		{
			
		}


		// ========== Print Labels ==========
		/** Prints all labels for this order, a list of labels must be set to this order (typically done by a rule when it is applied to an order). **/
		public void PrintLabels()
		{
			// No Labels:
			if (this.Labels == null || this.Labels.Count () <= 0) {
				Program.Log("Labels", "UberDespatch has no additional labels to print for this order.");
				return;
			}

			Program.LogAlert("Labels", "Printing labels...");
			foreach (Label label in this.Labels) {
				if (label == null) {
					Program.LogError("Labels", "A label entry was null!");
					continue;
				}
				label.PrintOrder (this);
			}
		}
	}
}

