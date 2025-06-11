using Moq;
using NUnit.Framework.Legacy;
using Squares.Business.Exceptions;
using Squares.Business.Services;
using Squares.Persistence.Entities;
using Squares.Persistence.Repositories;

namespace Squares.Business.UnitTests;

[TestFixture]
public class PointsServiceTests
{
    private Mock<IPointsRepository> _pointsRepositoryMock = null!;
    private PointsService _pointsService = null!;

    [SetUp]
    public void Setup()
    {
        _pointsRepositoryMock = new Mock<IPointsRepository>();
        _pointsService = new PointsService(_pointsRepositoryMock.Object);
    }

    [Test]
    public async Task AddPointAsync_ShouldCallRepositoryAdd()
    {
        var point = new Point(1, 2);
        _pointsRepositoryMock.Setup(r => r.AddAsync(point)).Returns(Task.CompletedTask);

        await _pointsService.AddPointAsync(point);

        _pointsRepositoryMock.Verify(r => r.AddAsync(point), Times.Once);
    }

    [Test]
    public void AddPointAsync_WhenRepositoryThrowsInvalidOperationException_ShouldThrowDuplicatePointException()
    {
        var point = new Point(1, 2);
        _pointsRepositoryMock.Setup(r => r.AddAsync(point)).ThrowsAsync(new InvalidOperationException("Duplicate"));

        Assert.ThrowsAsync<DuplicatePointException>(async () => await _pointsService.AddPointAsync(point));
    }

    [Test]
    public async Task DeletePointAsync_WhenPointExists_ShouldCallDelete()
    {
        var point = new Point(1, 2) { Id = 5 };
        _pointsRepositoryMock.Setup(r => r.FindByCoordinatesAsync(1, 2)).ReturnsAsync(point);
        _pointsRepositoryMock.Setup(r => r.DeleteAsync(point.Id)).Returns(Task.CompletedTask);

        await _pointsService.DeletePointAsync(1, 2);

        _pointsRepositoryMock.Verify(r => r.DeleteAsync(point.Id), Times.Once);
    }

    [Test]
    public void DeletePointAsync_WhenPointNotFound_ShouldThrowInvalidOperationException()
    {
        _pointsRepositoryMock.Setup(r => r.FindByCoordinatesAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((Point?)null);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await _pointsService.DeletePointAsync(1, 2));
    }

    [Test]
    public async Task ImportPointsAsync_WithValidJson_ShouldCallAddRange()
    {
        var json = "[{\"X\":1,\"Y\":2},{\"X\":3,\"Y\":4}]";
        _pointsRepositoryMock.Setup(r => r.AddRangeAsync(It.IsAny<List<Point>>())).Returns(Task.CompletedTask);

        await _pointsService.ImportPointsAsync(json);

        _pointsRepositoryMock.Verify(r => r.AddRangeAsync(It.Is<List<Point>>(pts => pts.Count == 2)), Times.Once);
    }

    [Test]
    public void ImportPointsAsync_WithEmptyJson_ShouldThrowArgumentException()
    {
        var json = "[]";

        Assert.ThrowsAsync<ArgumentException>(async () => await _pointsService.ImportPointsAsync(json));
    }

    [Test]
    public async Task IdentifySquaresAsync_ShouldReturnSquares()
    {
        // Arrange
        var points = new List<Point>
        {
            new Point(0,0),
            new Point(0,1),
            new Point(1,0),
            new Point(1,1)
        };
        _pointsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(points);

        // Act
        var squares = await _pointsService.IdentifySquaresAsync();

        // Assert
        Assert.That(squares, Has.Count.EqualTo(1));
        var squarePoints = squares[0].Points;

        var expectedCoords = points.Select(p => (p.X, p.Y)).OrderBy(p => p.X).ThenBy(p => p.Y);
        var actualCoords = squarePoints.Select(p => (p.X, p.Y)).OrderBy(p => p.X).ThenBy(p => p.Y);

        CollectionAssert.AreEqual(expectedCoords.ToList(), actualCoords.ToList());
    }
}
