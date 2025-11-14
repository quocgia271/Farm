using MyFarm.Domain.Configs;
using UnityEngine;
using UnityEngine.UI;

namespace MyFarm.Presentation.UI
{
    public class PlotActionMenu : MonoBehaviour
    {
        [SerializeField] private DynamicActionButton _buttonPrefab;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private Button _closeButton;

        private string _currentTargetPlotId;

        private void Start()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseMenu);
        }

        public void Initialize(string plotId)
        {
            _currentTargetPlotId = plotId;
            PopulateButtons();
        }

        private void PopulateButtons()
        {
            foreach (Transform child in _buttonContainer) Destroy(child.gameObject);

            var player = GameManager.Instance.DataRepository.LoadPlayer();
            var configLoader = GameManager.Instance.ConfigLoader;

            var allProductionConfigs = configLoader.GetAllProductionConfigs();

            foreach (ProductionConfig prodConfig in allProductionConfigs)
            {
                int count = player.GetItemCount(prodConfig.Id);

                if (count > 0)
                {
                    var itemConfig = configLoader.GetItemConfig(prodConfig.Id);
                    string friendlyName = itemConfig != null ? itemConfig.Name : prodConfig.Id;
                    
                    DynamicActionButton newButton = Instantiate(_buttonPrefab, _buttonContainer);
                    string localId = prodConfig.Id;

                    // --- SỬA LẠI CÁCH GỌI HÀM ---
                    // Giờ chúng ta truyền Tên (string) và Số lượng (int) riêng biệt
                    newButton.Initialize(friendlyName, count, () => OnActionButtonClicked(localId));
                }
            }
        }

        private void OnActionButtonClicked(string productionId)
        {
            GameManager.Instance.StartProductionUseCase.Execute(_currentTargetPlotId, productionId);
            CloseMenu();
        }

        private void CloseMenu()
        {
            UIManager.Instance.HidePlotActionMenu();
        }
    }
}