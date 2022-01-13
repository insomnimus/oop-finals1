using System;

namespace PaymentApp;

public enum CardType {
	MasterCard,
	Visa
}

public struct Expiry {
	public Expiry(byte month, ushort year) {
		if (month == 0 || month > 12) throw new ArgumentException($"month cannot be {month}");
		this._month = month;
		this.Year = year;
	}

	private byte _month;


	public ushort Year { get; set; }
	public byte Month {
		get => this._month;
		set {
			if (value == 0 || value > 12) throw new ArgumentException($"month can't be {value}");
			this._month = value;
		}
	}

	public override string ToString() => $"{this.Month:D2}/{this.Year}";
}

public abstract class Payment {
	public Payment(ulong amount) => this.Amount = amount;

	public ulong Amount { get; internal set; }
}

public class CreditCard: Payment {
	public CreditCard(ulong amount, uint no, CardType typ, Expiry expiry)
	: base(amount) {
		this.No = no;
		this.Type = typ;
		this.Expiry = expiry;
	}

	public uint No { get; private set; }
	public CardType Type { get; private set; }
	public Expiry Expiry { get; private set; }
	public bool Authorized { get; set; } = false;
}

public class Check: Payment {
	public Check(ulong amount, string name, string bank)
	: base(amount) {
		this.Name = name;
		this.Bank = bank;
	}

	public string Name { get; private set; }
	public string Bank { get; private set; }
	public bool Authorized { get; set; } = false;
}

public class Cash: Payment {
	public Cash(ulong amount, ulong cashQuantity) : base(amount) {
		this.CashQuantity = cashQuantity;
	}

	public ulong CashQuantity { get; private set; }
}
