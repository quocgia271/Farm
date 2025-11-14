using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    // Script này gắn vào Prefab "Khuôn mẫu" của 1 cái nút
    public class DynamicActionButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private TextMeshProUGUI _countText; // <-- THÊM MỚI
        [SerializeField] private Button _button;

        // THAY ĐỔI: Hàm "cài đặt" giờ nhận Tên và Số lượng (int) riêng biệt
        public void Initialize(string text, int count, UnityAction onClickAction)
        {
            _labelText.text = text;
            
            // Cập nhật text số lượng (nếu có)
            if (_countText != null)
            {
                _countText.text = count.ToString();
            }

            // Xóa hết listener cũ
            _button.onClick.RemoveAllListeners(); 
            
            // Thêm listener mới
            _button.onClick.AddListener(onClickAction);
        }
    }
}