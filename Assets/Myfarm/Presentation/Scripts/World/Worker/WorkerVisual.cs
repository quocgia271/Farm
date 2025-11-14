// File: Myfarm/Presentation/Scripts/World/WorkerVisual.cs
using MyFarm.Domain.Models;
using MyFarm.Domain.Enums;
using UnityEngine;
using UnityEngine.AI;
using MyFarm.Presentation;
using MyFarm.Presentation.World;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
public class WorkerVisual : MonoBehaviour
{
    private Worker _domainWorker;
    private NavMeshAgent _agent;
    private FarmPlotView _targetPlotView;
    private Vector3 _homePosition;

    private bool _isInitialPositionSet = false;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (_agent == null)
        {
            Debug.LogError($"WorkerVisual ({gameObject.name}) BỊ THIẾU NavMeshAgent component!", this);
        }
    }

  public void Initialize(Worker workerModel, Vector3 homePosition)
    {
       
    if (workerModel == null)
    {

        return; // Hoặc throw exception nếu muốn dừng
    }

    _domainWorker = workerModel;
    _homePosition = homePosition; // Vector3 không null, nên không cần check

    if (_domainWorker != null) 
        gameObject.name = $"WorkerVisual_{_domainWorker.WorkerId}";
    else
        gameObject.name = "WorkerVisual_NULL";
}

