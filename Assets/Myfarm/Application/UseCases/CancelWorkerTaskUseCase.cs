// File: MyFarm/2_Application/UseCases/CancelWorkerTaskUseCase.cs
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    // Tầng 4 (FarmPlotView) gọi UseCase này khi Player hái trộm
    public class CancelWorkerTaskUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IEventNotifier _eventNotifier; // <-- THÊM DÒNG NÀY
       // Sửa Constructor
        public CancelWorkerTaskUseCase(
            IGameDataRepository dataRepository, 
            IEventNotifier eventNotifier) // <-- THÊM THAM SỐ
        {
            _dataRepository = dataRepository;
            _eventNotifier = eventNotifier; // <-- THÊM DÒNG NÀY
        }

        public void Execute(string workerId)
        {
            var farm = _dataRepository.LoadFarm();
            var worker = farm.GetWorkerById(workerId);
            
            // Chỉ hủy nếu worker đang trên đường đi
            if (worker != null && worker.State == Domain.Enums.WorkerState.MovingToTarget)
            {
                // Quay về Idle
                worker.CompleteTask();
                _dataRepository.SaveFarm(farm);
                _eventNotifier.NotifyWorkerStateChanged(worker.WorkerId, worker.State);
            }
        }
    }
}