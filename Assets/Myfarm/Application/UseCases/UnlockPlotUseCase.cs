// File: MyFarm/2_Application/UseCases/UnlockPlotUseCase.cs
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;
using MyFarm.Domain.Enums;

namespace MyFarm.Application.UseCases
{
    public class UnlockPlotUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IConfigLoader _configLoader;
        private readonly IEventNotifier _eventNotifier;

        public UnlockPlotUseCase(IGameDataRepository dataRepository, IConfigLoader configLoader, IEventNotifier eventNotifier)
        {
            _dataRepository = dataRepository;
            _configLoader = configLoader;
            _eventNotifier = eventNotifier;
        }

        public bool Execute(string plotId)
        {
            var player = _dataRepository.LoadPlayer();
            var farm = _dataRepository.LoadFarm();
            var plot = farm.GetPlotById(plotId);
            var farmConfig = _configLoader.GetFarmConfig(1);

            if (plot == null || plot.State != PlotState.Locked) return false;
            if (farmConfig == null) return false;

            if (player.TrySpendGold(farmConfig.PlotPrice))
            {
                plot.Unlock(); // Tầng 1 tự đổi State
                
                _dataRepository.SavePlayer(player);
                _dataRepository.SaveFarm(farm);
                
                _eventNotifier.NotifyGoldChanged(player.Gold);
                _eventNotifier.NotifyPlotStateChanged(plotId, PlotState.Empty); // Báo cho UI 3D
                return true;
            }
            
            _eventNotifier.NotifyError("Hết tiền!", $"Cần {farmConfig.PlotPrice} Vàng để mở khóa.");
            return false;
        }
    }
}