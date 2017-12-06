using System;
namespace UberDespatch
{
	public partial class ManualWindow : Gtk.Window
	{
		public Order Order;

		public ManualWindow(Order order) : base(Gtk.WindowType.Toplevel)
		{
			this.Modal = true;
			this.Parent = Program.mainWindow;
			this.Order = order;
			this.Build();
		}

		protected void OnCancelButtonReleased(object sender, EventArgs e)
		{
			if (this.Order != null) {
				this.Order.Cancelled = true;
				Program.Log ("Order", "Manually cancelled order, no despatch details will be sent.");
			}
			this.Destroy();
		}

		protected void OnConfirmButtonReleased(object sender, EventArgs e)
		{
			if (this.Order != null) {
				this.Order.Processed = true;
				this.Order.CarrierName = this.CarrierNameEntry.Text;
				this.Order.TrackingNumber = this.TrackingNumberEntry.Text;
			}
			this.Destroy();
		}
	}
}
