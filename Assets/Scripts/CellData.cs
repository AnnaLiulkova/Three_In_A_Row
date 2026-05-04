using UnityEngine;

public class CellData
{
    public enum CellType
    {
        Hole = -1,
        Blank = 0,
        Apple = 1,
        Banana = 2,
        Grape = 3,
        Blueberry = 4,
        Pear = 5,
        BonusArrowH = 6,
        BonusArrowV = 7,
        BonusBomb = 8,
        BonusColorBomb = 9
    }

    public enum BonusType
    {
        None = 0,
        Arrow = 1,
        Bomb = 2,
        ColorBomb = 3
    }

    public CellType cellType;
    public Point point;
    
    public BonusType bonusType = BonusType.None;
    public bool arrowIsHorizontal = false; 

    private Cell _cell; 

    public CellData(CellType cellType, Point point)
    {
        this.cellType = cellType;
        this.point = point;
    }

    public Cell GetCell()
    {
        return _cell;
    }

    public void SetCell(Cell newCell)
    {
        _cell = newCell;
        if (_cell == null)
        {
            cellType = CellType.Blank;
            bonusType = BonusType.None;
        }
        else
        {
            cellType = newCell.CellType;
            bonusType = newCell.BonusType; 
            arrowIsHorizontal = newCell.arrowIsHorizontal;
            _cell.SetCellPoint(point);
        }
    }
}