using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PaymentApp;

public struct Item {
	public uint ShipmentWeight { get; set; }
	public string Description { get; set; }
	public ulong Price { get; set; }
	public ulong ID;

	public ulong GetPriceForQuantity(uint quantity) => this.Price * quantity;
}

public struct ItemInfo {
	public TaxStatus Tax;
	public Item Item;

	public ItemInfo(Item item, TaxStatus tax) => (this.Item, this.Tax) = (item, tax);
}

public class ItemRepository {
	public ItemRepository() { }

	public ItemRepository(string jsonFile) {
		var data = File.ReadAllText(jsonFile);
		var parsed = JsonConvert.DeserializeObject<Dictionary<ulong, ItemInfo>>(data);
		if (parsed is not null) this.items = parsed;
	}

	private Dictionary<ulong, ItemInfo> items = new Dictionary<ulong, ItemInfo>() { };
	public ICollection<ItemInfo> Items => this.items.Values;
	public int Count => this.items.Count;

	public void ExportToFile(string file) {
		var data = JsonConvert.SerializeObject(this.items, Formatting.Indented);
		File.WriteAllText(file, data);
	}

	public void AddItem(Item item, TaxStatus tax) => this.items.Add(item.ID, new ItemInfo(item, tax));
	public void AddItem(ItemInfo info) => this.items.Add(info.Item.ID, info);

	public ItemInfo GetInfo(ulong id) => this.items[id];
	public TaxStatus GetTaxStatus(ulong id) => this.items[id].Tax;

	public List<ItemInfo> GetItemsByID(params ulong[] ids) {
		var xs = new List<ItemInfo>() { };
		foreach (var id in ids) {
			var info = default(ItemInfo);
			if (this.items.TryGetValue(id, out info)) xs.Add(info);
		}
		return xs;
	}

	public int InsertOrReplaceItems(params ItemInfo[] values) {
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

	public int RemoveItemsByID(params ulong[] ids) {
		int removed = 0;
		foreach (var id in ids) {
			if (this.items.Remove(id)) removed++;
		}
		return removed;
	}
}
