using System.Collections.Generic;
using UnityEngine;

public class CellPooling : MonoBehaviour
{
    public Cell cellPrefab;
    public List<Cell> cellList;
    public Transform cellParent;

    public Cell GetAvailableCell()
    {
        for (int i = 0; i < cellList.Count; i++)
        {
            if (!cellList[i].gameObject.activeSelf)
            {
                return cellList[i];
            }
        }

        return SpawnCell();
    }

    public Cell SpawnCell()
    {
        if (cellParent == null)
            cellParent = new GameObject("Cells").transform;

        Cell newCell = Instantiate(cellPrefab, cellParent);
        cellList.Add(newCell);
        return newCell;
    }

    public void RemoveCell(Cell cell)
    {
        cellList.Remove(cell);
    }

    public void ClearCells()
    {
        cellList.Clear();
    }
}
