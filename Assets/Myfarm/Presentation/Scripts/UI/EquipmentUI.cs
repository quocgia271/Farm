using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MyFarm.Presentation.UI; // Cần namespace này để gọi UIManager

namespace MyFarm.Presentation.UI
{
    public class EquipmentUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Button _upgradeButton;

        private void Start()
        {
            GameManager.Instance.EventNotifier.OnEquipmentUpgraded += UpdateLevelText;
            if (_upgradeButton != null) _upgradeButton.onClick.AddListener(OnUpgradeClicked);
            UpdateLevelText(GameManager.Instance.DataRepository.LoadFarm().EquipmentLevel);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EventNotifier.OnEquipmentUpgraded -= UpdateLevelText;
        }

        private void UpdateLevelText(int newLevel)
        {
            if (_levelText != null) _levelText.text = $"Trang bị: Lv.{newLevel}";
        }

        private void OnUpgradeClicked()
        {
            bool success = GameManager.Instance.UpgradeEquipmentUseCase.Execute();
            if (!success)
            {
                UIManager.Instance.ShowNotification("Không đủ 500 Vàng!");
            }
        }
    }
}