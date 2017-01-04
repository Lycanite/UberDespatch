using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;

namespace UberDespatch
{
	public class Rule
	{
		// Rules List:
		public static List<Rule> Rules;

		// Service:
		public string name = "";
		public Carrier carrier;
		public string service = "";
		public string enhancement = "";
		public string format = "";
		public bool edit = false; // If true, this rule set the order to edit where it will ask the user to check over the order, edit it if needed and then click send order to continue.

		// Query:
		public int priority = 0; // The priority of the rule. Lowest comes last. Ex: A 30 priority rule will be considered before a 10 priority rule.
		public Zone[] zones; // The Country Zone of the rule, see the Zone class.
		public string channel; // The sales channel of the order (which store).
		public string carrierType; // The type of carrier as set by the WMS, ignored if blank. For example: Large Letter.
		public double shippingCostMin = 0; // The minimum price amount.
		public double orderCostMin = 0; // The minimum price amount.
		public bool bothCostsRequired = false; // If true, an order must match both the shipping cost and the order cost, if false, it must match either or.
		public double weightMin = 0; // The minimum total order weight.
		public int itemCountMax = 0; // The minimum total item count.
		public string packStation = "allow"; // Are Pack Station addresses allowed, can be: allow (can be both), deny (no pack stations), only (must have a pack station).
		public string[] tags; // A list of special tags for orders to be filtered through the rules by.

		// Labels:
		public Label[] labels; // A list of labels that any order matching this rule should print out. The labels must be defined in a labels json or they will be ignored.

		// Alerts:
		public Alert[] alerts; // A list of alerts that this rule should display.


		// ========== Rule JSON ==========
		public class RuleJSON
		{
			public string name;
			public string carrier;
			public string service;
			public string enhancement;
			public string format;
			public bool edit;

			public int priority;
			public string[] zones;
			public string channel;
			public string carrierType;
			public double shippingCostMin;
			public double orderCostMin;
			public bool bothCostsRequired;
			public double weightMin;
			public int itemCountMax;
			public string packStation;

			public string[] tags;
			public string[] labels;
			public string[] alerts;
		}


		// ========== Constructor ==========
		public Rule ()
		{
			// Rule classes are used to match orders.
		}


		// ========== Load Rules ==========
		/** Loads all rules from either remote or local JSON data. This should be invoked in a seperate thread to keep the application responsive. This will also load Zones. **/
		public static void LoadRules () {
			Alert.LoadAlerts();
			Label.LoadLabels();
			Zone.LoadZones ();
			Program.Log ("Rules", "Loading rules...");
			Rules = new List<Rule> ();

			// Get JSON:
			string jsonData = "";
			string rulesPath = Program.configGlobal.localRulesPath + Program.configGlobal.localRulesFilename;
			string rulesURL = Program.configGlobal.remoteRulesPath;
			bool rulesLoaded = false;

			if (rulesURL != "") {
				WebClient web = new WebClient();
				try {
					Stream stream = web.OpenRead(rulesURL);
					using (StreamReader reader = new StreamReader(stream)) {
						jsonData = reader.ReadToEnd();
					}
					rulesLoaded = true;
				}
				catch (Exception e) {
					Program.LogWarning ("Rules", "Unable to connect to " + rulesURL);
					Program.LogException (e);
					Program.LogAlert("Rules", "Searching for local rules instead...");
				}
			}
			else
				Program.Log ("Rules", "No remote rules url, reading from local file...");

			if (!rulesLoaded) {
				if (File.Exists (rulesPath)) {
					jsonData = File.ReadAllText (rulesPath);
					rulesLoaded = true;
				} else {
					Program.LogWarning ("Rules", "Warning: No remote rules url set and no local rules file found at " + rulesPath + ".");
					Program.LogError ("Rules", "No rules loaded.");
					return;
				}
			}

			// Read From JSON:
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			try {
				RuleJSON[] ruleJSONs = serializer.Deserialize<RuleJSON[]> (jsonData);
				foreach(RuleJSON ruleJSON in ruleJSONs) {
					Rule rule = new Rule();
					rule.name = ruleJSON.name;

					rule.carrier = Carrier.GetCarrier (ruleJSON.carrier);
					rule.service = ruleJSON.service;
					rule.enhancement = ruleJSON.enhancement;
					rule.format = ruleJSON.format;
					rule.edit = ruleJSON.edit;

					rule.priority = ruleJSON.priority;

					if(ruleJSON.zones != null) {
						List<Zone>zones = new List<Zone>();
						foreach (string zoneName in ruleJSON.zones) {
							Zone zone = Zone.GetZone (zoneName);
							if(zone != null)
								zones.Add (zone);
						}
						rule.zones = zones.ToArray ();
					}

					rule.channel = ruleJSON.channel;
					rule.carrierType = ruleJSON.carrierType;
					rule.orderCostMin = ruleJSON.orderCostMin;
					rule.shippingCostMin = ruleJSON.shippingCostMin;
					rule.bothCostsRequired = ruleJSON.bothCostsRequired;
					rule.weightMin = ruleJSON.weightMin;
					rule.itemCountMax = ruleJSON.itemCountMax;
					rule.packStation = ruleJSON.packStation;
					rule.tags = ruleJSON.tags;

					if (ruleJSON.labels != null) {
						var labels = new List<Label>();
						foreach (string labelName in ruleJSON.labels) {
							Label label = Label.GetLabel(labelName);
							if (label != null)
								labels.Add(label);
						}
						rule.labels = labels.ToArray();
					}

					if (ruleJSON.alerts != null)
					{
						var alerts = new List<Alert> ();
						foreach (string alertName in ruleJSON.alerts) {
							Alert alert = Alert.GetAlert(alertName);
							if (alert != null)
								alerts.Add(alert);
						}
						rule.alerts = alerts.ToArray();
					}

					Rules.Add (rule);
				}
			} catch (Exception e) {
				Program.LogError ("Rules", "Invalid rules JSON format:");
				Program.LogException (e);
				Program.LogError ("Rules", "No rules loaded.");
				return;
			}

			// Sort:
			Rules.Sort (
				delegate(Rule x, Rule y) {
					return y.priority.CompareTo(x.priority);
				}
			);

			Program.LogSuccess ("Rules", Rules.Count + " rule(s) loaded.");
		}


