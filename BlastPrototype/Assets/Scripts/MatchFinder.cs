using System.Collections.Generic;
using UnityEngine;

public class MatchFinder : Singleton<MatchFinder>
{
    public List<Cell> FindMatchList(Cell cell)
    {
        Vector2Int gridSize = GridController.Instance.gridSize;
        bool[,] visitedCells = new bool[gridSize.x, gridSize.y];
        List<Cell> resultCells = new List<Cell>();
        FindMatches(cell, cell, resultCells, visitedCells);

        return resultCells.Count > 1 ? resultCells : null;
    }

    void FindMatches(Cell newCell, Cell selectedCell, List<Cell> resultCells, bool[,] visitedCells)
    {
        if (newCell == null)
            return;

        Vector2Int gridPos = newCell.gridPosition;

        if (visitedCells[gridPos.x, gridPos.y])
            return;

        visitedCells[gridPos.x, gridPos.y] = true;

        if (newCell is IMatchable matchable && matchable.CheckMatch(selectedCell.GetCellId()))
        {
            resultCells.Add(newCell);

            if (newCell is not INeighbour neighbour)
                return;

            List<Cell> neighbours = neighbour.GetNeighbours();
            for (int i = 0; i < neighbours.Count; i++)
            {
                FindMatches(neighbours[i], selectedCell, resultCells, visitedCells);
            }
        }
    }
}
