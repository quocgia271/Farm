using System;
using System.Collections.Generic;
using Newtonsoft.Json; // <-- Sẽ không còn báo lỗi

namespace MyFarm.Domain.Models
{
    public class Player
    {
        [JsonProperty]
        public long Gold { get; private set; }
        
        [JsonProperty]
        private readonly Dictionary<string, int> _inventory = new Dictionary<string, int>();
        
        [JsonIgnore]
        public IReadOnlyDictionary<string, int> Inventory => _inventory;

        // Thêm constructor rỗng để Json có thể tạo mới khi tải
        public Player() { } 
        
        public Player(long initialGold)
        {
            Gold = initialGold;
        }

        // ... (phần còn lại của code AddGold, TrySpendGold...)
        public void AddGold(long amount)
        {
            if (amount < 0) throw new ArgumentException("Cannot add negative gold");
            Gold += amount;
        }

        public bool TrySpendGold(long amount)
        {
            if (amount < 0) return false;
            if (Gold >= amount)
            {
                Gold -= amount;
                return true;
            }
            return false;
        }

        public void AddItem(string itemId, int amount = 1)
        {
             if (!_inventory.ContainsKey(itemId)) _inventory[itemId] = 0;
             _inventory[itemId] += amount;
        }

        public bool TryRemoveItem(string itemId, int amount = 1)
        {
            if (_inventory.TryGetValue(itemId, out int currentAmount) && currentAmount >= amount)
            {
                _inventory[itemId] -= amount;
                if (_inventory[itemId] == 0) _inventory.Remove(itemId);
                return true;
            }
            return false;
        }
        
        public int GetItemCount(string itemId)
        {
            return _inventory.TryGetValue(itemId, out int count) ? count : 0;
        }
    }
}