using Newtonsoft.Json;
namespace MyFarm.Domain.Configs
{
    public class FarmConfig
    {
      [JsonProperty]  public int Id { get; private set; }
      [JsonProperty]  public int LandPlots { get; private set; }
      
      // --- SỬA Ở ĐÂY ---
      [JsonProperty] public int CarrotSeeds { get; private set; } // THAY a
      [JsonProperty] public int BroccoliSeeds { get; private set; } // THAY b
      [JsonProperty] public int CauliflowerSeeds { get; private set; } // THÊM MỚI
      [JsonProperty] public int Cows { get; private set; } // GIỮ NGUYÊN
      // --- HẾT SỬA ---

      [JsonProperty]  public int Workers { get; private set; }
      [JsonProperty] public int EquipmentLevel { get; private set; }
      [JsonProperty] public long Gold { get; private set; }
      [JsonProperty] public long GoldtoWin { get; private set; } // ĐỔI TÊN
      [JsonProperty] public int PlotPrice { get; private set; } // THÊM MỚI

        [JsonConstructor]
        // --- SỬA CONSTRUCTOR ---
        public FarmConfig(int id, int landPlots, 
            int carrotSeeds, int broccoliSeeds, int cauliflowerSeeds, int cows, // THAY ĐỔI DÒNG NÀY
            int workers, int equipmentLevel, long gold, long goldtoWin, int plotPrice)
        {
            Id = id;
            LandPlots = landPlots;
            
            // --- SỬA PHÉP GÁN ---
            CarrotSeeds = carrotSeeds;
            BroccoliSeeds = broccoliSeeds;
            CauliflowerSeeds = cauliflowerSeeds;
            Cows = cows;
            
            Workers = workers;
            EquipmentLevel = equipmentLevel;
            Gold = gold;
            GoldtoWin = goldtoWin; // ĐỔI TÊN
            PlotPrice = plotPrice; // THÊM MỚI
        }
    }

    public class WorkerConfig
    {
    [JsonProperty]     public int Id { get; private set; }
    [JsonProperty]     public string Name { get; private set; }
    [JsonProperty]     public int ActionTimeMinutes { get; private set; }
    [JsonProperty] public long HirePrice { get; private set; }

        [JsonConstructor]
        public WorkerConfig(int id, string name, int actionTimeMinutes, long hirePrice)
        {
            Id = id;
            Name = name;
            ActionTimeMinutes = actionTimeMinutes;
            HirePrice = hirePrice;
        }
    }

    public class EquipmentConfig
    {
    [JsonProperty]     public int Id { get; private set; }
    [JsonProperty]     public string Name { get; private set; }
    [JsonProperty]     public long UpgradePrice { get; private set; }
    [JsonProperty]     public int ProductionBoostPercent { get; private set; } // Sửa đây thành int
    [JsonConstructor]
        public EquipmentConfig(int id, string name, long upgradePrice, int productionBoostPercent) // Sửa đây thành int
        {
            Id = id;
            Name = name;
            UpgradePrice = upgradePrice;
            ProductionBoostPercent = productionBoostPercent;
        }
    }
}