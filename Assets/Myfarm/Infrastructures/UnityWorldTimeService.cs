using MyFarm.Application.Interfaces;
using System;

namespace MyFarm.Infrastructure
{
    // Triển khai IWorldTimeService
    // Cách triển khai này dùng đồng hồ thật của hệ thống.
    public class UnityWorldTimeService : IWorldTimeService
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
    }
}