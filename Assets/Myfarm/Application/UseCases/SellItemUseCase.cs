using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    public class SellItemUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IConfigLoader _configLoader;
        private readonly IEventNotifier _eventNotifier;

        public SellItemUseCase(IGameDataRepository dataRepository, IConfigLoader configLoader, IEventNotifier eventNotifier)
        {
            _dataRepository = dataRepository;
            _configLoader = configLoader;
            _eventNotifier = eventNotifier;
        }

        // --- HÀM NÂNG CẤP ---
        // Giờ đây nó nhận cả số lượng (amount)
        public bool Execute(string itemId, int amount)
        {
            if (amount <= 0) return false;

            Player player = _dataRepository.LoadPlayer();
            var config = _configLoader.GetItemConfig(itemId);
            
            if (config == null) return false; // Không có cấu hình item này

            // Thử bán 'amount'
            if (player.TryRemoveItem(itemId, amount))
            {
                long goldEarned = config.SellPrice * amount;
                player.AddGold(goldEarned);

                // Lưu & Thông báo
                _dataRepository.SavePlayer(player);
                _eventNotifier.NotifyGoldChanged(player.Gold);
                _eventNotifier.NotifyInventoryItemChanged(itemId, player.GetItemCount(itemId));
                return true;
            }

            return false;
        }

        // (Hàm cũ) - chúng ta giữ lại để tương thích
        // hoặc để bán nhanh 1 cái
        public bool Execute(string itemId)
        {
            return Execute(itemId, 1);
        }
    }
}