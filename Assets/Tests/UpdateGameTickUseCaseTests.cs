// Tên file: MyFarm.Application.Tests/UpdateGameTickUseCaseTests.cs
using NUnit.Framework;
using NSubstitute;
using MyFarm.Application.Interfaces;
using MyFarm.Application.UseCases;
using MyFarm.Domain.Models;
using MyFarm.Domain.Configs;
using MyFarm.Domain.Enums;
using System;
using System.Collections.Generic; // <--- THÊM DÒNG NÀY

[TestFixture]
public class UpdateGameTickUseCaseTests
{
    // Mocks
    private IGameDataRepository _repoMock;
    private IWorldTimeService _timeMock;
    private IEventNotifier _notifierMock;
    private IConfigLoader _configMock;
    
    // Real HarvestUseCase (it's simple and tested separately)
    private HarvestUseCase _harvestUseCase; 

    // SUT
    private UpdateGameTickUseCase _updateTickUseCase;

    // Data
    private Farm _farm;
    private Player _player;
    private DateTime _currentTime;
    private ProductionConfig _tomatoConfig;
    private WorkerConfig _workerConfig;
    private EquipmentConfig _equipConfig;

    [SetUp]
    public void Setup()
    {
        // 1. Create Mocks
        _repoMock = Substitute.For<IGameDataRepository>();
        _timeMock = Substitute.For<IWorldTimeService>();
        _notifierMock = Substitute.For<IEventNotifier>();
        _configMock = Substitute.For<IConfigLoader>();

        // 2. Create Data
        _currentTime = new DateTime(2025, 1, 1, 12, 0, 0); // 12:00
        _player = new Player(100);
        _farm = new Farm(new List<FarmPlot>(), new List<Worker>(), 1); // Level 1

        _tomatoConfig = new ProductionConfig("Tomato", "tomato_seed", "tomato_fruit", 10, 5, 10, 1, false, 0);
        _workerConfig = new WorkerConfig(1, "Worker", 2, 500); // 2 minutes (120s) work time
        _equipConfig = new EquipmentConfig(1, "Hoe", 500, 10); // 10% bonus / level

        // 3. Teach Mocks
        _repoMock.LoadFarm().Returns(_farm);
        _repoMock.LoadPlayer().Returns(_player);
        _timeMock.GetCurrentTime().Returns(_currentTime);
        
        _configMock.GetEquipmentConfig(1).Returns(_equipConfig);
        _configMock.GetWorkerConfig(1).Returns(_workerConfig);
        _configMock.GetProductionConfig(Arg.Is<string>(s => s.Contains("seed"))).Returns(_tomatoConfig);

        // 4. Initialize Use Cases
        // Pass the real mocks to HarvestUseCase
        _harvestUseCase = new HarvestUseCase(_repoMock, _timeMock, _notifierMock); 
        
        _updateTickUseCase = new UpdateGameTickUseCase(
            _repoMock,
            _timeMock,
            _notifierMock,
            _configMock
        );
    }
    
    [Test]
    public void Execute_Should_AssignIdleWorkerToReadyPlot()
    {
        // Arrange
        // Add an idle worker
        var worker = new Worker("worker_1");
        // Add a READY plot
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _currentTime.AddMinutes(-10)); // Planted 10 mins ago
        plot.UpdateState(_currentTime, 0f); // Update to Ready
        
        // Load the "real" farm data into the mock
        _farm = new Farm(new List<FarmPlot> { plot }, new List<Worker> { worker }, 1);
        _repoMock.LoadFarm().Returns(_farm); // Re-teach the mock

        Assert.AreEqual(PlotState.Ready, plot.State, "Plot must be Ready");
        Assert.AreEqual(1, plot.ProductsInQueue);
        Assert.AreEqual(WorkerState.Idle, worker.State, "Worker must be Idle");

        // Act
        _updateTickUseCase.Execute();

        // Assert
        // Worker must be assigned and moving
        Assert.AreEqual(WorkerState.MovingToTarget, worker.State);
        Assert.AreEqual("Plot_1", worker.TargetPlotId);
        
