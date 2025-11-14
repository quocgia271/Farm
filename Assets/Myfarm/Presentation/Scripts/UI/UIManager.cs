// File: Myfarm/Presentation/Scripts/UI/UIManager.cs (Đã sửa)
using MyFarm.Presentation.World;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MyFarm.Presentation.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Menus")]
        [SerializeField] private PlotActionMenu _plotActionMenu;
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private ShopPanel _shopPanel;

        // --- ĐẢM BẢO BẠN CÓ KHỐI NÀY ---
        [Header("Notification")]
        [SerializeField] private TextMeshProUGUI _notificationText;
        [SerializeField] private float _notificationTime = 2f; // <-- Dòng này bạn bị thiếu
        private Coroutine _notificationCoroutine;

        private long _goldToWin = 0;
        private bool _isGameWon = false;
        // --- KẾT THÚC KHỐI ---

        // --- BIẾN "KHÓA" TỪ LẦN TRƯỚC (Giữ nguyên) ---
        private bool _isAMenuOpen = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (_plotActionMenu) _plotActionMenu.gameObject.SetActive(false);
            if (_inventoryPanel) _inventoryPanel.gameObject.SetActive(false);
            if (_shopPanel) _shopPanel.gameObject.SetActive(false);
            if (_notificationText) _notificationText.gameObject.SetActive(false);
            StartCoroutine(RegisterErrorListener());
            StartCoroutine(LoadWinConditionAndListen());
        }
        // (Thêm hàm mới này vào bất kỳ đâu bên trong class UIManager)
        private IEnumerator LoadWinConditionAndListen()
        {
            yield return null; // Chờ 1 frame cho GameManager sẵn sàng
            if (GameManager.Instance != null && GameManager.Instance.ConfigLoader != null)
            {
                // Lấy config
                var farmConfig = GameManager.Instance.ConfigLoader.GetFarmConfig(1);
                if (farmConfig != null)
                {
                    _goldToWin = farmConfig.GoldtoWin;
                    
                    // Đăng ký lắng nghe sự kiện Vàng thay đổi
                    GameManager.Instance.EventNotifier.OnGoldChanged += CheckForWinCondition;
                }
            }
            else
            {
                Debug.LogError("UIManager không thể lấy ConfigLoader để đọc mốc chiến thắng!");
            }
        }
        // (Thêm hàm mới này vào bất kỳ đâu bên trong class UIManager)
        private void CheckForWinCondition(long newGold)
        {
            // Nếu game chưa thắng VÀ vàng đã đủ
            if (!_isGameWon && newGold >= _goldToWin)
            {
                _isGameWon = true; // Đánh dấu đã thắng (để không hiện thông báo 100 lần)
                
                // Gọi hàm thông báo có sẵn của bạn
                ShowNotification("BẠN ĐÃ CHIẾN THẮNG TRÒ CHƠI!");

                // (Tùy chọn) Bạn có thể thêm logic đóng băng game ở đây
                // Time.timeScale = 0; 
            }
        }
        private IEnumerator RegisterErrorListener()
        {
            yield return null; 
            if (GameManager.Instance != null && GameManager.Instance.EventNotifier != null)
            {
                GameManager.Instance.EventNotifier.OnError += ShowErrorNotification;
            }
            else
            {
                Debug.LogError("UIManager không thể đăng ký EventNotifier");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.EventNotifier != null)
                GameManager.Instance.EventNotifier.OnError -= ShowErrorNotification;
                GameManager.Instance.EventNotifier.OnGoldChanged -= CheckForWinCondition;
        }
        
        private void ShowErrorNotification(string title, string message)
        {
            ShowNotification(message);
        }

        public void ShowNotification(string message)
        {
            if (_notificationText == null) return;
            _notificationText.text = message;
            _notificationText.gameObject.SetActive(true);
            if (_notificationCoroutine != null) StopCoroutine(_notificationCoroutine);
            _notificationCoroutine = StartCoroutine(HideNotificationRoutine());
        }

        private IEnumerator HideNotificationRoutine()
        {
            yield return new WaitForSeconds(_notificationTime);
            if (_notificationText != null)
            {
                _notificationText.text = "";
                _notificationText.gameObject.SetActive(false);
            }
        }

        // --- BẮT ĐẦU SỬA LOGIC MỞ/ĐÓNG ---

        public void ShowPlotActionMenu(string plotId) 
        { 
            // Nếu đã có menu khác đang mở, không làm gì cả
            if (_isAMenuOpen) return; 
            
            if (_plotActionMenu) 
            {
                _plotActionMenu.Initialize(plotId); 
                _plotActionMenu.gameObject.SetActive(true); 
                _isAMenuOpen = true; // --- Đặt khóa
            }
        }

        public void HidePlotActionMenu() 
        { 
            if (_plotActionMenu && _plotActionMenu.gameObject.activeSelf) 
            {
                _plotActionMenu.gameObject.SetActive(false);
                _isAMenuOpen = false; // --- Nhả khóa
            } 
        }

        public void ToggleInventoryPanel() 
        { 
            if (_inventoryPanel == null) return;

            bool isCurrentlyOpen = _inventoryPanel.gameObject.activeSelf;

            if (isCurrentlyOpen)
            {
                // Đang mở -> Đóng lại
                _inventoryPanel.gameObject.SetActive(false);
                _isAMenuOpen = false; // --- Nhả khóa
            }
            else
            {
                // Đang đóng -> Mở ra
                // Kiểm tra xem có menu khác đang mở không
                if (_isAMenuOpen) return; // <-- KHÓA Ở ĐÂY

                // Không có gì cản trở, mở ra
                _inventoryPanel.gameObject.SetActive(true);
                _isAMenuOpen = true; // --- Đặt khóa
            }
        }

        public void HideInventoryPanel() 
        { 
            if (_inventoryPanel && _inventoryPanel.gameObject.activeSelf)
            {
                _inventoryPanel.gameObject.SetActive(false);
                _isAMenuOpen = false; // --- Nhả khóa
            } 
        }

        public void ToggleShopPanel() 
        { 
            if (_shopPanel == null) return;

            bool isCurrentlyOpen = _shopPanel.gameObject.activeSelf;

            if (isCurrentlyOpen)
            {
                // Đang mở -> Đóng lại
                _shopPanel.gameObject.SetActive(false);
                _isAMenuOpen = false; // --- Nhả khóa
            }
            else
            {
                // Đang đóng -> Mở ra
                if (_isAMenuOpen) return; // <-- KHÓA Ở ĐÂY

                _shopPanel.gameObject.SetActive(true);
                _isAMenuOpen = true; // --- Đặt khóa
            }
        }

        public void HideShopPanel() 
        { 
            if (_shopPanel && _shopPanel.gameObject.activeSelf)
            {
                _shopPanel.gameObject.SetActive(false);
                _isAMenuOpen = false; // --- Nhả khóa
            } 
        }
    }
}