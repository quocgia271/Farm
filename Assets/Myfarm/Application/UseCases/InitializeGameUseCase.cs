using System;
using MyFarm.Application.Interfaces;
using MyFarm.Domain.Enums;
using MyFarm.Domain.Models;

namespace MyFarm.Application.UseCases
{
    public class InitializeGameUseCase
    {
        private readonly IGameDataRepository _repository;
        private readonly IConfigLoader _configLoader;
        private const int TOTAL_PLOTS_IN_WORLD = 20;

        public InitializeGameUseCase(IGameDataRepository repository, IConfigLoader configLoader)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configLoader = configLoader ?? throw new ArgumentNullException(nameof(configLoader));
        }

        public void Execute()
        {
            InitializePlayer();
            InitializeFarm();
        }

        private void InitializePlayer()
        {
            var player = _repository.LoadPlayer();
            if (player != null)
                return; // Player đã tồn tại, không cần khởi tạo

            var config = _configLoader.GetFarmConfig(1) ?? throw new InvalidOperationException("Farm config not found.");

            player = new Player(config.Gold);

            // --- SỬA Ở ĐÂY ---
            // 1. Thêm Carrot (từ cột CarrotSeeds)
            if (config.CarrotSeeds > 0)
                player.AddItem("carrot_seed", config.CarrotSeeds);

            // 2. Thêm Broccoli (từ cột BroccoliSeeds)
            if (config.BroccoliSeeds > 0)
                player.AddItem("broccoli_seed", config.BroccoliSeeds);

            // 3. Thêm Bò (từ cột Cows)
            if (config.Cows > 0)
                player.AddItem("cow", config.Cows);
            
            // 4. Thêm Cauliflower (nếu có)
            if (config.CauliflowerSeeds > 0)
                player.AddItem("cauliflower_seed", config.CauliflowerSeeds);
            // --- HẾT SỬA ---

            _repository.SavePlayer(player);
        }

        private void InitializeFarm()
        {
            var farm = _repository.LoadFarm();
            if (farm != null)
                return; // Farm đã tồn tại, không cần khởi tạo

            var config = _configLoader.GetFarmConfig(1) ?? throw new InvalidOperationException("Farm config not found.");

            farm = new Farm(); // Constructor rỗng

            // Khởi tạo Workers
            if (config.Workers > 0)
            {
                for (int i = 0; i < config.Workers; i++)
                {
                    farm.AddWorker();
                }
            }

            // Khởi tạo Plots (3 Mảnh đất ban đầu)
            int initialPlots = Math.Clamp(config.LandPlots, 0, TOTAL_PLOTS_IN_WORLD);

            for (int i = 1; i <= TOTAL_PLOTS_IN_WORLD; i++)
            {
                string plotId = $"Plot_{i}";
                // Các mảnh đất từ 1 -> initialPlots (3) sẽ là Empty, còn lại là Locked
                PlotState state = i <= initialPlots ? PlotState.Empty : PlotState.Locked;
                farm.AddPlot(plotId, state);
            }

            // Khởi tạo Equipment (Cấp 1)
            // Sửa logic: UpgradeEquipment() tăng 1 cấp, nên cần gọi (Level - 1) lần
            // Nếu level config là 1, ta không gọi lần nào.
            for (int i = 1; i < config.EquipmentLevel; i++) // Bắt đầu từ 1
            {
                farm.UpgradeEquipment();
            }
           
            _repository.SaveFarm(farm);
        }
    }
}