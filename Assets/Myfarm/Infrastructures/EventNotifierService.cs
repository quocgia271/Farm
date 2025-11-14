using MyFarm.Application.Interfaces;
using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;
using System;

namespace MyFarm.Infrastructure
{
    public class EventNotifierService : IEventNotifier
    {
        public event Action<long> OnGoldChanged;
        public event Action<string, int> OnInventoryItemChanged;
        public event Action<string, PlotState> OnPlotStateChanged;
        public event Action<int> OnWorkerCountChanged;
        public event Action<int> OnEquipmentUpgraded;
        // --- THÊM MỚI ---
        public event Action<FarmPlot> OnPlotAdded;
        public event Action<string, string> OnError;
        public event Action<string, WorkerState> OnWorkerStateChanged;

        public void NotifyGoldChanged(long newGold) => OnGoldChanged?.Invoke(newGold);
        public void NotifyInventoryItemChanged(string itemId, int newAmount) => OnInventoryItemChanged?.Invoke(itemId, newAmount);
        public void NotifyPlotStateChanged(string plotId, PlotState newState) => OnPlotStateChanged?.Invoke(plotId, newState);
        public void NotifyWorkerCountChanged(int totalWorkers) => OnWorkerCountChanged?.Invoke(totalWorkers);
        public void NotifyPlotAdded(FarmPlot newPlot) => OnPlotAdded?.Invoke(newPlot);
        public void NotifyError(string title, string message) => OnError?.Invoke(title, message);

        // --- THÊM HÀM MỚI ---
        public void NotifyEquipmentUpgraded(int newLevel)
        {
            OnEquipmentUpgraded?.Invoke(newLevel);
        }
        public void NotifyWorkerStateChanged(string workerId, WorkerState newState)
        {
            OnWorkerStateChanged?.Invoke(workerId, newState);
        }
    }
}