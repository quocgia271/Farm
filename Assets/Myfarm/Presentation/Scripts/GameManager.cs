using MyFarm.Application.Interfaces;
using MyFarm.Application.UseCases;
using MyFarm.Infrastructure;
using UnityEngine;

namespace MyFarm.Presentation
{
    // Đây là Composition Root - Nơi duy nhất "biết tất cả" để lắp ráp hệ thống.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        // --- Các Dịch vụ (Infrastructure) ---
        public IGameDataRepository DataRepository { get; private set; }
        public IConfigLoader ConfigLoader { get; private set; }
        public IWorldTimeService TimeService { get; private set; }
        public IEventNotifier EventNotifier { get; private set; }

        // --- Các Use Case (Application) ---
        public StartProductionUseCase StartProductionUseCase { get; private set; }
        public HarvestUseCase HarvestUseCase { get; private set; }
        public SellItemUseCase SellItemUseCase { get; private set; }
        public UpdateGameTickUseCase UpdateGameTickUseCase { get; private set; }
        public BuyItemUseCase BuyItemUseCase { get; private set; } 
        public UpgradeEquipmentUseCase UpgradeEquipmentUseCase { get; private set; }
        public HireWorkerUseCase HireWorkerUseCase { get; private set; }
        public InitializeGameUseCase InitializeGameUseCase { get; private set; } // <-- Thêm UseCase khởi tạo
        public UnlockPlotUseCase UnlockPlotUseCase { get; private set; }
        public WorkerArrivedUseCase WorkerArrivedUseCase { get; private set; }
        public CancelWorkerTaskUseCase CancelWorkerTaskUseCase { get; private set; }
        public ClearPlotUseCase ClearPlotUseCase { get; private set; }
        public WorkerArrivedHomeUseCase WorkerArrivedHomeUseCase { get; private set; }
        public ValidateGameStateOnLoadUseCase ValidateGameStateOnLoadUseCase { get; private set; }
        

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 1. Khởi tạo Infrastructure (Tầng 3)
            ConfigLoader = new CsvConfigLoader();
            DataRepository = new JsonDataRepository(ConfigLoader); 
            TimeService = new UnityWorldTimeService();
            EventNotifier = new EventNotifierService();

            // 2. Khởi tạo Use Cases (Tầng 2) - TIÊM (Inject) các dịch vụ vào
            HarvestUseCase = new HarvestUseCase(DataRepository, TimeService, EventNotifier);
            StartProductionUseCase = new StartProductionUseCase(DataRepository, ConfigLoader, TimeService, EventNotifier);
            SellItemUseCase = new SellItemUseCase(DataRepository, ConfigLoader, EventNotifier);
            UpdateGameTickUseCase = new UpdateGameTickUseCase(DataRepository, TimeService, EventNotifier,ConfigLoader);
            BuyItemUseCase = new BuyItemUseCase(DataRepository, ConfigLoader, EventNotifier); 
            UpgradeEquipmentUseCase = new UpgradeEquipmentUseCase(DataRepository, EventNotifier,ConfigLoader);
            HireWorkerUseCase = new HireWorkerUseCase(DataRepository, EventNotifier, ConfigLoader);
            UnlockPlotUseCase = new UnlockPlotUseCase(DataRepository, ConfigLoader, EventNotifier);
            WorkerArrivedUseCase = new WorkerArrivedUseCase(DataRepository, TimeService,EventNotifier);
            CancelWorkerTaskUseCase = new CancelWorkerTaskUseCase(DataRepository,EventNotifier);
            ClearPlotUseCase = new ClearPlotUseCase(DataRepository, EventNotifier);
            WorkerArrivedHomeUseCase = new WorkerArrivedHomeUseCase(DataRepository);
            ValidateGameStateOnLoadUseCase = new ValidateGameStateOnLoadUseCase(DataRepository, TimeService, ConfigLoader, HarvestUseCase);
            
            

            

            // 3. Khởi tạo UseCase InitializeGame
            InitializeGameUseCase = new InitializeGameUseCase(DataRepository, ConfigLoader);
        }   

        private void Start()
        {
            // --- Thêm khởi tạo game nếu dữ liệu null ---
            InitializeGameUseCase.Execute(); // <-- Bây giờ game sẽ tự tạo dữ liệu mặc định nếu chưa có

            ValidateGameStateOnLoadUseCase.Execute();

            var player = DataRepository.LoadPlayer();
            EventNotifier.NotifyGoldChanged(player.Gold);

            foreach (var item in player.Inventory)
                EventNotifier.NotifyInventoryItemChanged(item.Key, item.Value);

            var farm = DataRepository.LoadFarm();
            EventNotifier.NotifyEquipmentUpgraded(farm.EquipmentLevel);
        }

        private void Update()
        {
            UpdateGameTickUseCase.Execute();
        }

        private void OnApplicationQuit()
        {
            DataRepository.SavePlayer(DataRepository.LoadPlayer());
            DataRepository.SaveFarm(DataRepository.LoadFarm());
        }
    }
}
