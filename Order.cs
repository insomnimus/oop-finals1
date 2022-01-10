using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace PaymentApp;

public enum OrderStatus {
	NotFinalized,
	Queued,
	Failed,
	Shipping,
	Completed
}

public struct TaxStatus {
	public TaxStatus(double percentage) {
		if (percentage < 0.0) throw new ArgumentException($"tax percetnage can't be negative (value: {percentage})");
		this._percentage = percentage;
	}

	private double _percentage;
	public double Percentage {
		get => this._percentage;
		set {
			// Value must be positive.
			if (value < 0.0) throw new ArgumentException($"tax cannot be negative (input: {value}");
			else this._percentage = value;
		}
	}

	public ulong Calc(ulong price) => (ulong)(this._percentage * price / 100);
}

public class Order {
	public Order(Customer customer) => this.Customer = customer;

	public DateTime Date { get; private set; } = DateTime.Now;
	public OrderStatus Status { get; set; } = OrderStatus.NotFinalized;
	public Customer Customer { get; private set; }
	public Payment? Payment { get; private set; }
	private List<OrderDetails> _ordered = new List<OrderDetails>() { };
	public ReadOnlyCollection<OrderDetails> Ordered => this._ordered.AsReadOnly();

	public ulong CalcTax() {
		ulong total = 0;
		foreach (var x in this._ordered) total += x.TotalTax;
		return total;
	}

	public ulong CalcTotalWeight() {
		ulong total = 0;
		foreach (var x in this._ordered) total += x.CalcWeight();
		return total;
	}

	public void Add(ItemInfo item, uint quantity) {
		if (this.Status != OrderStatus.NotFinalized) throw new InvalidOperationException("the order is already finalized or failed");
		var details = OrderDetails.FromItemInfo(item, quantity);
		this._ordered.Add(details);
	}

	public void Finalize(CreditCard payment) {
		if (this.Status != OrderStatus.NotFinalized) throw new InvalidOperationException("the order is already finalized/failed");
		else if (this._ordered.Count == 0) throw new InvalidOperationException("there are 0 orders");

		this.Payment = payment;
		if (payment.Authorized) this.Status = OrderStatus.Queued;
		else this.Status = OrderStatus.NotFinalized;
	}

	public void Finalize(Check payment) {
		if (this.Status != OrderStatus.NotFinalized) throw new InvalidOperationException("the order is already finalized/failed");
		else if (this._ordered.Count == 0) throw new InvalidOperationException("there are 0 orders");
		this.Payment = payment;

		if (payment.Authorized) this.Status = OrderStatus.Queued;
		else this.Status = OrderStatus.NotFinalized;
	}

	public void Finalize(Cash payment) {
		if (this.Status != OrderStatus.NotFinalized) throw new InvalidOperationException("the order is already finalized/failed");
		else if (this._ordered.Count == 0) throw new InvalidOperationException("there are 0 orders");
		this.Payment = payment;

		this.Status = OrderStatus.Queued;
		this.Payment = payment;
	}
}

public class OrderDetails {
	public OrderDetails(Item item, uint quantity, TaxStatus tax) {
		if (quantity == 0) throw new ArgumentException("can't create an OrderDetails object with Quantity = 0");
		this.Quantity = quantity;
		this.Item = item;
		this.TaxStatus = tax;
	}

	public uint Quantity { get; private set; }
	public TaxStatus TaxStatus { get; private set; }
	public Item Item;

	public ulong TotalTax => this.TaxStatus.Calc(this.Item.Price * this.Quantity);
	public ulong CalcWeight() => this.Quantity * this.Item.ShipmentWeight;

	public static OrderDetails FromItemInfo(ItemInfo info, uint quantity) => new OrderDetails(info.Item, quantity, info.Tax);
}
