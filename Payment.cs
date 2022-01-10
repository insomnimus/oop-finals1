using System;

namespace PaymentApp;

public enum CardType {
	MasterCard,
	Visa
}

public enum Bank {
	IsBank,
	AkBank,
	ZiraatBank,
	GarantiBank
}

public abstract class Payment {
	public Payment(ulong amount) => this.Amount = amount;

	public ulong Amount { get; internal set; }
}

public class CreditCard: Payment {
	public CreditCard(ulong amount, uint no, CardType typ, DateTime expiry)
	: base(amount) {
		this.No = no;
		this.Type = typ;
		this.Expiry = expiry;
	}

	public uint No { get; private set; }
	public CardType Type { get; private set; }
	public DateTime Expriy { get; private set; }
	private bool _authorized = false;

	public bool AuthorizePayment() {
		if (this._authorized) return true;
		// Authorize payment here.
		if (this.Expiry < DateTime.Now) return false;

		this._authorized = true;
		return true;
	}
}

public class Check: Payment {
	public Check(ulong amount, string name, Bank bank)
	: base(amount) {
		this.Name = name;
		this.Bank = bank;
	}

	public string Name { get; private set; }
	public Bank Bank { get; private set; }
	private bool _authorized = false;

	public bool Authorized => this._authorized;
	public bool Authorize() {
		if (this._authorized) return true;
		// Authorize payment here.
		this._authorized = true;
		return true;
	}
}

public class Cash: Payment {
	public Cash(ulong amount) : base(amount) { }

	// public uint CashQuantity {get; private set;}
}
