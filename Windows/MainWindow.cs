using System;
using Gtk;
using UberDespatch;
//using AppIndicator;
using System.Threading;
using Pango;
using System.Collections.Generic;
using Gdk;
using System.Diagnostics;

public partial class MainWindow: Gtk.Window
{
	//ImageMenuItem menuItemShow;

	// Text View:
	TextTag tagMain;
	TextTag tagBold;
	TextTag tagAlert;
	TextTag tagSuccess;
	TextTag tagWarning;
	TextTag tagError;
	TextTag tagException;

	// Icons:
	Pixbuf iconYes;
	Pixbuf iconYesSmall;
	Pixbuf iconNo;
	Pixbuf iconNoSmall;
	Pixbuf iconExec;
	Pixbuf iconPause;
	Pixbuf iconFind;
	Pixbuf iconEdit;
	Pixbuf iconRefresh;
	Pixbuf iconSkip;
	Pixbuf iconText;
	Pixbuf iconInternet;

	// Order Table:
	Gtk.TreeViewColumn OrderNameCol;
	Gtk.TreeViewColumn OrderValueCol;
	Gtk.TreeStore OrderTreeStore;
	Gtk.TreeIter OrderCategoryCustomer;
	Gtk.TreeIter OrderCategoryAddress;
	Gtk.TreeIter OrderCategoryDetails;
	Gtk.TreeIter OrderCategoryCarrier;
	Gtk.TreeIter OrderCategoryTranslations;

	public Order CurrentOrder;
	public bool animationTickActive = false; // Set to true when the animation tick is still processing, set to false when ready for a new animation update.

	public MainWindow () : base (Gtk.WindowType.Toplevel)
	{
		BuildMenu ();
		Build ();

		// Text View:
		TextBuffer logBuffer = this.LogTextView.Buffer;

		this.tagMain = new TextTag("main");
		this.tagMain.FontDesc = FontDescription.FromString("Normal 8");
		logBuffer.TagTable.Add (this.tagMain);

		this.tagBold = new TextTag("bold");
		this.tagBold.Weight = Pango.Weight.Bold;
		logBuffer.TagTable.Add (this.tagBold);

		this.tagAlert = new TextTag("alert");
		this.tagAlert.Foreground = "blue";
		logBuffer.TagTable.Add (this.tagAlert);

		this.tagSuccess = new TextTag("success");
		this.tagSuccess.Foreground = "green";
		logBuffer.TagTable.Add (this.tagSuccess);

		this.tagWarning = new TextTag("warning");
		this.tagWarning.Foreground = "orange";
		logBuffer.TagTable.Add (this.tagWarning);

		this.tagError = new TextTag("error");
		this.tagError.Foreground = "red";
		logBuffer.TagTable.Add (this.tagError);

		this.tagException = new TextTag("exception");
		this.tagException.Foreground = "grey";
		logBuffer.TagTable.Add (this.tagException);


		// Icons:
		string iconPath = Program.ExecutableFolder + System.IO.Path.DirectorySeparatorChar + "Icons" + System.IO.Path.DirectorySeparatorChar;
		try {
			this.LoadIcons (iconPath + "svg" + System.IO.Path.DirectorySeparatorChar, ".svg");
		}
		catch (Exception e) {
			this.LoadIcons (iconPath + "png" + System.IO.Path.DirectorySeparatorChar, ".png");
		}

		// Buttons:
		this.AutoCheckButton.Image = new Gtk.Image(this.iconNoSmall);
		this.AutoSendButton.Image = new Gtk.Image(this.iconYesSmall);


		// Order Table:
		this.OrderTreeStore = new Gtk.TreeStore (typeof (string), typeof (string));
		this.OrderTableTreeview.Model = this.OrderTreeStore;

		this.OrderNameCol = new Gtk.TreeViewColumn ();
		this.OrderNameCol.Title = "Property";
		this.OrderNameCol.Resizable = true;
		this.OrderNameCol.MinWidth = 150;
		this.OrderTableTreeview.AppendColumn (this.OrderNameCol);
		Gtk.CellRendererText orderNameCell = new Gtk.CellRendererText ();
		this.OrderNameCol.PackStart (orderNameCell, true);
		this.OrderNameCol.AddAttribute (orderNameCell, "text", 0);

		this.OrderValueCol = new Gtk.TreeViewColumn();
		this.OrderValueCol.Title = "Value";
		this.OrderValueCol.Resizable = true;
		this.OrderTableTreeview.AppendColumn(this.OrderValueCol);
		Gtk.CellRendererText orderValueCell = new Gtk.CellRendererText();
		orderValueCell.Editable = true;
		orderValueCell.Edited += OnOrderValueEdited;
		this.OrderNameCol.PackStart (orderValueCell, true);
		this.OrderNameCol.AddAttribute (orderValueCell, "text", 1);
	}


