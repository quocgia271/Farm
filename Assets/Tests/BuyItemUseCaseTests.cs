// Tên file: MyFarm.Application.Tests/BuyItemUseCaseTests.cs
using NUnit.Framework;
using NSubstitute;
using MyFarm.Application.Interfaces;
using MyFarm.Application.UseCases;
using MyFarm.Domain.Models;
using MyFarm.Domain.Configs;
using System;

[TestFixture]
public class BuyItemUseCaseTests
{
    // Mocks
    private IGameDataRepository _repoMock;
    private IConfigLoader _configMock;
    private IEventNotifier _notifierMock;

    // Data
    private Player _player;
    private ProductionConfig _seedConfig_Retail; // Cấu hình mua lẻ
    private ProductionConfig _seedConfig_Wholesale; // Cấu hình mua sỉ

    // SUT
    private BuyItemUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        // 1. Create Mocks
        _repoMock = Substitute.For<IGameDataRepository>();
        _configMock = Substitute.For<IConfigLoader>();
        _notifierMock = Substitute.For<IEventNotifier>();

        // 2. Create Data
        _player = new Player(1000); // Có 1000 vàng
        
        // Mua lẻ (ví dụ: Cà chua, 10 vàng/cái)
        _seedConfig_Retail = new ProductionConfig("Tomato Seed", "tomato_seed", "tomato_fruit", 10, 5, 
            buyPrice: 10, minAmountToBuy: 1, wholesale: false, priceBuyWholesale: 0);
        
        // Mua sỉ (ví dụ: Dâu tây, 100 vàng/10 cái, phải mua 10 cái 1 lần)
        _seedConfig_Wholesale = new ProductionConfig("Strawberry Seed", "strawberry_seed", "strawberry", 20, 3, 
            buyPrice: 0, minAmountToBuy: 10, wholesale: true, priceBuyWholesale: 100);

        // 3. Teach Mocks
        _repoMock.LoadPlayer().Returns(_player);
        _configMock.GetProductionConfig("tomato_seed").Returns(_seedConfig_Retail);
        _configMock.GetProductionConfig("strawberry_seed").Returns(_seedConfig_Wholesale);

        // 4. Initialize UseCase
        _useCase = new BuyItemUseCase(_repoMock, _configMock, _notifierMock);
    }

    [Test]
    public void Execute_Should_Succeed_When_BuyingStandardItemWithEnoughGold()
    {
        // Arrange
        // Player có 1000 vàng, muốn mua 5 hạt cà chua (10 vàng/hạt)
        
        // Act
        bool result = _useCase.Execute("tomato_seed", 5); // Tổng cộng 50 vàng

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(950, _player.Gold, "Vàng phải bị trừ 50");
        Assert.AreEqual(5, _player.GetItemCount("tomato_seed"));
        
        _repoMock.Received(1).SavePlayer(_player);
        _notifierMock.Received(1).NotifyGoldChanged(950);
        _notifierMock.Received(1).NotifyInventoryItemChanged("tomato_seed", 5);
    }

    [Test]
    public void Execute_Should_Fail_When_BuyingStandardItemWithNotEnoughGold()
    {
        // Arrange
        _player.TrySpendGold(950); // Player chỉ còn 50 vàng
        Assert.AreEqual(50, _player.Gold);
        
        // Act
        bool result = _useCase.Execute("tomato_seed", 6); // Muốn mua 6 hạt (cần 60 vàng)

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(50, _player.Gold, "Vàng phải giữ nguyên");
        Assert.AreEqual(0, _player.GetItemCount("tomato_seed"));
        
        _repoMock.DidNotReceive().SavePlayer(Arg.Any<Player>());
    }

    [Test]
    public void Execute_Should_Succeed_When_BuyingWholesaleItemWithCorrectAmount()
    {
        // Arrange
        // Player có 1000 vàng, mua sỉ dâu tây (100 vàng / 10 hạt)
        
        // Act
        bool result = _useCase.Execute("strawberry_seed", 10); // Mua 10 hạt (tổng 100 vàng)

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(900, _player.Gold, "Vàng phải bị trừ 100");
        Assert.AreEqual(10, _player.GetItemCount("strawberry_seed"));
        
        _repoMock.Received(1).SavePlayer(_player);
        _notifierMock.Received(1).NotifyGoldChanged(900);
    }

    [Test]
    public void Execute_Should_Fail_When_BuyingWholesaleItemWithIncorrectAmount()
    {
        // Arrange
        // Mua sỉ dâu tây (phải mua bội số của 10)
        
        // Act
        bool result = _useCase.Execute("strawberry_seed", 5); // Mua 5 hạt (không hợp lệ)

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(1000, _player.Gold, "Vàng phải giữ nguyên");
        
        _repoMock.DidNotReceive().SavePlayer(Arg.Any<Player>());
    }
}