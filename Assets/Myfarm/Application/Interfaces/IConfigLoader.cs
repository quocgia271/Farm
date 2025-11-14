using MyFarm.Domain.Configs;
using System.Collections.Generic;

namespace MyFarm.Application.Interfaces
{
    /// <summary>
    /// Interface này chịu trách nhiệm ĐỌC dữ liệu tĩnh (từ CSV)
    /// Đây là "S" - Single Responsibility
    /// </summary>
    public interface IConfigLoader
    {
        // Production & Item
        ProductionConfig GetProductionConfig(string id);
        ItemConfig GetItemConfig(string id);
        IEnumerable<ProductionConfig> GetAllProductionConfigs();
        IEnumerable<ItemConfig> GetAllItemConfigs();

        // Farm
        FarmConfig GetFarmConfig(int id);
        IEnumerable<FarmConfig> GetAllFarmConfigs();

        // Worker
        WorkerConfig GetWorkerConfig(int id);
        IEnumerable<WorkerConfig> GetAllWorkerConfigs();

        // Equipment
        EquipmentConfig GetEquipmentConfig(int id);
        IEnumerable<EquipmentConfig> GetAllEquipmentConfigs();
    }
}
