namespace GameOfLife.Api.Dtos;

public record NextBoardResultDto(Guid boardId, int generation, int[][] grid);