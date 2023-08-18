using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;
    public int goalId;
    public int count;
    public int remainingCount;

    void Start()
    {
        RegisterGoal();
    }

    void RegisterGoal()
    {
        GoalHandler.Instance.AddGoalToDict(goalId, this);
    }

    public void InitSlot(Sprite cellSprite, int _goalId, int _count)
    {
        icon.sprite = cellSprite;
        goalId = _goalId;
        count = _count;
        remainingCount = count;
        UpdateCountText(count);
    }

    public void DecreaseCount()
    {
        count--;
        UpdateCountText(count);
    }

    void UpdateCountText(int value)
    {
        countText.text = $"{value}";
    }

    public void DecreaseRemainingCount()
    {
        remainingCount--;
    }
}
