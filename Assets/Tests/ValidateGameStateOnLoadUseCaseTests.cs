
using NUnit.Framework;
using NSubstitute;
using MyFarm.Application.Interfaces;
using MyFarm.Application.UseCases;
using MyFarm.Domain.Models;
using MyFarm.Domain.Configs;
using MyFarm.Domain.Enums;
using System;
using System.Collections.Generic;

[TestFixture]
public class ValidateGameStateOnLoadUseCaseTests
{
    // Mocks
    private IGameDataRepository _repoMock;
    private IWorldTimeService _timeMock;
    private IConfigLoader _configMock;
    private IEventNotifier _notifierMock; // Cần cho HarvestUseCase
    
    // Use Case phụ thuộc (cũng sẽ được mock)
    private HarvestUseCase _harvestUseCaseMock;

    // System Under Test (SUT)
    private ValidateGameStateOnLoadUseCase _validateUseCase;
    private EquipmentConfig _equipConfig;
    private ProductionConfig _tomatoConfig;

    // Data
    private Farm _farm;
    private Player _player;
    private DateTime _currentTime;
    private WorkerConfig _workerConfig;

    [SetUp]
    public void Setup()
    {
        // 1. Create Mocks
        _repoMock = Substitute.For<IGameDataRepository>();
        _timeMock = Substitute.For<IWorldTimeService>();
        _configMock = Substitute.For<IConfigLoader>();
        _notifierMock = Substitute.For<IEventNotifier>(); // Thêm mock này

        // 2. Mock HarvestUseCase
        // Chúng ta không cần test logic của HarvestUseCase ở đây
        _harvestUseCaseMock = Substitute.For<HarvestUseCase>(_repoMock, _timeMock, _notifierMock);

        // 3. Create Data
        _currentTime = new DateTime(2025, 1, 1, 12, 0, 0); // 12:00
        _player = new Player(100);
        
        // Sẽ thêm worker vào farm trong từng test case
        _farm = new Farm(new List<FarmPlot>(), new List<Worker>(), 1); 

        _workerConfig = new WorkerConfig(1, "Worker", 2, 500); // 2 phút (120s) làm việc
        _equipConfig = new EquipmentConfig(1, "Hoe", 500, 10);
        _tomatoConfig = new ProductionConfig("Tomato", "tomato_seed", "tomato_fruit", 10, 5, 10, 1, false, 0);

        // 4. Teach Mocks
        _repoMock.LoadFarm().Returns(_farm);
        _repoMock.LoadPlayer().Returns(_player);
        _timeMock.GetCurrentTime().Returns(_currentTime);
        _configMock.GetWorkerConfig(1).Returns(_workerConfig);

        // 5. Initialize UseCase
        _validateUseCase = new ValidateGameStateOnLoadUseCase(
            _repoMock,
            _timeMock,
            _configMock,
            _harvestUseCaseMock // Tiêm mock HarvestUseCase vào
        );
    }

    [Test]
    public void Execute_Should_ResetWorker_When_StateIsMovingToTarget()
    {
        // Arrange
        var worker = new Worker("w1");
        worker.AssignTask("Plot_1", _currentTime.AddMinutes(-1)); // Đang đi
        _farm.AddWorker(); // (Giả sử AddWorker thêm worker này, hoặc dùng constructor)
        _farm = new Farm(new List<FarmPlot>(), new List<Worker> { worker }, 1);
        _repoMock.LoadFarm().Returns(_farm);

        Assert.AreEqual(WorkerState.MovingToTarget, worker.State);

        // Act
        _validateUseCase.Execute();

        // Assert
        Assert.AreEqual(WorkerState.Idle, worker.State, "Worker phải về Idle");
        Assert.IsFalse(worker.DoneHarvestingOnetime, "Flag phải được reset");
        _repoMock.Received(1).SaveFarm(_farm); // Phải lưu thay đổi
    }

    [Test]
    public void Execute_Should_ResetWorker_When_StateIsMovingHome()
    {
        // Arrange
        var worker = new Worker("w1");
        worker.CompleteTask(); // Đang về nhà
        _farm = new Farm(new List<FarmPlot>(), new List<Worker> { worker }, 1);
        _repoMock.LoadFarm().Returns(_farm);

        Assert.AreEqual(WorkerState.MovingHome, worker.State);
        Assert.IsTrue(worker.DoneHarvestingOnetime);

        // Act
        _validateUseCase.Execute();

        // Assert
        Assert.AreEqual(WorkerState.Idle, worker.State, "Worker phải về Idle");
        Assert.IsFalse(worker.DoneHarvestingOnetime, "Flag phải được reset");
        _repoMock.Received(1).SaveFarm(_farm); // Phải lưu thay đổi
    }
[Test]
public void Execute_Should_HarvestAndResetWorker_When_TaskFinishedOffline()
{
    // Sắp xếp (Arrange)
    var worker = new Worker("w1");
    
    // (Giả sử bạn đã sửa lỗi TimeSpan.FromMinutes)
    DateTime startTime = _currentTime.AddMinutes(-3); // Bắt đầu 3 phút trước
    worker.AssignTask("Plot_1", startTime);
    worker.StartWorking(startTime); // State là Working (config là 2 phút, nên đã xong)
    
    // --- BƯỚC SỬA LỖI: THÊM PLOT HỢP LỆ VÀO FARM ---
    // Cần tạo 1 plot ở trạng thái Ready để worker có thể thu hoạch
    var plot = new FarmPlot("Plot_1", PlotState.Ready); 
    _farm = new Farm(new List<FarmPlot> { plot }, new List<Worker> { worker }, 1);
    _repoMock.LoadFarm().Returns(_farm); // Dạy lại mock với farm mới
    // --- KẾT THÚC SỬA LỖI ---
    
    Assert.AreEqual(WorkerState.Working, worker.State);

    // Hành động (Act)
    _validateUseCase.Execute();

    // Khẳng định (Assert)
    // Bây giờ plot != null, và thời gian đã xong, nên HarvestUseCase PHẢI được gọi
    _harvestUseCaseMock.Received(1).Execute("Plot_1");
    
    // (Lưu ý: Logic gốc của bạn set worker về Idle.
    // Nếu bạn muốn nó về MovingHome, bạn phải sửa ValidateGameStateOnLoadUseCase.cs)
    Assert.AreEqual(WorkerState.Idle, worker.State, "Worker phải về Idle");
    _repoMock.Received(1).SaveFarm(_farm);
}

