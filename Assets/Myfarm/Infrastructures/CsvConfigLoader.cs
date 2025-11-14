using MyFarm.Application.Interfaces;
using MyFarm.Domain.Configs;
using System; // Thêm để dùng Exception
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MyFarm.Infrastructure
{
    public class CsvConfigLoader : IConfigLoader
    {
        private readonly Dictionary<string, ProductionConfig> _productionConfigs = new Dictionary<string, ProductionConfig>();
        private readonly Dictionary<string, ItemConfig> _itemConfigs = new Dictionary<string, ItemConfig>();
        private readonly Dictionary<int, FarmConfig> _farmConfigs = new Dictionary<int, FarmConfig>();
        private readonly Dictionary<int, WorkerConfig> _workerConfigs = new Dictionary<int, WorkerConfig>();
        private readonly Dictionary<int, EquipmentConfig> _equipmentConfigs = new Dictionary<int, EquipmentConfig>();

        public CsvConfigLoader()
        {
            LoadProductionConfigs();
            LoadItemConfigs();
            LoadFarmConfigs();
            LoadWorkerConfigs();
            LoadEquipmentConfigs();
        }

        #region Load CSV (Đã "Bọc thép")
        
        // Hàm tiện ích đọc CSV (Đã an toàn)
        private IEnumerable<string> ReadCsv(string path)
        {
            var textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"[CsvConfigLoader] RẤT QUAN TRỌNG: Không tìm thấy file '{path}' trong 'Resources/'!");
                return Enumerable.Empty<string>();
            }
            return textAsset.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1);
        }

        private void LoadProductionConfigs()
        {
            foreach (var line in ReadCsv("Configs/production_config"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var parts = line.Trim().Split(',');
                    if (parts.Length < 9) continue; // Cần 9 cột
                    var config = new ProductionConfig(
                        name:parts[0],
                        id: parts[1],
                        productId: parts[2],
                        growthTimeMinutes: double.Parse(parts[3]), // Sửa thành double
                        maxHarvestTimes: int.Parse(parts[4]),
                        buyPrice: long.Parse(parts[5]),
                        minAmountToBuy: int.Parse(parts[6]),
                        wholesale: bool.Parse(parts[7]),
                        priceBuyWholesale: int.Parse(parts[8]) // Sửa thành int
                    );
                    _productionConfigs[config.Id] = config;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CsvConfigLoader] Lỗi đọc dòng production_config: {line}. Lỗi: {e.Message}");
                }
            }
        }

        private void LoadItemConfigs()
        {
            foreach (var line in ReadCsv("Configs/item_config"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var parts = line.Trim().Split(',');
                    if (parts.Length < 3) continue;
                    var config = new ItemConfig(parts[0], parts[1], long.Parse(parts[2]));
                    _itemConfigs[config.ItemId] = config;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CsvConfigLoader] Lỗi đọc dòng item_config: {line}. Lỗi: {e.Message}");
                }
            }
        }

        private void LoadFarmConfigs()
        {
            foreach (var line in ReadCsv("Configs/farm_config"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    // --- SỬA Ở ĐÂY ---
                    var parts = line.Trim().Split(',');
                    if (parts.Length < 11) continue; // Cần 11 cột
                    var config = new FarmConfig(
                        id: int.Parse(parts[0]),
                        landPlots: int.Parse(parts[1]),
                        carrotSeeds: int.Parse(parts[2]),      // Cột 2
                        broccoliSeeds: int.Parse(parts[3]),  // Cột 3
                        cauliflowerSeeds: int.Parse(parts[4]),// Cột 4
                        cows: int.Parse(parts[5]),              // Cột 5
                        workers: int.Parse(parts[6]),           // Cột 6
                        equipmentLevel: int.Parse(parts[7]),    // Cột 7
                        gold: long.Parse(parts[8]),             // Cột 8
                        goldtoWin: long.Parse(parts[9]),        // Cột 9
                        plotPrice : int.Parse(parts[10])         // Cột 10
                    );
                    // --- HẾT SỬA ---
                    _farmConfigs[config.Id] = config;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CsvConfigLoader] Lỗi đọc dòng farm_config: {line}. Lỗi: {e.Message}");
                }
            }
        }

        private void LoadWorkerConfigs()
        {
            foreach (var line in ReadCsv("Configs/worker_config"))
            {
                 if (string.IsNullOrWhiteSpace(line)) continue;
                 try
                 {
                    var parts = line.Trim().Split(',');
                    if (parts.Length < 4) continue;
                    var config = new WorkerConfig(
                        id: int.Parse(parts[0]),
                        name: parts[1],
                        actionTimeMinutes: int.Parse(parts[2]),
                        hirePrice: long.Parse(parts[3])
                    );
                    _workerConfigs[config.Id] = config;
                 }
                 catch (Exception e)
                 {
                    Debug.LogWarning($"[CsvConfigLoader] Lỗi đọc dòng worker_config: {line}. Lỗi: {e.Message}");
                 }
            }
        }

        private void LoadEquipmentConfigs()
        {
            foreach (var line in ReadCsv("Configs/equipment_config"))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var parts = line.Trim().Split(',');
                    if (parts.Length < 4) continue;
                    var config = new EquipmentConfig(
                        id: int.Parse(parts[0]),
                        name: parts[1],
                        upgradePrice: long.Parse(parts[2]),
                        // --- SỬA BUG Ở ĐÂY ---
                        productionBoostPercent: int.Parse(parts[3]) // Sửa thành int.Parse
                    );
                    _equipmentConfigs[config.Id] = config;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[CsvConfigLoader] Lỗi đọc dòng equipment_config: {line}. Lỗi: {e.Message}");
                }
            }
        }
        #endregion

        #region Getters
        public ProductionConfig GetProductionConfig(string id) => _productionConfigs.TryGetValue(id, out var config) ? config : null;
        public ItemConfig GetItemConfig(string id) => _itemConfigs.TryGetValue(id, out var config) ? config : null;
        public FarmConfig GetFarmConfig(int id) => _farmConfigs.TryGetValue(id, out var config) ? config : null;
        public WorkerConfig GetWorkerConfig(int id) => _workerConfigs.TryGetValue(id, out var config) ? config : null;
        public EquipmentConfig GetEquipmentConfig(int id) => _equipmentConfigs.TryGetValue(id, out var config) ? config : null;

        public IEnumerable<ProductionConfig> GetAllProductionConfigs() => _productionConfigs.Values;
        public IEnumerable<ItemConfig> GetAllItemConfigs() => _itemConfigs.Values;
        public IEnumerable<FarmConfig> GetAllFarmConfigs() => _farmConfigs.Values;
        public IEnumerable<WorkerConfig> GetAllWorkerConfigs() => _workerConfigs.Values;
        public IEnumerable<EquipmentConfig> GetAllEquipmentConfigs() => _equipmentConfigs.Values;
        #endregion
    }
}