// File: MyFarm/2_Application/UseCases/WorkerArrivedUseCase.cs
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    // Tầng 4 (Visual) gọi UseCase này khi worker 3D ĐẾN NƠI
    public class WorkerArrivedUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IWorldTimeService _timeService;
        private readonly IEventNotifier _eventNotifier; // <-- THÊM DÒNG NÀY

        public WorkerArrivedUseCase(
            IGameDataRepository dataRepository, 
            IWorldTimeService timeService, 
            IEventNotifier eventNotifier) // <-- THÊM THAM SỐ
        {
            _dataRepository = dataRepository;
            _timeService = timeService;
            _eventNotifier = eventNotifier; // <-- THÊM DÒNG NÀY
        }

        public void Execute(string workerId)
        {
            var farm = _dataRepository.LoadFarm();
            var worker = farm.GetWorkerById(workerId); 
            
            if (worker != null && worker.State == Domain.Enums.WorkerState.MovingToTarget)
            {
                // Bắt đầu tính giờ 2 phút (theo logic data)
                worker.StartWorking(_timeService.GetCurrentTime());

                _dataRepository.SaveFarm(farm);
                
                // 2. Thông báo cho các hệ thống khác (như Animator) biết
                _eventNotifier.NotifyWorkerStateChanged(worker.WorkerId, worker.State);
            }
        }
    }
}