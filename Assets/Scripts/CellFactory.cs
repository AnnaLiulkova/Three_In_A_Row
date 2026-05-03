using UnityEngine;
using StaticData;
using System.Collections.Generic;
using System.Drawing;

public class CellFactory : MonoBehaviour
{
    public BoardService boardService;
    private readonly List<KilledCell> _killedCells = new List<KilledCell>();
    
    [Header("BoardRects")]
    [SerializeField] private RectTransform _boardRect;
    [SerializeField] private RectTransform _killedBoardRect;
    
    [Header("Prefabs")]
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private KilledCell _killedCellPrefab;

    public List<KilledCell> KilledCells => _killedCells;

    public void InstantiateBoard(BoardService boardService, CellMover cellMover)
    {
        this.boardService = boardService;
        for (int y = 0; y < Config.BoardHeight; y++)
        {
            for (int x = 0; x < Config.BoardWidth; x++)
            {
                
                var point = new Point(x, y);
                var cellData = boardService.GetCellAtPoint(point);
                var cellType = cellData.cellType;

                if (cellType <= 0) 
                {
                    continue;
                }

                var cell = InstantiateCell();
                cell.rect.anchoredPosition = BoardService.GetBoardPositionFromPoint(point);
                cell.Initialize(
                    new CellData(cellType, new Point(x, y)), 
                    boardService.CellSprites[(int)cellType - 1], 
                    cellMover
                    ); 
                cellData.SetCell(cell);
            }
        }
    }

    public Cell InstantiateCell()
        => Instantiate(_cellPrefab, _boardRect); 
    public void KillCell(Point point)
    {
        var availableCells = new List<KilledCell>();
        foreach (var killedCell in KilledCells)
        {
            if (!killedCell.isFalling)
                availableCells.Add(killedCell);
        }

        KilledCell showedKilledCell;
        if (availableCells.Count > 0)
        {
            showedKilledCell = availableCells[0];
        }
        else
        {
            var newKilledCell = Instantiate(_killedCellPrefab, _killedBoardRect);
            showedKilledCell = newKilledCell;
            KilledCells.Add(newKilledCell);
        }

        var cellTypeIndex = (int)boardService.GetCellTypeAtPoint(point) - 1;
        
        if (showedKilledCell != null && cellTypeIndex >= 0 && cellTypeIndex < boardService.CellSprites.Length)
        {
            showedKilledCell.gameObject.SetActive(true); 
            
            showedKilledCell.Initialize(
                boardService.CellSprites[cellTypeIndex],
                BoardService.GetBoardPositionFromPoint(point)
            );
        }
    }
}

