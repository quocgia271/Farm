using MyFarm.Domain.Models;
using UnityEngine;
using MyFarm.Presentation; // Thêm
using MyFarm.Presentation.UI; // Thêm
using System.Collections.Generic; // Thêm
using System.Linq; // Thêm
using UnityEngine.EventSystems;
namespace MyFarm.Presentation.World
{
    // Script này chịu trách nhiệm "vẽ" các mảnh đất 3D khi game bắt đầu
    public class WorldManager : MonoBehaviour
    {
        public static WorldManager Instance { get; private set; } // Thêm
        
        [Header("Prefabs")]
        [SerializeField] private FarmPlotView _plotPrefab; // Kéo Prefab "Đất" của bạn vào đây

        [Header("Layout")]
        [SerializeField] private Transform _plotsContainer; // (Tùy chọn) Kéo 1 Empty "Plots" vào đây
        [SerializeField] private int _columns = 5; // Xếp thành lưới 5 cột
        [SerializeField] private float _plotSpacing = 2.5f; // Khoảng cách giữa các ô
        // --- THÊM MỚI ---
        // Cache (bộ nhớ đệm) để Worker tìm Plot nhanh hơn
        private Dictionary<string, FarmPlotView> _plotViewCache = new Dictionary<string, FarmPlotView>();
        private void Awake()
        {
            // Thiết lập Singleton
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Phát hiện WorldManager bị trùng lặp. Hủy bỏ.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
        void Start()
        {
            // Phải chờ GameManager
            StartCoroutine(InitializeWorld());
        }

        System.Collections.IEnumerator InitializeWorld()
        {
            // Chờ 1 frame để GameManager sẵn sàng
            yield return null; 
            
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager không tìm thấy!");
                yield break;
            }
            
            GenerateWorld();
        }

        private void GenerateWorld()
        {
            var farm = GameManager.Instance.DataRepository.LoadFarm();
            if (farm == null)
            {
                Debug.LogError("WorldManager: Không load được Farm data!");
                return;
            }

            int i = 0;
            foreach (var plotModel in farm.Plots)
            {
                // --- LOGIC XẾP LƯỚI ĐƠN GIẢN ---
                float x = (i % _columns) * _plotSpacing;
                float z = (i / _columns) * _plotSpacing;
                Vector3 position = new Vector3(x, 0, z);
                
                // --- SPANW (ĐẺ) PREFAB 3D ---
                FarmPlotView plotView = Instantiate(_plotPrefab, position, Quaternion.identity, _plotsContainer);
                
                // --- KẾT NỐI (LINK) ---
                // Gắn Model (data) vào View (3D)
                // (Hàm Initialize() nằm bên trong FarmPlotView.cs)
                plotView.Initialize(plotModel); 
                _plotViewCache[plotModel.PlotId] = plotView; // Lưu vào cache
                i++;
            }
        }
        // --- THÊM HÀM MỚI (Cho WorkerVisual gọi) ---
        public FarmPlotView FindPlotView(string plotId)
        {
            _plotViewCache.TryGetValue(plotId, out var plotView);
            return plotView;
        }
        // --- XỬ LÝ CLICK CHUỘT ---
        // (Chúng ta chuyển logic Raycast từ FarmPlotView sang WorldManager
        // để quản lý tập trung, nhưng cách cũ của bạn vẫn OK)
        private void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        // Nếu chuột đang ở trên UI, bỏ qua raycast 3D
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Kiểm tra xem có click trúng FarmPlotView không
            var plotView = hit.collider.GetComponentInParent<FarmPlotView>();
            if (plotView != null)
            {
                // Nếu có, bảo nó tự xử lý (OnInteract)
                plotView.OnInteract();
            }
        }
    }
}
    }
}