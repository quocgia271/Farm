// File: Myfarm/Presentation/Scripts/UI/InventorySlotUI.cs
using MyFarm.Domain.Configs;
using TMPro; // Thêm thư viện InputField
using UnityEngine;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private Button _sellButton; // Nút "Bán" (theo số lượng nhập)
        [SerializeField] private Button _sellMaxButton; // Nút "Bán Hết"
        [SerializeField] private TMP_InputField _amountInput; // Ô nhập số lượng

        private string _itemId;
        private int _currentAmount;

        public void Initialize(string itemId, string friendlyName)
        {
            _itemId = itemId;
            _itemNameText.text = friendlyName;

            _sellButton.onClick.AddListener(OnSellClicked);
            _sellMaxButton.onClick.AddListener(OnSellMaxClicked);

            // Đăng ký lắng nghe sự kiện
            GameManager.Instance.EventNotifier.OnInventoryItemChanged += OnItemChanged;
            
            // Lấy số lượng ban đầu
            _currentAmount = GameManager.Instance.DataRepository.LoadPlayer().GetItemCount(_itemId);
            UpdateAmount(_currentAmount);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EventNotifier.OnInventoryItemChanged -= OnItemChanged;
        }

        // Tự cập nhật khi kho thay đổi
        private void OnItemChanged(string changedItemId, int newAmount)
        {
            if (changedItemId == _itemId)
                UpdateAmount(newAmount);
        }

        // Cập nhật UI và số lượng
        private void UpdateAmount(int amount)
        {
            _currentAmount = amount;
            _amountText.text = $"Có: {amount}";
            
            bool hasItem = amount > 0;
            _sellButton.interactable = hasItem;
            _sellMaxButton.interactable = hasItem;
            _amountInput.interactable = hasItem;
            
            // Nếu không còn hàng, xóa số
            if (!hasItem) 
            {
                _amountInput.text = "0";
            }
            // Nếu nhập vào mà số lượng về 0, reset về 1 (nếu vẫn còn hàng)
            else if (_amountInput.text == "0")
            {
                 _amountInput.text = "1";
            }
        }

        // --- HÀM ĐÃ SỬA LẠI LOGIC THÔNG BÁO ---
        private void OnSellClicked()
        {
            if (int.TryParse(_amountInput.text, out int amountToSell))
            {
                // Kịch bản 1: Nhập số âm hoặc 0
                if (amountToSell <= 0)
                {
                    UIManager.Instance.ShowNotification("Số lượng bán phải lớn hơn 0.");
                    _amountInput.text = "1";
                    return;
                }

                // Kịch bản 2: Bán nhiều hơn số đang có (ĐÂY LÀ THÔNG BÁO BẠN MUỐN)
                if (amountToSell > _currentAmount)
                {
                    string message = $"Không đủ! Bạn chỉ có {_currentAmount} {_itemNameText.text}.";
                    UIManager.Instance.ShowNotification(message);
                    
                    // Tự động sửa về số lượng tối đa bạn có thể bán
                    _amountInput.text = _currentAmount.ToString();
                    return;
                }

                // Kịch bản 3: Thành công
                // (amountToSell > 0 && amountToSell <= _currentAmount)
                GameManager.Instance.SellItemUseCase.Execute(_itemId, amountToSell);
            }
            else
            {
                // Kịch bản 4: Người chơi gõ chữ (ví dụ: "abc")
                UIManager.Instance.ShowNotification("Vui lòng nhập một con số.");
                _amountInput.text = "1";
            }
        }
        // --- KẾT THÚC SỬA LỖI ---

        // Nút "Bán Hết"
        private void OnSellMaxClicked()
        {
            if (_currentAmount > 0)
            {
                GameManager.Instance.SellItemUseCase.Execute(_itemId, _currentAmount);
            }
            else
            {
                UIManager.Instance.ShowNotification("Không có gì để bán!");
            }
        }
    }
}