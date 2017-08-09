using System;
using System.IO;

namespace UberDespatch
{
	public class CarrierPOSTGroup : CarrierGroup
	{
		public ConfigCarrierPOST Config;

		// ========== Constructor ==========
		public CarrierPOSTGroup () : base ("POST") {}


		// ========== Config Class ==========
		public class ConfigCarrierPOST : ConfigBase<ConfigCarrierPOST>
		{
			public CarrierEntry[] Carriers = new CarrierEntry[0];

			public static ConfigCarrierPOST LoadFile(string filename) {
				return Load ("config" + Path.DirectorySeparatorChar + "carrier" + filename + ".json");
			}

			public void SaveFile(string filename) {
				Save ("config" + Path.DirectorySeparatorChar + "carrier" + filename + ".json");
			}
		}


		// ========== Carrier Entry Class ==========
		public class CarrierEntry
		{
			public string Name = "POST Carrier";
			public string URL = "http://domain/api/servicename/shipping";
			public string PrinterProfile = "Default";
			public string AdditionalPOST = "";
		}


		// ========== Load Carriers ==========
		public override void LoadCarriers ()
		{
			this.Config = ConfigCarrierPOST.LoadFile(this.Name);
			foreach (CarrierEntry carrierEntry in this.Config.Carriers) {
				CarrierPOST hiveCarrier = new CarrierPOST (carrierEntry.Name, "");
				hiveCarrier.Group = this;
				hiveCarrier.Name = carrierEntry.Name;
				hiveCarrier.Description = "";
				hiveCarrier.CarrierEntry = carrierEntry;
				this.Carriers.Add (hiveCarrier.Name, hiveCarrier);
			}
		}


		// ========== Save Carriers ==========
		public override void SaveCarriers ()
		{
			this.Config.SaveFile (this.Name);
		}


		// ========== Create Carrier ==========
		public override Carrier CreateCarrier (string name, string url, string printerProfile, string additionalPOST)
		{
			CarrierEntry carrierEntry = new CarrierEntry ();
			carrierEntry.Name = name;
			carrierEntry.URL = url;
			carrierEntry.PrinterProfile = printerProfile;
			carrierEntry.AdditionalPOST = additionalPOST;

			CarrierPOST hiveCarrier = new CarrierPOST (name, "");
			hiveCarrier.Group = this;
			hiveCarrier.Name = carrierEntry.Name;
			hiveCarrier.Description = "";
			hiveCarrier.CarrierEntry = carrierEntry;

			this.Carriers.Add (hiveCarrier.Name, hiveCarrier);
			this.UpdateCarrierEntries ();

			return hiveCarrier;
		}


		// ========== Remove Carrier ==========
		public override void RemoveCarrier (Carrier carrier)
		{
			this.Carriers.Remove (carrier.Name);
			this.UpdateCarrierEntries ();
		}


		// ========== Get Carrier ==========
		public override Carrier GetCarrier (string name)
		{
			if (this.Carriers.ContainsKey (name))
				return this.Carriers [name];
			return null;
		}


		// ========== Rename Carrier ==========
		public override void RenameCarrier (string currentName, string newName)
		{
			if (!this.Carriers.ContainsKey (currentName)) {
				Program.LogWarning (this.Name, "Tried to rename a carrier that doesn't exist in this group.");
				return;
			}
			Carrier carrier = this.Carriers[currentName];
			this.Carriers.Remove (currentName);
			carrier.Name = newName;
			this.Carriers.Add (newName, carrier);
			this.UpdateCarrierEntries ();
		}


		// ========== Update Carrier Entries ==========
		public void UpdateCarrierEntries ()
		{
			this.Config.Carriers = new CarrierEntry[this.Carriers.Count];
			int i = 0;
			foreach (CarrierPOST carrier in this.Carriers.Values) {
				this.Config.Carriers [i] = carrier.CarrierEntry;
				i++;
			}
		}


		// ========== Menu Action Activated ==========
		public override void OpenSettingsWindow () {
			CarrierPOSTWindow settingsWindow = new CarrierPOSTWindow (this);
			settingsWindow.Show ();
		}
	}
}

