using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    // Tên mới: StartProductionUseCase
    // Chịu trách nhiệm cho BẤT KỲ hành động nào bắt đầu một quy trình sản xuất
    public class StartProductionUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IConfigLoader _configLoader;
        private readonly IWorldTimeService _timeService;
        private readonly IEventNotifier _notifier;

        // Constructor cập nhật tên
        public StartProductionUseCase(IGameDataRepository dataRepository, IConfigLoader configLoader, 
                                      IWorldTimeService timeService, IEventNotifier notifier)
        {
            _dataRepository = dataRepository;
            _configLoader = configLoader;
            _timeService = timeService;
            _notifier = notifier;
        }

        // Tên biến cũng được đổi: seedId -> productionConfigId
        public bool Execute(string plotId, string productionConfigId)
        {
            var player = _dataRepository.LoadPlayer();
            var farm = _dataRepository.LoadFarm();
            
            // Lấy "bảng thông số" từ config
            var config = _configLoader.GetProductionConfig(productionConfigId);
            if (config == null) return false; // Không tìm thấy loại config (hạt giống, con giống...)

            // 1. Kiểm tra logic nghiệp vụ
            // Kiểm tra xem player có item "gốc" không (vd: "tomato_seed", "cow")
            // Dùng chính config.Id vì nó đại diện cho item "giống"
            if (!player.TryRemoveItem(config.Id))
            {
                return false; // Không có hạt giống / con giống
            }

            var plot = farm.GetPlotById(plotId);
            if (plot == null || plot.State != Domain.Enums.PlotState.Empty)
            {
                // Nếu đất không trống, trả lại "giống" cho player
                player.AddItem(config.Id);
                return false;
            }

            // 2. Thay đổi trạng thái Domain
            // Ra lệnh cho "Nồi cơm điện" (FarmPlot) bắt đầu "nấu"
            plot.StartProduction(config, _timeService.GetCurrentTime());

            // 3. Lưu trạng thái
            _dataRepository.SavePlayer(player);
            _dataRepository.SaveFarm(farm);

            // 4. Thông báo cho UI (View)
            _notifier.NotifyInventoryItemChanged(config.Id, player.GetItemCount(config.Id));
            _notifier.NotifyPlotStateChanged(plotId, plot.State);

            return true;
        }
    }
}