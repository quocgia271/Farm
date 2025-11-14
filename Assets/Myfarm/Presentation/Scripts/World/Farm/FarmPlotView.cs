// File: MyFarm/4_Presentation/Scripts/World/FarmPlotView.cs
using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;
using MyFarm.Presentation.UI; 
using UnityEngine;
using TMPro;
using System;
using System.Linq;

namespace MyFarm.Presentation.World
{
    public class FarmPlotView : MonoBehaviour
    {
        // ... (các biến public giữ nguyên) ...
        private FarmPlot _domainPlot;
        public string PlotId => _domainPlot?.PlotId;
        public PlotState CurrentState => _domainPlot != null ? _domainPlot.State : PlotState.Locked;

        [Header("Visuals (Fixed)")]
        [SerializeField] private GameObject _lockedVisual;
        [SerializeField] private GameObject _emptyVisual;
        
        [Header("Visuals (Dynamic)")]
        [SerializeField] private Transform _cropContainer;

        // --- SỬA Ở ĐÂY (V6) ---
        private GameObject _visualInstance; 
        // Đổi type từ AnimalAnimatorManager thành AnimalLeanTweenManager
        private AnimalLeanTweenManager _animalManager; 
        // --- HẾT SỬA ---
        
        [Header("UI (World Space Canvas)")]
        // ... (các biến UI giữ nguyên) ...
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _cropNameText;
        [SerializeField] private TextMeshProUGUI _productInCountText; 
        [SerializeField] private TextMeshProUGUI _harvestedCountText;
        [SerializeField] private TextMeshProUGUI _workerTimerText;
        [SerializeField] private TextMeshProUGUI _productTimerText;

        // ... (các biến hỗ trợ giữ nguyên) ...
        private float _bonusPercent = 0f;
        private Worker _busyWorkerModel = null;
        private TimeSpan _workerFixedWorkTime = TimeSpan.Zero;
        private PlotState _lastVisualState = (PlotState)(-1); 
        private string _lastProductionId = null; 

        // --- HÀM ĐIỀU PHỐI (ROUTER) ĐÃ CẬP NHẬT ---
        // (Hàm Initialize, OnDestroy, Update, OnStateChanged giữ nguyên)
        public void Initialize(FarmPlot plotModel)
        {
            _domainPlot = plotModel;
            if (GameManager.Instance == null) return;
            GameManager.Instance.EventNotifier.OnPlotStateChanged += OnStateChanged;
            
            UpdateBonusCache();
            _lastVisualState = (PlotState)(-1); 
            _lastProductionId = "FORCE_UPDATE"; 
            UpdateVisuals(_domainPlot.State); 
            UpdateUIText(); 
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.EventNotifier != null)
                GameManager.Instance.EventNotifier.OnPlotStateChanged -= OnStateChanged;
            ClearVisualInstance(); 
        }

        private void Update()
        {
             if (_domainPlot != null && GameManager.Instance != null)
             {
                UpdateVisuals(_domainPlot.State); 
                UpdateUIText(); 
             }
        }
        
        private void OnStateChanged(string changedPlotId, PlotState newState)
        {
            if (_domainPlot == null || changedPlotId != _domainPlot.PlotId) return;

            if (newState == PlotState.Empty && _lastVisualState != PlotState.Empty)
            {
                ClearVisualInstance(); 
                _lastProductionId = null; 
            }
            
            UpdateVisuals(newState);
            UpdateUIText();
        }

        private void UpdateVisuals(PlotState newState)
        {
            string currentProductionId = _domainPlot?.CurrentConfig?.Id;

            if (newState == _lastVisualState && currentProductionId == _lastProductionId)
                return;

            if (currentProductionId != _lastProductionId)
            {
                ClearVisualInstance();
            }

            if (_lockedVisual) _lockedVisual.SetActive(newState == PlotState.Locked);
            if (_emptyVisual) _emptyVisual.SetActive(newState != PlotState.Locked);

            // --- SỬA Ở ĐÂY (V6) ---
            // 4. Quyết định logic (Plant hay Animal)
            if (_visualInstance == null && !string.IsNullOrEmpty(currentProductionId))
            {
                string animalPath = $"Prefabs/Crops/{currentProductionId}";
                GameObject animalPrefab = Resources.Load<GameObject>(animalPath);

                // Kiểm tra xem prefab "cow" có component AnimalLeanTweenManager không
                if (animalPrefab != null && animalPrefab.GetComponent<AnimalLeanTweenManager>() != null)
                {
                    // Đây LÀ ĐỘNG VẬT
                    _visualInstance = SpawnVisual(animalPath, ""); 
                    _animalManager = _visualInstance.GetComponent<AnimalLeanTweenManager>();
                }
            }
            // --- HẾT SỬA ---

            // 5. Chạy logic cập nhật tương ứng
            if (_animalManager != null)
            {
                // Logic Động vật: Chỉ cập nhật LeanTween
                _animalManager.SetState(newState);
            }
            else
            {
                // Logic Thực vật: Đổi prefab (logic cũ)
                UpdatePlantVisual(newState, currentProductionId);
            }

            _lastVisualState = newState;
            _lastProductionId = currentProductionId;
        }

