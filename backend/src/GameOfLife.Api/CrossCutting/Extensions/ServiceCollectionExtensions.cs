using GameOfLife.Api.Dtos;
using GameOfLife.Api.Validations;
using GameOfLife.CrossCutting.Cache;
using GameOfLife.Repositories;
using GameOfLife.Services;

namespace GameOfLife.CrossCutting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameOfLifeServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateBoardValidation<CreateBoardDto>, CreateBoardValidation>();

        services.AddHostedService<GameOfLifeBackgroundService>();
        services.AddHostedService<AdvanceNStepsBackgroundService>();

        services.AddScoped<IGameOfLifeService, GameOfLifeService>();
        services.AddScoped<IAdvanceNStepsService, AdvanceNStepsService>();

        services.AddScoped<IBoardRepository, BoardRepository>();

        services.AddSingleton<IClockService, ClockService>();
        services.AddSingleton<IBoardLockService, BoardLockService>();
        services.AddSingleton<IBoardCache, BoardCache>();

        services.AddSingleton<IAdvanceNStepsQueue, AdvanceNStepsQueue>();

        return services;
    }
}