using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Camera mainCam;
    List<int> processList = new List<int>();

    [SerializeField] GameObject gameOverCanvas;
    [SerializeField] TMP_Text resultText;

    #region Processes
    public void AddProcess(int instanceId)
    {
        if (processList.Contains(instanceId))
            return;

        processList.Add(instanceId);
    }

    public void RemoveProcess(int instanceId)
    {
        processList.Remove(instanceId);
    }

    public int GetProcessCount()
    {
        return processList.Count;
    }

    #endregion

    #region GameOverStuff
    public void SetGameOverCanvas(bool value, string result)
    {
        SetResultText(result);
        gameOverCanvas.SetActive(value);
    }

    void SetResultText(string result)
    {
        resultText.text = result;
    }
    #endregion

    #region SceneManagement
    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
    #endregion
}
