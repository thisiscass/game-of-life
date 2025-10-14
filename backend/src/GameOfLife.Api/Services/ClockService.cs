namespace GameOfLife.Services;

public class ClockService : IClockService
{
    public DateTime CurrentUtc => DateTime.UtcNow;
}