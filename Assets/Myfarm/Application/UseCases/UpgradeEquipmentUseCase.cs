using System;
using System.Diagnostics;
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    public class UpgradeEquipmentUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IEventNotifier _eventNotifier;
        private readonly IConfigLoader _configLoader; // --- THÊM DÒNG NÀY ---

        // --- SỬA HÀM KHỞI TẠO ---
        public UpgradeEquipmentUseCase(IGameDataRepository dataRepository, IEventNotifier eventNotifier, IConfigLoader configLoader)
        {
            _dataRepository = dataRepository;
            _eventNotifier = eventNotifier;
            _configLoader = configLoader; // --- THÊM DÒNG NÀY ---
        }

        public bool Execute()
        {
            Player player = _dataRepository.LoadPlayer();
            Farm farm = _dataRepository.LoadFarm();

            // --- SỬA LOGIC LẤY GIÁ ---
            // Lấy config của level HIỆN TẠI để biết giá nâng cấp LÊN level tiếp theo
            var currentLevelConfig = _configLoader.GetEquipmentConfig(1);
            if (currentLevelConfig == null) return false; // Không tìm thấy config

            if (player.TrySpendGold(currentLevelConfig.UpgradePrice))
            {
                farm.UpgradeEquipment();
                
                _dataRepository.SavePlayer(player);
                _dataRepository.SaveFarm(farm);
                
                _eventNotifier.NotifyGoldChanged(player.Gold);
                _eventNotifier.NotifyEquipmentUpgraded(farm.EquipmentLevel); // Báo level mới
                return true;
            }
            return false;
        }
    }
}