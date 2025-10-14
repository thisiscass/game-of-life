using GameOfLife.CrossCutting.Cache;
using GameOfLife.CrossCutting.Hubs;
using GameOfLife.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameOfLife.Tests.Services;

    public class GameOfLifeBackgroundServiceTests
    {
        private readonly BoardCache _cache;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceScope _scope;
        private readonly IGameOfLifeService _gameOfLifeService;
        private readonly IHubContext<BoardHub> _hubContext;
        private readonly IHubClients _hubClients;
        private readonly IClientProxy _clientProxy;
        private readonly ILogger<GameOfLifeBackgroundService> _logger;
        private readonly GameOfLifeBackgroundService _service;

        public GameOfLifeBackgroundServiceTests()
        {
            _cache = Substitute.For<BoardCache>();
            _scopeFactory = Substitute.For<IServiceScopeFactory>();
            _scope = Substitute.For<IServiceScope>();
            _gameOfLifeService = Substitute.For<IGameOfLifeService>();
            _hubContext = Substitute.For<IHubContext<BoardHub>>();
            _hubClients = Substitute.For<IHubClients>();
            _clientProxy = Substitute.For<IClientProxy>();
            _logger = Substitute.For<ILogger<GameOfLifeBackgroundService>>();

            _scope.ServiceProvider.GetService(typeof(IGameOfLifeService))
                .Returns(_gameOfLifeService);
            _scopeFactory.CreateScope().Returns(_scope);

            _hubContext.Clients.Returns(_hubClients);
            _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

            _service = new GameOfLifeBackgroundService(
                _cache,
                _scopeFactory,
                _hubContext,
                _logger
            );
        }

        [Fact]
        public async Task GivenServiceStart_WhenCalled_ThenCleansRunningBoardsAndLogs()
        {
            // Act
            await _service.StartAsync(CancellationToken.None);

            // Assert
            await _gameOfLifeService.Received(1)
                .CleanRunningBoards(Arg.Any<CancellationToken>());

            _logger.Received().Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Is<object>(x => x.ToString()!.Contains("is running")),
                null,
                Arg.Any<Func<object, Exception?, string>>());
        }
    }

