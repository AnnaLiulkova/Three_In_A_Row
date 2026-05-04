using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using StaticData;

public class Cell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform rect;
    [SerializeField] private Image _image; 

    private CellData _cellData;
    private CellMover _cellMover;
    [SerializeField] private float _moveSpeed = 10f;
    private Vector2 _position;
    private bool _isUpdating; 

    public Point Point => _cellData.point;
    public CellData.CellType CellType => _cellData.cellType;

    public CellData.BonusType BonusType => _cellData.bonusType;
    public bool arrowIsHorizontal => _cellData.arrowIsHorizontal;
    
    public void Initialize(CellData cellData, Sprite sprite, CellMover cellMover)
    {
        _cellData = cellData;
        _image.sprite = sprite;
        _cellMover = cellMover;
        UpdataName();
        ResetPosition(); 
    }

    public void SetBonus(CellData.BonusType type, Sprite sprite, bool isHorizontal)
    {
        _cellData.bonusType = type;
        _cellData.arrowIsHorizontal = isHorizontal;
        if (sprite != null)
        {
            _image.sprite = sprite;
        }
    }

    public bool UpdateCell()
    {
        if (Vector3.Distance((Vector3)rect.anchoredPosition, (Vector3)_position) > 1)
        {
            MoveToPosition(_position);
            _isUpdating = true;
        }
        else
        {
            rect.anchoredPosition = _position;
            _isUpdating = false;
        }

        return _isUpdating;
    }
    
    private void UpdataName() => transform.name = $"Cell [{Point.x}, {Point.y}]";

    public void OnPointerDown(PointerEventData eventData)
    {
        _cellMover.MoveCell(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _cellMover.DropCell();
    }
    
    public void MoveToPosition(Vector2 position)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, position, Time.deltaTime * _moveSpeed);
    }
    
    public void ResetPosition()
    {
        _position = BoardService.GetBoardPositionFromPoint(Point);
    }

    public void SetCellPoint(Point point)
    {
        _cellData.point = point;
        UpdataName();
        ResetPosition();
    }
}