	// ========== Load Icons ==========
	protected void LoadIcons (string iconPath, string iconExtension) {
		this.iconYes = new Pixbuf (iconPath + "yes" + iconExtension);
		this.iconYesSmall = new Pixbuf (iconPath + "apply-small" + iconExtension);
		this.iconNo = new Pixbuf (iconPath + "no" + iconExtension);
		this.iconNoSmall = new Pixbuf (iconPath + "no-small" + iconExtension);
		this.iconExec = new Pixbuf (iconPath + "execute" + iconExtension);
		this.iconPause = new Pixbuf (iconPath + "pause" + iconExtension);
		this.iconFind = new Pixbuf (iconPath + "find" + iconExtension);
		this.iconEdit = new Pixbuf (iconPath + "edit" + iconExtension);
		this.iconRefresh = new Pixbuf (iconPath + "refresh" + iconExtension);
		this.iconSkip = new Pixbuf (iconPath + "skip" + iconExtension);
		this.iconText = new Pixbuf (iconPath + "text" + iconExtension);
		this.iconInternet = new Pixbuf (iconPath + "internet" + iconExtension);
	}


	// ========== Quit App ==========
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		a.RetVal = true;
		this.ToggleWindow ();
		Program.Quit ();
	}
	protected void OnQuitActionActivated (object sender, EventArgs e)
	{
		Program.Quit ();
	}


	// ========== Settings ==========
	protected void OnOptionsActionActivated (object sender, EventArgs e)
	{
		var settingsWindow = new SettingsWindow ();
		settingsWindow.Show ();
	}

	protected void OnPrinterActionActivated(object sender, EventArgs e)
	{
		var printerWindow = new PrinterManagerWindow();
		printerWindow.Show();
	}

	protected void OnRulesActionActivated(object sender, EventArgs e)
	{
		var rulesWindow = new RulesWindow();
		rulesWindow.Show();
	}

	protected void OnWMSActionActivated(object sender, EventArgs e)
	{
		var wmsWindow = new WMSPeoplevoxWindow();
		wmsWindow.Show();
	}

	protected void OnCarriersActionActivated(object sender, EventArgs e)
	{
		if (Carrier.carriers.Count == 0) {
			NoCarriersDialog noPluginsDialog = new NoCarriersDialog ();
			noPluginsDialog.Show ();
		} else if (Carrier.carriers.Count > 1) {
			CarriersWindow carriersWindow = new CarriersWindow ();
			carriersWindow.Show ();
		} else { // Opens the window of the last carrier added, in this case it is the only carrier added.
			Carrier carrier = Carrier.GetCarrier ();
			if(carrier != null)
				carrier.OpenSettingsWindow ();
		}
	}


	// ========== Reload ==========
	protected void OnReloadActivated(object sender, EventArgs e)
	{
		Program.Reload ();
	}


	// ========== Open Archive ==========
	protected void OnOpenArchiveActivated(object sender, EventArgs e)
	{
		string archivePath = "";
		try {
			archivePath = Program.configGlobal.archivePath;
			if (archivePath.Substring(0, 1) == System.IO.Path.DirectorySeparatorChar.ToString ())
				archivePath = Program.ExecutableFolder + archivePath;
			Process.Start("file://" + archivePath);
		}
		catch (Exception ex) {
			Program.LogError ("Archive", "Unable to open the archive folder: " + archivePath);
			Program.LogException (ex);
		}
	}


	// ========== Open Order ==========
	protected void OnOpenOrderActivated(object sender, EventArgs e)
	{
		var fileChooser = new FileChooserDialog ("Open Order", this, FileChooserAction.Open);
		fileChooser.SetCurrentFolder (Program.configGlobal.archivePath);
		fileChooser.AddButton (Stock.Cancel, ResponseType.Cancel);
		fileChooser.AddButton (Stock.Open, ResponseType.Accept);
		fileChooser.Filter = new FileFilter ();
		fileChooser.Filter.AddPattern ("*");
		if (Program.wms != null) {
			foreach (string fileFilter in Program.wms.FileFilters) {
				fileChooser.Filter.AddPattern(fileFilter);
			}
		}

		if (fileChooser.Run() == (int)Gtk.ResponseType.Accept) {
			Program.orderChecker.LoadOrder (fileChooser.Filename);
		}

		fileChooser.Destroy ();
	}


	// ========== Auto Check ==========
	protected void OnAutoCheckButtonReleased (object sender, EventArgs e)
	{
		this.SetAutoCheckButton (Program.orderChecker.ToggleAutoCheck());
	}

	public void SetAutoCheckButton(bool autocheckEnabled)
	{
		if (autocheckEnabled)
		{
			this.AutoCheckButton.Label = "Auto Check (On)";
			this.AutoCheckButton.Image = new Gtk.Image(this.iconYesSmall);
		}
		else
		{
			this.AutoCheckButton.Label = "Auto Check (Off)";
			this.AutoCheckButton.Image = new Gtk.Image(this.iconNoSmall);
		}
	}


	// ========== Auto Send ==========
	protected void OnAutoSendButtonReleased(object sender, EventArgs e)
	{
		if (Program.orderChecker.ToggleAutoSend())
		{
			this.AutoSendButton.Label = "Auto Send (On)";
			this.AutoSendButton.Image = new Gtk.Image(this.iconYesSmall);
		}
		else {
			this.AutoSendButton.Label = "Auto Send (Off)";
			this.AutoSendButton.Image = new Gtk.Image(this.iconNoSmall);
		}
	}


	// ========== Manual Check ==========
	protected void OnManualCheckButtonReleased (object sender, EventArgs e)
	{
		Program.orderChecker.ScheduleManualCheck ();
	}


	// ========== Send Order ==========
	protected void OnSendOrderButtonReleased (object sender, EventArgs e)
	{
		Program.orderChecker.SendOrder ();
	}


	// ========== Skip Order ==========
	protected void OnSkipOrderButtonReleased (object sender, EventArgs e)
	{
		Program.orderChecker.SkipOrder ();
	}


	// ========== Repeat Order ==========
	protected void OnRepeatOrderButtonReleased (object sender, EventArgs e)
	{
		Program.orderChecker.RepeatOrder ();
	}


	// ========== Order Value Edited ==========
	protected void OnOrderValueEdited(object sender, Gtk.EditedArgs args)
	{
		if (this.CurrentOrder == null)
			return;

		Gtk.TreeIter iter;
		this.OrderTreeStore.GetIter (out iter, new Gtk.TreePath (args.Path));
		string propertyName = (string)this.OrderTreeStore.GetValue (iter, 0);
		string propertyValueOld = (string)this.OrderTreeStore.GetValue(iter, 1);
		string propertyValue = args.NewText;

		if (propertyValue == propertyValueOld)
			return;

		try {
			if (propertyName == "Customer Name")
				this.CurrentOrder.CustomerName = propertyValue;
			if (propertyName == "Customer Email") {
				this.CurrentOrder.CustomerEmail = propertyValue;
				this.OrderEmailLabel.Text = this.CurrentOrder.CustomerEmail;
			}
			if (propertyName == "Customer Phone")
				this.CurrentOrder.CustomerPhone = propertyValue;

			if (propertyName == "Postcode")
				this.CurrentOrder.Postcode = propertyValue;
			if (propertyName == "Country")
				this.CurrentOrder.Country = propertyValue;
			if (propertyName == "Region")
				this.CurrentOrder.Region = propertyValue;
			if (propertyName == "City")
				this.CurrentOrder.City = propertyValue;
			if (propertyName == "Street")
				this.CurrentOrder.Street = propertyValue;
			if (propertyName == "Locality")
				this.CurrentOrder.Locality = propertyValue;

			if (propertyName == "Order Status")
				this.CurrentOrder.OrderStatus = propertyValue;
			if (propertyName == "Order Number") {
				this.CurrentOrder.OrderNumber = propertyValue;
				this.OrderNumberLabel.Text = this.CurrentOrder.OrderNumber;
			}
			if (propertyName == "Date")
				this.CurrentOrder.OrderDate = propertyValue;
			if (propertyName == "Total Weight")
				this.CurrentOrder.OrderWeight = Convert.ToDouble (propertyValue);
			if (propertyName == "Total Cost")
				this.CurrentOrder.OrderCost = Convert.ToDouble (propertyValue);
			if (propertyName == "Shipping Cost")
				this.CurrentOrder.ShippingCost = Convert.ToDouble (propertyValue);
			if (propertyName == "Tax")
				this.CurrentOrder.TaxAmount = Convert.ToDouble (propertyValue);
			if (propertyName == "Item Count")
				this.CurrentOrder.ItemAmount = Convert.ToInt16 (propertyValue);

			if (propertyName == "Channel")
				this.CurrentOrder.Channel = propertyValue;
			if (propertyName == "Zone") {
				Zone zone = Zone.GetZone(propertyValue);
				if (zone != null)
					this.CurrentOrder.Zone = zone;
				else {
					Program.LogWarning ("Order Editor", "The Zone: " + propertyValue + " does not exist, this is case sensitive.");
					return;
				}
			}
			if (propertyName == "Carrier Name") {
				Carrier carrier = Carrier.GetCarrier(propertyValue);
				if (carrier != null)
					this.CurrentOrder.Carrier = carrier;
				else {
					Program.LogWarning ("Order Editor", "The Carrier: " + propertyValue + " does not exist, this is case sensitive.");
					return;
				}
			}
			if (propertyName == "Carrier Type")
				this.CurrentOrder.CarrierType = propertyValue;
			if (propertyName == "Carrier Service")
				this.CurrentOrder.Service = propertyValue;
			if (propertyName == "Carrier Enhancement")
				this.CurrentOrder.Enhancement = propertyValue;
			if (propertyName == "Carrier Format")
				this.CurrentOrder.Format = propertyValue;
			
			this.OrderTreeStore.SetValue(iter, 1, propertyValue);
		}
		catch (Exception e) {
			Program.LogError("Order Editor", "Invalid order input, please ensure that entries like Weight or Cost are numeric only, etc:\n" + e.ToString());
			Program.LogException(e);
			return;
		}

		Program.Log ("Order Editor", "Changed " + propertyName + " to " + propertyValue + ".");
	}


	// ========== Update Log ==========
	/** Prints an additional line of text to the log text view. This will add a trailing new line after the message automatically. **/
	public void UpdateLog(string category, string message, string type = "main") {
		TextBuffer buffer = this.LogTextView.Buffer;
		TextIter endIter = buffer.EndIter;

		// Insert Text:
		List<string> catTypes = new List<string> ();
		catTypes.Add ("main");
		catTypes.Add ("bold");
		List<string> types = new List<string> ();
		types.Add ("main");
		if (type != "main") {
			catTypes.Add (type);
			types.Add (type);
		}

		if(category != "")
			buffer.InsertWithTagsByName(ref endIter, "[" + category + "] ", catTypes.ToArray ());
		buffer.InsertWithTagsByName(ref endIter, message + "\n", types.ToArray ());

		// Scroll To End:
		this.LogTextView.ScrollToMark (buffer.InsertMark, 0, false, 0, 0);
	}


	// ========== Update Main State ==========
	/** Sets the main state to show the current main activity. **/
	//Stetic.IconLoader.LoadIcon (this, Stock.Execute, IconSize.Dialog)
	public void UpdateMainState(string state) {
		if (state == "startup") {
			this.ProcessStatusImage.Pixbuf = this.iconExec;
			this.ProcessStatusLabel.Text = "Starting Up...";
		} else if (state == "idle") {
			this.ProcessStatusImage.Pixbuf = this.iconPause;
			this.ProcessStatusLabel.Text = "Idle";
		} else if (state == "reload") {
			this.ProcessStatusImage.Pixbuf = this.iconExec;
			this.ProcessStatusLabel.Text = "Reloading...";
		}

		else if (state == "order-check") {
			this.ProcessStatusImage.Pixbuf = this.iconFind;
			this.ProcessStatusLabel.Text = "Checking for Orders...";
		} else if (state == "order-none") {
			this.ProcessStatusImage.Pixbuf = this.iconNo;
			this.ProcessStatusLabel.Text = "No Orders Found";
		} else if (state == "order-read") {
			this.ProcessStatusImage.Pixbuf = this.iconText;
			this.ProcessStatusLabel.Text = "Reading Order...";
		} else if (state == "order-send") {
			this.ProcessStatusImage.Pixbuf = this.iconRefresh;
			if (Program.orderChecker.activeOrder != null
			    && Program.orderChecker.activeOrder.Carrier != null
			    && Program.orderChecker.activeOrder.Carrier.GetIcon () != null)
				this.ProcessStatusImage.Pixbuf = Program.orderChecker.activeOrder.Carrier.GetIcon ();
			this.ProcessStatusLabel.Text = "Sending Order...";
		} else if (state == "order-update") {
			this.ProcessStatusImage.Pixbuf = this.iconRefresh;
			this.ProcessStatusLabel.Text = "Updating Order...";
		}

		else if (state == "order-complete") {
			this.ProcessStatusImage.Pixbuf = this.iconYes;
			this.ProcessStatusLabel.Text = "Order Completed";
		} else if (state == "order-fail") {
			this.ProcessStatusImage.Pixbuf = this.iconNo;
			this.ProcessStatusLabel.Text = "Order Failed";
		} else if (state == "order-skip") {
			this.ProcessStatusImage.Pixbuf = this.iconSkip;
			this.ProcessStatusLabel.Text = "Order Skipped";
		} else if (state == "order-edit") {
			this.ProcessStatusImage.Pixbuf = this.iconEdit;
			this.ProcessStatusLabel.Text = "Order Editing";
		}
	}


	// ========== Update Animation ==========
	/** This is called from the animation thread to keep the progress bar and other visual animations up to date. **/
	public void UpdateAnimation() {
		this.animationTickActive = true;
		string state = Program.mainState;
		if (state == "order-check") {
			this.StatusProgress.Pulse ();
			this.StatusProgress.PulseStep = 0.01;
		} else if (state == "order-read") {
			this.StatusProgress.Fraction = 0.1;
		} else if (state == "order-send") {
			this.StatusProgress.Fraction = 0.4;
		} else if (state == "order-edit") {
			this.StatusProgress.Fraction = 0.4;
		} else if (state == "order-update") {
			this.StatusProgress.Fraction = 0.8;
		} else if (state == "order-complete") {
			this.StatusProgress.Fraction = 1;
		} else {
			this.StatusProgress.Fraction = 0;
		}
		this.animationTickActive = false;
	}


	// ========== Update API State ==========
	/** Sets the API state to show the current API connection. 0 = Disconnected, 1 = Connecting, 2 = Connected **/
	public void UpdateAPIState(byte state) {
		if (state == 2) {
			this.APIStatusImage.Pixbuf = this.iconInternet;
			this.APIStatusLabel.Text = "PVX: Connected";
		} else if (state == 1) {
			this.APIStatusImage.Pixbuf = this.iconRefresh;
			this.APIStatusLabel.Text = "PVX: Connecting";
		} else if (state == 0) {
			this.APIStatusImage.Pixbuf = this.iconNo;
			this.APIStatusLabel.Text = "PVX: Disconnected";
		}
	}


	// ========== Display Order ==========
	/** Loads the provided order into the order view fields, if provided with null, the fields will be cleared. **/
	public void DisplayOrder(Order order) {
		this.OrderTreeStore.Clear();
		this.OrderEmailLabel.Text = "";
		this.OrderNumberLabel.Text = "";

		this.CurrentOrder = order;
		if (order == null)
			return;

		try {
			this.OrderEmailLabel.Text = order.CustomerEmail;
			this.OrderNumberLabel.Text = order.OrderNumber;

			this.OrderCategoryCustomer = this.OrderTreeStore.AppendValues("Customer");
			this.OrderTreeStore.AppendValues(this.OrderCategoryCustomer, "Customer Name", order.CustomerName);
			this.OrderTreeStore.AppendValues(this.OrderCategoryCustomer, "Customer Email", order.CustomerEmail);
			this.OrderTreeStore.AppendValues(this.OrderCategoryCustomer, "Customer Phone", order.CustomerPhone);

			this.OrderCategoryAddress = this.OrderTreeStore.AppendValues("Address");
			this.OrderTreeStore.AppendValues(this.OrderCategoryAddress, "Postcode", order.Postcode);
			this.OrderTreeStore.AppendValues(this.OrderCategoryAddress, "Country", order.Country);
			this.OrderTreeStore.AppendValues(this.OrderCategoryAddress, "Region", order.Region);
			this.OrderTreeStore.AppendValues(this.OrderCategoryAddress, "City", order.City);
			this.OrderTreeStore.AppendValues(this.OrderCategoryAddress, "Street", order.Street);
			this.OrderTreeStore.AppendValues(this.OrderCategoryAddress, "Locality", order.Locality);

			this.OrderCategoryDetails = this.OrderTreeStore.AppendValues("Details");
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Order Status", order.OrderStatus);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Order Number", order.OrderNumber);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Date", order.OrderDate);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Total Weight", order.OrderWeight);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Total Cost", order.OrderCost);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Shipping Cost", order.ShippingCost);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Tax", order.TaxAmount);
			this.OrderTreeStore.AppendValues(this.OrderCategoryDetails, "Item Count", order.ItemAmount);

			this.OrderCategoryCarrier = this.OrderTreeStore.AppendValues("Carrier");
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Channel", order.Channel);
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Zone", order.Zone != null ? order.Zone.name : "");
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Carrier Name", order.Carrier != null ? order.Carrier.name : "");
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Carrier Type", order.CarrierType);
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Carrier Service", order.Service);
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Carrier Enhancement", order.Enhancement);
			this.OrderTreeStore.AppendValues(this.OrderCategoryCarrier, "Carrier Format", order.Format);

			if (order.Translations != null && order.Translations.Count > 0) {
				this.OrderCategoryTranslations = this.OrderTreeStore.AppendValues("Translations");
				foreach (KeyValuePair<string, string> translationEntry in order.Translations) {
					this.OrderTreeStore.AppendValues(this.OrderCategoryTranslations, translationEntry.Key, translationEntry.Value);
				}
			}

			this.OrderTableTreeview.ExpandAll ();
		}
		catch (Exception e) {
			Program.LogError ("Order Editor", "An error occured while trying to load this order into the editor:");
			Program.LogException (e);
		}
	}


	// ========== Toggle Window ==========
	/** Hides or Shows the main window. **/
	public void ToggleWindow() {
		if (this.Visible) {
			this.Visible = false;
		} else {
			this.Visible = true;
		}
	}


	// ========== App Indicator ==========
	private void BuildMenu()
	{
		/*ApplicationIndicator indicator = new ApplicationIndicator ("uberdespatch", "app-icon", Category.ApplicationStatus, Program.ExecutableFolder);

		// Build Popup Menu:
		Menu popupMenu = new Menu ();

		// Show:
		this.menuItemShow = new ImageMenuItem ("Show/Hide");
		this.menuItemShow.Image = new Gtk.Image(Stock.Info, IconSize.Menu);
		this.menuItemShow.Activated += (sender, e) => this.ToggleWindow ();
		popupMenu.Append(this.menuItemShow);

		popupMenu.Append(new SeparatorMenuItem());

		// Quit:
		ImageMenuItem menuItemQuit = new ImageMenuItem ("Quit");
		menuItemQuit.Image = new Gtk.Image (Stock.Quit, IconSize.Menu);
		menuItemQuit.Activated += (sender, e) => Application.Quit ();
		popupMenu.Append (menuItemQuit);

		popupMenu.ShowAll();

		// Assign Menu and Activate:
		indicator.Menu = popupMenu;
		indicator.Status = AppIndicator.Status.Active;*/
	}
}