        // --- (Các hàm còn lại giữ nguyên y hệt) ---
        private void UpdatePlantVisual(PlotState newState, string currentProductionId)
        {
            if (_visualInstance != null)
                _visualInstance.SetActive(false);

            string suffix = "";
            switch (newState)
            {
                case PlotState.Growing: suffix = "_Growing"; break;
                case PlotState.Ready:   suffix = "_Ready"; break;
                case PlotState.Spoiled: suffix = "_Spoiled"; break;
                case PlotState.Empty:
                case PlotState.Locked:
                    ClearVisualInstance(); 
                    return;
            }

            if (string.IsNullOrEmpty(currentProductionId) || string.IsNullOrEmpty(suffix))
                return;

            string plantPath = $"Prefabs/Crops/{currentProductionId}{suffix}";

            if (_visualInstance == null || _visualInstance.name != plantPath)
            {
                ClearVisualInstance();
                _visualInstance = SpawnVisual(plantPath, plantPath); 
            }
                
            if (_visualInstance)
                _visualInstance.SetActive(true);
        }

        private void ClearVisualInstance()
        {
            if (_visualInstance != null)
            {
                // Dừng mọi tween trên object này trước khi hủy
                LeanTween.cancel(_visualInstance); 
                Destroy(_visualInstance);
            }
            _visualInstance = null;
            _animalManager = null; 
        }

        private GameObject SpawnVisual(string prefabPath, string instanceName)
        {
            GameObject prefabToSpawn = Resources.Load<GameObject>(prefabPath);

            if (prefabToSpawn != null)
            {
                if (_cropContainer == null) {
                    Debug.LogError($"[FarmPlotView] _cropContainer chưa được gán! PlotId: {PlotId}", this);
                    return null;
                }
                var instance = Instantiate(prefabToSpawn, _cropContainer.position, _cropContainer.rotation, _cropContainer);
                instance.name = instanceName; 
                
                // Dòng fix scale vẫn giữ lại, rất quan trọng
                instance.transform.localScale = prefabToSpawn.transform.localScale;

                return instance;
            }
            else
            {
                if (_animalManager == null)
                    Debug.LogWarning($"[FarmPlotView] Không tìm thấy prefab tại: {prefabPath}. PlotId: {PlotId}", this);
                return null;
            }
        }
        
        // ... (Toàn bộ các hàm UI và OnInteract giữ nguyên) ...
        #region UI Text & Interaction Logic (Không thay đổi)
        private void UpdateUIText() 
        { 
            if (_domainPlot == null || GameManager.Instance == null || GameManager.Instance.ConfigLoader == null) return;

            string status = "", cropName = "", productIn = "", harvested = "", workerTimer = "", productTimer = "";

            UpdateBonusCache();
            UpdateWorkerStatus();
            WorkerState workerState = _busyWorkerModel?.State ?? WorkerState.Idle;
            var plotConfig = _domainPlot.CurrentConfig;

            // --- THÊM LOGIC NHỎ ĐỂ ĐỔI TÊN TEXT ---
            string growingText = "Đang mọc...";
            if (plotConfig != null && (plotConfig.Id == "cow" || plotConfig.Id == "chicken"))
            {
                growingText = "Đang sản xuất...";
            }
            // --- HẾT THÊM ---

            switch (_domainPlot.State)
            {
                case PlotState.Locked:
                    status = "Bị khóa";
                    break;
                case PlotState.Empty:
                    status = "Đất trống";
                    break;
                case PlotState.Growing:
                    status = growingText; 
                    if (workerState == WorkerState.MovingToTarget) status = "Worker đang đến...";
                    if (plotConfig != null)
                    {
                        cropName = plotConfig.Name;
                        harvested = $"{_domainPlot.TotalProductsProduced}/{plotConfig.MaxHarvestTimes}";
                    }
                    TimeSpan remaining = _domainPlot.CurrentCycleStartTime + _domainPlot.GetEffectiveGrowthTime(_bonusPercent) - DateTime.Now;
                    productTimer = remaining.TotalSeconds > 0 ? remaining.ToString(@"mm\:ss") : "00:00";
                    break;
                case PlotState.Ready:
                    if (plotConfig != null)
                    {
                        cropName = plotConfig.Name;
                        harvested = $"{_domainPlot.TotalProductsProduced}/{plotConfig.MaxHarvestTimes}";
                    }
                    if(_domainPlot.ProductsInQueue > 0)
                        productIn = $"x{_domainPlot.ProductsInQueue}";
                    if (plotConfig != null && _domainPlot.TotalProductsProduced < plotConfig.MaxHarvestTimes)
                    {
                        TimeSpan nextRemaining = _domainPlot.CurrentCycleStartTime + _domainPlot.GetEffectiveGrowthTime(_bonusPercent) - DateTime.Now;
                        productTimer = nextRemaining.TotalSeconds > 0 ? nextRemaining.ToString(@"mm\:ss") : "Sắp xong...";
                    }
                    else if(_domainPlot.FinalHarvestDeadline.HasValue)
                    {
                        TimeSpan spoilTime = _domainPlot.FinalHarvestDeadline.Value - DateTime.Now;
                        productTimer = spoilTime.TotalSeconds > 0 ? $"Hỏng: {spoilTime:mm\\:ss}" : "ĐÃ HỎNG!";
                    }
                    if (workerState == WorkerState.Working)
                    {
                        status = "Đang thu hoạch...";
                        DateTime taskEndTime = _busyWorkerModel.TaskStartTime + _workerFixedWorkTime;
                        TimeSpan remainingWorkTime = taskEndTime - DateTime.Now;
                        workerTimer = remainingWorkTime.TotalSeconds > 0 ? remainingWorkTime.ToString(@"mm\:ss") : "00:00";
                    }
                    else if (workerState == WorkerState.MovingToTarget)
                    {
                        status = "Worker đang đến...";
                    }
                    else
                    {
                        status = "Sẵn sàng!";
                    }
                    break;
                case PlotState.Spoiled:
                    status = "Đã bị hỏng\nClick để xóa";
                    if (plotConfig != null)
                    {
                        cropName = plotConfig.Name;
                    }
                    break;
            }
            SetTextAndVisibility(_statusText, status);
            SetTextAndVisibility(_cropNameText, cropName);
            SetTextAndVisibility(_productInCountText, productIn);
            SetTextAndVisibility(_harvestedCountText, harvested);
            SetTextAndVisibility(_workerTimerText, workerTimer);
            SetTextAndVisibility(_productTimerText, productTimer);
        }

