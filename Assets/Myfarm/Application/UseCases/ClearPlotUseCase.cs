// File: MyFarm/2_Application/UseCases/ClearPlotUseCase.cs
using MyFarm.Application.Interfaces; // Nơi chứa IDataRepository, IEventNotifier
using MyFarm.Domain.Enums;
using System;
using UnityEngine; // Dùng cho Debug.LogError

namespace MyFarm.Application.UseCases
{
    public class ClearPlotUseCase
    {
        private readonly IGameDataRepository _dataRepository;
        private readonly IEventNotifier _eventNotifier;

        public ClearPlotUseCase(IGameDataRepository dataRepository, IEventNotifier eventNotifier)
        {
            _dataRepository = dataRepository;
            _eventNotifier = eventNotifier;
        }

        public void Execute(string plotId)
        {
            try
            {
                var farm = _dataRepository.LoadFarm();
                if (farm == null)
                {
                    Debug.LogError("[ClearPlotUseCase] Farm data not found.");
                    return;
                }

                var plot = farm.GetPlotById(plotId);
                if (plot == null)
                {
                    Debug.LogError($"[ClearPlotUseCase] Plot {plotId} not found.");
                    return;
                }

                // 1. Chỉ cho phép dọn dẹp khi plot THỰC SỰ BỊ HỎNG
                if (plot.State != PlotState.Spoiled)
                {
                    Debug.LogWarning($"[ClearPlotUseCase] Attempted to clear plot {plotId} which is not spoiled. State: {plot.State}");
                    return;
                }

                // 2. Gọi Logic Domain
                plot.Clear(); // Hàm này sẽ tự động set State = PlotState.Empty

                // 3. Lưu lại
                _dataRepository.SaveFarm(farm);

                // 4. Thông báo cho UI cập nhật
                // (plot.State bây giờ đã là PlotState.Empty)
                _eventNotifier.NotifyPlotStateChanged(plot.PlotId, plot.State);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ClearPlotUseCase] Error clearing plot {plotId}: {ex.Message}");
            }
        }
    }
}