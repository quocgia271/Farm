using System;
using MyFarm.Domain.Configs;
using MyFarm.Domain.Enums;
using Newtonsoft.Json;

namespace MyFarm.Domain.Models
{
    public class FarmPlot
    {
        [JsonProperty] public string PlotId { get; private set; }
        [JsonProperty] public PlotState State { get; private set; } = PlotState.Empty;
        [JsonProperty] public ProductionConfig CurrentConfig { get; private set; }
        
        [JsonProperty] public DateTime CurrentCycleStartTime { get; private set; }
        [JsonProperty] public int TotalProductsProduced { get; private set; }
        [JsonProperty] public int ProductsInQueue { get; private set; }
        [JsonProperty] public DateTime? FinalHarvestDeadline { get; private set; }
        

        [JsonConstructor]
        public FarmPlot(string plotId, PlotState state, ProductionConfig currentConfig, DateTime currentCycleStartTime, int totalProductsProduced, int productsInQueue, DateTime? finalHarvestDeadline)
        {
            PlotId = plotId;
            State = state;
            CurrentConfig = currentConfig;
            CurrentCycleStartTime = currentCycleStartTime;
            TotalProductsProduced = totalProductsProduced;
            ProductsInQueue = productsInQueue;
            FinalHarvestDeadline = finalHarvestDeadline;
        }

        public FarmPlot(string plotId, PlotState initialState)
        {
            PlotId = plotId;
            State = initialState;
        }

        public void Unlock()
        {
            if (State == PlotState.Locked)
            {
                State = PlotState.Empty;
            }
        }

        public void StartProduction(ProductionConfig config, DateTime startTime)
        {
            if (State != PlotState.Empty) throw new InvalidOperationException("Plot not empty");
            CurrentConfig = config;
            CurrentCycleStartTime = startTime;
            TotalProductsProduced = 0;
            ProductsInQueue = 0;
            FinalHarvestDeadline = null;
            State = PlotState.Growing;
        }

        public void UpdateState(DateTime currentTime, float totalBonusPercent)
        {
            if (State == PlotState.Empty || State == PlotState.Spoiled || CurrentConfig == null) return;

            if (State == PlotState.Ready && ProductsInQueue == 0)
            {
                if (TotalProductsProduced >= CurrentConfig.MaxHarvestTimes)
                {
                    Clear();
                    return;
                }
                else
                {
                    State = PlotState.Growing;
                }
            }
    
            double multiplier = 1.0 + (totalBonusPercent / 100.0);
            if (multiplier <= 0) multiplier = 0.0001;
            TimeSpan effectiveGrowthTime = TimeSpan.FromTicks((long)(CurrentConfig.GrowthTime.Ticks / multiplier));

            while (TotalProductsProduced < CurrentConfig.MaxHarvestTimes && 
                   currentTime - CurrentCycleStartTime >= effectiveGrowthTime)
            {
                TotalProductsProduced++;
                ProductsInQueue++;
                State = PlotState.Ready; 
                CurrentCycleStartTime += effectiveGrowthTime;
            }

            // --- SỬA Ở ĐÂY ---
            // 3. Logic Hỏng (Spoiled) - CHỈ ÁP DỤNG KHI ĐÃ HẾT VÒNG ĐỜI
            if (TotalProductsProduced >= CurrentConfig.MaxHarvestTimes)
            {
                // Nếu đây là lần đầu tiên chạm mốc tối đa, thiết lập deadline 1 giờ
                if (FinalHarvestDeadline == null)
                {
                    // Đổi 10 giây thành 1 giờ (theo GDD mới)
                    FinalHarvestDeadline = currentTime + TimeSpan.FromHours(1); 
                }

                // Nếu đã quá deadline mà vẫn chưa thu hoạch hết
                if (currentTime >= FinalHarvestDeadline.Value)
                {
                    State = PlotState.Spoiled;
                    ProductsInQueue = 0; // Mất hết sản phẩm
                }
            }
            // --- HẾT SỬA ---
        }

        public (string, int) Harvest()
        {
            if (State != PlotState.Ready || ProductsInQueue <= 0) 
                throw new InvalidOperationException("Nothing to harvest");

            string productId = CurrentConfig.ProductId;
            int amountHarvested = ProductsInQueue;

            ProductsInQueue = 0;

            if (TotalProductsProduced >= CurrentConfig.MaxHarvestTimes)
            {
                Clear();
            }
            // (Để UpdateState() tự chuyển về Growing)

            return (productId, amountHarvested);
        }
        
        public void Clear()
        {
            State = PlotState.Empty;
            CurrentConfig = null;
            TotalProductsProduced = 0;
            ProductsInQueue = 0;
            FinalHarvestDeadline = null;
        }
        
        public TimeSpan GetEffectiveGrowthTime(float totalBonusPercent)
        {
             if (CurrentConfig == null) return TimeSpan.Zero;
             double multiplier = 1.0 + (totalBonusPercent / 100.0);
             if (multiplier <= 0) multiplier = 0.0001;
             return TimeSpan.FromTicks((long)(CurrentConfig.GrowthTime.Ticks / multiplier));
        }
    }
}