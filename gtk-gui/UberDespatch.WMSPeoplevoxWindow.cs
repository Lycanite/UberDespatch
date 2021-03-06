
// This file has been generated by the GUI designer. Do not modify.
namespace UberDespatch
{
	public partial class WMSPeoplevoxWindow
	{
		private global::Gtk.VBox MainLayout;

		private global::Gtk.Frame APIFrame;

		private global::Gtk.Alignment APIAlignment;

		private global::Gtk.VBox APILayout;

		private global::Gtk.HBox URLLayout;

		private global::Gtk.Label URLLabel;

		private global::Gtk.Entry URLEntry;

		private global::Gtk.HBox ClientIDLayout;

		private global::Gtk.Label ClientIDLabel;

		private global::Gtk.Entry ClientIDEntry;

		private global::Gtk.HBox UsernameLayout;

		private global::Gtk.Label UsernameLabel;

		private global::Gtk.Entry UsernameEntry;

		private global::Gtk.HBox PasswordLayout;

		private global::Gtk.Label PasswordLabel;

		private global::Gtk.Entry PasswordEntry;

		private global::Gtk.HBox UpdateTemplateLayout;

		private global::Gtk.Label UpdateTemplateLabel;

		private global::Gtk.Entry UpdateTemplateEntry;

		private global::Gtk.Label APILabel;

		private global::Gtk.HBox MainControlsLayout;

		private global::Gtk.HBox TestLayout;

		private global::Gtk.Button TestButton;

		private global::Gtk.Image TestImage;

		private global::Gtk.Alignment MainControlsSpacerAlignment;

		private global::Gtk.Button CancelButton;

