using System;
using PaymentApp;

namespace PaymentApp.Cmd;

public class User: Cmd {
	public User(string dbPath, string name, string addr) : base(dbPath) {
		this.customer = new Customer(name, addr);
	}

	private Customer customer;
	protected override string prompt => "USER> ";
	protected override string helpMsg => @"Commands:
list: List available products.
exit: Exit the app.
buy <id>: Buy an item.";

	internal override void onStartup() => Console.WriteLine($"Welcome, {this.customer.Name}\n{this.helpMsg}");

	protected override void execute(string cmd, string arg) {
		switch (cmd.ToLower()) {
			case "buy" when string.IsNullOrEmpty(arg):
				Console.WriteLine("You need to specify an id to buy an item.");
				break;
			case "buy" when this.repo.Count == 0:
				Console.WriteLine("Sorry, therre are no items for sale at the moment.");
				break;
			case "buy":
				ulong id = 0;
				if (ulong.TryParse(arg, out id)) this.buyItem(id);
				else Console.WriteLine($"{arg}: the value must be a positive integer or 0.");
				break;
			default:
				Console.WriteLine($"{arg} is not recognized as a known command. Run `help` for the list of available commands.");
				break;
		}
	}

	private void buyItem(ulong id) {
		var items = this.repo.GetItemsByID(id);
		if (items.Count < 1) {
			Console.WriteLine($"Sorry, the item with the id {id} does not exist. Run `list` to see the list of available items.");
			return;
		}
		var x = items[0];


		Console.WriteLine($"Buying '{x.Item.Description}'\nPrice per purchase is {x.Item.Price} cents and there's {x.Tax} tax.");
		var quantity = base.ReadUlong("How many would you like to buy? (0 = cancel): ");
		if (quantity == 0) {
			Console.WriteLine("Cancelled...");
			return;
		}
		var order = new Order(this.customer);
		order.Add(x, (uint)quantity);

		Console.WriteLine($"Total Price (including tax): {order.CalcTotalPrice()} cents.");
		this.finalizeOrder(order);
	}

	private void finalizeOrder(Order order) {
		Console.WriteLine(@"How would you like to pay?
		0) 1) 2) Check
3) Cash[not available online]");


		while (true) {
			switch (this.ReadUlong("[0-3]: ")) {
				case 0:
					Console.WriteLine("Cancelled...");
					return;
				case 1:
					var cred = this.readCCInfo(order.CalcTotalPrice());
					order.Finalize(cred);
					Console.WriteLine("Success");
					return;
				case 2:
					var check = this.readCheckInfo(order.CalcTotalPrice());
					order.Finalize(check);
					Console.WriteLine("Success");
					return;
				case 3:
					Console.WriteLine("Cash payment is not available through the app.");
					break;
				default:
					Console.WriteLine("Please specify one 0, 1 or 2.");
					break;
			}
		}
	}

	private CreditCard readCCInfo(ulong amount) {
		Console.WriteLine(@"Choose a card type:
1) MasterCard
2) Visa");

		CardType cc;
		while (true) {
			var n = this.ReadUlong("[1 or 2]: ");
			if (n == 1) {
				cc = CardType.MasterCard;
				break;
			}
			if (n == 2) {
				cc = CardType.Visa;
				break;
			}
			Console.WriteLine("Please specify 1 or 2.");
		}

		var no = this.ReadUlong("Card number: ");
		var exp = this.readExpiry();
		return new CreditCard(amount, (uint)no, cc, exp);
	}

	private Check readCheckInfo(ulong amount) {
		var bank = this.ReadPrompt("Bank Name: ");
		return new Check(amount, this.customer.Name, bank);
	}

	private Expiry readExpiry() {
		while (true) {
			var s = this.ReadPrompt("Expiry Date (MM/YY): ");
			var split = s.Split('/', 2);
			if (split.Length != 2) {
				Console.WriteLine("Please enter in the form MM/YY. Example: 01/22.");
				continue;
			}

			byte m = 0;
			ushort y = 0;
			if (!byte.TryParse(split[0].Trim(), out m)
			|| !ushort.TryParse(split[1].Trim(), out y)) {
				Console.WriteLine("Please enter in the form MM/YY. Example: 01/22.");
				continue;
			}
			if (m == 0 || m > 12) {
				Console.WriteLine("The month value must be between 1 and 12 inclusive.");
				continue;
			}
			if (y < 100) y += 2000;
			return new Expiry(m, y);
		}
	}
}
