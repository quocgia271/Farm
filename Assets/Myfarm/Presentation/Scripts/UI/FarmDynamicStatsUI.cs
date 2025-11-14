using MyFarm.Domain.Enums;
using System.Linq;
using TMPro;
using UnityEngine;

namespace MyFarm.Presentation.UI
{
    public class FarmDynamicStatsUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _workerCountText;
        [SerializeField] private TextMeshProUGUI _plotCountText;

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.DataRepository == null) return;

            var farm = GameManager.Instance.DataRepository.LoadFarm();
            if (farm == null) return; // Vẫn đang load

            // --- Đếm Công nhân (Giữ nguyên) ---
            int totalWorkers = farm.Workers.Count;
            int idleWorkers = farm.Workers.Count(w => w.State == WorkerState.Idle);
            int busyWorkers = totalWorkers - idleWorkers;

            if (_workerCountText != null)
            {
                _workerCountText.text = $"Công nhân: {busyWorkers} Bận / {idleWorkers} Rảnh";
            }

            // --- SỬA LỖI ĐẾM ĐẤT ---
            int totalPlots = farm.Plots.Count; // 20
            int lockedPlots = farm.Plots.Count(p => p.State == PlotState.Locked); // 17
            int emptyPlots = farm.Plots.Count(p => p.State == PlotState.Empty); // 3
            
            // Đất "Dùng" là đất KHÔNG khóa VÀ KHÔNG trống
            int usedPlots = totalPlots - lockedPlots - emptyPlots; // 0

            if (_plotCountText != null)
            {
                // Hiển thị cả 3 trạng thái cho rõ ràng
                _plotCountText.text = $"Đất: {usedPlots} Dùng / {emptyPlots} Trống / {lockedPlots} Khóa";
            }
        }
    }
}