/// <summary>
/// Hàm này chỉ chạy 1 lần khi load game để "dịch chuyển" (Warp)
/// agent đến đúng vị trí mà nó được lưu trong save file.
/// </summary>
private void SetInitialPositionOnLoad()
{
    Vector3 startPosition = _homePosition; // Mặc định là ở nhà

    // 1. Nếu state là Working (hoặc MovingToTarget), vị trí phải là ở thửa đất
    if (_domainWorker.State == WorkerState.Working )
    {
        if (!string.IsNullOrEmpty(_domainWorker.TargetPlotId))
        {
            var plotView = WorldManager.Instance.FindPlotView(_domainWorker.TargetPlotId);
            if (plotView != null)
            {
                // Tìm thấy thửa đất -> Vị trí bắt đầu là ở thửa đất
                startPosition = plotView.transform.position;
            }
            else
            {
                // Lỗi: Save file nói đang làm ở Plot X, nhưng không tìm thấy Plot X
                // (Có thể do plot đã bị xóa hoặc lỗi game)
                // -> An toàn nhất là cho về nhà và hủy task
                Debug.LogWarning($"Worker {_domainWorker.WorkerId} load ở state {_domainWorker.State} nhưng không tìm thấy PlotView {_domainWorker.TargetPlotId}. Cho về nhà và hủy task.");
                startPosition = _homePosition;

                // (Bạn đã có CancelWorkerTaskUseCase, hãy dùng nó)
                GameManager.Instance.CancelWorkerTaskUseCase.Execute(_domainWorker.WorkerId);
            }
        }
        
    }
    // (Nếu state là Idle hoặc MovingHome, vị trí mặc định _homePosition đã là đúng)
    // 2. Dịch chuyển NavMeshAgent đến vị trí đúng ngay lập tức
    // Dùng Warp() là an toàn nhất cho NavMeshAgent, nó sẽ "teleport" agent
    // và snap vào NavMesh gần nhất.
    if (_agent.isOnNavMesh)
    {
         _agent.Warp(startPosition);
    }
    else
    {
         // Nếu agent chưa "on" navmesh (có thể xảy ra ở frame đầu tiên)
         // thì teleport transform, NavMeshAgent sẽ tự snap vào sau.
         transform.position = startPosition;
    }

    Debug.Log($"Worker {_domainWorker.WorkerId} [Khởi tạo vị trí] tại state {_domainWorker.State} -> Vị trí: {startPosition}");
}

    private void Update()
    {
        // --- SỬA LỖI 1: Thêm kiểm tra an toàn (Fix NRE) ---
        // Bắt lỗi nếu _agent (NavMeshAgent) bị thiếu
        // HOẶC nếu các Manager (Game/World) chưa sẵn sàng
        if (_agent == null || _domainWorker == null || GameManager.Instance == null || WorldManager.Instance == null)
        {
            return; // Đợi frame sau
        }
        if (!_isInitialPositionSet)
        {
            // Gọi hàm này 1 lần duy nhất khi mọi thứ đã sẵn sàng
            SetInitialPositionOnLoad();
            _isInitialPositionSet = true;
        }

        // Logic chính: "Bắt chước" (Mirror) trạng thái của Model (Tầng 1)
        switch (_domainWorker.State)
        {
            case WorkerState.MovingToTarget:
                // (1. Chỉ tìm 1 lần nếu chưa có mục tiêu hoặc sai mục tiêu)
                if (_targetPlotView == null || _targetPlotView.PlotId != _domainWorker.TargetPlotId)
                {
                    _targetPlotView = WorldManager.Instance.FindPlotView(_domainWorker.TargetPlotId);

                    if (_targetPlotView != null)
                    {
                        // Tìm thấy -> Ra lệnh di chuyển
                        _agent.SetDestination(_targetPlotView.transform.position);
                    }
                    else
                    {
                        // Không tìm thấy (lỗi hiếm gặp) -> Đứng im
                        _agent.ResetPath();
                        Debug.LogWarning($"Worker {_domainWorker.WorkerId} không tìm thấy PlotView {_domainWorker.TargetPlotId}!");

                        // Nếu không tìm thấy plot, chúng ta cũng nên hủy task
                        GameManager.Instance.CancelWorkerTaskUseCase.Execute(_domainWorker.WorkerId); //
                        break; // Thoát switch
                    }
                }

                // (2. Nếu ĐÃ có mục tiêu...)
                if (_targetPlotView != null)
                {
                    // --- LOGIC MỚI: KIỂM TRA PLOT BỊ HỎNG ---
                    // (Sử dụng property 'CurrentState' mới thêm vào FarmPlotView)
                    if (_targetPlotView.CurrentState == PlotState.Spoiled)
                    {
                        Debug.Log($"Worker {_domainWorker.WorkerId} HỦY TASK vì plot {_targetPlotView.PlotId} đã bị Spoiled!", this);

                        // Yêu cầu Tầng 2 (Application) hủy bỏ nhiệm vụ này
                        GameManager.Instance.CancelWorkerTaskUseCase.Execute(_domainWorker.WorkerId); //

                        _targetPlotView = null; // Xóa mục tiêu
                        break; // Thoát khỏi switch, frame sau Tầng 1 sẽ là MovingHome
                    }
                    // --- KẾT THÚC LOGIC MỚI ---


                    // (3. LOGIC GỐC: KIỂM TRA ĐẾN NƠI)
                    // (Chỉ chạy nếu plot KHÔNG bị hỏng và có đường đi)
                    if (_agent.hasPath)
                    {
                        // Kiểm tra đến nơi
                        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                        {
                            Debug.Log($"Worker {_domainWorker.WorkerId} ĐÃ ĐẾN NƠI Plot {_targetPlotView.PlotId}!");
                            GameManager.Instance.WorkerArrivedUseCase.Execute(_domainWorker.WorkerId); //
                        }
                    }
                }
                break;


            case WorkerState.Working:
                // Tắt di chuyển (đứng im tại chỗ)
                if (_agent.hasPath) _agent.ResetPath();
                break;

            case WorkerState.Idle:
                // Đứng im tại chỗ (thường là ở nhà)
                if (_agent.hasPath) _agent.ResetPath();
                break;

            case WorkerState.MovingHome:

                // Quay về nhà
                _agent.SetDestination(_homePosition);
                _targetPlotView = null;

                // Kiểm tra xem đã về đến nhà chưa
                if (_agent.hasPath && !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                {
                    // ĐÃ VỀ ĐẾN NHÀ

                    // Chỉ gọi 1 LẦN DUY NHẤT (khi cờ đang là true)
                    if (_domainWorker.DoneHarvestingOnetime)
                    {
                        Debug.Log($"Worker {_domainWorker.WorkerId} ĐÃ VỀ NHÀ. Báo cho hệ thống reset.");

                        // Tầng 4 (Visual) THÔNG BÁO cho Tầng 2 (UseCase)
                        // (Giả sử bạn đã gán UseCase mới vào GameManager.Instance)
                        GameManager.Instance.WorkerArrivedHomeUseCase.Execute(_domainWorker.WorkerId);
                    }
                }
                break;
        }
    }
}