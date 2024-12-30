using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class UIManager : MonoBehaviour
{
    public Text timerText; 
    public Text stageText;        
    public Text countdownText;    
    public Text instructionText; 
    
    public GameObject crystalIcon; 
    public GameObject crystalIcon2; 
    public GameObject restartText;    
    public GameObject titleText;   
    public GameObject gameOverPanel;
    
    private float timeLimit; 
    

    
    public List<Image> hpIcons; 
    public List<Image> goalIcons; // ゴールのアイコンリスト
    
    
    
    public void InitializeGoalIcons()
    {
        foreach (var icon in goalIcons)
        {
            Color color = icon.color;
            color.a = 150f / 255f; 
            icon.color = color;
        }
    }
    public void UpdateGoalIcon(int goalIndex)
    {
        if (goalIndex >= 0 && goalIndex < goalIcons.Count)
        {
            Color color = goalIcons[goalIndex].color;
            color.a = 1f; 
            goalIcons[goalIndex].color = color;
        }
    }
    
    // HPアイコンを初期化（全て完全表示）
    public void InitializeHpIcons()
    {
        foreach (var icon in hpIcons)
        {
            Color color = icon.color;
            color.a = 1f; // 完全不透明
            icon.color = color;
        }
    }

    // 指定されたHPアイコンを透明化
    public void UpdateHpIcon(int remainingHp)
    {
        for (int i = 0; i < hpIcons.Count; i++)
        {
            Color color = hpIcons[i].color;
            color.a = i < remainingHp ? 1f : 0.3f; // 残りHP以下は完全表示、他は透明化
            hpIcons[i].color = color;
        }
    }

    
    public void UpdateStageText(string text)
    {
        if (stageText != null)
        {
            stageText.text = text;
        }
    }
    
    public async UniTask ShowCountdown(int seconds)
    {

        for (int i = seconds; i > 0; i--)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = i.ToString();
            await UniTask.Delay(200);  // 1秒待機
        }

        countdownText.text = "結晶を集めよ";
        await UniTask.Delay(1500);  // 2秒待機
        countdownText.gameObject.SetActive(false);
    }

    /*
    public async UniTask ShowCollectGoalsMessage()
    {
        if (instructionText == null) return;

        instructionText.gameObject.SetActive(true);
        instructionText.text = "結晶を集めよ";
        await UniTask.Delay(2000); // 1秒間表示
        instructionText.gameObject.SetActive(false);
    }
    */
    
    public void UpdateTimer(float timeRemaining)
    {
        // 残り時間を整数にしてテキスト表示
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }
    
    public void ShowGameOverPanel(int defaultOption)
    {
        gameOverPanel.SetActive(true);
        UpdateSelection(defaultOption); 
    }
    
    public void UpdateSelection(int selectedOption)
    {
        if (selectedOption == 0)
        {
            crystalIcon.SetActive(true);
            crystalIcon2.SetActive(false);
        }
        else if (selectedOption == 1)
        {
            crystalIcon.SetActive(false);
            crystalIcon2.SetActive(true);
        }
    }
    
}