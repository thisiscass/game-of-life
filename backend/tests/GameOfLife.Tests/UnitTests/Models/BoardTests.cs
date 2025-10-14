using GameOfLife.Models;

namespace GameOfLife.Tests.Models;

public class BoardTests
{
    [Fact]
    public void GivenNewBoard_WhenStartIsCalled_ThenIsRunningShouldBeTrue()
    {
        // Arrange
        var board = new Board(Guid.NewGuid(), "0,0;0,0", DateTime.UtcNow);

        // Act
        board.Start();

        // Assert
        Assert.True(board.IsRunning);
    }

    [Fact]
    public void GivenRunningBoard_WhenStopIsCalled_ThenIsRunningShouldBeFalse()
    {
        // Arrange
        var board = new Board(Guid.NewGuid(), "0,0;0,0", DateTime.UtcNow);
        board.Start();

        // Act
        board.Stop();

        // Assert
        Assert.False(board.IsRunning);
    }

    [Fact]
    public void GivenGrid_WhenSerialized_ThenShouldReturnCorrectString()
    {
        // Arrange
        int[][] grid = new[]
        {
                new[] {1, 0},
                new[] {0, 1}
            };

        // Act
        var result = Board.SerializeGrid(grid);

        // Assert
        Assert.Equal("1,0;0,1", result);
    }

    [Fact]
    public void GivenSerializedGrid_WhenDeserialized_ThenShouldReturnCorrectArray()
    {
        // Arrange
        string serialized = "1,0;0,1";

        // Act
        var result = Board.DeserializeGrid(serialized);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal(new[] { 1, 0 }, result[0]);
        Assert.Equal(new[] { 0, 1 }, result[1]);
    }

    [Fact]
    public void GivenEmptyGrid_WhenBuildNextGeneration_ThenShouldThrowArgumentException()
    {
        // Arrange
        int[][] emptyGrid = new int[0][];

        // Act - Assert
        Assert.Throws<ArgumentException>(() => Board.BuildNextGeneration(emptyGrid));
    }

    [Fact]
    public void GivenSimpleGrid_WhenBuildNextGeneration_ThenShouldFollowConwayRules()
    {
        // Arrange
        int[][] grid = new[]
        {
                new[] {0,1,0},
                new[] {0,1,0},
                new[] {0,1,0}
            };

        // Act
        var next = Board.BuildNextGeneration(grid);

        // Assert
        int[][] expected = new[]
        {
                new[] {0,0,0},
                new[] {1,1,1},
                new[] {0,0,0}
            };

        Assert.Equal(expected, next);
    }

    [Fact]
    public void GivenBoard_WhenNextGenerationIsCalled_ThenShouldUpdateGridAndIncrementGeneration()
    {
        // Arrange
        var initialGrid = "0,1,0;0,1,0;0,1,0";
        var board = new Board(Guid.NewGuid(), initialGrid, DateTime.UtcNow);
        var updateTime = DateTime.UtcNow.AddMinutes(1);

        // Act
        board.NextGeneration(updateTime);

        // Assert
        Assert.Equal(1, board.Generation);
        Assert.Equal(updateTime, board.LatestUpdateAt);

        // Expected grid after next generation
        var expectedGrid = "0,0,0;1,1,1;0,0,0";
        Assert.Equal(expectedGrid, board.Grid);
    }

    [Fact]
    public void GivenBoard_WhenMultipleGenerationsAdvance_ThenShouldAccumulateGenerationCount()
    {
        // Arrange
        var initialGrid = "0,1,0;0,1,0;0,1,0";
        var board = new Board(Guid.NewGuid(), initialGrid, DateTime.UtcNow);

        // Act
        board.NextGeneration(DateTime.UtcNow.AddSeconds(1));
        board.NextGeneration(DateTime.UtcNow.AddSeconds(2));

        // Assert
        Assert.Equal(2, board.Generation);
    }
}

