using System;
using Newtonsoft.Json;
namespace MyFarm.Domain.Configs
{
    /// <summary>
    /// Cấu hình cho bất kỳ thứ gì có thể sản xuất (Cà chua, Bò, Dâu tây...)
    /// </summary>
    public class ProductionConfig
    {
        [JsonProperty] public string Name;
    [JsonProperty]     public string Id { get; private set; }             // VD: "tomato_seed", "cow"
    [JsonProperty]     public string ProductId { get; private set; }      // VD: "tomato_fruit", "milk"
    [JsonProperty]     public TimeSpan GrowthTime { get; private set; }   // Thời gian chờ ra sản phẩm (VD: 10 phút)
    [JsonProperty]     public int MaxHarvestTimes { get; private set; }   // Vòng đời (VD: 40 lần)
    [JsonProperty]     public long BuyPrice { get; private set; }
    [JsonProperty]     public int MinAmountToBuy { get; private set; }

    [JsonProperty]     public bool Wholesale { get; private set; }
    [JsonProperty]     public int PriceBuyWholesale { get; private set; }
    
    [JsonConstructor] 
        public ProductionConfig(string name,string id, string productId, double growthTimeMinutes, int maxHarvestTimes, long buyPrice, int minAmountToBuy,bool wholesale,int priceBuyWholesale)
        {
            Name = name;
            Id = id;
            ProductId = productId;
            GrowthTime = TimeSpan.FromMinutes(growthTimeMinutes);
            MaxHarvestTimes = maxHarvestTimes;
            BuyPrice = buyPrice;
            MinAmountToBuy = minAmountToBuy;
            Wholesale = wholesale;
            PriceBuyWholesale = priceBuyWholesale;
        }
    }
}