using Squares.Business.Models;
using Squares.Persistence.Entities;

namespace Squares.Business.Services;

public interface IPointsService
{
    Task AddPointAsync(Point point);
    Task DeletePointAsync(int x, int y);
    Task ImportPointsAsync(string json);
    Task<List<Square>> IdentifySquaresAsync();
    Task<List<Point>> GetAllPointsAsync();
}