using System.Collections.Generic;
using UnityEngine;
using StaticData;

[RequireComponent(typeof(CellFactory))]
[RequireComponent(typeof(BonusManager))]
public class BoardService : MonoBehaviour
{
    public ArrayLayout boardLayout;

    [SerializeField] private Sprite[] _cellSprites;
    [SerializeField] private ParticleSystem _matchFxPrefab;
    [SerializeField] private ScoreService _scoreService;

    [Header("Bonus Sprites")]
    [SerializeField] private Sprite _arrowHorizontalSprite;
    [SerializeField] private Sprite _arrowVerticalSprite;
    [SerializeField] private Sprite _bombSprite;
    [SerializeField] private Sprite _colorBombSprite;

    public Sprite[] CellSprites => _cellSprites;

    private CellData[,] _board;
    private CellFactory _cellFactory;
    private MatchMachine _matchMachine;
    private CellMover _cellMover;
    
    private BonusManager _bonusManager; 

    private readonly int[] _fillingCellsCountByColumn = new int[Config.BoardWidth];
    private readonly List<Cell> _updatingCells = new List<Cell>();
    private readonly List<Cell> _deadCells = new List<Cell>();
    private readonly List<CellFlip> _flippedCells = new List<CellFlip>();
    private readonly List<ParticleSystem> _matchFxs = new List<ParticleSystem>();

    private void Awake()
    {
        _cellFactory = GetComponent<CellFactory>();
        _matchMachine = new MatchMachine(this);
        _cellMover = new CellMover(this);
        
        _bonusManager = GetComponent<BonusManager>();
        _bonusManager.Initialize(this);
    }

    private void Start()
    {
        // InitializeBoard();
        // VerifyBoardOnMatches();
        // _cellFactory.InstantiateBoard(this, _cellMover);
    }

    private void Update()
    {
        _cellMover.Update();

        var finishedUpdating = new List<Cell>();
        foreach (var cell in _updatingCells)
        {
            if (!cell.UpdateCell())
                finishedUpdating.Add(cell);
        }

        foreach (var cell in finishedUpdating)
        {
            var x = cell.Point.x;
            _fillingCellsCountByColumn[x] = Mathf.Clamp(_fillingCellsCountByColumn[x] - 1, 0, Config.BoardWidth);

            var flip = GetFlip(cell);
            Cell flippedCell = flip?.GetOtherCell(cell);

            var cellBonus   = GetBonusAtPoint(cell.Point);
            var flippedBonus = flippedCell != null ? GetBonusAtPoint(flippedCell.Point) : CellData.BonusType.None;

            if ((cellBonus != CellData.BonusType.None || flippedBonus != CellData.BonusType.None) && flippedCell != null)
            {
                List<Point> pointsToDestroy = new List<Point>();

                if (cellBonus == CellData.BonusType.ColorBomb)
                    pointsToDestroy = _bonusManager.GetPointsForColorBomb(GetCellTypeAtPoint(flippedCell.Point), cell.Point);
                else if (flippedBonus == CellData.BonusType.ColorBomb)
                    pointsToDestroy = _bonusManager.GetPointsForColorBomb(GetCellTypeAtPoint(cell.Point), flippedCell.Point);
                else if (cellBonus != CellData.BonusType.None)
                    pointsToDestroy = _bonusManager.GetPointsForStandardBonus(cell.Point);
                else
                    pointsToDestroy = _bonusManager.GetPointsForStandardBonus(flippedCell.Point);

                DestroyCells(pointsToDestroy);
                ApplyGravityToBoard();

                _flippedCells.Remove(flip);
                _updatingCells.Remove(cell);
                continue;
            }

            var connectedPoints = _matchMachine.GetMatchedPoints(cell.Point, main: true);
            if (flippedCell != null)
                MatchMachine.AddPoints(ref connectedPoints, _matchMachine.GetMatchedPoints(flippedCell.Point, true));

            if (connectedPoints.Count == 0)
            {
                if (flippedCell != null)
                    FlipCells(cell.Point, flippedCell.Point, false);
            }
            else
            {
                bool arrowHorizontal;
                CellData.BonusType bonusToSpawn = _bonusManager.DetermineBonusToSpawn(connectedPoints, out arrowHorizontal);
                Point bonusPoint = cell.Point;

                _bonusManager.ApplyBonusEffects(ref connectedPoints);

                DestroyCells(connectedPoints);

                if (bonusToSpawn != CellData.BonusType.None)
                    SpawnBonus(bonusPoint, bonusToSpawn, arrowHorizontal);

                ApplyGravityToBoard();
            }

            _flippedCells.Remove(flip);
            _updatingCells.Remove(cell);
        }
    }
    public void StartNewGame(ArrayLayout newLayout)
    {
        boardLayout = newLayout;

        Cell[] allCells = FindObjectsOfType<Cell>(true); 
        foreach (var cell in allCells)
        {
            if (cell != null) 
            {
                Destroy(cell.gameObject);
            }
        }

        _updatingCells.Clear();
        _deadCells.Clear();
        _flippedCells.Clear();
        
        for (int i = 0; i < Config.BoardWidth; i++)
        {
            _fillingCellsCountByColumn[i] = 0;
        }
        
        InitializeBoard();
        VerifyBoardOnMatches();
        _cellFactory.InstantiateBoard(this, _cellMover);
    }

