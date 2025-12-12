using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSwapper : MonoBehaviour
{
    [Header("Tile Assets")]
    [Tooltip("Drag all 64 barren tiles here (indices 0-63)")]
    public TileBase[] barrenTiles = new TileBase[64];
    [Tooltip("Drag all 64 fresh tiles here (SAME ORDER as barren, indices 0-63)")]
    public TileBase[] freshTiles = new TileBase[64];
    
    [Header("Freshness Settings")]
    [Tooltip("How much opacity to add per tree trigger (0.2 = 20%)")]
    public float opacityIncreasePerTree = 0.2f;
    [Tooltip("Duration of opacity fade in seconds")]
    public float fadeDuration = 1f;
    
    private Tilemap barrenTilemap;
    private Tilemap freshTilemap;
    private TilemapRenderer freshRenderer;
    private float currentOpacity = 0f;
    private float targetOpacity = 0f;
    private bool isFading = false;
    
    void Start()
    {
        // Get reference to this tilemap (the barren one)
        barrenTilemap = GetComponent<Tilemap>();
        
        // Validate setup
        if (barrenTiles.Length != freshTiles.Length)
        {
            Debug.LogError("Barren and Fresh tile arrays must have the same length!");
        }
        
        if (barrenTilemap == null)
        {
            Debug.LogError("TilemapSwapper must be attached to a Tilemap GameObject!");
        }
    }
    
    /// <summary>
    /// Called by colorchange trigger - creates fresh overlay on first call, increases opacity on subsequent calls
    /// </summary>
    public void IncreaseFreshness()
    {
        // If fresh tilemap doesn't exist yet, create it
        if (freshTilemap == null)
        {
            CreateFreshOverlay();
        }
        
        // Set new target opacity
        targetOpacity = Mathf.Min(targetOpacity + opacityIncreasePerTree, 1f);
        
        // Start smooth transition
        if (!isFading)
        {
            StartCoroutine(FadeToTargetOpacity());
        }
        
        Debug.Log($"Fresh tilemap opacity fading to {targetOpacity * 100f}%");
    }
    
    /// <summary>
    /// Creates a new tilemap overlay with fresh tiles
    /// </summary>
    private void CreateFreshOverlay()
    {
        Debug.Log("Creating fresh tilemap overlay...");
        
        // Create new GameObject for fresh tilemap
        GameObject freshTilemapObj = new GameObject("FreshTilemap_Auto");
        freshTilemapObj.transform.SetParent(barrenTilemap.transform.parent);
        freshTilemapObj.transform.localPosition = Vector3.zero;
        freshTilemapObj.transform.localRotation = Quaternion.identity;
        freshTilemapObj.transform.localScale = Vector3.one;
        
        // Set the GameObject layer to Environment
        freshTilemapObj.layer = LayerMask.NameToLayer("Environment");
        
        // Add Tilemap component
        freshTilemap = freshTilemapObj.AddComponent<Tilemap>();
        
        // Add TilemapRenderer component
        freshRenderer = freshTilemapObj.AddComponent<TilemapRenderer>();
        
        // Copy sorting layer settings from barren tilemap BEFORE creating material
        TilemapRenderer barrenRenderer = barrenTilemap.GetComponent<TilemapRenderer>();
        if (barrenRenderer != null)
        {
            freshRenderer.sortingLayerID = barrenRenderer.sortingLayerID;
            freshRenderer.sortingLayerName = barrenRenderer.sortingLayerName;
            freshRenderer.sortingOrder = barrenRenderer.sortingOrder + 1;
            
            Debug.Log($"Fresh tilemap sorting: Layer={freshRenderer.sortingLayerName}, Order={freshRenderer.sortingOrder}");
        }
        else
        {
            Debug.LogWarning("Barren renderer not found, using default settings");
            freshRenderer.sortingLayerName = "Environment";
            freshRenderer.sortingOrder = 0;
        }
        
        // Create a material instance for alpha control
        freshRenderer.material = new Material(freshRenderer.material);
        
        // Copy all tiles from barren to fresh, swapping barren tiles with fresh tiles
        CopyAndSwapTiles();
        
        // Set initial opacity to 0
        currentOpacity = 0f;
        UpdateFreshOpacity();
        
        Debug.Log("Fresh tilemap overlay created successfully!");
    }
    
    /// <summary>
    /// Copies all tiles from barren tilemap and swaps barren tiles with fresh equivalents
    /// </summary>
    private void CopyAndSwapTiles()
    {
        // Get the bounds of the barren tilemap
        BoundsInt bounds = barrenTilemap.cellBounds;
        
        // Iterate through all positions in the tilemap
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = barrenTilemap.GetTile(pos);
            
            if (tile != null)
            {
                // Check if this tile is one of the barren tiles
                TileBase freshTile = GetFreshEquivalent(tile);
                
                if (freshTile != null)
                {
                    // Place the fresh equivalent
                    freshTilemap.SetTile(pos, freshTile);
                }
                else
                {
                    // If no mapping found, just copy the original tile
                    freshTilemap.SetTile(pos, tile);
                }
            }
        }
    }
    
    /// <summary>
    /// Finds the fresh tile equivalent for a given barren tile
    /// </summary>
    private TileBase GetFreshEquivalent(TileBase barrenTile)
    {
        for (int i = 0; i < barrenTiles.Length; i++)
        {
            if (barrenTiles[i] == barrenTile)
            {
                return freshTiles[i];
            }
        }
        return null;
    }
    
    /// <summary>
    /// Updates the opacity/alpha of the fresh tilemap
    /// </summary>
    private void UpdateFreshOpacity()
    {
        if (freshRenderer != null)
        {
            Color color = freshRenderer.material.color;
            color.a = currentOpacity;
            freshRenderer.material.color = color;
        }
    }
    
    /// <summary>
    /// Smoothly fades opacity to target over time
    /// </summary>
    private System.Collections.IEnumerator FadeToTargetOpacity()
    {
        isFading = true;
        float startOpacity = currentOpacity;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            currentOpacity = Mathf.Lerp(startOpacity, targetOpacity, t);
            UpdateFreshOpacity();
            yield return null;
        }
        
        currentOpacity = targetOpacity;
        UpdateFreshOpacity();
        isFading = false;
    }
    
    // Debug method - press G to manually increase freshness
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            IncreaseFreshness();
        }
    }
}
