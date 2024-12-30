using UnityEngine;

public class TileHighlighter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // タイルを明るくする
    public void ActivateTile()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true; 
        }
    }

    // タイルを暗くする
    public void DeactivateTile()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false; 
        }
    }
}