// Tên file: MyFarm.Application.Tests/SellItemUseCaseTests.cs
using NUnit.Framework;
using NSubstitute;
using MyFarm.Application.Interfaces;
using MyFarm.Application.UseCases;
using MyFarm.Domain.Models;
using MyFarm.Domain.Configs;

[TestFixture]
public class SellItemUseCaseTests
{
    private IGameDataRepository _repoMock;
    private IConfigLoader _configMock;
    private IEventNotifier _notifierMock;
    private Player _player;
    private ItemConfig _tomatoConfig;
    private SellItemUseCase _useCase;

    [SetUp]
    public void Setup()
    {
        // 1. Create Mocks
        _repoMock = Substitute.For<IGameDataRepository>();
        _configMock = Substitute.For<IConfigLoader>();
        _notifierMock = Substitute.For<IEventNotifier>();

        // 2. Create Data
        _player = new Player(initialGold: 100);
        _player.AddItem("tomato_fruit", 10); // Has 100 gold, 10 tomatoes
        
        _tomatoConfig = new ItemConfig("tomato_fruit", "Tomato", sellPrice: 5);

        // 3. Teach Mocks
        _repoMock.LoadPlayer().Returns(_player);
        _configMock.GetItemConfig("tomato_fruit").Returns(_tomatoConfig);

        // 4. Initialize UseCase
        _useCase = new SellItemUseCase(_repoMock, _configMock, _notifierMock);
    }

    [Test]
    public void Execute_Should_Succeed_WhenSufficientItemsToSell()
    {
        // Arrange
        // Player has 100 gold, 10 tomatoes
        
        // Act
        // Sell 3 tomatoes (3 * 5 = 15 gold)
        bool result = _useCase.Execute("tomato_fruit", 3);

        // Assert (State)
        Assert.IsTrue(result); 
        Assert.AreEqual(115, _player.Gold, "Gold must increase to 115");
        Assert.AreEqual(7, _player.GetItemCount("tomato_fruit"), "Inventory must decrease to 7");

        // Assert (Verify) - Ensure UseCase called services
        _repoMock.Received(1).SavePlayer(_player); 
        _notifierMock.Received(1).NotifyGoldChanged(115);
        _notifierMock.Received(1).NotifyInventoryItemChanged("tomato_fruit", 7);
    }

    [Test]
    public void Execute_Should_Fail_WhenInsufficientItems()
    {
        // Arrange
        // Player has 100 gold, 10 tomatoes
        
        // Act
        // Try to sell 11 (only has 10)
        bool result = _useCase.Execute("tomato_fruit", 11);

        // Assert (State)
        Assert.IsFalse(result); 
        Assert.AreEqual(100, _player.Gold, "Gold must not change");
        Assert.AreEqual(10, _player.GetItemCount("tomato_fruit"), "Inventory must not change");

        // Assert (Verify) - Must not save or notify
        _repoMock.DidNotReceive().SavePlayer(Arg.Any<Player>());
        _notifierMock.DidNotReceive().NotifyGoldChanged(Arg.Any<long>());
    }
}