using Squares.Persistence.Entities;

namespace Squares.Persistence.Repositories;

public interface IPointsRepository
{
    Task AddAsync(Point point);
    Task DeleteAsync(int id);
    Task<List<Point>> GetAllAsync();
    Task AddRangeAsync(IEnumerable<Point> points);
    Task<Point?> FindByCoordinatesAsync(int x, int y);
}
