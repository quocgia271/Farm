using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    public class HireWorkerUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IEventNotifier _eventNotifier;
        private readonly IConfigLoader _configLoader; // --- THÊM DÒNG NÀY ---

        // --- SỬA HÀM KHỞI TẠO ---
        public HireWorkerUseCase(IGameDataRepository dataRepository, IEventNotifier eventNotifier, IConfigLoader configLoader)
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
            // Lấy giá từ config (giả sử dùng worker ID 1 là "Công Nhân Cơ Bản")
            var workerConfig = _configLoader.GetWorkerConfig(1);
            if (workerConfig == null) return false; // Không tìm thấy config

            if (player.TrySpendGold(workerConfig.HirePrice))
            {
                farm.AddWorker();
                
                _dataRepository.SavePlayer(player);
                _dataRepository.SaveFarm(farm);

                _eventNotifier.NotifyGoldChanged(player.Gold);
                // (FarmDynamicStatsUI sẽ tự động cập nhật số lượng worker)
                // --- 🚩 SỬA LỖI 1: THÊM DÒNG NÀY ---
                _eventNotifier.NotifyWorkerCountChanged(farm.Workers.Count);
        // --- HẾT SỬA LỖI ---
                return true;
            }

            // Báo lỗi (nếu UI có xử lý)
            return false;
        }
    }
}