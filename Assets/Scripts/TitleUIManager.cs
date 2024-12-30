using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleUIManager : MonoBehaviour
{
    public GameObject crystalIcon; 
    public GameObject crystalIcon2; 
    
    public GameObject configPanel;
    
    private int selectedOption = 0; 
    
    public CustomSceneManager sceneManager;

    private void Start()
    {
        configPanel.SetActive(false);
    }

    void Update()
    {
        // 上下キーまたはW/Sキーで選択を切り替え
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedOption = 1 - selectedOption; // 0 ↔ 1 切り替え
            UpdateSelection(selectedOption);
            SoundManager.Instance.PlaySE(SESoundData.SE.Move);
        }
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
            SoundManager.Instance.PlaySE(SESoundData.SE.Select);
        }
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
    
    private void ConfirmSelection()
    {
        if (selectedOption == 0)
        {
            sceneManager.LoadMainScene(); 
        }
        else
        {
            configPanel.SetActive(true);
        }
    }

    public void CloseConfigPanel()
    {
        configPanel.SetActive(false);
    }
}
