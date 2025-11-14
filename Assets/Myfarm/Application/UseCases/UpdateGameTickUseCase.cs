// File: UpdateGameTickUseCase.cs (Đã sửa lỗi kẹt + đảm bảo biến tồn tại)
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Enums;
using System;
using System.Linq;
using UnityEngine;

namespace MyFarm.Application.UseCases
{
    public class UpdateGameTickUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IWorldTimeService _timeService;
        private readonly IEventNotifier _eventNotifier;
        private readonly IConfigLoader _configLoader; 

        public UpdateGameTickUseCase(IGameDataRepository dataRepository, 
                                     IWorldTimeService timeService, 
                                     IEventNotifier eventNotifier, 
                                     IConfigLoader configLoader) 
        {
            _dataRepository = dataRepository;
            _timeService = timeService;
            _eventNotifier = eventNotifier;
            _configLoader = configLoader;
        }

        public void Execute()
        {
            var currentTime = _timeService.GetCurrentTime();
            var farm = _dataRepository.LoadFarm();
            var player = _dataRepository.LoadPlayer(); 

            // 1. TÍNH TOÁN BONUS % (ĐẢM BẢO PHẦN NÀY CÒN NGUYÊN)
            var equipmentConfig = _configLoader.GetEquipmentConfig(1);
            float totalBonusPercent = 0f; // <-- DÒNG NÀY PHẢI TỒN TẠI
            if (equipmentConfig != null)
            {
                totalBonusPercent = (farm.EquipmentLevel - 1) * equipmentConfig.ProductionBoostPercent;
            }

            // 2. LẤY THỜI GIAN WORKER (ĐẢM BẢO PHẦN NÀY CÒN NGUYÊN)
            var workerConfig = _configLoader.GetWorkerConfig(1);
            if (workerConfig == null) { Debug.LogError("Thiếu worker config 1!"); return; }
            // (Chuyển về FromMinutes khi test xong)
            TimeSpan fixedWorkTime = TimeSpan.FromMinutes(workerConfig.ActionTimeMinutes); 

            // 3. CẬP NHẬT PLOTS (DÙNG totalBonusPercent)
            foreach (var plot in farm.Plots)
            {
                plot.UpdateState(currentTime, totalBonusPercent); // <-- Dòng này cần 'totalBonusPercent'
            }

            // 4. CẬP NHẬT AI WORKER
            var busyPlotIds = farm.Workers
                                .Where(w => w.State == WorkerState.Working || w.State == WorkerState.MovingToTarget)
                                .Select(w => w.TargetPlotId)
                                .ToList();
            var readyPlots = farm.Plots
                                .Where(p => p.State == PlotState.Ready && p.ProductsInQueue > 0 && !busyPlotIds.Contains(p.PlotId))
                                .ToList();

            foreach (var worker in farm.Workers)
            {
                if (worker.State == WorkerState.Working)
                {
                    var plot = farm.GetPlotById(worker.TargetPlotId);
                    // KIỂM TRA HẾT GIỜ (DÙNG fixedWorkTime)
                    if (currentTime - worker.TaskStartTime >= fixedWorkTime)
                    {
                        
                        
                        if (plot != null && plot.State == PlotState.Ready && plot.ProductsInQueue > 0)
                        {
                            (string productId, int amount) = plot.Harvest();
                            player.AddItem(productId, amount);
                            _eventNotifier.NotifyInventoryItemChanged(productId, player.GetItemCount(productId));
                            _eventNotifier.NotifyPlotStateChanged(plot.PlotId, plot.State);
                        }
                        
                        // SỬA LỖI KẸT
                        worker.CompleteTask(); 
                    }
                    else if (plot == null || plot.State == PlotState.Spoiled)
                    {
                        // Plot đã bị hỏng trong khi worker đang làm
                        // Hủy task và cho đi về
                        worker.CompleteTask();
                      
                    }
                }

                if (worker.State == WorkerState.Idle && !worker.DoneHarvestingOnetime && readyPlots.Count > 0)
                {
                    var plotToHarvest = readyPlots[0];
                    readyPlots.RemoveAt(0);
                    worker.AssignTask(plotToHarvest.PlotId, currentTime);
                }
            }
            
            _dataRepository.SavePlayer(player); 
            _dataRepository.SaveFarm(farm);
        }
    }
}