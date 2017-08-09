using System;
using System.Threading;

namespace UberDespatch
{
	public class CarrierManual : Carrier
	{

		// ========== Constructor ==========
		public CarrierManual()
		{
			this.Name = "Manual";
			this.Timeout = 0; // Infinite

			// Icon:
			string iconPath = Program.ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar;
			string iconDir = "svg" + System.IO.Path.DirectorySeparatorChar;
			string iconExtension = ".svg";
			try
			{
				this.Icon = new Gdk.Pixbuf(iconPath + iconDir + "edit" + iconExtension);
			}
			catch (Exception e)
			{
				iconDir = "png" + System.IO.Path.DirectorySeparatorChar;
				iconExtension = ".png";
				this.Icon = new Gdk.Pixbuf(iconPath + iconDir + "edit" + iconExtension);
			}

			// Create Carrier Directory:
			System.IO.Directory.CreateDirectory(Program.configGlobal.archivePath + System.IO.Path.DirectorySeparatorChar + this.Name);
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
			Program.Log (this.Name, "This order must be completed manually, waiting for details...");
			Gtk.Application.Invoke(delegate {
				ManualWindow manualWindow = new ManualWindow(order);
				manualWindow.Show();
			});
			while (!order.Cancelled && !order.Processed && !order.Error) {
				Thread.Sleep (1000);
			}
		}
	}
}
