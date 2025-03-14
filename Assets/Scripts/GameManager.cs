using UnityEngine;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using unityroom.Api;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } 

    public MazeGenerator mazeGenerator; 
    public GameObject player; 
    public LightController lightController; 
    public UIManager uiManager; 
    public CustomSceneManager sceneManager;

    public int initialWidth = 21;
    public int initialHeight = 21;
    public float initialTimeLimit = 30f;
    public int maxStages = 5;
    public int initialHealth = 3;

    private int currentStage = 1;
    private int collectedGoals = 0;
    private float currentTimeLimit = 30;
    private int playerHealth;
    
    private int selectedOption = 0; 
    
    public bool isStageTransitioning = false;
    public bool isGameOver = false;
    private bool isTimerActive = false; 
    private CancellationTokenSource tileDeactivationCTS; // タイル消去のキャンセル用
    
    private void Awake()
    {
        HidePlayer();
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    /*
    private async void Start()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        
        playerHealth = initialHealth; 
        uiManager.UpdateHealth(playerHealth); 
        
        await StartStage(currentStage);
    }

    private async UniTask StartStage(int stageNumber)
    {
        Debug.Log($"Stage {stageNumber} Start!");

        // ステージ設定
        mazeGenerator.width = initialWidth;
        mazeGenerator.height = initialHeight;
        currentTimeLimit = initialTimeLimit - (stageNumber - 1) * 5;

        // 迷路生成とゴール配置
        mazeGenerator.GenerateAndVisualizeMaze();
        mazeGenerator.PlaceGoals(stageNumber);

        PlacePlayerAtStart();

        collectedGoals = 0;
        //uiManager.UpdateGoals(collectedGoals, stageNumber);

        await HighlightPathAsync();

        await UniTask.Delay(2000);
        lightController.DeactivateRandomTiles(10);
        HighlightInitialTile();

        EnablePlayerControl();

        await StageTimer(currentTimeLimit);

        RestartStage();
    }
    */
    
    /*
    private async void Start()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
    
        playerHealth = initialHealth; // 初期体力を設定
        uiManager.UpdateHealth(playerHealth); // 初期体力をUIに反映
    
        await StartStage(currentStage);
    }
    */
    private async void Start()
    {
        InitializeGame();
        await StartStage(currentStage);
    }
    
    private void Update()
    {
        if (!isGameOver) return;
        
        HandleGameOverInput();
    }

    private void InitializeGame()
    {
        playerHealth = initialHealth;
        isGameOver = false;
    }
    
    private async UniTask StartStage(int stageNumber)
    {
        isStageTransitioning = true; 
        
        // 既存のタイル消去処理をキャンセル（ステージ進行後）
        tileDeactivationCTS?.Cancel();
        tileDeactivationCTS = new CancellationTokenSource(); 
        
        mazeGenerator.SetMazeVisibility(false);
        
        string stageText = ConvertStageNumberToKanji(stageNumber);
        uiManager.UpdateStageText($"{stageText}");
        
        await uiManager.ShowCountdown(3);
        
        mazeGenerator.SetMazeVisibility(true);

        // ステージの初期化
        InitializeStage(stageNumber);
        
        isStageTransitioning = false; 

        // ゲーム進行処理
        EnablePlayerControl();
        StartRandomTileDeactivation(stageNumber, tileDeactivationCTS.Token).Forget();
        
        await StageTimer(currentTimeLimit);
    }

    /*
    private async UniTask StartStage(int stageNumber)
    {
        Debug.Log($"Stage {stageNumber} Start!");

        // ステージ設定
        mazeGenerator.width = initialWidth;
        mazeGenerator.height = initialHeight;
        currentTimeLimit = initialTimeLimit - (stageNumber - 1) * 5;

        // 迷路生成とゴール配置
        mazeGenerator.GenerateAndVisualizeMaze();
        mazeGenerator.PlaceGoals(3);

        // プレイヤー初期化
        PlacePlayerAtStart();

        collectedGoals = 0;

        // ステージタイマー開始
        EnablePlayerControl();
        StartRandomTileDeactivation(stageNumber).Forget();
        await StageTimer(currentTimeLimit);

        Debug.Log("Time's up!");
        RestartStage();
    }
    
    */
    private void InitializeStage(int stageNumber)
    {
        mazeGenerator.width = initialWidth ;
        mazeGenerator.height = initialHeight ;
        mazeGenerator.GenerateAndVisualizeMaze();
        mazeGenerator.PlaceGoals(3);
        
        
        playerHealth = initialHealth;
        currentTimeLimit = initialTimeLimit;
        
        uiManager.InitializeHpIcons();
        uiManager.InitializeGoalIcons();
        
        collectedGoals = 0;
        PlacePlayerAtStart();
        
        ShowPlayer();
    }

    private async UniTask StartRandomTileDeactivation(int stageNumber, CancellationToken token)
    {
        float interval = Mathf.Max(5.0f, 10.0f - (stageNumber - 1) * 0.5f); 

        while (!isGameOver)
        {
            await UniTask.Delay((int)(interval * 1000), cancellationToken: token); 
            if (token.IsCancellationRequested) return; 

            int tilesToDeactivate = 5 + (stageNumber - 1) * 2; 
            lightController.DeactivateRandomTiles(tilesToDeactivate);
        }
    }
    

    private async UniTask StageTimer(float timeLimit)
    {
        float timeRemaining = timeLimit;
        isTimerActive = true;

        while (timeRemaining > 0 && !isGameOver && isTimerActive)
        {
            timeRemaining -= Time.deltaTime;
            uiManager.UpdateTimer(timeRemaining);
            
            await UniTask.Yield();
        }
        
        if (timeRemaining <= 0 && !isGameOver)
        {
            isGameOver = true; 
            GameOver();        
        }
        
        isTimerActive = false;
    }

    public void OnWrongPath()
    {
        if (isGameOver) return;
        
        playerHealth--;
        uiManager.UpdateHpIcon(playerHealth); 
        
        SoundManager.Instance.PlaySE(SESoundData.SE.Damage);

        if (playerHealth <= 0)
        {
            GameOver();
        }
        else
        {
            RestartStage();
        }
    }
    /*

    public void OnGoalReached(Vector3 goalPosition)
    {
        collectedGoals++;
        mazeGenerator.RemoveGoal(goalPosition); 
        
        uiManager.UpdateGoalIcon(collectedGoals - 1);
        //uiManager.UpdateGoals(collectedGoals, currentStage);

        if (collectedGoals == 3)
        {
            NextStage();
        }
    }
    */
    
    public void OnGoalReached(Vector3 goalPosition)
    {
        collectedGoals++;
        mazeGenerator.RemoveGoal(goalPosition);
        
        uiManager.UpdateGoalIcon(collectedGoals - 1);
        
        SoundManager.Instance.PlaySE(SESoundData.SE.Get);

        if (collectedGoals >= 3)
        {
            HidePlayer();
            NextStage().Forget();
            SoundManager.Instance.PlaySE(SESoundData.SE.StageClear);
        }
    }
    
    private async UniTaskVoid NextStage()
    {
        currentStage++;

        isTimerActive = false;
        
        if (currentStage > maxStages)
        {
            GameClear();
        }
        else
        {
            if (currentStage == 3)
            {
                UnityroomApiClient.Instance.SendScore(2, 100, ScoreboardWriteMode.HighScoreDesc);
            }

            await StartStage(currentStage);
        }
    }

    /*
    private void NextStage()
    {
        currentStage++;

        if (currentStage > maxStages)
        {
            GameClear();
        }
        else
        {
            StartStage(currentStage).Forget();
        }
    }
    */

    private void RestartStage()
    {
        PlacePlayerAtStart();
    }
    
    private void HidePlayer()
    {
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void ShowPlayer()
    {
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    private void PlacePlayerAtStart()
    {
        Vector3 startPosition = new Vector3(mazeGenerator.startPosition.x, mazeGenerator.startPosition.y, 0);
        player.transform.position = startPosition;
    }

    private async UniTask HighlightPathAsync()
    {
        foreach (Vector3 position in mazeGenerator.path)
        {
            Collider2D tile = Physics2D.OverlapPoint(position);
            if (tile != null)
            {
                lightController.ActivateTile(tile.gameObject);
                await UniTask.Delay(100);
            }
        }
    }

    private void HighlightInitialTile()
    {
        Vector2Int initialTilePosition = mazeGenerator.startPosition;
        Collider2D tile = Physics2D.OverlapPoint(new Vector2(initialTilePosition.x, initialTilePosition.y));
        if (tile != null)
        {
            lightController.ActivateTile(tile.gameObject);
        }
    }

    private void EnablePlayerControl()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.EnableControl(mazeGenerator.path, mazeGenerator.goalPositions);
        }
    }
    
    private string ConvertStageNumberToKanji(int number)
    {
        string[] kanjiNumbers = { "零", "壱", "弍", "参", "肆", "伍" };

        if (number >= 0 && number < kanjiNumbers.Length)
        {
            return kanjiNumbers[number];
        }

        return null;
    }

    private void GameClear()
    {
        sceneManager.LoadResultScene();
        UnityroomApiClient.Instance.SendScore(1, 100, ScoreboardWriteMode.HighScoreDesc);
    }

    private void GameOver()
    {
        isGameOver = true;
        HidePlayer();
        mazeGenerator.SetMazeVisibility(false);
        uiManager.ShowGameOverPanel(0);
    }
    
    private void HandleGameOverInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedOption = 1 - selectedOption; // 0 ↔ 1 切り替え
            uiManager.UpdateSelection(selectedOption);
            SoundManager.Instance.PlaySE(SESoundData.SE.Move);
        }

        if (Input.GetKeyDown(KeyCode.Return)|| Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
            SoundManager.Instance.PlaySE(SESoundData.SE.Select);
        }
    }

    private void ConfirmSelection()
    {
        tileDeactivationCTS?.Cancel(); // 既存のタイル消去処理をキャンセル（シーン遷移後）
        
        if (selectedOption == 0)
        {
            sceneManager.ReloadCurrentScene(); 
        }
        else
        {
            sceneManager.LoadTitleScene(); 
        }
    }
    
    
}