		// ========== Rules Available ==========
		/** Returns true if there is at least one valid rule. If there are no rules, orders cannot be processed. **/
		public static bool RulesAvailable () {
			if (Rules == null)
				return false;
			return Rules.Count > 0;
		}


		// ========== Get Rule ==========
		/** Gets a rule for the provided order and applies the rule to it (setting the orders carrier details). **/
		public static Rule GetRule (Order order) {
			if (Rules == null)
				return null;

			foreach (Rule rule in Rules) {
				if(rule.OrderMatch (order)) {
					return rule;
				}
			}

			return null;
		}


		// ========== Order Match ==========
		/** Returns true if the provided order matches this rule. **/
		public bool OrderMatch (Order order) {
			// Check Zone:
			bool zoneMatch = false;
			if (this.zones != null && this.zones.Length > 0) {
				foreach (Zone zone in this.zones) {
					if (zone.OrderMatch (order)) {
						zoneMatch = true;
						order.Zone = zone;
						break;
					}
				}
			} else
				zoneMatch = true;
			if (!zoneMatch)
				return false;

			// Check Pack Station:
			bool hasPackingStation = order.HasPackingStation ();
			if (this.packStation == "deny" && hasPackingStation) {
				return false;
			}
			if (this.packStation == "only" && !hasPackingStation) {
				return false;
			}

			// Check Channel and Carrier:
			if (this.channel != "" && !order.Channel.Contains(this.channel))
				return false;
			if (this.carrierType != "" && order.CarrierType != this.carrierType)
				return false;

			// Check Cost:
			if (this.bothCostsRequired) {
				if (order.ShippingCost < this.shippingCostMin || order.OrderCost < this.orderCostMin)
					return false;
			} else {
				if (this.shippingCostMin > 0) {
					if (order.ShippingCost < this.shippingCostMin) {
						if (this.orderCostMin > 0) {
							if (order.OrderCost < this.orderCostMin)
								return false;
						} else
							return false;
					}
				} else {
					if (order.OrderCost < this.orderCostMin)
						return false;
				}
			}

			// Check Weight and Size:
			if (order.OrderWeight < this.weightMin)
				return false;
			if (this.itemCountMax > 0 && order.ItemAmount > this.itemCountMax)
				return false;

			// Check Tags:
			bool ruleTags = this.tags != null && this.tags.Length > 0;
			bool orderTags = order.Tags != null && order.Tags.Length > 0;
			if (ruleTags) {
				if (!orderTags)
					return false;
				bool tagMatch = false;
				foreach (string ruleTag in this.tags) {
					foreach (string orderTag in order.Tags) {
						if (ruleTag.ToLower () == orderTag.ToLower ()) {
							tagMatch = true;
							break;
						}
					}
					if (tagMatch)
						break;
				}
				if (!tagMatch)
					return false;
			}

			return true;
		}


		// ========== Apply To Order ==========
		/** Applies this rule to the provided order setting fields such as carrier or labels. **/
		public void ApplyToOrder (Order order) {
			order.Carrier = this.carrier;
			order.Service = this.service;
			order.Enhancement = this.enhancement;
			order.Format = this.format;
			if(this.edit) // Only set an order edit to true, never set to false by rule.
				order.Edit = this.edit;
			// Order Zone is set in OrderMatch ().
			order.Labels = this.labels;
			order.RuleAlerts = new List<Alert> ();
			if (this.alerts != null) {
				foreach (Alert alert in this.alerts) {
					if (alert != null)
						order.RuleAlerts.Add(alert);
				}
			}
		}
	}
}

