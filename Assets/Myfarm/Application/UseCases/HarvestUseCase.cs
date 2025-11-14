using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;
using MyFarm.Domain.Enums; // Thêm để dùng PlotState

namespace MyFarm.Application.UseCases
{
    public class HarvestUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IWorldTimeService _timeService;
        private readonly IEventNotifier _notifier;

        public HarvestUseCase(IGameDataRepository dataRepository, IWorldTimeService timeService, IEventNotifier eventNotifier)
        {
            _dataRepository = dataRepository;
            _timeService = timeService;
            _notifier = eventNotifier;
        }

        public virtual bool Execute(string plotId)
        {
            var player = _dataRepository.LoadPlayer();
            var farm = _dataRepository.LoadFarm();
            var plot = farm.GetPlotById(plotId);

            if (plot == null || plot.State != PlotState.Ready) return false;

            // --- GỌI HÀM HARVEST MỚI ---
            // Lấy cả ID và Số lượng
            (string productId, int amount) = plot.Harvest();
            
            // Thêm TẤT CẢ vào kho
            player.AddItem(productId, amount);

            _dataRepository.SavePlayer(player);
            _dataRepository.SaveFarm(farm);

            _notifier.NotifyInventoryItemChanged(productId, player.GetItemCount(productId));
            _notifier.NotifyPlotStateChanged(plotId, plot.State);
            
            return true;
        }
    }
}