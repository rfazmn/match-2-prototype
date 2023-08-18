using DG.Tweening;
using UnityEngine;

public abstract class Cell : MonoBehaviour
{
    public CellType cellType;
    public Vector2Int gridPosition;
    public bool blasted;

    void Start()
    {
        GridController.Instance.SetGridElementOnStart(this);
    }

    public virtual void InitCell(CellType type, Vector2Int gridPos, Vector3 position, int id = -1, Sprite sprite = null)
    {
        cellType = type;
        gridPosition = gridPos;
        blasted = false;

        transform.localScale = Vector3.one;
        transform.position = position;
        gameObject.SetActive(true);
    }

    public virtual int GetCellId()
    {
        return -1;
    }

    public abstract void Blast();

    public void Tickle()
    {
        transform.DOKill(true);
        transform.DOPunchRotation(new Vector3(0f, 0f, 15f), .15f, 20).SetEase(Ease.Linear);
    }

    public virtual bool CheckGoal()
    {
        //implement default check goal stuff
        return false;
    }
}
