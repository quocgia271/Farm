using MyFarm.Application.Interfaces;
using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;
using System;
using System.Linq;
using UnityEngine; // Để dùng Debug

namespace MyFarm.Application.UseCases
{
    /// <summary>
    /// UseCase này chỉ chạy 1 LẦN KHI LOAD GAME.
    /// Nhiệm vụ: Dọn dẹp và tính toán lại trạng thái sau khi offline.
    /// </summary>
    public class ValidateGameStateOnLoadUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IWorldTimeService _timeService;
        private readonly IConfigLoader _configLoader;
        private readonly HarvestUseCase _harvestUseCase; // Cần để thu hoạch offline

        public ValidateGameStateOnLoadUseCase(
            IGameDataRepository dataRepository,
            IWorldTimeService timeService,
            IConfigLoader configLoader,
            HarvestUseCase harvestUseCase)
        {
            _dataRepository = dataRepository;
            _timeService = timeService;
            _configLoader = configLoader;
            _harvestUseCase = harvestUseCase;
        }

        public void Execute()
        {
            var farm = _dataRepository.LoadFarm();
            var currentTime = _timeService.GetCurrentTime();
            bool farmWasModified = false;

            // --- BẮT ĐẦU SỬA: THÊM KHỐI CẬP NHẬT PLOT ---
            // 1. TÍNH TOÁN BONUS %
            var equipmentConfig = _configLoader.GetEquipmentConfig(1);
            float totalBonusPercent = 0f;
            if (equipmentConfig != null)
            {
                totalBonusPercent = (farm.EquipmentLevel - 1) * equipmentConfig.ProductionBoostPercent;
            }

            // 2. CẬP NHẬT TẤT CẢ PLOT TRƯỚC
            // Đây là bước quan trọng nhất: tính toán xem plot nào đã bị hỏng (Spoiled)
            // trong thời gian offline.
            foreach (var plot in farm.Plots)
            {
                plot.UpdateState(currentTime, totalBonusPercent);
            }
            // --- KẾT THÚC SỬA ---


            // Lấy config (giả sử worker dùng chung config ID 1)
            var workerConfig = _configLoader.GetWorkerConfig(1);
            if (workerConfig == null)
            {
                Debug.LogError("ValidateGameState: Thiếu worker config 1!");
                return;
            }
            TimeSpan fixedWorkTime = TimeSpan.FromMinutes(workerConfig.ActionTimeMinutes);

            // Dùng ToList() để tránh lỗi khi sửa collection
            foreach (var worker in farm.Workers.ToList()) 
            {
                switch (worker.State)
                {
                    // 1. ĐANG DI CHUYỂN (TỚI HOẶC VỀ)
                    // -> Hủy, cho về Idle.
                    case WorkerState.MovingToTarget:
                    case WorkerState.MovingHome:
                        Debug.Log($"[Offline] Worker {worker.WorkerId} đang {worker.State} -> Reset về Idle.");
                
                        worker.SetStateIdle();
                        worker.ResetHarvestingFlag();   
                        
                        farmWasModified = true;
                        break;

                    // 2. ĐANG LÀM VIỆC
                    case WorkerState.Working:
                        var plot = farm.GetPlotById(worker.TargetPlotId);

                        // 2a. Đất bị hỏng (spoiled) hoặc không tìm thấy
                        // (Vì chúng ta đã chạy UpdateState ở trên, plot.State bây giờ đã là Spoiled)
                        if (plot == null || plot.State == PlotState.Spoiled)
                        {
                           Debug.Log($"[Offline] Worker {worker.WorkerId} (Working) -> Plot hỏng. Reset về Idle.");

                            // Set về Idle và reset cờ
                            worker.SetStateIdle();
                            worker.ResetHarvestingFlag();
                            
                            farmWasModified = true;
                            break; // Dừng xử lý worker này
                        }

                        // 2b. Tính toán thời gian (Nếu đất KHÔNG hỏng)
                        DateTime taskEndTime = worker.TaskStartTime + fixedWorkTime;

                        if (currentTime >= taskEndTime)
                        {
                            // Công việc ĐÃ HOÀN THÀNH khi offline
                            Debug.Log($"[Offline] Worker {worker.WorkerId} ĐÃ LÀM XONG. Thu hoạch và Reset về Idle.");
                            
                            // Thực hiện thu hoạch (plot vẫn đang Ready)
                            _harvestUseCase.Execute(worker.TargetPlotId);
                            
                            // Set về Idle và reset cờ
                            worker.SetStateIdle();
                            worker.ResetHarvestingFlag();
                            
                            farmWasModified = true;
                        }
                        else
                        {
                            // Công việc VẪN ĐANG DIỄN RA
                            // -> Không làm gì cả, để UpdateGameTickUseCase tiếp tục
                            Debug.Log($"[Offline] Worker {worker.WorkerId} vẫn đang làm việc (còn {taskEndTime - currentTime}).");
                        }
                        break;
                }
            }

            // Nếu có bất kỳ thay đổi nào, lưu lại farm
            if (farmWasModified)
            {
                _dataRepository.SaveFarm(farm);
            }
        }
    }
}