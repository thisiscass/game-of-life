namespace GameOfLife.Api.Services;

public interface IClockService
{
    public DateTime CurrentUtc { get; }
}