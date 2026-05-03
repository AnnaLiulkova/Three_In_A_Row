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
        Pear = 5
    }

    public CellType cellType;
    public Point point;
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
        }
        else
        {
            cellType = newCell.CellType;
            _cell.SetCellPoint(point);
        }
         
    }
}