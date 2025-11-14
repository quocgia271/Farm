using System;

namespace MyFarm.Application.Interfaces
{
    // Interface này chịu trách nhiệm cung cấp THỜI GIAN
    // Điều này giúp ta test game (chạy 10 phút) mà không cần chờ 10 phút thật
    public interface IWorldTimeService
    {
        DateTime GetCurrentTime();
    }
}