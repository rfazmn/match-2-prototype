using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridController : Singleton<GridController>
{
    public CellPooling cellPooling;

    [SerializeField] Transform topPart;
    [SerializeField] SpriteRenderer gridBg;
    public Vector2Int gridSize;
    Cell[,] grid;

    float cellUnitSize = 1f;
    float zOffset = .05f;
    int gridCellCounter = 0;

    [SerializeField] float fallTime = .1f;
    [SerializeField] float fallDelay = .05f;
    [SerializeField] float fillTime = .1f;
    [SerializeField] float fillDelay = .05f;

    [SerializeField] float shuffleTime = 1f;

    #region GridStuff
#if UNITY_EDITOR
    [ContextMenu("Create Grid")]
    public void CreateGrid()
    {
        ClearGrid();

        if (gridSize.x <= 0 || gridSize.y <= 0)
            return;

        GoalHandler goalHandler = FindObjectOfType<GoalHandler>();

        float backgroundSpacing = .225f;
        float cellUpperBound = .2f;

        Vector3 bgPosition = new Vector3(0f, -(cellUnitSize * .5f), 1f);
        Vector2 bgSize = new Vector2(gridSize.x + backgroundSpacing, gridSize.y + backgroundSpacing + cellUpperBound);
        InitGridBackground(bgPosition, bgSize);
        InitGameCamByGridWidth();

        float xStart = -gridSize.x * .5f + cellUnitSize * .5f;
        float yStart = -gridSize.y * .5f;
        Vector3 startPosition = new Vector3(xStart, yStart, 0f);
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Cell cell = cellPooling.SpawnCell();
                int cellId = Random.Range(0, 5);
                cell.InitCell(CellType.cube, new Vector2Int(x, y), startPosition, cellId, goalHandler.cellSprites[cellId]);
                startPosition.y += cellUnitSize;
                startPosition.z -= zOffset;
            }

            startPosition.y = yStart;
            startPosition.z = 0f;
            startPosition.x += cellUnitSize;
        }
    }

    public void LoadGrid(List<CellData> _grid)
    {
        ClearGrid();

        if (gridSize.x <= 0 || gridSize.y <= 0)
            return;

        GoalHandler goalHandler = FindObjectOfType<GoalHandler>();

        float backgroundSpacing = .225f;
        float cellUpperBound = .2f;

        Vector3 bgPosition = new Vector3(0f, -(cellUnitSize * .5f), 1f);
        Vector2 bgSize = new Vector2(gridSize.x + backgroundSpacing, gridSize.y + backgroundSpacing + cellUpperBound);
        InitGridBackground(bgPosition, bgSize);
        InitGameCamByGridWidth();

        for (int i = 0; i < _grid.Count; i++)
        {
            Sprite cellSprite = goalHandler.cellSprites[_grid[i].id];
            Cell cell = cellPooling.GetAvailableCell();
            cell.InitCell(_grid[i].cellType, new Vector2Int(_grid[i].gridPos.x, _grid[i].gridPos.y), new Vector3(_grid[i].position.x, _grid[i].position.y, _grid[i].position.z), _grid[i].id, cellSprite);
        }
    }
