// File: MyFarm/Presentation/Scripts/UI/ShopSlotUI.cs
using MyFarm.Application.UseCases;
using MyFarm.Domain.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    public class ShopSlotUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        
        [Header("Actions")]
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_InputField _amountInput;

        private string _productionId;
        private BuyItemUseCase _buyItemUseCase;
        private long _basePrice = 0;
        private int _minIncrement = 1; 

        public void Initialize(ProductionConfig prodConfig, ItemConfig itemConfig)
        {
            _productionId = prodConfig.Id;
            _buyItemUseCase = GameManager.Instance.BuyItemUseCase;
            _itemNameText.text = itemConfig.Name;

            if (prodConfig.Wholesale) // <-- Sửa lại cho linh hoạt
            {
                _minIncrement = prodConfig.MinAmountToBuy;
                _amountInput.text = _minIncrement.ToString();
                // Hiển thị giá sỉ
                _priceText.text = $"{prodConfig.PriceBuyWholesale} Vàng / {_minIncrement} cái"; 
            }
            else
            {
                _basePrice = prodConfig.BuyPrice;
                _minIncrement = 1;
                _amountInput.text = "1";
                _priceText.text = $"{_basePrice} Vàng / 1 cái";
            }

            _buyButton.onClick.AddListener(OnBuyClicked);
            // _amountInput.onValueChanged.AddListener(OnAmountInputChanged); // (Đã xóa)
        }

        private void OnDestroy()
        {
        }
        
        // (Đã xóa hàm OnAmountInputChanged)

        
        private void OnBuyClicked()
        {
            if (int.TryParse(_amountInput.text, out int amountToBuy))
            {
                if (amountToBuy <= 0)
                {
                    _amountInput.text = _minIncrement.ToString();
                    return;
                }

                // KIỂM TRA MUA SỈ (ví dụ: gõ 55, trong khi minIncrement là 10)
                if (amountToBuy % _minIncrement != 0)
                {
                    // Tự động làm tròn LÊN
                    int roundedAmount = ((amountToBuy / _minIncrement) + 1) * _minIncrement;
                    _amountInput.text = roundedAmount.ToString(); // Cập nhật UI
                    amountToBuy = roundedAmount; // Cập nhật số lượng sẽ mua

                    // --- BƯỚC QUAN TRỌNG: HIỆN THÔNG BÁO ---
                    string itemName = _itemNameText.text; 
                    string message = $"Chỉ bán {itemName} theo gói {_minIncrement}. Đã tự động làm tròn lên {roundedAmount}.";
                    
                    // (Giả sử bạn có UIManager.Instance)
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowNotification(message);
                    }
                    // --- KẾT THÚC THÔNG BÁO ---
                }
                
                // Thực hiện mua
                _buyItemUseCase.Execute(_productionId, amountToBuy);
            }
            else
            {
                _amountInput.text = _minIncrement.ToString();
            }
        }
    }
}