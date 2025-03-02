# MazeGame

## 作品概要
迷路ゲームです。
プレイヤーはランダムに生成される迷路を進み、特定のゴールを目指して次のステージへと進行します。

公開先: [MazeGame on Unityroom](https://unityroom.com/games/snowtrail)

---

## 使用言語・環境
- Unity (C#)


### 使用ライブラリ
- DOTween (アニメーション)
- UniTask (非同期処理)

---

## アルゴリズム的な工夫点

### 1. 迷路生成アルゴリズムの最適化 (MazeGenerator)

- **深さ優先探索 (DFS) とバックトラッキング:** 迷路をランダムに生成する際、深さ優先探索を使用し、行き止まりに達した場合はスタックを用いてバックトラッキングを行います。
- **動的経路生成:** 迷路の経路が毎回異なるため、プレイごとに新鮮な体験を提供。

```csharp
while (stack.Count > 0)
{
    Vector2Int current = stack.Peek();
    List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

    if (neighbors.Count > 0)
    {
        Vector2Int chosen = neighbors[random.Next(neighbors.Count)];
        Vector2Int wallToBreak = current + (chosen - current) / 2;

        maze[wallToBreak.x, wallToBreak.y] = 0;
        maze[chosen.x, chosen.y] = 0;
        stack.Push(chosen);
    }
    else
    {
        stack.Pop();
    }
}
```

### 2. 動的なゴール配置 (PlaceGoals)

- **行き止まりを優先:** ゴールは可能な限り迷路の行き止まりに配置することで、プレイヤーに探索性を与えています。
- **ランダム要素の導入:** 行き止まりが足りない場合は、ランダムにゴールを配置することで多様な難易度を提供。

```csharp
while (goalPositions.Count < goalCount)
{
    if (deadEnds.Count > 0)
    {
        Vector2Int goalCell = deadEnds[random.Next(deadEnds.Count)];
        Vector3 goalWorldPosition = new Vector3(goalCell.x, goalCell.y, 0);
        GameObject goalInstance = Instantiate(goalPrefab, goalWorldPosition, Quaternion.identity);
        goalPositions.Add(goalWorldPosition);
        goalInstances.Add(goalInstance);
    }
}
```

### 3. ステージ制御と非同期処理 (GameManager)

- **UniTask の活用:** 各ステージの開始、カウントダウン、時間制限を非同期で実行。
- **ステージ進行をシームレスに:** シーンをまたぐことなく、ゲーム内で連続してステージを進行できます。

```csharp
private async UniTask StartStage(int stageNumber)
{
    uiManager.UpdateStageText($"Stage {stageNumber}");
    await uiManager.ShowCountdown(3);
    EnablePlayerControl();
    await StageTimer(currentTimeLimit);
}
```


---

## 特記事項
- `DOTween` によるアニメーション、`UniTask` による非同期処理を実装。

