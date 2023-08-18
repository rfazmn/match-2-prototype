using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveLoadSystem : MonoBehaviour
{
    public SaveLoadType saveLoadType;
    public string fileName;

    ISaveLoad GetSaveLoadByType(SaveLoadType saveLoadType)
    {
        ISaveLoad saveLoad = null;
        switch (saveLoadType)
        {
            case SaveLoadType.json:
                saveLoad = new JSONSaveLoad();
                break;
        }

        return saveLoad;
    }

#if UNITY_EDITOR
    [ContextMenu("Load Scene")]
    void LoadScene()
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.Log("File name cannot be empty");
            return;
        }

        string fullPath = $"Levels/{saveLoadType}/{fileName}";
        SceneData sceneData = GetSaveLoadByType(saveLoadType)?.LoadScene(fullPath);
        if (sceneData == null)
        {
            Debug.Log("Data could not found");
            return;
        }

        GridController gridController = FindObjectOfType<GridController>();
        GoalHandler goalHandler = FindObjectOfType<GoalHandler>();

        if (gridController == null || goalHandler == null)
        {
            Debug.Log("Some of object references could not found");
            return;
        }

        goalHandler.moveCount = sceneData.moveCount;
        goalHandler.goals = sceneData.goals;
        goalHandler.CreateGoalUI();

        gridController.gridSize = new Vector2Int(sceneData.gridSize.x, sceneData.gridSize.y);
        gridController.LoadGrid(sceneData.grid);
    }

    [ContextMenu("Save Scene")]
    void SaveScene()
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.Log("File name cannot be empty");
            return;
        }

        GridController gridController = FindObjectOfType<GridController>();
        GoalHandler goalHandler = FindObjectOfType<GoalHandler>();

        if (gridController == null || goalHandler == null)
        {
            Debug.Log("Some of object references could not found");
            return;
        }

        Vector2Int gridSize = gridController.gridSize;
        Transform cellParent = gridController.cellPooling.cellParent;
        int cellsCount = cellParent.childCount;

        if (cellsCount != gridSize.x * gridSize.y)
        {
            Debug.Log("Grid size and cells are not matching");
            return;
        }

        List<CellData> grid = new List<CellData>();

        for (int i = 0; i < cellsCount; i++)
        {
            if (cellParent.GetChild(i).TryGetComponent(out Cell cell))
                grid.Add(new CellData(cell.cellType, cell.GetCellId(), cell.gridPosition, cell.transform.position));
        }

        SceneData sceneData = new SceneData(gridSize, grid, goalHandler.moveCount, goalHandler.goals);
        FileInfo fileInfo = new FileInfo($"{Application.dataPath}/Resources/Levels/{saveLoadType}/{fileName}.json");
        fileInfo.Directory.Create();
        GetSaveLoadByType(saveLoadType)?.SaveScene(fileInfo.FullName, sceneData);
        AssetDatabase.Refresh();
    }
#endif
}
