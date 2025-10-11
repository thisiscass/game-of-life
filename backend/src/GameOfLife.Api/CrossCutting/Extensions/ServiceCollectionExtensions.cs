using GameOfLife.Api.Dtos;
using GameOfLife.Api.Services;
using GameOfLife.Api.Validations;

namespace GameOfLife.CrossCutting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameOfLifeServices(this IServiceCollection services)
    {
        services.AddScoped<IGameOfLifeService, GameOfLifeService>();
        services.AddScoped<ICreateBoardValidation<CreateBoardDto>, CreateBoardValidation>();
        services.AddSingleton<IClockService, ClockService>();

        return services;
    }
}