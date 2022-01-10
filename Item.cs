using System;
using System.IO.File;
using System.Text.Json;
using System.Collections.Generic;

namespace PaymentApp;

public struct Item {
	public uint ShippingWeight { get; set; }
	public string Description { get; set; }
	public ulong Price { get; set; }
	public ulong ID;

	public ulong GetPriceForQuantity(uint quantity) => this.price * quantity;
}

public struct ItemInfo {
	public TaxState Tax;
	public Item Item;

	public ItemInfo(Item item, TaxState tax) => (this.Item, this.Tax) = (item, tax);
}

public class ItemRepository {
	public ItemRepository() {
		this.items = new Dictionary<ulong, ItemInfo>() { };
	}

	public ItemRepository(string jsonFile) {
		var data = File.ReadAllBytes(jsonFile);
		this.items = JsonSerializer.Deserialize < Dictionary<ulong, ItemInfo>(new ReadOnlySpan<byte>(data));
	}

	private Dictionary<ulong, ItemInfo> Items;
	public Dictionary<ulong, ItemInfo>.ValueCollection Items => this.items.Values;
	public int Count => this.items.Count;

	public void ExportToFile(string file) {
		var writer = File.Create(file);
		var options = new JsonSerializerOptions { WriteIndented = true };
		JsonSerializer.Serialize(writer, this.items, typeof(Dictionary<ulong, ItemInfo>), options);
		writer.Close();
	}

	public void AddItem(Item item, TaxStatus tax) => this.items.Add(item.ID, new ItemInfo(item, tax));
	public void AddItem(ItemInfo info) => this.items.Add(info.Item.ID, info);

	public ItemInfo GetInfo(ulong id) => this.items[id];
	public TaxStatus GetTaxStatus(ulong id) => this.items[id].Tax;

	public List<ItemInfo> GetItemsByID(params ulong ids) {
		var xs = new List<ItemInfo>() { };
		foreach (var id in ids) {
			var info = default(ItemInfo);
			if (this.items.TryGetValue(id, out info)) xs.Add(info);
		}
		return xs;
	}

	public int InsertOrReplaceItems(params ItemInfo values) {
		var added = 0;
		foreach (var info in values) {
			if (this.items.TryAdd(info.Item.ID, info)) {
				added++;
			} else {
				this.items[info.Item.ID] = info;
			}
		}
		return added;
	}
}
