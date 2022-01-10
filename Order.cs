namespace PaymentApp;

public enum OrderStatus {
	NotFinalized,
	Processing,
	Failed,
	Queued,
	Shipping,
	Completed
}

public class TaxStatus {
	public TaxStatus(double percentage) => this.Percentage = percentage;

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
	public OrderStatus Status { get; private set; } = OrderStatus.NotFinalized;
	public Customer Customer;
	private Payment payment;
	private List<OrderDetails> ordered = new List<OrderDetails>() { };

	public ulong CalcTax() {
		ulong total = 0;
		foreach (var x in this.ordered) total += x.TotalTax;
		return total;
	}

	public ulong CalcTotalWeight() {
		ulong total = 0;
		foreach (var x in this.ordered) total += x.CalcWeight();
		return total;
	}

	public void AddItem(Item item, uint quantity) {
		if (quantity > 0) this.ordered.Add(new OrderDetails(item, quantity));
	}
}

public class OrderDetails {
	public uint Quantity { get; private set; }
	public TaxStatus TaxStatus { get; private set; }
	private Item item;

	public ulong TotalTax => this.TaxStatus.Calc(this.item.Price * this.Quantity);
	public ulong CalcWeight() => this.Quantity * this.item.ShipmentWeight;
}
