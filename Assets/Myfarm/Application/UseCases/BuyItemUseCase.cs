using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;
using UnityEngine; // Để dùng Debug.LogWarning

namespace MyFarm.Application.UseCases
{
    public class BuyItemUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IConfigLoader _configLoader;
        private readonly IEventNotifier _eventNotifier;

        public BuyItemUseCase(IGameDataRepository dataRepository, IConfigLoader configLoader, IEventNotifier eventNotifier)
        {
            _dataRepository = dataRepository;
            _configLoader = configLoader;
            _eventNotifier = eventNotifier;
        }

        // Trả về true nếu mua thành công
        public bool Execute(string productionId, int amount)
        {
            long totalCost = 0;
            if (amount <= 0) return false;

            Player player = _dataRepository.LoadPlayer();
            var config = _configLoader.GetProductionConfig(productionId);

            if (config == null)
            {
                Debug.LogWarning($"Không thể mua '{productionId}': Không tìm thấy ProductionConfig.");
                return false;
            }

          
            if (config.Wholesale)
            {
                if (amount % config.MinAmountToBuy != 0)
                {   
                    Debug.LogWarning("ko du so luong toi thieu can mua!");
                    return false;
                }
                int bundles = amount / config.MinAmountToBuy;
                totalCost = config.PriceBuyWholesale * bundles;
            }
            else
            {
                totalCost = config.BuyPrice *  amount;
            }
            // Tính tổng tiền
         

            // Thử tiêu tiền
            if (player.TrySpendGold(totalCost))
            {
                // Thêm item
                player.AddItem(productionId, amount);

                // Lưu & Thông báo
                _dataRepository.SavePlayer(player);
                _eventNotifier.NotifyGoldChanged(player.Gold);
                _eventNotifier.NotifyInventoryItemChanged(productionId, player.GetItemCount(productionId));
                return true;
            }
            
            // Không đủ tiền
            Debug.LogWarning("Không đủ tiền!");
            return false;
        }
    }
}