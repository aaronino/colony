using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class HexMaster : MonoBehaviour {

    [SerializeField] int GridWidth;
    [SerializeField] int GridHeight;
    [SerializeField] Vector3 TopLeftPosition;
    [SerializeField] float XSpacing;
    [SerializeField] float YSpacing;
    [SerializeField] GameObject HexTemplate;
    [SerializeField] public GameObject GridParent;
    [SerializeField] public Gameplay Master;
    
    public List<GameObject> CurrentHexGrid;

    public Dictionary<Vector2Int, HexInfo> HexDataDictionary;

    // temp color helpers
    [SerializeField] public Color UnpathableColor;
    [SerializeField] public Color DefaultColor;
    [SerializeField] public Color HighlightColor;
    public GameObject TextX;
    public GameObject TextY;
    
    public void InitializeHexGrid()
    {
        // Reset values and destroy old grid
        if (CurrentHexGrid != null) CurrentHexGrid.ForEach(o => Destroy(o));

        CurrentHexGrid = new List<GameObject>();
        HexDataDictionary = new Dictionary<Vector2Int, HexInfo>();

        for (int y = 0; y < GridHeight; y++) {
            for (int x = 0; x < GridWidth; x++) {
                if (UnityEngine.Random.Range(0, 10) == 0) {
                    // Only instantiate unpathable tiles
                    GameObject o = CreateHexAt(x, y, UnpathableColor);
                    o.GetComponent<Hex>().NeverPathable = true;
                }
            }
        }
    
    }
    
    public void HighlightHex()
    {
        string xs = TextX.GetComponentInChildren<UnityEngine.UI.Text>().text;
        string ys = TextY.GetComponentInChildren<UnityEngine.UI.Text>().text;
        Debug.Log("x " + xs + ", y" + ys);
        int x = System.Convert.ToInt32(xs);
        int y = System.Convert.ToInt32(ys);
        GameObject o = GetHexAt(new Vector2Int(x, y));
        o.GetComponent<Hex>().InitializeHex(HighlightColor, x, y);
    }

    private GameObject CreateHexAt(int x, int y, Color c)
    {
        GameObject o = Instantiate(HexTemplate, CalculatePosition(x, y), Quaternion.identity, GridParent.transform);
        o.GetComponent<Hex>().InitializeHex(c, y, x);
        CurrentHexGrid.Add(o);
        return o;
    }



    /// <summary>
    /// Returns the hex at position x/y (can create it if it doesn't exist)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private GameObject GetHexAt(Vector2Int coords)
    {
        // Note - change the createhexat color value to see which hexes are created as a result of ants wandering. interesting!
        return CurrentHexGrid
                   .FirstOrDefault(o => (o.GetComponent<Hex>().Row == coords.y) 
                   && (o.GetComponent<Hex>().Column == coords.x)) 
                   ?? CreateHexAt(coords.x, coords.y, DefaultColor);   
    }

    /// <summary>
    /// Returns hex info at a particular location
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    public HexInfo GetHexInfoAt(Vector2Int coords)
    {
        HexInfo info;

        if (HexDataDictionary.TryGetValue(coords, out info))
        {
            return info;
        }

        var hexInfo = new HexInfo(coords);

        HexDataDictionary.Add(coords, hexInfo);

        return hexInfo;
    }

    public Vector2 CalculatePosition(int x, int y)
    {
        // eve rows need + .5 x spacing
        float extraX = y % 2 == 1 ? XSpacing * .5f : 0;

        return new Vector2(
            TopLeftPosition.x + (x * XSpacing) + extraX,
            TopLeftPosition.y - (y * YSpacing)
        );

    }

    /// <summary>
    /// Returns all neighboring hexes
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="pathableOnly"></param>
    /// <returns></returns>
    public List<HexInfo> GetNeighboringHexInfo(int x, int y, bool pathableOnly = false)
    {
        return GetRadiusHexInfo(x, y, 1, pathableOnly);
    }

    public List<HexInfo> GetRadiusHexInfo(int x, int y, int radius, bool pathableOnly = false)
    {
        List<Vector2Int> coord = GetNeighboringHexCoordinates(x, y, radius, pathableOnly);
        return coord.Select(GetHexInfoAt).ToList();
    }

    /// <summary>
    /// There's probably a cool math way to do this
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private List<Vector2Int> GetNeighboringHexCoordinates(int x, int y, int radius, bool pathableOnly = false)
    {
        // All values for odd rows, offset is -1 on even rows where marked
        // Neighbors with radius 1 = 6
        // y - 1 = 2 | x, x + 1 (offset)
        // y = 2     | x -1, x + 1
        // Neighbors with radius 2 = 12
        // y - 2 = 3 | (x -1, x, x + 1)
        // y - 1 = 2 | (x - 1, x + 2) (offset)
        // y  = 2    | (x -2, x + 2)
        // Neighbors with radius 3 = 18
        // y - 3 = 4 | x - 1, x, x + 1, x + 2 (offset)
        // y - 2 = 2 | x - 2, x + 2
        // y - 1 = 2 | x - 2, x + 3 (offset)
        // y = 2     | x - 3, x + 3
        // + 2 + 2 + 4

        List<Vector2Int> neighbors;
        int offset = (y % 2 == 1) ? 0 : 1;
        // offset some values by 1
        if (radius == 1) {
            neighbors = new List<Vector2Int>() {
                new Vector2Int(x - offset, y - 1),
                new Vector2Int(x + 1 - offset, y - 1),
                new Vector2Int(x - 1, y),
                new Vector2Int(x + 1, y),
                new Vector2Int(x - offset, y + 1),
                new Vector2Int(x + 1 - offset, y + 1)
                };
        }
        else if (radius == 2) {
            neighbors = new List<Vector2Int>() {
                new Vector2Int(x - 1, y - 2),
                new Vector2Int(x, y - 2),
                new Vector2Int(x + 1, y - 2),
                new Vector2Int(x - 1 - offset, y - 1),
                new Vector2Int(x + 2 - offset, y - 1),
                new Vector2Int(x - 2, y),
                new Vector2Int(x + 2, y),
                new Vector2Int(x - 1, y + 2),
                new Vector2Int(x, y + 2),
                new Vector2Int(x + 1, y + 2),
                new Vector2Int(x - 1 - offset, y + 1),
                new Vector2Int(x + 2 - offset, y + 1),
                };
        }
        else {
            Debug.LogError("Cant determine radius higher than 2");
            neighbors = new List<Vector2Int>();
        }

        neighbors.RemoveAll(o => o.x < 0 || o.y < 0 || o.x > GridWidth || o.y > GridWidth);
        
        if (pathableOnly) {
            neighbors.RemoveAll(o => GetHexAt(o).GetComponent<Hex>().NeverPathable);
        } 
        
        return neighbors;
    } 
}
