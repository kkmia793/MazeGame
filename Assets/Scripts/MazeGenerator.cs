using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public int width = 21; // 奇数
    public int height = 21; // 奇数
    public GameObject wallPrefab; // 壁
    public GameObject floorPrefab; // 床
    public GameObject goalPrefab; // ゴール（雪の結晶）

    private int[,] maze; // 迷路データ
    private GameObject mazeParent; // 迷路の親オブジェクト

    public Vector2Int startPosition; // スタート位置
    public List<Vector3> path { get; private set; } // 経路情報
    public List<Vector3> goalPositions { get; private set; } 
    
    private List<GameObject> goalInstances = new List<GameObject>();

    public List<Vector3> GenerateAndVisualizeMaze()
    {
        ClearPreviousMaze(); // 既存の迷路を削除
        InitializeMaze();
        GeneratePath();
        path = VisualizeMaze();
        ActivateStartTile(); // スタート位置をアクティブに
        return path;
    }

    private void ClearPreviousMaze()
    {
        if (mazeParent != null)
        {
            Destroy(mazeParent);
        }
    }

    private void InitializeMaze()
    {
        maze = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                maze[x, y] = 1; // 全て壁
            }
        }
    }

    private void GeneratePath()
    {
        startPosition = new Vector2Int(1, 1);
        maze[startPosition.x, startPosition.y] = 0; // スタート地点

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
                maze[chosen.x, chosen.y] = 0; // 道を作成
                stack.Push(chosen);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    private List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up * 2, Vector2Int.down * 2, Vector2Int.left * 2, Vector2Int.right * 2 };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = cell + dir;
            if (IsInBounds(neighbor) && maze[neighbor.x, neighbor.y] == 1)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private bool IsInBounds(Vector2Int position)
    {
        return position.x > 0 && position.x < width && position.y > 0 && position.y < height;
    }

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
                else if (maze[x, y] == 0) // 道
                {
                    obj = Instantiate(floorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    path.Add(new Vector3(x, y, 0)); // 経路に追加
                }

                if (obj != null)
                {
                    obj.transform.parent = mazeParent.transform;
                }
            }
        }

        return path;
    }

    private void ActivateStartTile()
    {
        Vector3 startWorldPosition = new Vector3(startPosition.x, startPosition.y, 0);
        Collider2D startTile = Physics2D.OverlapPoint(startWorldPosition);

        if (startTile != null)
        {
            LightController.Instance.ActivateTile(startTile.gameObject); // スタート位置をアクティブに
        }
    }
    
    public void PlaceGoals(int goalCount = 3)
    {
        goalCount = 3; 
        goalPositions = new List<Vector3>();
        goalInstances.Clear(); 

        List<Vector2Int> deadEnds = FindDeadEnds();

        System.Random random = new System.Random();

        // 必ず3つのゴールを配置
        while (goalPositions.Count < goalCount)
        {
            if (deadEnds.Count > 0)
            {
                // 行き止まりからランダムに選択
                int index = random.Next(deadEnds.Count);
                Vector2Int goalCell = deadEnds[index];
                deadEnds.RemoveAt(index);

                Vector3 goalWorldPosition = new Vector3(goalCell.x, goalCell.y, 0);
                if (!goalPositions.Contains(goalWorldPosition)) // 重複防止
                {
                    GameObject goalInstance = Instantiate(goalPrefab, goalWorldPosition, Quaternion.identity);
                    goalPositions.Add(goalWorldPosition);
                    goalInstances.Add(goalInstance);
                }
            }
            else
            {
                // 行き止まりが足りない場合、ランダムな道をゴールに設定
                Vector2Int randomCell = GetRandomPathCell();
                Vector3 goalWorldPosition = new Vector3(randomCell.x, randomCell.y, 0);
                if (!goalPositions.Contains(goalWorldPosition)) // 重複防止
                {
                    GameObject goalInstance = Instantiate(goalPrefab, goalWorldPosition, Quaternion.identity);
                    goalPositions.Add(goalWorldPosition);
                    goalInstances.Add(goalInstance);
                }
            }
        }
    }

    
    public void RemoveGoal(Vector3 goalPosition)
    {
        // ゴール位置に一致するインスタンスを検索して削除 重いかも
        GameObject goalToRemove = goalInstances.Find(goal => goal.transform.position == goalPosition);
        if (goalToRemove != null)
        {
            goalInstances.Remove(goalToRemove);
            Destroy(goalToRemove);
        }
    }

    private List<Vector2Int> FindDeadEnds()
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (maze[x, y] == 0) // 道
                {
                    int wallCount = 0;
                    foreach (Vector2Int dir in new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                    {
                        Vector2Int neighbor = new Vector2Int(x, y) + dir;
                        if (maze[neighbor.x, neighbor.y] == 1) wallCount++;
                    }

                    if (wallCount == 3 && (x != startPosition.x || y != startPosition.y)) // 行き止まりかつスタート地点以外
                    {
                        deadEnds.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return deadEnds;
    }
    
    public void SetMazeVisibility(bool visible)
    {
        if (mazeParent != null)
        {
            mazeParent.SetActive(visible);
        }
    }

    private Vector2Int GetRandomPathCell()
    {
        System.Random random = new System.Random();
        while (true)
        {
            int x = random.Next(1, width - 1);
            int y = random.Next(1, height - 1);

            if (maze[x, y] == 0) 
            {
                return new Vector2Int(x, y);
            }
        }
    }
    public List<Vector3> GetGoalPositions()
    {
        return goalPositions;
    }

    public List<Vector3> GetPath()
    {
        return path;
    }
}