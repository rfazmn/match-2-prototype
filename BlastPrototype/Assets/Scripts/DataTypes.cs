using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#region CellStuff
public enum CellType
{
    cube
}

public enum CellTypeEditor
{
    cubeYellow,
    cubeRed,
    cubeBlue,
    cubeGreen,
    cubePurple,
}

public interface INeighbour
{
    List<Cell> GetNeighbours();

    void SetNeighbours(List<Cell> neighbours);
}

public interface IMatchable
{
    bool CheckMatch(int cellId);
}

public struct FillData
{
    public Vector2Int gridPos;
    public Vector3 position;

    public FillData(Vector2Int _gridPos, Vector3 _position)
    {
        gridPos = _gridPos;
        position = _position;
    }
}

#endregion

#region Goal
[Serializable]
public class GoalData
{
    public CellTypeEditor cellType;
    public int count;
}
#endregion

#region SaveLoad
[Serializable]
public class SceneData
{
    [JsonProperty("s0")] public Vector2Ints gridSize;
    [JsonProperty("s1")] public List<CellData> grid;
    [JsonProperty("s2")] public int moveCount;
    [JsonProperty("s3")] public List<GoalData> goals;

    public SceneData(Vector2Int _gridSize, List<CellData> _grid, int _moveCount, List<GoalData> _goals)
    {
        gridSize = new Vector2Ints(_gridSize.x, _gridSize.y);
        grid = _grid;
        moveCount = _moveCount;
        goals = _goals;
    }
}

[Serializable]
public class CellData
{
    [JsonProperty("c0")] public CellType cellType;
    [JsonProperty("c1")] public int id;
    [JsonProperty("c2")] public Vector2Ints gridPos;
    [JsonProperty("c3")] public Vector3s position;

    public CellData(CellType _cellType, int _id, Vector2Int _gridPos, Vector3 _pos)
    {
        cellType = _cellType;
        id = _id;
        gridPos =  new Vector2Ints(_gridPos.x, _gridPos.y);
        position = new Vector3s(_pos.x, _pos.y, _pos.z);
    }
}

public interface ISaveLoad
{
    SceneData LoadScene(string fileName);
    void SaveScene(string fileName, SceneData sceneData);
}

public enum SaveLoadType
{
    json
}

[Serializable]
public struct Vector2Ints
{
    public int x;
    public int y;
    public Vector2Ints(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}

[Serializable]
public struct Vector3s
{
    public float x;
    public float y;
    public float z;
    public Vector3s(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}
#endregion
