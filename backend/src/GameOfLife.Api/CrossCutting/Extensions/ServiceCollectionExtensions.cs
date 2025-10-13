using GameOfLife.Api.Dtos;
using GameOfLife.Api.Validations;
using GameOfLife.CrossCutting.Cache;
using GameOfLife.Services;

namespace GameOfLife.CrossCutting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameOfLifeServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateBoardValidation<CreateBoardDto>, CreateBoardValidation>();

        services.AddScoped<IGameOfLifeService, GameOfLifeService>();
        services.AddSingleton<IClockService, ClockService>();
        services.AddSingleton<IBoardLockService, BoardLockService>();
        services.AddSingleton<IAdvanceNStepsService, AdvanceNStepsService>();
        services.AddSingleton<BoardCache>();

        services.AddHostedService<GameOfLifeBackgroundService>();
        services.AddHostedService<AdvanceNStepsBackgroundService>();

        services.AddSingleton<IAdvanceNStepsQueue, AdvanceNStepsQueue>();

        return services;
    }
}