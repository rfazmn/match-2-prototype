using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : Singleton<TouchHandler>
{
    public bool processTouch = true;
    void Start()
    {
        Input.multiTouchEnabled = false;
    }

    IEnumerator ExecuteTouch(GameObject hitObj)
    {
        Cell cell = hitObj.GetComponent<Cell>();
        List<Cell> matchList = FindMatchList(cell);

        bool isValidMove = matchList != null && matchList.Count > 0;
        if (isValidMove)
        {
            processTouch = false;
            GoalHandler.Instance.DecreaseMoveCount();

            foreach (Cell tempCell in matchList)
            {
                tempCell.Blast();
            }

            Dictionary<int, List<FillData>> fillData = new Dictionary<int, List<FillData>>();
            yield return StartCoroutine(GridController.Instance.FallCells(fillData));
            yield return StartCoroutine(GridController.Instance.FillGrid(fillData));
            GridController.Instance.UpdateCellsNeighbours();
            processTouch = GoalHandler.Instance.ProcessNextTouch();

            if (processTouch)
            {
                processTouch = false;
                yield return StartCoroutine(GridController.Instance.CheckShuffle());
                processTouch = true;
            }
            else
            {
                yield return new WaitUntil(() => GameManager.Instance.GetProcessCount() == 0);
                GoalHandler.Instance.CheckGoalsCompleted();
            }
        }
        else
            TickleCell(cell);
    }

    public List<Cell> FindMatchList(Cell cell)
    {
        //checks for other cells can be done here
        return MatchFinder.Instance.FindMatchList(cell);
    }

    void TickleCell(Cell cell)
    {
        cell.Tickle();
    }

    #region TouchDetection

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        GetTouchEditor();
#else
		GetTouchMobile();
#endif
    }

    private void GetTouchEditor()
    {
        if (Input.GetMouseButtonUp(0))
        {
            CheckHit();
        }
    }

    private void GetTouchMobile()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    CheckHit();
                    break;
            }
        }
    }

    private void CheckHit()
    {
        BoxCollider2D hit = (BoxCollider2D)Physics2D.OverlapPoint(GameManager.Instance.mainCam.ScreenToWorldPoint(Input.mousePosition));

        if (!processTouch || hit == null || !hit.TryGetComponent(out Cell cell) || cell.blasted)
            return;

        StartCoroutine(ExecuteTouch(hit.gameObject));
    }

    #endregion
}
