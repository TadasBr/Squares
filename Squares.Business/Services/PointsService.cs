using System.Text.Json;
using Squares.Business.Exceptions;
using Squares.Business.Models;
using Squares.Persistence.Entities;
using Squares.Persistence.Repositories;

namespace Squares.Business.Services;

public class PointsService(IPointsRepository pointsRepository) : IPointsService
{
    private readonly IPointsRepository _pointsRepository = pointsRepository;

    public async Task AddPointAsync(Point point)
    {
        try
        {
            await _pointsRepository.AddAsync(point);
        }
        catch (InvalidOperationException ex)
        {
            throw new DuplicatePointException(ex.Message);
        }
    }

    public async Task DeletePointAsync(int x, int y)
    {
        var pointToDelete = await _pointsRepository.FindByCoordinatesAsync(x, y) ?? throw new InvalidOperationException($"Point ({x}, {y}) not found.");
        await _pointsRepository.DeleteAsync(pointToDelete.Id);
    }


    public async Task ImportPointsAsync(string json)
    {
        var points = JsonSerializer.Deserialize<List<Point>>(json);
        if (points == null || points.Count == 0)
        {
            throw new ArgumentException("No valid points in JSON.");
        }
            

        try
        {
            await _pointsRepository.AddRangeAsync(points);
        }
        catch (InvalidOperationException ex)
        {
            throw new DuplicatePointException(ex.Message);
        }
    }

    public async Task<List<Square>> IdentifySquaresAsync()
    {
        var points = await _pointsRepository.GetAllAsync();
        var pointSet = points.Select(p => (p.X, p.Y));
        var squares = new List<Square>();
        var seenSquares = new HashSet<string>();

        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                var p1 = points[i];
                var p2 = points[j];

                int dx = p2.X - p1.X;
                int dy = p2.Y - p1.Y;

                var candidates = new[]
                {
                    (p3: (X: p1.X - dy, Y: p1.Y + dx), p4: (X: p2.X - dy, Y: p2.Y + dx)),
                    (p3: (X: p1.X + dy, Y: p1.Y - dx), p4: (X: p2.X + dy, Y: p2.Y - dx))
                };

                foreach (var c in candidates)
                {
                    if (pointSet.Contains(c.p3) && pointSet.Contains(c.p4))
                    {
                        var squarePoints = new[]
                        {
                        p1,
                        p2,
                        new Point(c.p3.X, c.p3.Y),
                        new Point(c.p4.X, c.p4.Y)
                        };

                        var orderedPoints = squarePoints.OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();
                        var key = string.Join(";", orderedPoints.Select(p => $"{p.X},{p.Y}"));

                        if (seenSquares.Contains(key))
                        {
                            continue;
                        }
                        seenSquares.Add(key);
                        squares.Add(new Square { Points = squarePoints });
                    }
                }
            }
        }

        return squares;
    }


    public async Task<List<Point>> GetAllPointsAsync()
    {
        return await _pointsRepository.GetAllAsync();
    }
}
