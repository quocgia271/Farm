using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;
using System;

namespace MyFarm.Application.Interfaces
{
    public interface IEventNotifier
    {
        event Action<long> OnGoldChanged;
        event Action<string, int> OnInventoryItemChanged;
        event Action<string, PlotState> OnPlotStateChanged;
        event Action<int> OnWorkerCountChanged;
        
        // --- THÊM SỰ KIỆN MỚI ---
        event Action<int> OnEquipmentUpgraded; // int: newLevel
        event Action<FarmPlot> OnPlotAdded; // Báo cho WorldManager "vẽ" đất
        event Action<string, string> OnError; // Báo cho UIManager (Tiêu đề, Nội dung)

        event Action<string, WorkerState> OnWorkerStateChanged;
        void NotifyGoldChanged(long newGold);
        void NotifyInventoryItemChanged(string itemId, int newAmount);
        void NotifyPlotStateChanged(string plotId, PlotState newState);
        void NotifyWorkerCountChanged(int totalWorkers);
        void NotifyWorkerStateChanged(string workerId, WorkerState newState);

        // --- THÊM HÀM MỚI ---
        void NotifyEquipmentUpgraded(int newLevel);
        // --- THÊM MỚI ---
        void NotifyPlotAdded(FarmPlot newPlot);
        void NotifyError(string title, string message);
        
    }
}