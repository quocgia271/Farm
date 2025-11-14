// Tên file: MyFarm.Domain.Tests/FarmPlotTests.cs
using NUnit.Framework;
using MyFarm.Domain.Models;
using MyFarm.Domain.Enums;
using MyFarm.Domain.Configs;
using System;

[TestFixture]
public class FarmPlotTests
{
    private ProductionConfig _tomatoConfig;
    private DateTime _startTime;

    [SetUp]
    public void Setup()
    {
        // Arrange: Create a sample config for testing (10 min growth, 5 harvests)
        _tomatoConfig = new ProductionConfig(
            name: "Tomato",
            id: "tomato_seed",
            productId: "tomato_fruit",
            growthTimeMinutes: 10,
            maxHarvestTimes: 5,
            buyPrice: 10,
            minAmountToBuy: 1,
            wholesale: false,
            priceBuyWholesale: 0
        );
        
        // Arrange: Set a fixed start time
        _startTime = new DateTime(2025, 1, 1, 12, 0, 0); // 12:00:00
    }

    [Test]
    public void StartProduction_Should_ChangeStateToGrowing_WhenCalled()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);

        // Act
        plot.StartProduction(_tomatoConfig, _startTime);

        // Assert
        Assert.AreEqual(PlotState.Growing, plot.State);
        Assert.AreEqual(0, plot.ProductsInQueue);
        Assert.AreEqual(_tomatoConfig, plot.CurrentConfig);
    }

    [Test]
    public void UpdateState_Should_ChangeToReady_WhenGrowthTimeHasPassed()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _startTime); // Planted at 12:00

        // Act
        // Simulate time passing exactly 10 minutes
        DateTime currentTime = _startTime.AddMinutes(10); // 12:10:00
        plot.UpdateState(currentTime, 0f); // 0% bonus

        // Assert
        Assert.AreEqual(PlotState.Ready, plot.State);
        Assert.AreEqual(1, plot.ProductsInQueue, "Must have 1 product in queue");
        Assert.AreEqual(1, plot.TotalProductsProduced, "Must count as 1 production cycle");
    }

    [Test]
    public void UpdateState_Should_StackProducts_WhenOfflineForLongDuration()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _startTime); // Planted at 12:00

        // Act
        // Simulate being offline for 35 mins (enough for 3 growth cycles at 10, 20, 30 min)
        DateTime currentTime = _startTime.AddMinutes(35); // 12:35:00
        plot.UpdateState(currentTime, 0f); 

        // Assert
        Assert.AreEqual(PlotState.Ready, plot.State);
        Assert.AreEqual(3, plot.ProductsInQueue, "Must have 3 stacked products");
        Assert.AreEqual(3, plot.TotalProductsProduced);
    }

    [Test]
    public void UpdateState_Should_StopProducing_WhenMaxHarvestTimesIsReached()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _startTime); // Planted at 12:00 (Max 5 harvests)

        // Act
        // Simulate being offline for 100 mins (enough for 10 cycles, but limited to 5)
        DateTime currentTime = _startTime.AddMinutes(100); // 13:40:00
        plot.UpdateState(currentTime, 0f); 

        // Assert
        Assert.AreEqual(PlotState.Ready, plot.State);
        Assert.AreEqual(5, plot.ProductsInQueue, "Should only have 5 products (max limit)");
        Assert.AreEqual(5, plot.TotalProductsProduced, "Should reach max production count");
        Assert.IsNotNull(plot.FinalHarvestDeadline, "Must set a spoil deadline");
    }

    [Test]
    public void UpdateState_Should_BecomeSpoiled_WhenDeadlinePassesAfterFinalHarvest()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _startTime); // Max 5
        
        // Let the plot produce all 5 items
        DateTime timeReady = _startTime.AddMinutes(50); // 12:50 (5 items ready)
        plot.UpdateState(timeReady, 0f);
        
        Assert.AreEqual(5, plot.TotalProductsProduced);
        Assert.IsNotNull(plot.FinalHarvestDeadline); // Spoil deadline is set

        // Act
        // Simulate time passing just after the spoil deadline (10s in logic)
        DateTime timeSpoiled = plot.FinalHarvestDeadline.Value.AddSeconds(1);
        plot.UpdateState(timeSpoiled, 0f);

        // Assert
        Assert.AreEqual(PlotState.Spoiled, plot.State);
        Assert.AreEqual(0, plot.ProductsInQueue, "Spoiled plot should lose all products");
    }

    [Test]
    public void Harvest_Should_ReturnProductsAndEmptyQueue_WhenHarvested()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _startTime);
        plot.UpdateState(_startTime.AddMinutes(25), 0f); // 2 products are ready
        
        Assert.AreEqual(PlotState.Ready, plot.State);
        Assert.AreEqual(2, plot.ProductsInQueue);

        // Act
        (string productId, int amount) = plot.Harvest();

        // Assert
        Assert.AreEqual("tomato_fruit", productId);
        Assert.AreEqual(2, amount);
        Assert.AreEqual(0, plot.ProductsInQueue, "Queue must be 0 after harvest");
        
        // Important: State remains Ready (it waits for the next UpdateState tick to clean up)
        Assert.AreEqual(PlotState.Ready, plot.State); 
    }

    [Test]
    public void Harvest_Should_ClearPlot_WhenItIsTheFinalHarvest()
    {
        // Arrange
        var plot = new FarmPlot("Plot_1", PlotState.Empty);
        plot.StartProduction(_tomatoConfig, _startTime); // Max 5
        plot.UpdateState(_startTime.AddMinutes(100), 0f); // 5 products are ready
        
        Assert.AreEqual(5, plot.ProductsInQueue);
        Assert.AreEqual(5, plot.TotalProductsProduced);

        // Act
        plot.Harvest();

        // Assert
        Assert.AreEqual(PlotState.Empty, plot.State, "Must be reset to Empty after final harvest");
        Assert.IsNull(plot.CurrentConfig);
    }
}