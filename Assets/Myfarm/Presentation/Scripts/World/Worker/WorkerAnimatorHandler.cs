// File: Myfarm/Presentation/Scripts/World/WorkerAnimatorHandler.cs
using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;
using UnityEngine;

// Yêu cầu prefab phải có component Animator
[RequireComponent(typeof(Animator))]
public class WorkerAnimatorHandler : MonoBehaviour
{
    private Worker _domainWorker;
    private Animator _animator;
    
    // Dùng Hash để tăng tốc độ, không phải tìm "State" bằng chữ mỗi frame
    private readonly int _animStateHash = Animator.StringToHash("State");
    private int _lastAnimState = -1; // Cache lại state cuối cùng

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Hàm này sẽ được gọi bởi WorkerVisualManager khi spawn worker
    public void Initialize(Worker workerModel)
    {
        _domainWorker = workerModel;
    }

    private void Update()
    {
        // Chờ đến khi worker data sẵn sàng
        if (_domainWorker == null || _animator == null)
        {
            return;
        }

        // Liên tục cập nhật trạng thái cho Animator
        UpdateAnimationState(_domainWorker.State);
    }

    /// <summary>
    /// Gửi trạng thái logic (enum) sang trạng thái animation (int)
    /// </summary>
    private void UpdateAnimationState(WorkerState state)
    {
        int newStateValue;

        switch (state)
        {
            case WorkerState.Idle:
                newStateValue = 0; // 0 = idle
                break;

            case WorkerState.MovingToTarget:
            case WorkerState.MovingHome:
                newStateValue = 1; // 1 = run
                break;

            case WorkerState.Working:
                newStateValue = 2; // 2 = harvest
                break;

            default:
                newStateValue = 0; // Mặc định về Idle
                break;
        }

        // Chỉ SetInteger nếu state thực sự thay đổi (để tối ưu)
        if (newStateValue != _lastAnimState)
        {
            _animator.SetInteger(_animStateHash, newStateValue);
            _lastAnimState = newStateValue;
        }
    }
}