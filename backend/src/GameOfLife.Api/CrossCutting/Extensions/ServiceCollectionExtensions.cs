using GameOfLife.Api.Dtos;
using GameOfLife.Api.Validations;
using GameOfLife.CrossCutting.Cache;
using GameOfLife.Services;

namespace GameOfLife.CrossCutting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameOfLifeServices(this IServiceCollection services)
    {
        services.AddScoped<IGameOfLifeService, GameOfLifeService>();
        services.AddScoped<ICreateBoardValidation<CreateBoardDto>, CreateBoardValidation>();
        services.AddSingleton<IClockService, ClockService>();
        services.AddSingleton<BoardCache>();

        services.AddHostedService<GameOfLifeBackgroundService>();


        return services;
    }
}