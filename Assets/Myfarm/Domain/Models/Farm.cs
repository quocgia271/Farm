using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MyFarm.Domain.Models
{
    public class Farm
    {
        [JsonProperty]
        private readonly List<FarmPlot> _plots;
        [JsonProperty]
        private readonly List<Worker> _workers;
        [JsonProperty]
        public int EquipmentLevel { get; private set; }
        

        public IReadOnlyList<FarmPlot> Plots => _plots;
        public IReadOnlyList<Worker> Workers => _workers;

        // Constructor cho JSON (để load save)
        [JsonConstructor]
        public Farm(List<FarmPlot> plots, List<Worker> workers, int equipmentLevel)
        {
            _plots = plots ?? new List<FarmPlot>();
            _workers = workers ?? new List<Worker>();
            EquipmentLevel = equipmentLevel;
        }

        // Constructor cho Game MỚI (ĐÃ SỬA LỖI)
        public Farm()
        {
            EquipmentLevel = 1;
            _workers = new List<Worker>();
            _plots = new List<FarmPlot>();

    
          
            // --------------------------------
        }
        public Worker GetWorkerById(string workerId)
        {
            return _workers.FirstOrDefault(w => w.WorkerId == workerId);
        }
        public void AddPlot(string plotId, PlotState initialState)
        {
            _plots.Add(new FarmPlot(plotId, initialState));
        }
        // --- HÀM MỚI ---
        public void UnlockPlot(string plotId)
        {
            var plot = GetPlotById(plotId);
            plot?.Unlock();
        }
        public void AddWorker()
        {
             _workers.Add(new Worker($"worker_{_workers.Count + 1}"));
        }

        public void UpgradeEquipment()
        {
            EquipmentLevel++;
        }
        
        public FarmPlot GetPlotById(string plotId)
        {
            return _plots.FirstOrDefault(p => p.PlotId == plotId);
        }
    }
}