using Newtonsoft.Json;

namespace MyFarm.Domain.Configs
{
    public class ItemConfig
    {
        [JsonProperty] public string ItemId { get; private set; }
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public long SellPrice { get; private set; }

        [JsonConstructor]
        public ItemConfig(string itemId, string name, long sellPrice)
        {
            ItemId = itemId;
            Name = name;
            SellPrice = sellPrice;
        }
    }
}