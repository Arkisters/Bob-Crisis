using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSwapper : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap targetTilemap;
    
    [Header("Tile Assets")]
    [Tooltip("Drag all 64 barren tiles here (indices 0-63)")]
    public TileBase[] barrenTiles = new TileBase[64];
    [Tooltip("Drag all 64 fresh tiles here (SAME ORDER as barren, indices 0-63)")]
    public TileBase[] freshTiles = new TileBase[64];
    
    private bool isFresh = false;
    
    void Start()
    {
        // Validate setup
        if (barrenTiles.Length != freshTiles.Length)
        {
            Debug.LogError("Barren and Fresh tile arrays must have the same length!");
        }
    }
    
    public void SwapToFresh()
    {
        if (isFresh) return;
        
        for (int i = 0; i < barrenTiles.Length; i++)
        {
            if (barrenTiles[i] != null && freshTiles[i] != null)
            {
                targetTilemap.SwapTile(barrenTiles[i], freshTiles[i]);
            }
        }
        
        isFresh = true;
        Debug.Log("Swapped to fresh tilemap");
    }
    
    public void SwapToBarren()
    {
        if (!isFresh) return;
        
        for (int i = 0; i < barrenTiles.Length; i++)
        {
            if (barrenTiles[i] != null && freshTiles[i] != null)
            {
                targetTilemap.SwapTile(freshTiles[i], barrenTiles[i]);
            }
        }
        
        isFresh = false;
        Debug.Log("Swapped to barren tilemap");
    }
    
    public void Toggle()
    {
        if (isFresh)
            SwapToBarren();
        else
            SwapToFresh();
    }
    
    void Update()
    {
        // Press G to toggle between tilesets
        if (Input.GetKeyDown(KeyCode.G))
        {
            Toggle();
        }
    }
}
