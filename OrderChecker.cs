using System;
using System.Threading;
using Gtk;

namespace UberDespatch
{
	public class OrderChecker
	{
		public bool autoCheck = false;
		public bool autoSend = true;
		public bool manualCheckScheduled = false;
		public bool waitingForUser = false;
		public bool orderScheduled = false;

		// Testing:
		public bool TestLabels = false;

		public Order activeOrder;
		public Order lastOrder;

		public OrderChecker ()
		{
			// Checks for orders and assigns them to Carriers (loaded via plugins) via Rules.
		}


		// ========== Manual Check ==========
		/** Used to schedule a manual check for an order which will be called on the main processing thread. **/
		public void ScheduleManualCheck () {
			if(!this.manualCheckScheduled)
				Program.Log ("Order", "Manual check scheduled, please wait...");
			else
				Program.LogAlert ("Order", "A manual check is already scheduled, chill your beans...");
			this.manualCheckScheduled = true;
		}

		/** Used to manually check for an order. It is recommended to use ScheduleManualCheck if calling from the GUI so that it can run on a different thread. **/
		public void ManualCheck () {
			Program.Log ("Order", "Manually checking for next order...");
			if(this.autoCheck)
				Program.LogAlert ("Order", "Note: Automatic checking is already enabled so there is no need to use the manual check. Manually checking anyway...");
			Program.UpdateState ("order-check");
			if (!this.CheckForOrder ()) {
				Program.Log ("Order", "No orders found.");
				Program.UpdateState ("order-none");
				System.Threading.Thread.Sleep (2000);
			}
			this.manualCheckScheduled = false;
		}


		// ========== Auto Check ==========
		/** Toggle auto check and returns what it is set to. True = On, False = Off **/
		public bool ToggleAutoCheck () {
			this.autoCheck = !this.autoCheck;
			if(this.autoCheck)
				Program.Log ("Order", "Automatically checking for orders...");
			else
				Program.Log ("Order", "Automatic order checking disabled.");
			return this.autoCheck;
		}


		// ========== Check For Order ==========
		/** Checks for an order using each carrier and then allows the carrier to process the order.
		 * If passed an order object, instead of checking for a new order, the provided order will be used instead, this is useful for repeating orders.
		 * Returns true if one can be found or false if one isn't.
		 * This will still return true if an order is found but cannot be processed. **/
		public bool CheckForOrder (Order order = null) {
			if (!Rule.RulesAvailable ()) {
				Program.LogError ("Rules", "No rules are currently available therefore no orders can be processed. Please Reload Rules and try again.");
				Program.UpdateState ("order-fail");
				if (this.autoCheck)
					this.ToggleAutoCheck ();
			}
			try {
				if(order == null)
					order = Program.wms.CheckForOrder ();
				if (order != null) {
					Program.DisplayOrder (order);
					Program.WebLogOrder (order);
					this.activeOrder = order;

					// Rule:
					Rule rule = Rule.GetRule (order);
					if(rule == null) {
						Program.LogAlert ("Order", "No rule matches this order. This order will be skipped and archived.");
						Program.UpdateState ("order-fail");
						this.ArchiveOrder (order);
						return true;
					}
					rule.ApplyToOrder (order);

					// Translate and Display:
					order.Translate();
					Program.DisplayOrder (order);

					// Inspection (Manual Order Sending):
					if(order.CarrierType == "Manual")
						order.Edit = true;
					if(!this.autoSend || order.Edit) {
						if (order.Edit)
							Program.LogAlert ("Order", "This order has been flagged for inspection. The order needs to be checked over and then manually sent.");
						else
							Program.LogAlert ("Order", "Click the Send Order button to send this order or enable Auto Send.");
						Program.UpdateState ("order-edit");
						this.waitingForUser = true;
						bool sendOrder = false;
						while(!sendOrder || order.Edit) {
							if (order.Cancelled) {
								Program.Log ("Order", "Skipped.");
								Program.UpdateState ("order-skip");
								this.ArchiveOrder (order);
								return true;
							}
							if (!this.waitingForUser) {
								this.waitingForUser = true;
								if(order.Carrier == null) {
									Program.LogAlert ("Order", "The carrier for the order was invalid, please enter a valid carrier or skip the order.");
									Program.Log("Order", "Available carriers (case sensitive): " + Carrier.GetCarrierNames ());
									Program.UpdateState ("order-edit");
								}
								else {
									Program.Log ("Order", "Sending order to carrier...");
									Program.UpdateState ("order-read");
									order.Edit = false;
									sendOrder = true;
								}
							}
							Thread.Sleep (1000);
						}
					}

					// Carrier:
					Carrier carrier = order.Carrier;
					if (carrier == null) {
						Program.LogAlert ("Order", "No carrier for this order is set. This order will be skipped and archived.");
						Program.UpdateState ("order-fail");
						this.ArchiveOrder (order);
						return true;
					}

					Program.Log ("Order", "Using " + carrier.name + " to process this order...");

					// Validate Order:
					this.waitingForUser = true;
					Program.Log (carrier.name, "Validating order...");
					bool orderValid = carrier.ValidateOrder (order);
					if (!orderValid) {
						Program.LogAlert (carrier.name, "The order is invalid please correct it using the fields below and click Send Order or skip the order.");
						Program.UpdateState ("order-edit");
					}
					while (!orderValid && !order.Cancelled && !order.Error) {
						if (!this.waitingForUser) {
							Program.Log (carrier.name, "Updated, re-validating...");
							Program.UpdateState ("order-read");
							orderValid = carrier.ValidateOrder (order);
							if (!orderValid) {
								Program.LogAlert (carrier.name, "The order is still invalid, please re-check the order details using the fields below.");
								Program.UpdateState ("order-edit");
								this.waitingForUser = true;
							}
						}
						Thread.Sleep (1000);
					}

					// Invalid/Skipped Order:
					if (!orderValid || order.Cancelled) {
						Program.Log ("Order", "Skipped.");
						Program.UpdateState ("order-skip");
						this.ArchiveOrder (order);
						this.activeOrder = null;
						return true;
					}

					// Process Order:
					Program.Log ("Order", "Valid.");
					Program.DisplayOrder (order);
					Program.WebLogOrder (order);
					Program.UpdateState ("order-send");
					this.ProcessOrder (order);
					return true;
				}
				return false;
			}
			catch (Exception e) {
				Program.LogError ("Order", "An error occured while checking for/processing orders:");
				Program.LogException (e);
				return true;
			}
		}


