using System;
namespace UberDespatch
{
	public class CarrierManual : Carrier
	{

		// ========== Constructor ==========
		public CarrierManual()
		{
			this.name = "Manual";

			// Icon:
			string iconPath = Program.ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar;
			string iconDir = "svg" + System.IO.Path.DirectorySeparatorChar;
			string iconExtension = ".svg";
			try
			{
				this.icon = new Gdk.Pixbuf(iconPath + iconDir + "edit" + iconExtension);
			}
			catch (Exception e)
			{
				iconDir = "png" + System.IO.Path.DirectorySeparatorChar;
				iconExtension = ".png";
				this.icon = new Gdk.Pixbuf(iconPath + iconDir + "edit" + iconExtension);
			}

			// Create Carrier Directory:
			System.IO.Directory.CreateDirectory(Program.configGlobal.archivePath + System.IO.Path.DirectorySeparatorChar + this.name);
		}


		// ========== Validate ==========
		/** Returns true if the order is valid for processing, if the order is invalid (missing some essential fields) then false is returned where the OrderChecker will wait for the user to either edit the order and try again or skip the order and send it straight to the archive. **/
		public override bool ValidateOrder(Order order)
		{
			return true;
		}


		// ========== Send To Carrier ==========
		// Sends an Order object to the carrier service. This is invoked on a new thread while the main thread waits until WaitForCarrier() returns true.
		public override void SendToCarrier(Order order)
		{
			Program.Log (this.name, "This order must be completed manually.");
			order.Processed = true;
		}
	}
}
