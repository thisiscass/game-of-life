using GameOfLife.Api.Dtos;
using GameOfLife.Api.Validations;

namespace GameOfLife.Tests.Validations
{
    public class CreateBoardValidationTests
    {
        private readonly CreateBoardValidation _validator = new();

        [Fact]
        public void Should_Pass_When_Grid_Is_Valid()
        {
            // Arrange
            var dto = new CreateBoardDto(new[]
                {
                    new[] { 0, 1, 0 },
                    new[] { 1, 0, 1 }
                }
            );

            // Act
            var result = _validator.PerformValidation(dto);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.GetErrors());
        }

        [Fact]
        public void Should_Fail_When_Grid_Is_Null()
        {
            // Arrange
            var dto = new CreateBoardDto(null!);

            // Act
            var result = _validator.PerformValidation(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.GetErrors(), e => e.Contains("Invalid board"));
        }

        [Fact]
        public void Should_Fail_When_Cell_Is_Not_Zero_Or_One()
        {
            // Arrange
            var dto = new CreateBoardDto(new[]
                {
                    new[] { 0, 2, 0 }
                }
            );

            // Act
            var result = _validator.PerformValidation(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.GetErrors(), e => e.Contains("accepts only 0 or 1"));
        }

        [Fact]
        public void Should_Fail_When_Grid_Is_Empty()
        {
            // Arrange
            var dto = new CreateBoardDto(Array.Empty<int[]>());

            // Act
            var result = _validator.PerformValidation(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.GetErrors(), e => e.Contains("Invalid board"));
        }

        [Fact]
        public void Should_Fail_When_Grid_Too_Large()
        {
            // Arrange
            var largeGrid = Enumerable.Range(0, 25)
                .Select(_ => Enumerable.Range(0, 25).Select(_ => 0).ToArray())
                .ToArray();

            var dto = new CreateBoardDto(largeGrid);

            // Act
            var result = _validator.PerformValidation(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.GetErrors(), e => e.Contains("Invalid board"));
        }
    }
}
