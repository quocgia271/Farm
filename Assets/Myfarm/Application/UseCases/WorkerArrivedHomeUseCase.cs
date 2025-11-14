using System.Linq;
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Enums; // Cần dùng WorkerState
using MyFarm.Domain.Models; // Cần dùng Worker

namespace MyFarm.Application.UseCases
{
    public class WorkerArrivedHomeUseCase
    {
        private readonly IGameDataRepository _dataRepository;

        public WorkerArrivedHomeUseCase(IGameDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public void Execute(string workerId)
        {
            var farm = _dataRepository.LoadFarm();
            // (Giả sử bạn có hàm GetWorkerById trên Farm, nếu không, dùng farm.Workers.FirstOrDefault)
            var worker = farm.Workers.FirstOrDefault(w => w.WorkerId == workerId);

            if (worker != null && worker.State == WorkerState.MovingHome)
            {
                // 1. Reset cờ để worker có thể nhận việc lại
                worker.ResetHarvestingFlag();

                // 2. Chuyển worker về trạng thái Idle (đã về nhà, rảnh rỗi)
                worker.SetStateIdle();

               _dataRepository.SaveFarm(farm);
            }
            
            
        }
    }
}