    // SPAWN ТА ЗНИЩЕННЯ

    private void DestroyCells(List<Point> points)
    {
        foreach (var point in points)
        {
            var cellData = GetCellAtPoint(point);
            var cell = cellData?.GetCell();

            if (cell != null)
            {
                ParticleSystem matchFx;
                if (_matchFxs.Count > 0 && _matchFxs[0].isStopped)
                {
                    matchFx = _matchFxs[0];
                    _matchFxs.RemoveAt(0);
                }
                else
                {
                    matchFx = Instantiate(_matchFxPrefab, transform);
                }
                _matchFxs.Add(matchFx);
                matchFx.transform.position = cell.rect.transform.position;
                matchFx.Play();

                _cellFactory.KillCell(point);
                cell.gameObject.SetActive(false);
                _deadCells.Add(cell);
            }

            cellData?.SetCell(null);
        }

        _scoreService.AddScore(points.Count);
    }

    private void SpawnBonus(Point point, CellData.BonusType bonusType, bool arrowHorizontal)
    {
        var slotData = GetCellAtPoint(point);
        if (slotData == null) return;

        Cell bonusCell;
        if (_deadCells.Count > 0)
        {
            bonusCell = _deadCells[0];
            _deadCells.RemoveAt(0);
        }
        else
        {
            bonusCell = _cellFactory.InstantiateCell();
        }

        bonusCell.gameObject.SetActive(true);

        CellData.CellType bonusCellType = CellData.CellType.Blank;
        if (bonusType == CellData.BonusType.Arrow)
            bonusCellType = arrowHorizontal ? CellData.CellType.BonusArrowH : CellData.CellType.BonusArrowV;
        else if (bonusType == CellData.BonusType.Bomb)
            bonusCellType = CellData.CellType.BonusBomb;
        else if (bonusType == CellData.BonusType.ColorBomb)
            bonusCellType = CellData.CellType.BonusColorBomb;

        var bonusSprite = GetBonusSprite(bonusType, arrowHorizontal);

        var bonusCellData = new CellData(bonusCellType, point)
        {
            bonusType = bonusType,
            arrowIsHorizontal = arrowHorizontal
        };

        bonusCell.Initialize(bonusCellData, bonusSprite, _cellMover);
        bonusCell.SetBonus(bonusType, bonusSprite, arrowHorizontal);
        bonusCell.rect.anchoredPosition = GetBoardPositionFromPoint(point);

        slotData.cellType = bonusCellType;
        slotData.bonusType = bonusType;
        slotData.arrowIsHorizontal = arrowHorizontal;
        slotData.SetCell(bonusCell);

        _updatingCells.Remove(bonusCell);
    }

    private Sprite GetBonusSprite(CellData.BonusType bonusType, bool arrowHorizontal = false) => bonusType switch
    {
        CellData.BonusType.Arrow      => arrowHorizontal ? _arrowHorizontalSprite : _arrowVerticalSprite,
        CellData.BonusType.Bomb       => _bombSprite,
        CellData.BonusType.ColorBomb  => _colorBombSprite,
        _ => null
    };

    // FLIP

    public void FlipCells(Point firstPoint, Point secondPoint, bool main)
    {
        if (GetCellTypeAtPoint(firstPoint) < 0) return;

        var firstCellData = GetCellAtPoint(firstPoint);
        var firstCell     = firstCellData.GetCell();

        if (GetCellTypeAtPoint(secondPoint) > 0)
        {
            var secondCellData = GetCellAtPoint(secondPoint);
            var secondCell     = secondCellData.GetCell();

            firstCellData.SetCell(secondCell);
            secondCellData.SetCell(firstCell);

            if (main)
                _flippedCells.Add(new CellFlip(firstCell, secondCell));

            _updatingCells.Add(firstCell);
            _updatingCells.Add(secondCell);
        }
        else
        {
            ResetCell(firstCell);
        }
    }

    // GRAVITY

