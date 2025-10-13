namespace GameOfLife.Services;

public interface IClockService
{
    public DateTime CurrentUtc { get; }
}