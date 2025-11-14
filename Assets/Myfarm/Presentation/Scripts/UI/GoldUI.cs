using MyFarm.Application.Interfaces;
using TMPro;
using UnityEngine;

namespace MyFarm.Presentation.UI
{
    public class GoldUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _goldText;

        private void Start()
        {
            // Đăng ký lắng nghe sự kiện từ "Loa phường" (EventNotifier)
            GameManager.Instance.EventNotifier.OnGoldChanged += UpdateGold;
            
            // Lấy giá trị ban đầu
            var player = GameManager.Instance.DataRepository.LoadPlayer();
            UpdateGold(player.Gold);
        }

        private void OnDestroy()
        {
            // Rất quan trọng: Hủy đăng ký khi object bị hủy để tránh lỗi memory leak
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EventNotifier.OnGoldChanged -= UpdateGold;
            }
        }

        // Hàm này được gọi tự động mỗi khi Vàng thay đổi
        private void UpdateGold(long newGold)
        {
            _goldText.text = $"Gold: {newGold:N0}"; // Định dạng số có dấu phẩy (vd: 1,000)
        }
    }
}