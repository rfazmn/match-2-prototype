using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoalHandler : Singleton<GoalHandler>
{
    public Sprite[] cellSprites;
    public int moveCount;
    [SerializeField] TMP_Text moveText;

    [SerializeField] RectTransform goalsParent;
    [SerializeField] Goal goalPrefab;
    public List<GoalData> goals;
    Dictionary<int, Goal> goalsDict = new Dictionary<int, Goal>();

    #region GoalPart

#if UNITY_EDITOR
    [ContextMenu("Create Goal UI")]
    public void CreateGoalUI()
    {
        ClearGoals();

        int goalsLen = goals.Count;
        float refUnitWidth = 120f; // 100f prefab width + 20f offset 
        float goalScale = 1f;
        int childCapacity = (int)(goalsParent.rect.width / refUnitWidth);

        if (goalsLen > childCapacity)
        {
            goalScale = Mathf.Max(0f, 1f - (goalsLen - childCapacity) * .15f);
            refUnitWidth *= goalScale;
        }

        float startPosX = goalsLen > 1 ? -refUnitWidth * Mathf.FloorToInt(goalsLen / 2f) : 0f;
        startPosX += goalsLen % 2 == 0 ? refUnitWidth * .5f : 0f;

        SpawnGoals(startPosX, refUnitWidth, goalScale);
        SetMoveText(moveCount);
    }
#endif

    void SpawnGoals(float startPosX, float increaseBy, float goalScale)
    {
        List<int> idCheckList = new List<int>();
        for (int i = 0; i < goals.Count; i++)
        {
            GoalData goalData = goals[i];
            int goalId = (int)goalData.cellType;
            if (idCheckList.Contains(goalId))
                continue;

            idCheckList.Add(goalId);
            Goal goal = Instantiate(goalPrefab, goalsParent);
            goal.transform.localScale = Vector3.one * goalScale;
            goal.GetComponent<RectTransform>().anchoredPosition = Vector2.right * startPosX;
            goal.InitSlot(cellSprites[goalId], goalId, goalData.count);
            startPosX += increaseBy;
        }
    }

    void ClearGoals()
    {
        int childCount = goalsParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(goalsParent.GetChild(0).gameObject);
        }
    }

    public void AddGoalToDict(int goalId, Goal goal)
    {
        goalsDict.Add(goalId, goal);
    }

    public Goal GetGoalWithId(int goalId)
    {
        return goalsDict[goalId];
    }

    //ui count on hit
    public void DecreaseGoalCount(int goalId)
    {
        goalsDict.TryGetValue(goalId, out Goal goal);
        goal.DecreaseCount();

        if (goal.count <= 0)
            goalsDict.Remove(goalId);
    }

    public bool IsInGoals(int goalId)
    {
        bool isInGoals = goalsDict.TryGetValue(goalId, out Goal goal) && goal.remainingCount > 0;
        if (isInGoals)
            goal.DecreaseRemainingCount();
        return isInGoals;
    }

    public bool ProcessNextTouch()
    {
        if (moveCount == 0)
            return false;

        int completedGoals = 0;
        foreach (var goalKVP in goalsDict)
        {
            if (goalKVP.Value.remainingCount == 0)
                completedGoals++;
        }

        if (completedGoals == goalsDict.Count)
            return false;

        return true;
    }

    public void CheckGoalsCompleted()
    {
        if (goalsDict.Count == 0 || moveCount == 0)
        {
            string result = goalsDict.Count == 0 ? "Congratulations!" : "No moves left!";
            GameManager.Instance.SetGameOverCanvas(true, result);
        }
    }

    #endregion

    #region MovePart
    public void SetMoveText(int value)
    {
        moveText.text = $"{value}";
    }

    public void DecreaseMoveCount()
    {
        SetMoveText(--moveCount);
    }
    #endregion

    public virtual void MoveCellToGoal(GameObject cellObj, Vector2Int gridPosition, SpriteRenderer renderer, int goalId, float baseGoalSpeed, float unitGoalFactor)
    {
        GameManager.Instance.AddProcess(cellObj.GetInstanceID());
        Transform goal = GetGoalWithId(goalId).icon.transform;
        renderer.sortingOrder = 2;
        float animTime = baseGoalSpeed + (GridController.Instance.gridSize.y - gridPosition.y + Mathf.Abs(Mathf.RoundToInt(GridController.Instance.gridSize.x * .5f - gridPosition.x))) * unitGoalFactor;
        float halfOfAnimTime = animTime * .5f;
        Vector3 targetPos = goal.transform.position;
        targetPos.z = transform.position.z;
        DOTween.Sequence()
            .Join(cellObj.transform.DOMove(targetPos, animTime).SetEase(Ease.InBack))
            .Join(cellObj.transform.DOScale(1.2f, halfOfAnimTime))
            .Join(cellObj.transform.DOScale(.8f, halfOfAnimTime).SetDelay(halfOfAnimTime))
            .OnComplete(() =>
            {
                goal.DOKill();
                goal.DOScale(1.1f, .05f).OnComplete(() => goal.DOScale(1f, .05f));
                DecreaseGoalCount(goalId);
                GameManager.Instance.RemoveProcess(cellObj.GetInstanceID());
                cellObj.SetActive(false);
            });
    }
}
