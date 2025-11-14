using MyFarm.Domain.Configs;
using System.Collections.Generic; // Thêm để dùng List nếu cần
using UnityEngine;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    public class InventoryPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySlotUI _slotPrefab;
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _sellAllButton; // Nút "Bán Tất Cả"

        void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(ClosePanel);
            if (_sellAllButton != null) _sellAllButton.onClick.AddListener(OnSellAllClicked);
        }

        void OnEnable()
        {
            PopulateInventorySlots();
            UpdateSellAllButtonState();
        }

        private void PopulateInventorySlots()
        {
            if (_slotContainer == null || _slotPrefab == null) return;

            // 1. Xóa các ô cũ
            foreach (Transform child in _slotContainer) Destroy(child.gameObject);

            var player = GameManager.Instance.DataRepository.LoadPlayer();
            var configLoader = GameManager.Instance.ConfigLoader;

            bool hasAnyProductToSell = false;

            // 2. Duyệt qua TẤT CẢ item trong kho
            foreach (var itemInInventory in player.Inventory)
            {
                string itemId = itemInInventory.Key;
                int amount = itemInInventory.Value;

                if (amount <= 0) continue;

                // --- LOGIC LỌC: Chỉ hiện SẢN PHẨM (Product) ---
                bool isProductionItem = configLoader.GetProductionConfig(itemId) != null;

                if (!isProductionItem)
                {
                    var itemConfig = configLoader.GetItemConfig(itemId);
                    if (itemConfig != null)
                    {
                        // Tạo ô mới
                        InventorySlotUI newSlot = Instantiate(_slotPrefab, _slotContainer);
                        newSlot.Initialize(itemId, itemConfig.Name);
                        hasAnyProductToSell = true;
                    }
                }
            }

            // Cập nhật trạng thái nút "Bán Tất Cả"
            if (_sellAllButton != null)
            {
                _sellAllButton.interactable = hasAnyProductToSell;
            }
        }

        // Hàm được gọi khi bấm nút "Bán Tất Cả"
        private void OnSellAllClicked()
        {
            var player = GameManager.Instance.DataRepository.LoadPlayer();
            var configLoader = GameManager.Instance.ConfigLoader;
            var sellUseCase = GameManager.Instance.SellItemUseCase;

            // Tạo một danh sách tạm để chứa các item cần bán
            // (Không thể vừa duyệt Dictionary vừa xóa item khỏi nó được)
            List<string> itemsToSell = new List<string>();

            foreach (var item in player.Inventory)
            {
                // Lọc ra các sản phẩm (không phải nguyên liệu)
                if (configLoader.GetProductionConfig(item.Key) == null && configLoader.GetItemConfig(item.Key) != null)
                {
                    itemsToSell.Add(item.Key);
                }
            }

            // Thực hiện bán từng loại
            foreach (string itemId in itemsToSell)
            {
                int amount = player.GetItemCount(itemId);
                if (amount > 0)
                {
                    sellUseCase.Execute(itemId, amount);
                }
            }

            // Sau khi bán hết, vẽ lại kho đồ
            PopulateInventorySlots();
        }

        private void UpdateSellAllButtonState()
        {
             // (Logic này đã được gộp vào cuối hàm PopulateInventorySlots cho tiện)
        }
        
        private void ClosePanel()
        {
            UIManager.Instance.HideInventoryPanel();
        }
    }
}