		// ========== Process Order ==========
		/** Finds a Carrier and processes a given order, it is up to the processor when archive the order. If no processor is found, the order will just be skipped an archived, it must be archived else the same invalid order will be constantly found in a loop! **/
		public void ProcessOrder (Order order) {
			
			// Process:
			order.OnPreProcess();
			if (this.TestLabels) {
				this.ArchiveOrder(order);
				return;
			}
			if (order.Carrier.ProcessOrder (order)) {
				Program.UpdateState ("order-update");
				Program.wms.UpdateOrder (order);
				Program.UpdateState ("order-complete");
			}
			Program.WebLogOrder (order);
			this.ArchiveOrder (order);
		}


		// ========== Archive Order ==========
		/** Archives the provided order. **/
		public void ArchiveOrder (Order order) {
			this.lastOrder = order;
			this.activeOrder = null;
			Program.DisplayOrder (null);
			order.Processed = false;
			order.Cancelled = false;
			order.Error = false;

			if (order.DontArchive) {
				Program.Log ("Order", "Not archived. The order is either repeated and already archived or stopped");
				return;
			}

			System.IO.Directory.CreateDirectory(Program.configGlobal.archivePath + System.IO.Path.DirectorySeparatorChar + order.Carrier.name);
			string savePath = Program.configGlobal.archivePath + System.IO.Path.DirectorySeparatorChar + order.Carrier.name + System.IO.Path.DirectorySeparatorChar + order.FileInfo.Name;
			if (System.IO.File.Exists (savePath))
				System.IO.File.Delete (savePath);
			System.IO.File.Move (order.FileInfo.FullName, savePath);
			order.FileInfo = new System.IO.FileInfo (savePath);
			order.DontArchive = true;
			Program.DisplayOrder (null);
			Program.Log ("Order", "Moved to archive.");
		}


		// ========== Send Order ==========
		/** Sends an order that is on hold (this is typically used when an order is invalid and needs to be corrected by the user). **/
		public void SendOrder () {
			this.waitingForUser = false;
		}


		// ========== Skip Order ==========
		/** Skips the active order is there is one by setting it's cancelled boolean to true, this is most effective for orders on hold but if used quick enough, it can cancel orders before they are sent. **/
		public void SkipOrder () {
			if (this.activeOrder == null)
				return;
			if(!this.activeOrder.Processed)
				this.activeOrder.Cancelled = true;
		}


		// ========== Repeat Order ==========
		/** Repeats the last order. This order must be archived first. **/
		public void RepeatOrder () {
			if (this.lastOrder == null) {
				Program.LogWarning("Order", "No orders have been ran since UberDespatch was started.");
				return;
			}
			Thread repeatThread = new Thread (new ThreadStart(delegate {
				this.ScheduleOrder (this.lastOrder);
			}));
			Program.Log ("Order", "Repeating last order...");
			repeatThread.Start ();
		}


		// ========== Schedule Order ==========
		/** Schedules the provided order to be next. **/
		public void ScheduleOrder (Order order) {
			this.orderScheduled = true;
			if (this.activeOrder != null) {
				Program.LogAlert ("Order", "An order is already active, this will be stopped (skipped but not archived so that it can run again) and the order before will be ran instead...");
				this.activeOrder.DontArchive = true;
				this.activeOrder.Cancelled = true;
			}
			while (this.activeOrder != null) {
				Thread.Sleep (1000);
			}
			this.CheckForOrder (order);
			this.orderScheduled = false;
		}


		// ========== Auto Send ==========
		/** Toggle auto send, when enabled, every order will be automatically sent to the carrier, if disabled the Send Order needs to be pressed. **/
		public bool ToggleAutoSend()
		{
			this.autoSend = !this.autoSend;
			if (this.autoSend) {
				Program.Log("Order", "Auto Send enabled, all orders will be immediately sent to the carrier.");
				this.SendOrder ();
			}
			else
				Program.Log("Order", "Auto Send disabled, you will need to manually click Send Order to send it to the carrier, this will allow for editing the order.");
			return this.autoSend;
		}


		// ========== Load Order ==========
		/** Loads an order from the specified path and schedules it to process next (after the current order). **/
		public void LoadOrder(string path) {
			Thread loadOrderThread = new Thread(new ThreadStart(delegate {
				Program.Log("Order", "Loading order from file: " + path);
				Order order = Program.wms.LoadOrder(path);
				if (order == null)
					return;
				order.DontArchive = true;
				this.ScheduleOrder(order);
			}));
			loadOrderThread.Start ();
		}


		// ========== On Quit ==========
		public void OnQuit () {
			if (this.activeOrder != null) {
				Program.Log ("Order", "Cancelling active order...");
				Program.UpdateState ("order-skip");
				this.activeOrder.Cancelled = true;
				this.activeOrder = null;
			}
		}
	}
}

