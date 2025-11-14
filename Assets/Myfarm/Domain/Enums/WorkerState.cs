namespace MyFarm.Domain.Enums
{
    public enum WorkerState
    {
        Idle,
        MovingToTarget,
        Working, // Đang thực hiện hành động mất 2 phút
        MovingHome
    }
}