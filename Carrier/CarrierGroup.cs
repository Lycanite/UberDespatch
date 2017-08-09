using System;
using System.Collections.Generic;

namespace UberDespatch
{
	public abstract class CarrierGroup
	{
		public Dictionary<string, Carrier> Carriers = new Dictionary<string, Carrier> ();
		public string Name;

		// ========== Constructor ==========
		public CarrierGroup (string name)
		{
			this.Name = name;
			this.LoadCarriers ();
		}


		// ========== Load Carriers ==========
		public abstract void LoadCarriers ();


		// ========== Save Carriers ==========
		public abstract void SaveCarriers ();


		// ========== Create Carrier ==========
		public abstract Carrier CreateCarrier (string name, string url, string printerProfile, string additionalPOST);


		// ========== Remove Carrier ==========
		public abstract void RemoveCarrier (Carrier carrier);


		// ========== Get Carrier ==========
		public abstract Carrier GetCarrier (string name);


		// ========== Rename Carrier ==========
		public abstract void RenameCarrier (string currentName, string newName);


		// ========== Open Settings Window ==========
		public abstract void OpenSettingsWindow ();
	}
}