    private void ApplyGravityToBoard()
    {
        for (int x = 0; x < Config.BoardWidth; x++)
        {
            for (int y = Config.BoardHeight - 1; y >= 0; y--)
            {
                var point           = new Point(x, y);
                var cellData        = GetCellAtPoint(point);
                var cellTypeAtPoint = GetCellTypeAtPoint(point);

                if (cellTypeAtPoint != 0) continue;

                for (int newY = y - 1; newY >= -1; newY--)
                {
                    var nextPoint    = new Point(x, newY);
                    var nextCellType = GetCellTypeAtPoint(nextPoint);

                    if (nextCellType == 0) continue;

                    if (nextCellType != CellData.CellType.Hole)
                    {
                        var cellAtPoint = GetCellAtPoint(nextPoint);
                        var cell        = cellAtPoint.GetCell();
                        cellData.SetCell(cell);
                        _updatingCells.Add(cell);
                        cellAtPoint.SetCell(null);
                    }
                    else
                    {
                        var cellType  = GetRandomCellType();
                        var fallPoint = new Point(x, -1 - _fillingCellsCountByColumn[x]);

                        Cell cell;
                        if (_deadCells.Count > 0)
                        {
                            cell = _deadCells[0];
                            cell.gameObject.SetActive(true);
                            _deadCells.RemoveAt(0);
                        }
                        else
                        {
                            cell = _cellFactory.InstantiateCell();
                        }

                        cell.Initialize(new CellData(cellType, point), _cellSprites[(int)(cellType - 1)], _cellMover);
                        cell.rect.anchoredPosition = GetBoardPositionFromPoint(fallPoint);

                        var holeCell = GetCellAtPoint(point);
                        holeCell.SetCell(cell);
                        ResetCell(cell);
                        _fillingCellsCountByColumn[x]++;
                    }
                    break;
                }
            }
        }
    }

    // HELPERS

    private CellFlip GetFlip(Cell cell)
    {
        foreach (var flip in _flippedCells)
            if (flip.GetOtherCell(cell) != null) return flip;
        return null;
    }

    public void ResetCell(Cell cell)
    {
        cell.ResetPosition();
        _updatingCells.Add(cell);
    }

    public CellData.BonusType GetBonusAtPoint(Point point)
    {
        if (point.x < 0 || point.x >= Config.BoardWidth
            || point.y < 0 || point.y >= Config.BoardHeight)
            return CellData.BonusType.None;
        return _board[point.x, point.y].bonusType;
    }

    public CellData.BonusType GetBonusTypeAtPoint(Point point) => GetBonusAtPoint(point);

    private void VerifyBoardOnMatches()
    {
        for (int y = 0; y < Config.BoardHeight; y++)
        {
            for (int x = 0; x < Config.BoardWidth; x++)
            {
                var point           = new Point(x, y);
                var cellTypeAtPoint = GetCellTypeAtPoint(point);
                if (cellTypeAtPoint <= 0) continue;

                var removeCellTypes = new List<CellData.CellType>();
                while (_matchMachine.GetMatchedPoints(point, main: true)?.Count > 0)
                {
                    if (!removeCellTypes.Contains(cellTypeAtPoint))
                        removeCellTypes.Add(cellTypeAtPoint);
                    SetCellTypeAtPoint(point, GetNewCellType(ref removeCellTypes));
                    cellTypeAtPoint = GetCellTypeAtPoint(point);
                }
            }
        }
    }

    private void SetCellTypeAtPoint(Point point, CellData.CellType newCellType)
        => _board[point.x, point.y].cellType = newCellType;

    private CellData.CellType GetNewCellType(ref List<CellData.CellType> removeCellTypes)
    {
        var available = new List<CellData.CellType>();
        for (int i = 0; i < 5; i++)
            available.Add((CellData.CellType)(i + 1));
            
        foreach (var r in removeCellTypes)
            available.Remove(r);
            
        return available.Count <= 0
            ? CellData.CellType.Blank
            : available[UnityEngine.Random.Range(0, available.Count)];
    }

    public CellData.CellType GetCellTypeAtPoint(Point point)
    {
        if (point.x < 0 || point.x >= Config.BoardWidth
            || point.y < 0 || point.y >= Config.BoardHeight)
            return CellData.CellType.Hole;
        return _board[point.x, point.y].cellType;
    }

    private void InitializeBoard()
    {
        _board = new CellData[Config.BoardWidth, Config.BoardHeight];
        for (int y = 0; y < Config.BoardHeight; y++)
            for (int x = 0; x < Config.BoardWidth; x++)
                _board[x, y] = new CellData(
                    boardLayout.rows[y].row[x] ? CellData.CellType.Hole : GetRandomCellType(),
                    new Point(x, y)
                );
    }

    public CellData GetCellAtPoint(Point point) => _board[point.x, point.y];

    private CellData.CellType GetRandomCellType()
        => (CellData.CellType)(UnityEngine.Random.Range(0, 5) + 1);

    public static Vector2 GetBoardPositionFromPoint(Point point)
        => new Vector2(
            Config.PieceSize / 2 + point.x * Config.PieceSize,
            -(Config.PieceSize / 2 + point.y * Config.PieceSize)
        );
}