   [Test]
public void Execute_Should_DoNothingToWorker_When_TaskIsStillInProgress()
{
    // Arrange
    var worker = new Worker("w1");
    DateTime startTime = _currentTime.AddMinutes(-1); // Bắt đầu 1 phút trước
    worker.AssignTask("Plot_1", startTime);
    worker.StartWorking(startTime); // State là Working (config là 2 phút, chưa xong)
    
    // --- BƯỚC SỬA LỖI: THÊM PLOT VÀO FARM ---
    var plot = new FarmPlot("Plot_1", PlotState.Growing); // Tạo 1 plot hợp lệ
    _farm = new Farm(new List<FarmPlot> { plot }, new List<Worker> { worker }, 1);
    _repoMock.LoadFarm().Returns(_farm);
    // --- KẾT THÚC SỬA LỖI ---

    Assert.AreEqual(WorkerState.Working, worker.State);

    // Act
    _validateUseCase.Execute();

    // Assert
    // Bây giờ plot != null, và thời gian chưa xong, nên worker sẽ giữ nguyên
    Assert.AreEqual(WorkerState.Working, worker.State, "Worker phải giữ nguyên Working");
    
    _harvestUseCaseMock.DidNotReceive().Execute(Arg.Any<string>());
    _repoMock.DidNotReceive().SaveFarm(Arg.Any<Farm>());
}
// --- TEST CASE MỚI ---
    [Test]
    public void Execute_Should_ResetWorker_When_PlotSpoiledOffline()
    {
        // Sắp xếp (Arrange)
        
        // 1. Dạy mock cho logic cập nhật plot mới (cần cho UpdateState)
        _configMock.GetProductionConfig("tomato_seed").Returns(_tomatoConfig);

        // 2. Thiết lập plot BỊ HỎNG
        // Plot này đã thu hoạch hết (5/5) và thời hạn thu hoạch cuối cùng (FinalHarvestDeadline)
        // là 1 phút trước thời điểm hiện tại (11:59)
        var plot = new FarmPlot(
            "Plot_1",
            PlotState.Ready, // Ban đầu vẫn Ready
            _tomatoConfig,
            _currentTime.AddMinutes(-10), // currentCycleStartTime
            5,  // totalProductsProduced (đã đủ 5/5)
            1,  // productsInQueue (vẫn còn 1)
            _currentTime.AddMinutes(-1) // FinalHarvestDeadline (ĐÃ HẾT HẠN)
        );
        
        // 3. Thiết lập worker đang làm việc trên plot đó
        var worker = new Worker("w1");
        DateTime startTime = _currentTime.AddMinutes(-3); // Bắt đầu 3 phút trước (đã xong)
        worker.AssignTask("Plot_1", startTime);
        worker.StartWorking(startTime); // State là Working

        _farm = new Farm(new List<FarmPlot> { plot }, new List<Worker> { worker }, 1);
        _repoMock.LoadFarm().Returns(_farm);
        
        Assert.AreEqual(WorkerState.Working, worker.State);
        Assert.AreEqual(PlotState.Ready, plot.State);

        // Hành động (Act)
        _validateUseCase.Execute(); // SUT

        // Khẳng định (Assert)
        
        // 1. Logic UpdateState của plot phải chạy ĐẦU TIÊN
        // và chuyển plot sang Spoiled
        Assert.AreEqual(PlotState.Spoiled, plot.State, "Plot phải bị chuyển thành Spoiled");

        // 2. Vì plot đã Spoiled, worker phải bị hủy task
        _harvestUseCaseMock.DidNotReceive().Execute(Arg.Any<string>()); // KHÔNG được thu hoạch
        
        // 3. Worker phải về Idle và reset cờ
        Assert.AreEqual(WorkerState.Idle, worker.State, "Worker phải về Idle");
        Assert.IsFalse(worker.DoneHarvestingOnetime, "Flag phải được reset");
        _repoMock.Received(1).SaveFarm(_farm);
    }
}