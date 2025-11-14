using MyFarm.Domain.Configs;
using UnityEngine;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private ShopSlotUI _slotPrefab; // Khuôn mẫu "Shop Slot"
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private Button _closeButton;

        private void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(ClosePanel);
        }

        private void OnEnable()
        {
            PopulateShopSlots();
        }

        private void PopulateShopSlots()
        {
            if (_slotContainer == null || _slotPrefab == null) return;

            // 1. Xóa các ô cũ
            foreach (Transform child in _slotContainer) Destroy(child.gameObject);

            var configLoader = GameManager.Instance.ConfigLoader;

            // 2. Lấy TẤT CẢ các item "Sản xuất" (Hạt, Bò...)
            var allProductionItems = configLoader.GetAllProductionConfigs();

            foreach (var prodConfig in allProductionItems)
            {
                var itemConfig = configLoader.GetItemConfig(prodConfig.Id);

                // Chỉ "vẽ" nếu nó có cả 2 config (sản xuất và item)
                if (itemConfig != null)
                {
                    ShopSlotUI newSlot = Instantiate(_slotPrefab, _slotContainer);
                    newSlot.Initialize(prodConfig, itemConfig);
                }
                else
                {
                // --- THÊM DÒNG NÀY ---
                // Báo cho bạn biết chính xác ID nào từ production_config không tìm thấy trong item_config
                Debug.LogWarning($"[ShopPanel] Không tìm thấy ItemConfig cho Production ID: '{prodConfig.Id}'. Hãy kiểm tra lại file item_config.csv!");
                }
            }
        }
        
        private void ClosePanel()
        {
            UIManager.Instance.HideShopPanel(); // <-- Phải gọi chính nó // (Hàm này chúng ta sẽ thêm vào UIManager)
        }
    }
}