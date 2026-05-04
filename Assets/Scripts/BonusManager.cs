using System.Collections.Generic;
using UnityEngine;
using StaticData;

public class BonusManager : MonoBehaviour
{
    private BoardService _boardService;

    public void Initialize(BoardService boardService)
    {
        _boardService = boardService;
    }

    public CellData.BonusType DetermineBonusToSpawn(List<Point> connectedPoints, out bool arrowHorizontal)
    {
        arrowHorizontal = false;

        if (connectedPoints.Count == 4)
        {
            bool isHorizontalMatch = (connectedPoints[0].y == connectedPoints[1].y);
            arrowHorizontal = isHorizontalMatch;
            return CellData.BonusType.Arrow;
        }
        
        if (connectedPoints.Count >= 5)
        {
            int sameX = 0, sameY = 0;
            foreach (var p in connectedPoints)
            {
                if (p.x == connectedPoints[0].x) sameX++;
                if (p.y == connectedPoints[0].y) sameY++;
            }

            return (sameX == connectedPoints.Count || sameY == connectedPoints.Count)
                ? CellData.BonusType.ColorBomb
                : CellData.BonusType.Bomb;
        }

        return CellData.BonusType.None;
    }

    public void ApplyBonusEffects(ref List<Point> pointsToDestroy)
    {
        for (int i = 0; i < pointsToDestroy.Count; i++)
        {
            var p = pointsToDestroy[i];
            var bonus = _boardService.GetBonusAtPoint(p);

            if (bonus == CellData.BonusType.Arrow)
            {
                bool horizontal = _boardService.GetCellAtPoint(p).arrowIsHorizontal;
                if (horizontal)
                {
                    for (int bx = 0; bx < Config.BoardWidth; bx++)
                        TryAddPoint(ref pointsToDestroy, new Point(bx, p.y));
                }
                else
                {
                    for (int by = 0; by < Config.BoardHeight; by++)
                        TryAddPoint(ref pointsToDestroy, new Point(p.x, by));
                }
            }
            else if (bonus == CellData.BonusType.Bomb)
            {
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                        TryAddPoint(ref pointsToDestroy, new Point(p.x + dx, p.y + dy));
            }
        }
    }

    public List<Point> GetPointsForStandardBonus(Point bonusPoint)
    {
        var pointsToDestroy = new List<Point> { bonusPoint };
        var bonus = _boardService.GetBonusAtPoint(bonusPoint);

        if (bonus == CellData.BonusType.Arrow)
        {
            bool horizontal = _boardService.GetCellAtPoint(bonusPoint).arrowIsHorizontal;
            if (horizontal)
            {
                for (int bx = 0; bx < Config.BoardWidth; bx++)
                    TryAddPoint(ref pointsToDestroy, new Point(bx, bonusPoint.y));
            }
            else
            {
                for (int by = 0; by < Config.BoardHeight; by++)
                    TryAddPoint(ref pointsToDestroy, new Point(bonusPoint.x, by));
            }
        }
        else if (bonus == CellData.BonusType.Bomb)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    TryAddPoint(ref pointsToDestroy, new Point(bonusPoint.x + dx, bonusPoint.y + dy));
        }

        ApplyBonusEffects(ref pointsToDestroy);
        return pointsToDestroy;
    }

    public List<Point> GetPointsForColorBomb(CellData.CellType targetType, Point bombPoint)
    {
        var pointsToDestroy = new List<Point>();

        if (targetType <= 0)
        {
            for (int bx = 0; bx < Config.BoardWidth; bx++)
                for (int by = 0; by < Config.BoardHeight; by++)
                    TryAddPoint(ref pointsToDestroy, new Point(bx, by));
            return pointsToDestroy;
        }

        pointsToDestroy.Add(bombPoint);
        for (int by = 0; by < Config.BoardHeight; by++)
        {
            for (int bx = 0; bx < Config.BoardWidth; bx++)
            {
                var p = new Point(bx, by);
                if (_boardService.GetCellTypeAtPoint(p) == targetType)
                    TryAddPoint(ref pointsToDestroy, p);
            }
        }

        ApplyBonusEffects(ref pointsToDestroy);
        return pointsToDestroy;
    }

    private void TryAddPoint(ref List<Point> list, Point p)
    {
        if (p.x < 0 || p.x >= Config.BoardWidth || p.y < 0 || p.y >= Config.BoardHeight) return;
        if (_boardService.GetCellTypeAtPoint(p) == CellData.CellType.Hole) return;
        
        foreach (var existing in list)
        {
            if (existing.Equals(p)) return;
        }
        list.Add(p);
    }
}