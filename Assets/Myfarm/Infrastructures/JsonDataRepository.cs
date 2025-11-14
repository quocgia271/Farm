using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace MyFarm.Infrastructure
{
    // Triển khai IGameDataRepository
    // Cách triển khai này dùng file Json và lưu vào Application.persistentDataPath
    public class JsonDataRepository : IGameDataRepository
    {
        private readonly string _playerSavePath;
        private readonly string _farmSavePath;
        
        // Chúng ta sẽ "cache" dữ liệu đã load để Tầng 2 không cần load file mỗi lần gọi
        private Player _playerCache;
        private Farm _farmCache;

        private readonly IConfigLoader _configLoader; // Thêm dependency để load CSV
        
        private JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            // Bảo Json lưu cả thông tin Type để nó biết cách tạo lại Config
            TypeNameHandling = TypeNameHandling.Auto 
        };

        // Constructor mới: nhận CsvConfigLoader từ tầng 3
        public JsonDataRepository(IConfigLoader configLoader)
        {
            _configLoader = configLoader;
            _playerSavePath = Path.Combine(UnityEngine.Application.persistentDataPath, "player.json");
            _farmSavePath = Path.Combine(UnityEngine.Application.persistentDataPath, "farm.json");
            
            Debug.Log($"Lưu game tại: {UnityEngine.Application.persistentDataPath}");
        }

        public Player LoadPlayer()
        {
            if (_playerCache != null) return _playerCache;
            
            if (!File.Exists(_playerSavePath))
            {
                // --- Cũ: hardcode player ---
                // _playerCache = new Player(initialGold: 0);
                // _playerCache.AddItem("tomato_seed", 10);
                // _playerCache.AddItem("blueberry_seed", 10);
                // _playerCache.AddItem("cow", 2);
                
                // --- Mới: khởi tạo từ CSV ---
                //var farmConfig = _configLoader.GetFarmConfig(1); // Lấy config mặc định id = 1
               // _playerCache = new Player(farmConfig.Gold);
                //_playerCache.AddItem("tomato_seed", farmConfig.TomatoSeeds);
               // Debug.Log(farmConfig.TomatoSeeds);
                //_playerCache.AddItem("blueberry_seed", farmConfig.BlueberrySeeds);
                //_playerCache.AddItem("cow", farmConfig.Cows);

               // SavePlayer(_playerCache); // Lưu lại
               return null;
            }
            else
            {
                var json = File.ReadAllText(_playerSavePath);
                _playerCache = JsonConvert.DeserializeObject<Player>(json, _settings);
            }
            return _playerCache;
        }

        public void SavePlayer(Player player)
        {
            _playerCache = player; // Cập nhật cache
            var json = JsonConvert.SerializeObject(player, Formatting.Indented, _settings);
            File.WriteAllText(_playerSavePath, json);
        }

        public Farm LoadFarm()
        {
            if (_farmCache != null) return _farmCache;

            if (!File.Exists(_farmSavePath))
            {
                // --- Cũ: hardcode farm ---
                // _farmCache = new Farm();
                // _farmCache.AddWorker();

                // --- Mới: khởi tạo từ CSV ---
                //var farmConfig = _configLoader.GetFarmConfig(1);
                //_farmCache = new Farm();

                // Tạo plots
                //for (int i = 0; i < farmConfig.LandPlots; i++)
                //    _farmCache.AddPlot();

                // Tạo workers
                //for (int i = 0; i < farmConfig.Workers; i++)
                //    _farmCache.AddWorker();

                // Thiết bị
                //while (_farmCache.EquipmentLevel < farmConfig.EquipmentLevel)
                //    _farmCache.UpgradeEquipment();

                //SaveFarm(_farmCache);
                return null;
            }
            else
            {
                var json = File.ReadAllText(_farmSavePath);
                _farmCache = JsonConvert.DeserializeObject<Farm>(json, _settings);
            }
            return _farmCache;
        }

        public void SaveFarm(Farm farm)
        {
            _farmCache = farm;
            var json = JsonConvert.SerializeObject(farm, Formatting.Indented, _settings);
            File.WriteAllText(_farmSavePath, json);
        }
    }
}
