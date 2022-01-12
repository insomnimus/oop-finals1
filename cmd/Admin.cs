using System;
using PaymentApp;

namespace PaymentApp.Cmd;

public class Admin: Cmd {
	public Admin(string dbPath) : base(dbPath) { }
	protected override string prompt => "ADMIN> ";
	protected override string helpMsg => @"Commands:
list: List available items.
remove <id>: Remove an item.
set <id>: Create or modify an item.
save [path]: Save the state of the database. You can omit the path parameter to apply the changes to the opened file.
exit: Exit the app without saving. You can also use ctrl+c.";

	protected override void execute(string cmd, string arg) {
		try {
			switch (cmd.ToLower()) {
				case "list":
					base.ListItems();
					break;
				case "save":
					var path = string.IsNullOrEmpty(arg) ? base.DBPath : arg;
					this.save(path);
					break;
				case "remove" when this.repo.Count <= 0:
					Console.WriteLine("There are no items to remove. Add some with `set`.");
					break;
				case "remove":
					ulong id = 0;
					if (string.IsNullOrEmpty(arg)) Console.WriteLine("you need to specify an id");
					else if (ulong.TryParse(arg, out id)) {
						if (this.repo.RemoveItemsByID(id) == 0) Console.WriteLine($"There is no item with the id {id}.");
						else Console.WriteLine($"Removed the item with the id {id}.");
					} else {
						Console.WriteLine($"{arg} is not a valid id; the value must be a positive integer or 0.");
					}
					break;
				case "set":
					if (string.IsNullOrEmpty(arg)) Console.WriteLine("You must specify an id.");
					else if (ulong.TryParse(arg, out id)) this.setItem(id);
					else Console.WriteLine($"{arg} is not a valid id; the value must be a positive integer or 0.");
					break;
				default:
					Console.WriteLine($"{cmd} is not recognized as a command. Type `help` to see available commands.");
					break;
			}
		} catch (Exception e) {
			Console.WriteLine($"error: {e}");
		}
	}

	private void setItem(ulong id) {
		var desc = base.ReadPrompt("Item Description: ");
		var price = base.ReadUlong("Item Price (in cents): ");
		var weight = base.ReadUlong("Shipment Weight (in grams): ");
		var tax = this.readTax();
		var item = new Item {
			ShipmentWeight = (uint)weight,
			Description = desc,
			Price = price,
			ID = id
		};
		var info = new ItemInfo(item, tax);

		if (this.repo.InsertOrReplaceItems(info) == 0)
			Console.WriteLine("Modified 1 item.");
		else
			Console.WriteLine("Added 1 item.");
	}

	private TaxStatus readTax() {
		while (true) {
			var s = base.ReadPrompt("Tax Status (at least 0.0, percentage): ");
			double tax = 0;
			if (string.IsNullOrEmpty(s)) continue;
			if (double.TryParse(s, out tax) && tax >= 0.0)
				return new TaxStatus(tax);

			Console.WriteLine("Value is invalid.");
		}
	}

	private void save(string path) {
		try {
			Console.WriteLine($"Saving the state of the database to {path}...");
			this.repo.ExportToFile(path);
			Console.WriteLine("Success.");
		} catch (Exception e) {
			Console.WriteLine($"error: {e}");
		}
	}
}
