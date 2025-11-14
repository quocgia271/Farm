using UnityEngine;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    public class WorkerUI : MonoBehaviour
    {
        [Header("Hire Worker")]
        [SerializeField] private Button _hireButton;
        // Bạn có thể thêm Text để hiện giá nếu muốn, nhưng hard-code vào nút cũng được

        private void Start()
        {
            if (_hireButton != null)
            {
                _hireButton.onClick.AddListener(OnHireClicked);
            }
        }

        private void OnHireClicked()
        {
            // Gọi Use Case và kiểm tra kết quả
            bool success = GameManager.Instance.HireWorkerUseCase.Execute();
            
            if (!success)
            {
                // Báo lỗi nếu không đủ tiền
                UIManager.Instance.ShowNotification("Không đủ 500 Vàng để thuê!");
            }
            // Nếu thành công, FarmDynamicStatsUI sẽ tự động cập nhật số lượng worker,
            // và GoldUI sẽ tự động cập nhật vàng. WorkerUI không cần làm gì thêm.
        }
    }
}