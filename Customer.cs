namespace PaymentApp;

public struct Customer {
	public Customer(string name, string addr) => (this.Name, this.Address) = (name, addr);

	public string Name { get; set; }
	public string Address { get; set; }
}
