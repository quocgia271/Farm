using System;
using System.Diagnostics;
using MyFarm.Domain.Enums;
using Newtonsoft.Json; // <-- Thêm vào

namespace MyFarm.Domain.Models
{
    public class Worker
    {
        [JsonProperty] // <-- Sửa
        public string WorkerId { get; private set; }
        
        [JsonProperty] // <-- Sửa
        public WorkerState State { get; private set; } = WorkerState.Idle;
        [JsonProperty] public string TargetPlotId { get; private set; }
        
        [JsonProperty] // <-- Sửa
        public DateTime TaskStartTime { get; private set; }
        [JsonProperty]
        public bool DoneHarvestingOnetime { get; private set; } = false;

        [JsonConstructor] // <-- Sửa
        public Worker(string id)
        {
            WorkerId = id;
        }

        // ... (phần còn lại của code)
        public void AssignTask(string plotId, DateTime currentTime)
        {
            if (State != WorkerState.Idle) return;
            TargetPlotId = plotId;
            State = WorkerState.MovingToTarget;
          
        }
        
        public void StartWorking(DateTime currentTime)
        {
             State = WorkerState.Working;
             TaskStartTime = currentTime;
        }

        public void CompleteTask()
        {
            State = WorkerState.MovingHome;
            TargetPlotId = null;

            // --- CẬP NHẬT ---
            // Đặt cờ báo hiệu đã thu hoạch xong 1 lần
            DoneHarvestingOnetime = true;
        }
        public void ResetHarvestingFlag()
        {
            DoneHarvestingOnetime = false;
        }
        public void SetStateIdle()
        {
            State = WorkerState.Idle;
        }
    }
}