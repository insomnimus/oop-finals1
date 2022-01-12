using System;
using System.Linq;
using PaymentApp;

namespace PaymentApp.Cmd;

public abstract class Cmd {
	public Cmd(string dbPath) {
		this.repo = new ItemRepository(dbPath);
		this.DBPath = dbPath;
	}

	internal protected ItemRepository repo;
	protected string DBPath;
	protected abstract string helpMsg { get; }
	protected abstract string prompt { get; }

	protected abstract void execute(string cmd, string arg);

	public void Run() {
		while (true) {
			try {
				var cmd = this.ReadPrompt(this.prompt);

				if (string.IsNullOrEmpty(cmd)) continue;

				var split = cmd.Split(' ', 2);
				if (string.Equals(split[0], "help", StringComparison.OrdinalIgnoreCase)) {
					Console.WriteLine(this.helpMsg);
				} else if (string.Equals(split[0], "list", StringComparison.OrdinalIgnoreCase)) {
					this.ListItems();
				} else if (string.Equals(split[0], "exit", StringComparison.OrdinalIgnoreCase)) {
					Console.WriteLine("Exiting...");
					return;
				} else {
					var arg = "";
					if (split.Length > 1) arg = split[1].Trim();
					this.execute(split[0], arg);
				}
			} catch (Exception e) {
				Console.WriteLine($"error: {e}");
			}
		}
	}

	internal string ReadPrompt(string msg) {
		Console.Write(msg);
		return Console.ReadLine()?.Trim() ?? "";
	}

	internal ulong ReadUlong(string msg) {
		while (true) {
			var s = this.ReadPrompt(msg);
			if (string.IsNullOrEmpty(s)) continue;
			ulong n = 0;
			if (ulong.TryParse(s, out n)) return n;

			Console.WriteLine($"{s}: value must be a positive integer or 0.");
		}
	}

	internal void ListItems() {
		if (this.repo.Count <= 0) {
			Console.WriteLine("There are no available items.");
			return;
		}
		var items = this.repo.Items.OrderBy(x => x.Item.ID);
		foreach (var x in items) {
			Console.WriteLine($"#{x.Item.ID}: price = {x.Item.Price}; weight = {x.Item.ShipmentWeight};\n   {x.Item.Description}");
		}
	}
}
