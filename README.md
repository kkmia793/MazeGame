# MazeGame

## 作品概要
MazeGame は、迷路生成アルゴリズムを活用したステージクリア型のゲームです。
プレイヤーはランダムに生成される迷路を進み、特定のゴールを目指して次のステージへと進行します。

公開先: [MazeGame on Unityroom](https://unityroom.com/games/snowtrail)

---

## 使用言語・環境
- Unity (C#)

### 使用ライブラリ
- DOTween (アニメーション)
- UniTask (非同期処理)

---

## プログラムの工夫・苦労した点

### 1. 深さ優先探索 (DFS) を用いた迷路生成

- **穴掘り法 (Digging Method) の採用:** 迷路をランダムに生成するアルゴリズムとして、**深さ優先探索 (DFS)** を使用。
- **スタック (Stack) を活用:** スタックを使ったバックトラッキングにより、効率的な迷路生成を実現。
- **プレイごとに異なる迷路:** ランダムな方向に掘り進めることで、毎回異なる迷路が生成され、リプレイ性を高めています。

```csharp
private void GeneratePath()
{
    startPosition = new Vector2Int(1, 1);
    maze[startPosition.x, startPosition.y] = 0;

    Stack<Vector2Int> stack = new Stack<Vector2Int>();
    stack.Push(startPosition);

    System.Random random = new System.Random();

    while (stack.Count > 0)
    {
        Vector2Int current = stack.Peek();
        List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

        if (neighbors.Count > 0)
        {
            Vector2Int chosen = neighbors[random.Next(neighbors.Count)];
            Vector2Int wallToBreak = current + (chosen - current) / 2;

            maze[wallToBreak.x, wallToBreak.y] = 0; // 壁を壊す
            maze[chosen.x, chosen.y] = 0;           // 道を作成
            stack.Push(chosen);
        }
        else
        {
            stack.Pop(); // 行き止まりに到達したらバックトラッキング
        }
    }
}
```

### 2. 行き止まりの検出とゴールの配置

- **行き止まりをゴールとして活用:** 迷路内の行き止まりを検出し、ゴールを配置することで自然なゲーム性を実現。
- **重複防止とランダム性:** `System.Random` を使い、行き止まりのセルからランダムにゴールを配置。

```csharp
public void PlaceGoals(int goalCount = 3)
{
    List<Vector2Int> deadEnds = FindDeadEnds();
    System.Random random = new System.Random();

    while (goalPositions.Count < goalCount)
    {
        int index = random.Next(deadEnds.Count);
        Vector2Int goalCell = deadEnds[index];
        deadEnds.RemoveAt(index);

        Vector3 goalWorldPosition = new Vector3(goalCell.x, goalCell.y, 0);
        GameObject goalInstance = Instantiate(goalPrefab, goalWorldPosition, Quaternion.identity);
        goalPositions.Add(goalWorldPosition);
    }
}
```

### 3. 通路情報の保存と視覚化

- **経路情報を再利用:** 迷路生成時に通路情報を保存し、**ゴール配置やライト演出**に活用。
- **親オブジェクトを利用した効率的なオブジェクト管理:** 迷路パーツをまとめて管理することで、リセットや再生成時のメモリ管理を最適化。

```csharp
private List<Vector3> VisualizeMaze()
{
    mazeParent = new GameObject("MazeParent");
    List<Vector3> path = new List<Vector3>();

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            GameObject obj = null;

            if (maze[x, y] == 1) // 壁
            {
                obj = Instantiate(wallPrefab, new Vector3(x, y, 0), Quaternion.identity);
            }
            else if (maze[x, y] == 0) // 通路
            {
                obj = Instantiate(floorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                path.Add(new Vector3(x, y, 0));
            }

            if (obj != null)
            {
                obj.transform.parent = mazeParent.transform;
            }
        }
    }

    return path;
}
```

---

## 今後の改善点

- 迷路生成アルゴリズムの多様化（別のアルゴリズムにも対応する）
- ゴール配置ロジックの最適化（特に行き止まりが少ない場合の対応）
- UI/UX 改善（視覚的なエフェクトやクリア時のフィードバック強化）

---

## 特記事項
- `DOTween` によるアニメーション演出。
- `UniTask` により、シームレスな非同期処理を実現。

