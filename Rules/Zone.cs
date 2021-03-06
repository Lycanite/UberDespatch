﻿using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace UberDespatch
{
	public class Zone
	{
		// Zones List:
		public static Dictionary<string, Zone> Zones = new Dictionary<string, Zone> ();

		public string name = "";
		public string[] countries;
		public string[] postcodes;
		public bool zipcode; // If true, the postcodes array is to be treated as a zipcode array instead.
		public bool blacklist = false; // If true, the list of postcodes will instead by used as postcodes that are not allowed in the zone.


		// ========== Zone JSON ==========
		public class ZoneJSON
		{
			public string name;
			public string[] countries;
			public string[] postcodes;
			public bool zipcode;
			public bool blacklist = false;
		}


		// ========== Constructor ==========
		public Zone ()
		{
			// Contains a list of country codes and postcodes (or zipcodes) used by Rules. Can be edited via the Zone Editor.
		}


		// ========== Load Zones ==========
		/** Loads all zones from JSON data. This is normally called by Rule.LoadRules. **/
		public static void LoadZones () {
			Program.Log ("Rules", "Loading rule zones...");
			Zones = new Dictionary<string, Zone> ();

			// Get JSON:
			string jsonData = "";
			string zonesPath = Program.configGlobal.localZonesPath + Program.configGlobal.localZonesFilename;
			string zonesURL = Program.configGlobal.remoteZonesPath;
			bool zonesLoaded = false;

			if (zonesURL != "") {
				WebClient web = new WebClient();
				try {
					Stream stream = web.OpenRead(zonesURL);
					using (StreamReader reader = new StreamReader(stream)) {
						jsonData = reader.ReadToEnd();
					}
					zonesLoaded = true;
				}
				catch (Exception e) {
					Program.LogWarning ("Rules", "Unable to connect to " + zonesURL);
					Program.LogException (e);
					Program.LogAlert ("Rules", "Searching for local zones instead...");
				}
			}
			else
				Program.Log ("Rules", "No remote zones url set, reading from local file...");

			if (!zonesLoaded) {
				if (File.Exists (zonesPath)) {
					jsonData = File.ReadAllText (zonesPath);
					zonesLoaded = true;
				} else {
					Program.LogWarning ("Rules", "Warning: No remote zones url set and no local file found at " + zonesPath + ".");
					Program.LogError ("Rules", "No zones loaded.");
					return;
				}
			}

			// Read From JSON:
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			try {
				ZoneJSON[] zoneJSONs = serializer.Deserialize<ZoneJSON[]> (jsonData);
				foreach(ZoneJSON zoneJSON in zoneJSONs) {
					Zone zone = new Zone();
					zone.name = zoneJSON.name;

					zone.countries = zoneJSON.countries;
					zone.postcodes = zoneJSON.postcodes;
					zone.zipcode = zoneJSON.zipcode;
					zone.blacklist = zoneJSON.blacklist;

					Zones.Add (zone.name, zone);
				}
			} catch (Exception e) {
				Program.LogError ("Rules", "Invalid zones JSON format:");
				Program.LogException (e);
				Program.LogError ("Rules", "No zones loaded.");
				return;
			}

			Program.LogSuccess ("Rules", Zones.Count + " zone(s) loaded.");
		}


		// ========== Get Zone ==========
		/** Gets a zone by name. **/
		public static Zone GetZone (string zoneName = "")
		{
			if (zoneName == "")
				return null;
			if (Zones.ContainsKey (zoneName))
				return Zones[zoneName];
			return null;
		}


		// ========== Order Match ==========
		/** Returns true if the provided order is within this zone. **/
		public bool OrderMatch (Order order) {
			if (!this.CountryMatch (order.Country))
				return false;
			if (!this.PostcodeMatch (order.Postcode))
				return false;
			return true;
		}


		// ========== Country Match ==========
		/** Returns true if the provided country is allowed by this zone. **/
		public bool CountryMatch (string testCountry) {
			if (this.countries == null || this.countries.Length == 0)
				return true;
			foreach (string country in this.countries) {
				if (testCountry == country || country == "")
					return true;
			}
			return false;
		}


		// ========== Postcode Match ==========
		/** Returns true if the provided postcode is allowed by this zone. Only the outward code is checked. **/
		public bool PostcodeMatch (string testPostcode) {
			if (this.postcodes == null || this.postcodes.Length == 0 || this.zipcode) {
				return !this.blacklist;
			}
			// TODO Check for zipcodes.
			testPostcode = testPostcode.Replace (" ", "");
			if (testPostcode.Length < 5) {
				Program.Log("Zone", "The order postcode " + testPostcode + " is not a valid postcode as it isn't long enough or is a zipcode which is not yet supported.");
				return false;
			}
			string outwardCode = testPostcode.Substring(0, testPostcode.Length - 3).Trim().ToUpper();
			//Program.Log ("Zone", this.name + " Testing postcode: " + testPostcode + " Outward code is: " + outwardCode);
			foreach (string postcode in this.postcodes) {
				if (postcode.Contains ("*")) {
					for (int i = 0; i <= 200; i++) {
						if (outwardCode == postcode.Replace ('*', Convert.ToChar("" + i))) {
							return !this.blacklist;
						}
					}
				}
				else {
					if (outwardCode == postcode || postcode == "") {
						return !this.blacklist;
					}
				}
			}
			return this.blacklist;
		}
	}
}

