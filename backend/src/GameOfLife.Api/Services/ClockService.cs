namespace GameOfLife.Api.Services;

public class ClockService : IClockService
{
    public DateTime CurrentUtc => DateTime.UtcNow;
}