        private void SetTextAndVisibility(TextMeshProUGUI textElement, string content)
        {
            if (textElement == null) return;
            bool hasContent = !string.IsNullOrEmpty(content);
            if (textElement.gameObject.activeSelf != hasContent)
            {
                textElement.gameObject.SetActive(hasContent);
            }
            if (hasContent)
            {
                textElement.text = content;
            }
        }

        private void UpdateWorkerStatus() 
        {
            if (GameManager.Instance.DataRepository == null) { _busyWorkerModel = null; return; }
            var farm = GameManager.Instance.DataRepository.LoadFarm();
            if (farm == null) { _busyWorkerModel = null; return; }
            _busyWorkerModel = farm.Workers.FirstOrDefault(w => 
                w.TargetPlotId == _domainPlot.PlotId && 
                (w.State == WorkerState.MovingToTarget || w.State == WorkerState.Working)
            );
        }

        private void UpdateBonusCache() 
        { 
            if (GameManager.Instance.DataRepository == null || GameManager.Instance.ConfigLoader == null) return;
            var farm = GameManager.Instance.DataRepository.LoadFarm();
            var equipConfig = GameManager.Instance.ConfigLoader.GetEquipmentConfig(1); 
            _bonusPercent = (equipConfig != null && farm != null) ? (farm.EquipmentLevel - 1) * equipConfig.ProductionBoostPercent : 0f;
            var workerConfig = GameManager.Instance.ConfigLoader.GetWorkerConfig(1);
            if (workerConfig != null)
            {
                _workerFixedWorkTime = TimeSpan.FromMinutes(workerConfig.ActionTimeMinutes);
            }
        }
       
        public void OnInteract()
        {
            if (_domainPlot == null || GameManager.Instance == null) return;
            var farm = GameManager.Instance.DataRepository.LoadFarm();
            if (_domainPlot.State == PlotState.Locked)
            {
                GameManager.Instance.UnlockPlotUseCase.Execute(_domainPlot.PlotId);
                return;
            }
            if (_domainPlot.State == PlotState.Empty)
            {
                UIManager.Instance.ShowPlotActionMenu(_domainPlot.PlotId);
                return;
            }
            if (_domainPlot.State == PlotState.Spoiled)
            {
                GameManager.Instance.ClearPlotUseCase.Execute(_domainPlot.PlotId);
                return;
            }
            if (_domainPlot.State == PlotState.Ready && _domainPlot.ProductsInQueue > 0)
            {
                var worker = farm.Workers.FirstOrDefault(w => w.TargetPlotId == _domainPlot.PlotId);
                if (worker != null && worker.State == WorkerState.Working)
                {
                    UIManager.Instance.ShowNotification("Công nhân đang thu hoạch!");
                    return; 
                }
                if (worker != null && worker.State == WorkerState.MovingToTarget)
                {
                    GameManager.Instance.CancelWorkerTaskUseCase.Execute(worker.WorkerId);
                }
                GameManager.Instance.HarvestUseCase.Execute(_domainPlot.PlotId);
            }
        }
        #endregion
    }
}