using MyFarm.Domain.Models;

namespace MyFarm.Application.Interfaces
{
    // Interface này chịu trách nhiệm LƯU/TẢI game (bằng Json, PlayerPrefs...)
    public interface IGameDataRepository
    {
        void SavePlayer(Player player);
        Player LoadPlayer();
        void SaveFarm(Farm farm);
        Farm LoadFarm();
    }
}