#endif

    void ClearGrid()
    {
        cellPooling.ClearCells();
        grid = null;

        Transform cellParent = cellPooling.cellParent;
        if (cellParent == null)
            return;

        int cellCount = cellParent.childCount;
        for (int i = 0; i < cellCount; i++)
        {
            DestroyImmediate(cellParent.GetChild(0).gameObject);
        }
    }

    void InitGridBackground(Vector3 position, Vector2 size)
    {
        gridBg.transform.position = position;
        gridBg.size = size;
    }

    void InitGameCamByGridWidth()
    {
        int sizeDiff = gridSize.y - gridSize.x;
        float orthographicSize = sizeDiff >= 3 ? gridSize.y - 2.5f : gridSize.x;
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.mainCam.orthographicSize = Mathf.Max(8, orthographicSize);
    }

    public void SetGridElementOnStart(Cell cell)
    {
        if (grid == null)
            grid = new Cell[gridSize.x, gridSize.y];

        grid[cell.gridPosition.x, cell.gridPosition.y] = cell;

        gridCellCounter++;
        if (gridCellCounter == grid.Length)
        {
            UpdateCellsNeighbours(true);
        }
    }

    void SetGridElement(Cell cell)
    {
        grid[cell.gridPosition.x, cell.gridPosition.y] = cell;
    }

    public void NullGridElement(Cell cell)
    {
        if (grid[cell.gridPosition.x, cell.gridPosition.y] == cell)
            grid[cell.gridPosition.x, cell.gridPosition.y] = null;
    }

    public IEnumerator FallCells(Dictionary<int, List<FillData>> fillData)
    {
        float maxColumnTime = 0f;

        float halfOfGridX = gridSize.x * .5f;
        float halfOfGridY = gridSize.y * .5f;
        float halfOfCellSize = cellUnitSize * .5f;

        for (int x = 0; x < gridSize.x; x++)
        {
            List<FillData> columnFill = new List<FillData>();
            float columnFallDelay = 0f;

            for (int y = 0; y < gridSize.y; y++)
            {
                Cell cell = grid[x, y];
                if (cell == null)
                {
                    Vector3 cellPosition = new Vector3(x - halfOfGridX + halfOfCellSize, y - halfOfGridY, y * -zOffset);
                    columnFill.Add(new FillData(new Vector2Int(x, y), cellPosition));
                }
                else
                {
                    if (columnFill.Count == 0)
                        continue;

                    NullGridElement(cell);
                    FillData temp = new FillData(cell.gridPosition, cell.transform.position);
                    cell.gridPosition = columnFill[0].gridPos;
                    cell.transform.DOMove(columnFill[0].position, fallTime).SetDelay(columnFallDelay);
                    SetGridElement(cell);

                    columnFill.RemoveAt(0);
                    columnFill.Add(temp);

                    columnFallDelay += fallDelay;
                }
            }

            if (columnFallDelay > maxColumnTime)
                maxColumnTime = columnFallDelay;

            if (columnFill.Count > 0)
                fillData.Add(x, columnFill);
        }

        yield return new WaitForSeconds(maxColumnTime + fallTime);
    }

    public IEnumerator FillGrid(Dictionary<int, List<FillData>> fillData)
    {
        if (fillData.Count == 0)
            yield break;

        float maxColumnTime = 0;

        foreach (var columnFill in fillData)
        {
            List<FillData> columnData = columnFill.Value;

            float columnFillDelay = 0f;

            for (int i = 0; i < columnData.Count; i++)
            {
                FillData currentFill = columnData[i];
                Cell cell = cellPooling.GetAvailableCell();
                Vector3 cellPos = currentFill.position;
                cellPos.y = topPart.transform.position.y;
                cell.InitCell(CellType.cube, currentFill.gridPos, cellPos);

                SetGridElement(cell);
                cell.transform.DOMove(currentFill.position, fillTime).SetDelay(columnFillDelay);

                columnFillDelay += fillDelay;
            }

            if (columnFillDelay > maxColumnTime)
                maxColumnTime = columnFillDelay;
        }

        yield return new WaitForSeconds(maxColumnTime + fillTime);
    }

    #endregion

    #region NeighbourPart
    void FindCellNeighbours(Cell cell, INeighbour neighbour)
    {
        List<Cell> neighbours = new List<Cell>();
        Vector2Int gridPos = cell.gridPosition;

        if (gridPos.x - 1 >= 0)
            neighbours.Add(grid[gridPos.x - 1, gridPos.y]);
        if (gridPos.x + 1 < gridSize.x)
            neighbours.Add(grid[gridPos.x + 1, gridPos.y]);
        if (gridPos.y - 1 >= 0)
            neighbours.Add(grid[gridPos.x, gridPos.y - 1]);
        if (gridPos.y + 1 < gridSize.y)
            neighbours.Add(grid[gridPos.x, gridPos.y + 1]);

        neighbour.SetNeighbours(neighbours);
    }

    public void UpdateCellsNeighbours(bool init = false)
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (grid[x, y] != null && grid[x, y] is INeighbour neighbour)
                {
                    FindCellNeighbours(grid[x, y], neighbour);
                }
            }
        }

        if (init)
            return;

        
    }
    #endregion

    #region ShuffleCheck
    public IEnumerator CheckShuffle()
    {
        bool shuffle = true;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Cell cell = grid[x, y];
                List<Cell> matchList = TouchHandler.Instance.FindMatchList(cell);

                if (matchList == null)
                    continue;

                if (matchList.Count > 0)
                {
                    shuffle = false;
                    goto outOfLoop;
                }
            }
        }

    outOfLoop:
        if (shuffle)
            yield return StartCoroutine(ShuffleCells());
    }

    IEnumerator ShuffleCells()
    {
        yield return new WaitForSeconds(.5f);

        List<Cell> matchables = new List<Cell>();
        List<FillData> shuffleList = new List<FillData>();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Cell cell = grid[x, y];
                matchables.Add(cell);
                shuffleList.Add(new FillData(cell.gridPosition, cell.transform.position));
            }
        }

        shuffleList.Shuffle();

        for (int i = 0; i < matchables.Count; i++)
        {
            Cell matchable = matchables[i];
            matchable.gridPosition = shuffleList[i].gridPos;
            SetGridElement(matchable);
            matchable.transform.DOMove(shuffleList[i].position, shuffleTime).SetEase(Ease.InOutBack);
        }

        yield return new WaitForSeconds(shuffleTime);
        UpdateCellsNeighbours();
        yield return StartCoroutine(CheckShuffle());
    }
    #endregion

    #region ChangeCells
    public CellTypeEditor targetType;

#if UNITY_EDITOR
    [ContextMenu("Change Cell")]
    void ChangeCellByType()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        GoalHandler goalHandler = FindObjectOfType<GoalHandler>();

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (!selectedObjects[i].TryGetComponent(out Cell selectedCell))
                continue;

            int targetTypeIndex = (int)targetType;
            Sprite cellSprite = goalHandler.cellSprites[targetTypeIndex];
            Cell cell = cellPooling.GetAvailableCell();
            cell.InitCell(CellType.cube, selectedCell.gridPosition, selectedCell.transform.position, targetTypeIndex, cellSprite);
            cellPooling.RemoveCell(selectedCell);
            DestroyImmediate(selectedCell.gameObject);
        }
    }
#endif
    #endregion
}
