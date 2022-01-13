using System;
using System.Collections.Generic;
using CommandLine;
using PaymentApp.Cmd;

namespace PaymentApp;

abstract class Opts {
	[Option('p', "database", Required = true, HelpText = "Path to a json file containing the items.")] public string? DBPath { get; set; }

	public abstract int Run();
}

[Verb("admin", HelpText = "Admin login.")]
class AdminLogin: Opts {
	public override int Run() {
		var cmd = new Admin(this.DBPath);
		cmd.Run();
		return 0;
	}
}

[Verb("customer", HelpText = "Customer login.")]
class CustomerLogin: Opts {
	[Option('n', "name", Required = true, HelpText = "Name of the user.")]
	public string? Name { get; set; }
	[Option('a', "address", Required = true, HelpText = "Shipment address.")]
	public string? Address { get; set; }

	public override int Run() {
		var cmd = new User(this.DBPath, this.Name, this.Address);
		cmd.Run();
		return 0;
	}
}

[Verb("db", HelpText = "Generate a simple json file containing a few items.")]
class DBOpts: Opts {
	public override int Run() {
		var repo = new ItemRepository();
		var tax = new TaxStatus(12.5);
		var items = new List<Item>() {
			new Item{ShipmentWeight = 1200, Description = "MSI mechanical keyboard", Price = 12345, ID = 1},
			new Item{ShipmentWeight = 2500, Description = "Office Chair", Price = 30000, ID = 2},
			new Item{ShipmentWeight = 35, Description = "Rotring .5 mechanical pen", Price = 25000, ID = 3}
		};
		foreach (var x in items) {
			repo.AddItem(x, tax);
		}
		Console.WriteLine($"Writing file to {this.DBPath}");
		repo.ExportToFile(this.DBPath);
		return 0;
	}
}

class Program {
	public static int Main(string[] argv) {
		try {
			int code = CommandLine.Parser.Default.ParseArguments<AdminLogin, CustomerLogin, DBOpts>(argv)
			.MapResult(
			(AdminLogin opts) => opts.Run(),
			(CustomerLogin opts) => opts.Run(),
			(DBOpts opts) => opts.Run(),
			errs => 2
			);
			return code;
		} catch (Exception e) {
			Console.Error.WriteLine($"error: {e}");
			return 2;
		}
	}
}
