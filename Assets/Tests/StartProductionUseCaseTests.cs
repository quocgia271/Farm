// Tên file: MyFarm.Application.Tests/StartProductionUseCaseTests.cs
using NUnit.Framework;
using NSubstitute;
using MyFarm.Application.Interfaces;
using MyFarm.Application.UseCases;
using MyFarm.Domain.Models;
using MyFarm.Domain.Enums;
using MyFarm.Domain.Configs;
using System;
using System.Collections.Generic; // <--- THÊM DÒNG NÀY

[TestFixture]
public class StartProductionUseCaseTests
{
    // Các dịch vụ giả lập
    private IGameDataRepository _repoMock;
    private IConfigLoader _configMock;
    private IWorldTimeService _timeMock;
    private IEventNotifier _notifierMock;

    // Dữ liệu thật để test
    private Player _player;
    private Farm _farm;
    private FarmPlot _plot;
    private ProductionConfig _tomatoConfig;
    private DateTime _currentTime;

    // Đối tượng cần test
    private StartProductionUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        // 1. Tạo Mocks
        _repoMock = Substitute.For<IGameDataRepository>();
        _configMock = Substitute.For<IConfigLoader>();
        _timeMock = Substitute.For<IWorldTimeService>();
        _notifierMock = Substitute.For<IEventNotifier>();

        // 2. Tạo Data
        _currentTime = new DateTime(2025, 1, 1, 12, 0, 0);
        _player = new Player(100);
        _player.AddItem("tomato_seed", 5); // Có 5 hạt
        
        _plot = new FarmPlot("Plot_1", PlotState.Empty); // Đất trống
        // Dòng 45 (giờ đã đúng)
        _farm = new Farm(new List<FarmPlot> { _plot }, new List<Worker>(), 1); 
        
        _tomatoConfig = new ProductionConfig("Tomato", "tomato_seed", "tomato_fruit", 10, 5, 10, 1, false, 0);

        // 3. Dạy Mocks
        _repoMock.LoadPlayer().Returns(_player);
        _repoMock.LoadFarm().Returns(_farm);
        _configMock.GetProductionConfig("tomato_seed").Returns(_tomatoConfig);
        _timeMock.GetCurrentTime().Returns(_currentTime);

        // 4. Khởi tạo UseCase
        _useCase = new StartProductionUseCase(
            _repoMock, 
            _configMock, 
            _timeMock, 
            _notifierMock
        );
    }

    [Test]
    public void Execute_Should_Succeed_WhenPlayerHasSeedsAndPlotIsEmpty()
    {
        // Arrange
        // Player has 5 seeds, plot is empty (from Setup)

        // Act
        bool result = _useCase.Execute("Plot_1", "tomato_seed");

        // Assert (State)
        Assert.IsTrue(result);
        Assert.AreEqual(4, _player.GetItemCount("tomato_seed"), "Must use 1 seed");
        Assert.AreEqual(PlotState.Growing, _plot.State, "Plot must change to Growing");
        Assert.AreEqual(_currentTime, _plot.CurrentCycleStartTime, "Must set the correct start time");

        // Assert (Verify) - Ensure the UseCase saved and notified
        _repoMock.Received(1).SavePlayer(_player);
        _repoMock.Received(1).SaveFarm(_farm);
        _notifierMock.Received(1).NotifyInventoryItemChanged("tomato_seed", 4);
        _notifierMock.Received(1).NotifyPlotStateChanged("Plot_1", PlotState.Growing);
    }

    [Test]
    public void Execute_Should_Fail_WhenPlayerHasNoSeeds()
    {
        // Arrange
        _player.TryRemoveItem("tomato_seed", 5); // Sell all seeds
        Assert.AreEqual(0, _player.GetItemCount("tomato_seed"));

        // Act
        bool result = _useCase.Execute("Plot_1", "tomato_seed");

        // Assert (State)
        Assert.IsFalse(result);
        Assert.AreEqual(PlotState.Empty, _plot.State, "Plot must remain Empty");

        // Assert (Verify) - Must not save or notify
        _repoMock.DidNotReceive().SavePlayer(Arg.Any<Player>());
        _repoMock.DidNotReceive().SaveFarm(Arg.Any<Farm>());
        _notifierMock.DidNotReceive().NotifyInventoryItemChanged(Arg.Any<string>(), Arg.Any<int>());
    }

    [Test]
    public void Execute_Should_Fail_WhenPlotIsNotEmpty()
    {
        // Arrange
        _plot.StartProduction(_tomatoConfig, _currentTime.AddMinutes(-5)); // Make the plot busy
        Assert.AreEqual(PlotState.Growing, _plot.State);
        Assert.AreEqual(5, _player.GetItemCount("tomato_seed")); // Still have 5 seeds

        // Act
        bool result = _useCase.Execute("Plot_1", "tomato_seed");

        // Assert (State)
        Assert.IsFalse(result);
        Assert.AreEqual(5, _player.GetItemCount("tomato_seed"), "Must return the seed if plot is busy");
        
        // Assert (Verify) - Must not save
        _repoMock.DidNotReceive().SavePlayer(Arg.Any<Player>());
        _repoMock.DidNotReceive().SaveFarm(Arg.Any<Farm>());
    }
}