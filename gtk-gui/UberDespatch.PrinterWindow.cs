
// This file has been generated by the GUI designer. Do not modify.
namespace UberDespatch
{
	public partial class PrinterWindow
	{
		private global::Gtk.VBox MainLayout;

		private global::Gtk.Frame PrinterSelectionFrame;

		private global::Gtk.Alignment PrinterSelectionAlignment;

		private global::Gtk.HBox PrinterSelectionLayout;

		private global::Gtk.Label PrinterSelectionLabel;

		private global::Gtk.ComboBox PrinterSelectionCombo;

		private global::Gtk.Label PrinterLabel;

		private global::Gtk.HBox MainControlsLayout;

		private global::Gtk.Alignment MainControlsSpacerAlignment;

		private global::Gtk.Button CancelButton;

		private global::Gtk.Button ConfirmButton;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget UberDespatch.PrinterWindow
			this.Name = "UberDespatch.PrinterWindow";
			this.Title = global::Mono.Unix.Catalog.GetString("PrinterWindow");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.BorderWidth = ((uint)(8));
			this.Resizable = false;
			this.DefaultWidth = 400;
			// Container child UberDespatch.PrinterWindow.Gtk.Container+ContainerChild
			this.MainLayout = new global::Gtk.VBox();
			this.MainLayout.Name = "MainLayout";
			this.MainLayout.Spacing = 6;
			// Container child MainLayout.Gtk.Box+BoxChild
			this.PrinterSelectionFrame = new global::Gtk.Frame();
			this.PrinterSelectionFrame.Name = "PrinterSelectionFrame";
			this.PrinterSelectionFrame.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child PrinterSelectionFrame.Gtk.Container+ContainerChild
			this.PrinterSelectionAlignment = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.PrinterSelectionAlignment.Name = "PrinterSelectionAlignment";
			this.PrinterSelectionAlignment.LeftPadding = ((uint)(12));
			// Container child PrinterSelectionAlignment.Gtk.Container+ContainerChild
			this.PrinterSelectionLayout = new global::Gtk.HBox();
			this.PrinterSelectionLayout.Name = "PrinterSelectionLayout";
			this.PrinterSelectionLayout.Spacing = 6;
			// Container child PrinterSelectionLayout.Gtk.Box+BoxChild
			this.PrinterSelectionLabel = new global::Gtk.Label();
			this.PrinterSelectionLabel.WidthRequest = 200;
			this.PrinterSelectionLabel.Name = "PrinterSelectionLabel";
			this.PrinterSelectionLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Printer");
			this.PrinterSelectionLayout.Add(this.PrinterSelectionLabel);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.PrinterSelectionLayout[this.PrinterSelectionLabel]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child PrinterSelectionLayout.Gtk.Box+BoxChild
			this.PrinterSelectionCombo = global::Gtk.ComboBox.NewText();
			this.PrinterSelectionCombo.Name = "PrinterSelectionCombo";
			this.PrinterSelectionLayout.Add(this.PrinterSelectionCombo);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.PrinterSelectionLayout[this.PrinterSelectionCombo]));
			w2.Position = 1;
			this.PrinterSelectionAlignment.Add(this.PrinterSelectionLayout);
			this.PrinterSelectionFrame.Add(this.PrinterSelectionAlignment);
			this.PrinterLabel = new global::Gtk.Label();
			this.PrinterLabel.Name = "PrinterLabel";
			this.PrinterLabel.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Printer Selection</b>");
			this.PrinterLabel.UseMarkup = true;
			this.PrinterSelectionFrame.LabelWidget = this.PrinterLabel;
			this.MainLayout.Add(this.PrinterSelectionFrame);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.MainLayout[this.PrinterSelectionFrame]));
			w5.Position = 0;
			w5.Expand = false;
			w5.Fill = false;
			// Container child MainLayout.Gtk.Box+BoxChild
			this.MainControlsLayout = new global::Gtk.HBox();
			this.MainControlsLayout.Name = "MainControlsLayout";
			this.MainControlsLayout.Spacing = 6;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.MainControlsSpacerAlignment = new global::Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
			this.MainControlsSpacerAlignment.Name = "MainControlsSpacerAlignment";
			this.MainControlsLayout.Add(this.MainControlsSpacerAlignment);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.MainControlsSpacerAlignment]));
			w6.Position = 0;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.CancelButton = new global::Gtk.Button();
			this.CancelButton.WidthRequest = 150;
			this.CancelButton.CanFocus = true;
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.UseUnderline = true;
			this.CancelButton.Label = global::Mono.Unix.Catalog.GetString("Cancel");
			global::Gtk.Image w7 = new global::Gtk.Image();
			this.CancelButton.Image = w7;
			this.MainControlsLayout.Add(this.CancelButton);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.CancelButton]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.ConfirmButton = new global::Gtk.Button();
			this.ConfirmButton.WidthRequest = 150;
			this.ConfirmButton.CanFocus = true;
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.UseUnderline = true;
			this.ConfirmButton.Label = global::Mono.Unix.Catalog.GetString("Ok");
			global::Gtk.Image w9 = new global::Gtk.Image();
			this.ConfirmButton.Image = w9;
			this.MainControlsLayout.Add(this.ConfirmButton);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.ConfirmButton]));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			this.MainLayout.Add(this.MainControlsLayout);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.MainLayout[this.MainControlsLayout]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			this.Add(this.MainLayout);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultHeight = 147;
			this.Show();
			this.CancelButton.Released += new global::System.EventHandler(this.OnCancelButtonReleased);
			this.ConfirmButton.Released += new global::System.EventHandler(this.OnConfirmButtonReleased);
		}
	}
}