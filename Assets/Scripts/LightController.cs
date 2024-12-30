using UnityEngine;

public class LightController : MonoBehaviour
{
    public static LightController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 全てのタイルを暗くする
    public void DeactivateAllTiles()
    {
        TileHighlighter[] allHighlighters = FindObjectsOfType<TileHighlighter>();
        foreach (var highlighter in allHighlighters)
        {
            highlighter.DeactivateTile();
            SoundManager.Instance.PlaySE(SESoundData.SE.Fubuki);
        }
    }

    // 指定されたタイルを明るくする
    public void ActivateTile(GameObject tile)
    {
        TileHighlighter highlighter = tile.GetComponent<TileHighlighter>();
        if (highlighter != null)
        {
            highlighter.ActivateTile();
        }
    }

    // ランダムにタイルを暗くする
    public void DeactivateRandomTiles(int numberOfTilesToDeactivate)
    {
        TileHighlighter[] activeHighlighters = FindObjectsOfType<TileHighlighter>();
        int totalActiveTiles = activeHighlighters.Length;

        if (numberOfTilesToDeactivate > totalActiveTiles)
        {
            numberOfTilesToDeactivate = totalActiveTiles;
        }

        System.Random random = new System.Random();

        for (int i = 0; i < numberOfTilesToDeactivate; i++)
        {
            int randomIndex = random.Next(activeHighlighters.Length);
            TileHighlighter randomTile = activeHighlighters[randomIndex];

            if (randomTile != null)
            {
                randomTile.DeactivateTile();
            }

            // 配列から削除（既に暗くしたタイルを選ばないようにする）
            activeHighlighters[randomIndex] = activeHighlighters[totalActiveTiles - 1];
            totalActiveTiles--;
        }
    }
    
}