        // Must save changes
        _repoMock.Received(1).SaveFarm(_farm);
    }

    [Test]
    public void Execute_Should_CompleteTask_WhenWorkerFinishesWorkTime()
    {
        // Arrange
        var worker = new Worker("worker_1");
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _currentTime.AddMinutes(-20)); // Planted 20 mins ago
        plot.UpdateState(_currentTime, 0f); // Plot has 2 products
        
        _farm = new Farm(new List<FarmPlot> { plot }, new List<Worker> { worker }, 1);
        _repoMock.LoadFarm().Returns(_farm);
        
        Assert.AreEqual(2, plot.ProductsInQueue);

        // Simulate a worker who started working 2 minutes ago (which is the full work time)
        DateTime workStartTime = _currentTime.AddMinutes(-2); // 11:58
        worker.AssignTask(plot.PlotId, workStartTime);
        worker.StartWorking(workStartTime);
        
        Assert.AreEqual(WorkerState.Working, worker.State);
        Assert.AreEqual(0, _player.GetItemCount("tomato_fruit")); // No fruit yet

        // Act
        _updateTickUseCase.Execute(); // Current time is 12:00, so 2 mins have passed

        // Assert
        // Worker must harvest (via HarvestUseCase) and go home
        Assert.AreEqual(WorkerState.MovingHome, worker.State);
        Assert.IsTrue(worker.DoneHarvestingOnetime);
        Assert.AreEqual(2, _player.GetItemCount("tomato_fruit"), "Player must receive 2 fruit");
        Assert.AreEqual(0, plot.ProductsInQueue, "Plot queue must be empty");
        
        // Must save changes
        _repoMock.Received(1).SaveFarm(_farm);
    }
    [Test]
    public void Execute_Should_CancelTask_WhenPlotSpoilsWhileWorking()
    {
        // Sắp xếp (Arrange)
        var worker = new Worker("worker_1");

        // 1. Tạo 1 plot SẼ BỊ HỎNG (Spoiled) ngay trong tick này
        // Giả sử nó đã thu hoạch hết (5/5) và FinalHarvestDeadline là 1 giây TRƯỚC thời gian hiện tại
        var plot = new FarmPlot(
            "Plot_1",
            PlotState.Ready,
            _tomatoConfig, // Config 10 phút, 5 lần thu hoạch
            _currentTime.AddMinutes(-10), // currentCycleStartTime (đã xong 1 chu kỳ)
            5,  // totalProductsProduced (đã đủ 5/5)
            1,  // productsInQueue (vẫn còn 1 sản phẩm)
            _currentTime.AddSeconds(-1) // FinalHarvestDeadline (HẾT HẠN 1 giây trước)
        );
        
        _farm = new Farm(new List<FarmPlot> { plot }, new List<Worker> { worker }, 1);
        _repoMock.LoadFarm().Returns(_farm);
        _configMock.GetProductionConfig("tomato_seed").Returns(_tomatoConfig); // Dạy mock cho UpdateState

        // 2. Worker đang làm việc trên plot này, NHƯNG CHƯA XONG
        DateTime workStartTime = _currentTime.AddMinutes(-1); // Bắt đầu lúc 11:59 (cần 2 phút, nên chưa xong)
        worker.AssignTask(plot.PlotId, workStartTime);
        worker.StartWorking(workStartTime);

        Assert.AreEqual(WorkerState.Working, worker.State);
        Assert.AreEqual(PlotState.Ready, plot.State, "Plot vẫn Ready TRƯỚC KHI tick");
        Assert.AreEqual(1, plot.ProductsInQueue);
        Assert.AreEqual(0, _player.GetItemCount("tomato_fruit")); // Player chưa có gì

        // Hành động (Act)
        _updateTickUseCase.Execute(); // Thời gian hiện tại là 12:00

        // Khẳng định (Assert)
        
        // 1. Logic plot.UpdateState() (ở đầu UpdateGameTickUseCase) phải chạy
        // và chuyển plot sang Spoiled
        Assert.AreEqual(PlotState.Spoiled, plot.State, "Plot phải bị Spoiled");
        Assert.AreEqual(0, plot.ProductsInQueue, "Sản phẩm phải bị mất khi Spoiled");

        // 2. Logic của Worker chạy:
        // - 'if (hết giờ làm)' -> FALSE (vì mới làm 1/2 phút)
        // - 'else if (plot.State == PlotState.Spoiled)' -> TRUE
        // - 'worker.CompleteTask()' được gọi
        Assert.AreEqual(WorkerState.MovingHome, worker.State, "Worker phải đi về nhà khi plot hỏng");
        Assert.IsTrue(worker.DoneHarvestingOnetime, "Flag DoneHarvestingOnetime phải được set");

        // 3. Worker KHÔNG được thu hoạch
        Assert.AreEqual(0, _player.GetItemCount("tomato_fruit"), "Player không được nhận sản phẩm hỏng");
        
        // 4. Phải lưu trạng thái
        _repoMock.Received(1).SaveFarm(_farm);
    }
}