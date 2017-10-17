﻿using System;
namespace UberDespatch
{
	public partial class ManualWindow : Gtk.Window
	{
		public Order Order;

		public ManualWindow(Order order) : base(Gtk.WindowType.Toplevel)
		{
			this.Order = order;
			this.Build();
		}

		protected void OnCancelButtonReleased(object sender, EventArgs e)
		{
			if (this.Order != null) {
				this.Order.Cancelled = true;
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