		private global::Gtk.Button ConfirmButton;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget UberDespatch.WMSPeoplevoxWindow
			this.Name = "UberDespatch.WMSPeoplevoxWindow";
			this.Title = global::Mono.Unix.Catalog.GetString("Peoplevox");
			this.Icon = global::Stetic.IconLoader.LoadIcon(this, "gtk-preferences", global::Gtk.IconSize.Menu);
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			this.BorderWidth = ((uint)(8));
			this.DefaultWidth = 640;
			this.DefaultHeight = 200;
			// Container child UberDespatch.WMSPeoplevoxWindow.Gtk.Container+ContainerChild
			this.MainLayout = new global::Gtk.VBox();
			this.MainLayout.Name = "MainLayout";
			this.MainLayout.Spacing = 6;
			// Container child MainLayout.Gtk.Box+BoxChild
			this.APIFrame = new global::Gtk.Frame();
			this.APIFrame.Name = "APIFrame";
			this.APIFrame.ShadowType = ((global::Gtk.ShadowType)(0));
			this.APIFrame.BorderWidth = ((uint)(8));
			// Container child APIFrame.Gtk.Container+ContainerChild
			this.APIAlignment = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.APIAlignment.Name = "APIAlignment";
			this.APIAlignment.LeftPadding = ((uint)(12));
			// Container child APIAlignment.Gtk.Container+ContainerChild
			this.APILayout = new global::Gtk.VBox();
			this.APILayout.Name = "APILayout";
			this.APILayout.Spacing = 6;
			// Container child APILayout.Gtk.Box+BoxChild
			this.URLLayout = new global::Gtk.HBox();
			this.URLLayout.Name = "URLLayout";
			this.URLLayout.Spacing = 6;
			// Container child URLLayout.Gtk.Box+BoxChild
			this.URLLabel = new global::Gtk.Label();
			this.URLLabel.WidthRequest = 200;
			this.URLLabel.Name = "URLLabel";
			this.URLLabel.LabelProp = global::Mono.Unix.Catalog.GetString("API URL");
			this.URLLabel.Wrap = true;
			this.URLLabel.SingleLineMode = true;
			this.URLLayout.Add(this.URLLabel);
			global::Gtk.Box.BoxChild w1 = ((global::Gtk.Box.BoxChild)(this.URLLayout[this.URLLabel]));
			w1.Position = 0;
			w1.Expand = false;
			w1.Fill = false;
			// Container child URLLayout.Gtk.Box+BoxChild
			this.URLEntry = new global::Gtk.Entry();
			this.URLEntry.TooltipMarkup = "The web address of the WMS API.";
			this.URLEntry.CanFocus = true;
			this.URLEntry.Name = "URLEntry";
			this.URLEntry.IsEditable = true;
			this.URLEntry.InvisibleChar = '•';
			this.URLLayout.Add(this.URLEntry);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.URLLayout[this.URLEntry]));
			w2.Position = 1;
			this.APILayout.Add(this.URLLayout);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.APILayout[this.URLLayout]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child APILayout.Gtk.Box+BoxChild
			this.ClientIDLayout = new global::Gtk.HBox();
			this.ClientIDLayout.Name = "ClientIDLayout";
			this.ClientIDLayout.Spacing = 6;
			// Container child ClientIDLayout.Gtk.Box+BoxChild
			this.ClientIDLabel = new global::Gtk.Label();
			this.ClientIDLabel.WidthRequest = 200;
			this.ClientIDLabel.Name = "ClientIDLabel";
			this.ClientIDLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Client ID");
			this.ClientIDLabel.Wrap = true;
			this.ClientIDLabel.SingleLineMode = true;
			this.ClientIDLayout.Add(this.ClientIDLabel);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.ClientIDLayout[this.ClientIDLabel]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child ClientIDLayout.Gtk.Box+BoxChild
			this.ClientIDEntry = new global::Gtk.Entry();
			this.ClientIDEntry.TooltipMarkup = "The Client ID used to login to the WMS.";
			this.ClientIDEntry.CanFocus = true;
			this.ClientIDEntry.Name = "ClientIDEntry";
			this.ClientIDEntry.IsEditable = true;
			this.ClientIDEntry.InvisibleChar = '•';
			this.ClientIDLayout.Add(this.ClientIDEntry);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(this.ClientIDLayout[this.ClientIDEntry]));
			w5.Position = 1;
			this.APILayout.Add(this.ClientIDLayout);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.APILayout[this.ClientIDLayout]));
			w6.Position = 1;
			w6.Expand = false;
			w6.Fill = false;
			// Container child APILayout.Gtk.Box+BoxChild
			this.UsernameLayout = new global::Gtk.HBox();
			this.UsernameLayout.Name = "UsernameLayout";
			this.UsernameLayout.Spacing = 6;
			// Container child UsernameLayout.Gtk.Box+BoxChild
			this.UsernameLabel = new global::Gtk.Label();
			this.UsernameLabel.WidthRequest = 200;
			this.UsernameLabel.Name = "UsernameLabel";
			this.UsernameLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Username");
			this.UsernameLabel.Wrap = true;
			this.UsernameLabel.SingleLineMode = true;
			this.UsernameLayout.Add(this.UsernameLabel);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.UsernameLayout[this.UsernameLabel]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child UsernameLayout.Gtk.Box+BoxChild
			this.UsernameEntry = new global::Gtk.Entry();
			this.UsernameEntry.TooltipMarkup = "The username used to login to the WMS.";
			this.UsernameEntry.CanFocus = true;
			this.UsernameEntry.Name = "UsernameEntry";
			this.UsernameEntry.IsEditable = true;
			this.UsernameEntry.InvisibleChar = '•';
			this.UsernameLayout.Add(this.UsernameEntry);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.UsernameLayout[this.UsernameEntry]));
			w8.Position = 1;
			this.APILayout.Add(this.UsernameLayout);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.APILayout[this.UsernameLayout]));
			w9.Position = 2;
			w9.Expand = false;
			w9.Fill = false;
			// Container child APILayout.Gtk.Box+BoxChild
			this.PasswordLayout = new global::Gtk.HBox();
			this.PasswordLayout.Name = "PasswordLayout";
			this.PasswordLayout.Spacing = 6;
			// Container child PasswordLayout.Gtk.Box+BoxChild
			this.PasswordLabel = new global::Gtk.Label();
			this.PasswordLabel.WidthRequest = 200;
			this.PasswordLabel.Name = "PasswordLabel";
			this.PasswordLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Password");
			this.PasswordLabel.Wrap = true;
			this.PasswordLabel.SingleLineMode = true;
			this.PasswordLayout.Add(this.PasswordLabel);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.PasswordLayout[this.PasswordLabel]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			// Container child PasswordLayout.Gtk.Box+BoxChild
			this.PasswordEntry = new global::Gtk.Entry();
			this.PasswordEntry.TooltipMarkup = "The password used to login to the WMS.";
			this.PasswordEntry.CanFocus = true;
			this.PasswordEntry.Name = "PasswordEntry";
			this.PasswordEntry.IsEditable = true;
			this.PasswordEntry.InvisibleChar = '•';
			this.PasswordLayout.Add(this.PasswordEntry);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.PasswordLayout[this.PasswordEntry]));
			w11.Position = 1;
			this.APILayout.Add(this.PasswordLayout);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.APILayout[this.PasswordLayout]));
			w12.Position = 3;
			w12.Expand = false;
			w12.Fill = false;
			// Container child APILayout.Gtk.Box+BoxChild
			this.UpdateTemplateLayout = new global::Gtk.HBox();
			this.UpdateTemplateLayout.Name = "UpdateTemplateLayout";
			this.UpdateTemplateLayout.Spacing = 6;
			// Container child UpdateTemplateLayout.Gtk.Box+BoxChild
			this.UpdateTemplateLabel = new global::Gtk.Label();
			this.UpdateTemplateLabel.WidthRequest = 200;
			this.UpdateTemplateLabel.Name = "UpdateTemplateLabel";
			this.UpdateTemplateLabel.LabelProp = global::Mono.Unix.Catalog.GetString("Update Template");
			this.UpdateTemplateLabel.Wrap = true;
			this.UpdateTemplateLabel.SingleLineMode = true;
			this.UpdateTemplateLayout.Add(this.UpdateTemplateLabel);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.UpdateTemplateLayout[this.UpdateTemplateLabel]));
			w13.Position = 0;
			w13.Expand = false;
			w13.Fill = false;
			// Container child UpdateTemplateLayout.Gtk.Box+BoxChild
			this.UpdateTemplateEntry = new global::Gtk.Entry();
			this.UpdateTemplateEntry.TooltipMarkup = "The password used to login to the WMS.";
			this.UpdateTemplateEntry.CanFocus = true;
			this.UpdateTemplateEntry.Name = "UpdateTemplateEntry";
			this.UpdateTemplateEntry.IsEditable = true;
			this.UpdateTemplateEntry.InvisibleChar = '•';
			this.UpdateTemplateLayout.Add(this.UpdateTemplateEntry);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.UpdateTemplateLayout[this.UpdateTemplateEntry]));
			w14.Position = 1;
			this.APILayout.Add(this.UpdateTemplateLayout);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.APILayout[this.UpdateTemplateLayout]));
			w15.Position = 4;
			w15.Expand = false;
			w15.Fill = false;
			this.APIAlignment.Add(this.APILayout);
			this.APIFrame.Add(this.APIAlignment);
			this.APILabel = new global::Gtk.Label();
			this.APILabel.Name = "APILabel";
			this.APILabel.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Peoplevox API</b>");
			this.APILabel.UseMarkup = true;
			this.APIFrame.LabelWidget = this.APILabel;
			this.MainLayout.Add(this.APIFrame);
			global::Gtk.Box.BoxChild w18 = ((global::Gtk.Box.BoxChild)(this.MainLayout[this.APIFrame]));
			w18.Position = 0;
			w18.Expand = false;
			w18.Fill = false;
			// Container child MainLayout.Gtk.Box+BoxChild
			this.MainControlsLayout = new global::Gtk.HBox();
			this.MainControlsLayout.Name = "MainControlsLayout";
			this.MainControlsLayout.Spacing = 6;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.TestLayout = new global::Gtk.HBox();
			this.TestLayout.Name = "TestLayout";
			this.TestLayout.Spacing = 6;
			// Container child TestLayout.Gtk.Box+BoxChild
			this.TestButton = new global::Gtk.Button();
			this.TestButton.TooltipMarkup = "Click to test UberDespatch\'s connection to the WMS API.";
			this.TestButton.WidthRequest = 150;
			this.TestButton.HeightRequest = 40;
			this.TestButton.CanFocus = true;
			this.TestButton.Name = "TestButton";
			this.TestButton.UseUnderline = true;
			this.TestButton.Label = global::Mono.Unix.Catalog.GetString("Test Connection");
			global::Gtk.Image w19 = new global::Gtk.Image();
			w19.Pixbuf = new global::Gdk.Pixbuf(global::System.IO.Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory, "./Icons/png/refresh-small.png"));
			this.TestButton.Image = w19;
			this.TestLayout.Add(this.TestButton);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.TestLayout[this.TestButton]));
			w20.Position = 0;
			w20.Expand = false;
			w20.Fill = false;
			// Container child TestLayout.Gtk.Box+BoxChild
			this.TestImage = new global::Gtk.Image();
			this.TestImage.WidthRequest = 48;
			this.TestImage.HeightRequest = 48;
			this.TestImage.Name = "TestImage";
			this.TestImage.Pixbuf = new global::Gdk.Pixbuf(global::System.IO.Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory, "./Icons/png/question.png"));
			this.TestLayout.Add(this.TestImage);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.TestLayout[this.TestImage]));
			w21.Position = 1;
			w21.Expand = false;
			w21.Fill = false;
			this.MainControlsLayout.Add(this.TestLayout);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.TestLayout]));
			w22.Position = 0;
			w22.Expand = false;
			w22.Fill = false;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.MainControlsSpacerAlignment = new global::Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
			this.MainControlsSpacerAlignment.Name = "MainControlsSpacerAlignment";
			this.MainControlsLayout.Add(this.MainControlsSpacerAlignment);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.MainControlsSpacerAlignment]));
			w23.Position = 1;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.CancelButton = new global::Gtk.Button();
			this.CancelButton.WidthRequest = 150;
			this.CancelButton.HeightRequest = 40;
			this.CancelButton.CanFocus = true;
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.UseUnderline = true;
			this.CancelButton.Label = global::Mono.Unix.Catalog.GetString("Cancel");
			global::Gtk.Image w24 = new global::Gtk.Image();
			w24.Pixbuf = new global::Gdk.Pixbuf(global::System.IO.Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory, "./Icons/png/no-small.png"));
			this.CancelButton.Image = w24;
			this.MainControlsLayout.Add(this.CancelButton);
			global::Gtk.Box.BoxChild w25 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.CancelButton]));
			w25.Position = 2;
			w25.Expand = false;
			w25.Fill = false;
			// Container child MainControlsLayout.Gtk.Box+BoxChild
			this.ConfirmButton = new global::Gtk.Button();
			this.ConfirmButton.WidthRequest = 150;
			this.ConfirmButton.HeightRequest = 40;
			this.ConfirmButton.CanFocus = true;
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.UseUnderline = true;
			this.ConfirmButton.Label = global::Mono.Unix.Catalog.GetString("Ok");
			global::Gtk.Image w26 = new global::Gtk.Image();
			w26.Pixbuf = new global::Gdk.Pixbuf(global::System.IO.Path.Combine(global::System.AppDomain.CurrentDomain.BaseDirectory, "./Icons/png/apply-small.png"));
			this.ConfirmButton.Image = w26;
			this.MainControlsLayout.Add(this.ConfirmButton);
			global::Gtk.Box.BoxChild w27 = ((global::Gtk.Box.BoxChild)(this.MainControlsLayout[this.ConfirmButton]));
			w27.Position = 3;
			w27.Expand = false;
			w27.Fill = false;
			this.MainLayout.Add(this.MainControlsLayout);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.MainLayout[this.MainControlsLayout]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			this.Add(this.MainLayout);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.Show();
			this.TestButton.Released += new global::System.EventHandler(this.OnTestButtonReleased);
			this.CancelButton.Released += new global::System.EventHandler(this.OnCancelButtonReleased);
			this.ConfirmButton.Released += new global::System.EventHandler(this.OnConfirmButtonReleased);
		}
	}
}
