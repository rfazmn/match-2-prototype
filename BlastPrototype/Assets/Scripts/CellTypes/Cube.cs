using System.Collections.Generic;
using UnityEngine;

public class Cube : Cell, IMatchable, INeighbour
{
    [SerializeField] SpriteRenderer cellSprite;
    [SerializeField] int cubeId;

    [SerializeField] float baseGoalSpeed = .4f;
    [SerializeField] float unitGoalFactor = .05f;

    public List<Cell> neighbours = new List<Cell>();

    public override void InitCell(CellType type, Vector2Int gridPos, Vector3 position, int id = -1, Sprite sprite = null)
    {
        base.InitCell(type, gridPos, position);

        cellSprite.sortingOrder = 0;
        cubeId = id == -1 ? Random.Range(0, 5) : id;
        cellSprite.sprite = (sprite != null) ? sprite : GoalHandler.Instance.cellSprites[cubeId];
    }

    public override int GetCellId()
    {
        return cubeId;
    }

    public override void Blast()
    {
        if (blasted)
            return;

        blasted = true;

        GridController.Instance.NullGridElement(this);
        if (!CheckGoal())
        {
            ParticlePooling.Instance.ActivateParticle(cubeId, transform.position);
            gameObject.SetActive(false);
        }
        else
        {
            GoalHandler.Instance.MoveCellToGoal(gameObject, gridPosition, cellSprite, cubeId, baseGoalSpeed, unitGoalFactor);
        }
    }

    public override bool CheckGoal()
    {
        return GoalHandler.Instance.IsInGoals(cubeId);
    }

    #region IMatchable
    public bool CheckMatch(int cellId)
    {
        return cubeId == cellId;
    }

    #endregion

    #region INeighbour
    public List<Cell> GetNeighbours()
    {
        return neighbours;
    }

    public void SetNeighbours(List<Cell> _neighbours)
    {
        neighbours = _neighbours;
    }
    #endregion
}
