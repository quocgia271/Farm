using UnityEngine;

/// <summary>
/// Quản lý camera cho game top-down 3D.
/// Cho phép Pan (kéo chuột giữa) và Zoom (cuộn chuột)
/// Đồng thời giới hạn camera trong một khu vực nhất định.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Tốc độ Di chuyển (Pan)")]
    [SerializeField]
    [Tooltip("Tốc độ di chuyển camera khi kéo chuột. Tinh chỉnh cho phù hợp.")]
    private float _panSpeed = 0.1f;

    [Header("Tốc độ Zoom")]
    [SerializeField]
    [Tooltip("Tốc độ zoom khi cuộn chuột.")]
    private float _zoomSpeed = 10f;

    [SerializeField]
    [Tooltip("Camera có thể zoom gần nhất (giá trị Y thấp nhất).")]
    private float _minZoomY = 10f;

    [SerializeField]
    [Tooltip("Camera có thể zoom xa nhất (giá trị Y cao nhất).")]
    private float _maxZoomY = 80f;

    [Header("Giới hạn Camera (Bounds)")]
    [Tooltip("Giới hạn di chuyển trục X (World Space). " +
             "Dựa trên 5 cột, cách nhau 15: (5-1)*15 = 60. Đặt là (-10, 70) để có đệm.")]
    [SerializeField]
    private Vector2 _xBounds = new Vector2(-10f, 70f);

    [Tooltip("Giới hạn di chuyển trục Z (World Space). " +
             "Giả sử 5 hàng, cách nhau 15: (5-1)*15 = 60. Đặt là (-10, 70) để có đệm.")]
    [SerializeField]
    private Vector2 _zBounds = new Vector2(-10f, 70f);

    // Lưu vị trí chuột cuối cùng khi kéo
    private Vector3 _lastPanPosition;

    void Update()
    {
        // Xử lý cả Pan và Zoom trong Update
        HandlePanInput();
        HandleZoomInput();
    }

    void LateUpdate()
    {
        // Áp dụng giới hạn VỊ TRÍ (Pan) trong LateUpdate
        // để đảm bảo nó chạy sau khi tất cả logic di chuyển trong Update đã hoàn tất.
        ClampCameraPanPosition();
    }

    /// <summary>
    /// Xử lý việc di chuyển camera bằng chuột giữa.
    /// </summary>
    private void HandlePanInput()
    {
        // Khi nhấn chuột giữa (nút 2)
        if (Input.GetMouseButtonDown(2))
        {
            _lastPanPosition = Input.mousePosition;
        }
        
        // Khi giữ chuột giữa
        if (Input.GetMouseButton(2))
        {
            // Tính toán sự chênh lệch (delta) so với frame trước
            Vector3 delta = _lastPanPosition - Input.mousePosition;

            // Tạo vector di chuyển. Chúng ta di chuyển camera ngược lại với hướng kéo chuột
            // để tạo cảm giác "kéo" thế giới. Chỉ di chuyển trên mặt phẳng XZ.
            Vector3 move = new Vector3(delta.x * _panSpeed, 0, delta.y * _panSpeed);

            // Di chuyển camera trong không gian World
            // Time.deltaTime làm cho chuyển động mượt mà và độc lập với framerate
            transform.Translate(move * Time.deltaTime * 50f, Space.World); 
                                                                    
            // Cập nhật lại vị trí chuột cuối cùng
            _lastPanPosition = Input.mousePosition;
        }
    }

    /// <summary>
    /// Xử lý zoom camera bằng cách cuộn chuột.
    /// </summary>
    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Nếu không có hành động cuộn, không làm gì cả
        if (Mathf.Abs(scroll) < 0.01f)
            return;

        // Tính toán vector di chuyển (dọc theo trục forward của camera)
        // transform.forward là hướng mà camera đang nhìn
        Vector3 move = transform.forward * scroll * _zoomSpeed;

        // Tính toán vị trí tiếp theo của camera
        Vector3 nextPos = transform.position + move;

        // KIỂM TRA GIỚI HẠN ZOOM:
        // Chỉ áp dụng zoom nếu vị trí Y (độ cao) tiếp theo nằm trong giới hạn
        if (nextPos.y >= _minZoomY && nextPos.y <= _maxZoomY)
        {
            // Áp dụng zoom (di chuyển camera)
            // Time.deltaTime làm cho chuyển động mượt mà
            transform.Translate(move * Time.deltaTime * 100f, Space.World);
        }
    }

    /// <summary>
    /// Giữ camera không đi ra khỏi giới hạn X và Z đã định.
    /// </summary>
    private void ClampCameraPanPosition()
    {
        Vector3 pos = transform.position;

        // Kẹp vị trí X trong giới hạn _xBounds
        pos.x = Mathf.Clamp(pos.x, _xBounds.x, _xBounds.y);

        // Kẹp vị trí Z trong giới hạn _zBounds
        pos.z = Mathf.Clamp(pos.z, _zBounds.x, _zBounds.y);

        // Cập nhật lại vị trí camera
        transform.position = pos;
    }
}