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
    private List<HexInfo> _scentedHexes;
    public bool _selectingHex;
    public HexInfo SelectedHex;

    // temp color helpers
    [SerializeField] public Color UnpathableColor;
    [SerializeField] public Color DefaultColor;
    [SerializeField] public Color HighlightColor;
    public GameObject TextX;
    public GameObject TextY;

    public void Update()
    {
        if (!_selectingHex && Input.GetMouseButtonDown(0))
        {
            _selectingHex = true;
        }
        else if (_selectingHex && !Input.GetMouseButtonDown(0))
        {
            // select the hex on button release (click)
            _selectingHex = false;
            var mousePos = Input.mousePosition;
            mousePos.z = 10; // select distance = 10 units from the camera
            var worldPoint = Master.MainCamera.ScreenToWorldPoint(mousePos);
            // xypoint are the coordinates of the hex that was clicked on. There is no bounds checking
            var xyPoint = ConvertXYToCoordinates(worldPoint.x - TopLeftPosition.x, worldPoint.y - TopLeftPosition.y);

            var hex = GetHexInfoAt(xyPoint);
            if (hex != null)
            {
                if (SelectedHex != null)
                {
                    HighlightHex(SelectedHex.Coordinates, SelectedHex.IsPathable ? DefaultColor : UnpathableColor);
                }
                SelectedHex = hex;
                Debug.Log(string.Format("Selected hex at {0},{1}.",xyPoint.x, xyPoint.y));
                Debug.Log(string.Format("Home Distance: {0}", hex.HomeInfo.Distance));
                Debug.Log(string.Format("Food Distance: {0}", hex.FoodInfo.Distance));
                Debug.Log(string.Format("Food Scent: {0}: {1}", hex.FoodScent.Strength, hex.FoodScent.State));
                HighlightHex(hex.Coordinates, HighlightColor);
            }
        }
    }

    /// <summary>
    /// Takes any x y (must be relative to grid parent) and returns the coordinates of the hex they would be on
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Vector2Int ConvertXYToCoordinates(float x, float y)
    {
        int row = (int)Math.Round(y / YSpacing) * -1;
        if (row % 2 == 1) x -= (XSpacing / 2);
        return new Vector2Int((int)Math.Round(x / XSpacing), row);

    }

    public void InitializeHexGrid()
    {
        // Reset values and destroy old grid
        if (CurrentHexGrid != null) CurrentHexGrid.ForEach(o => Destroy(o));

        CurrentHexGrid = new List<GameObject>();
        HexDataDictionary = new Dictionary<Vector2Int, HexInfo>();
        _scentedHexes = new List<HexInfo>();

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

    public void ClearHex(Vector2Int coords)
    {
        var hex = Master.MasterHex.GetHexAt(coords).GetComponent<Hex>();
        if (hex.NeverPathable)
        {
            hex.NeverPathable = false;
            hex.ChangeColor(DefaultColor);
        }
    }

    public void PropagateScents()
    {
        // first push new scents from stacks
        var stacks = HexDataDictionary.Where(x => x.Value.HasFoodStack).Select(x => x.Value);

        foreach (var hex in stacks)
        {
            if (hex.FoodScent.Strength == 0)
            {
                _scentedHexes.Add(hex);
            }
            hex.FoodScent.PushScent("food", Master.MasterFood.StackScentStrength);
        }

        // then push new scents from pellets
        var pellets = HexDataDictionary.Where(x => x.Value.HasPellet).Select(x => x.Value);

        foreach (var hex in pellets)
        {
            if (hex.FoodScent.Strength == 0)
            {
                _scentedHexes.Add(hex);
            }
            hex.FoodScent.PushScent("food", Master.MasterFood.PelletScentStrength);
        }

        // next spread scents around the map
        var scentedHex = _scentedHexes.FirstOrDefault(x => x.HasActiveScent);

        while (scentedHex != null)
        {
            var scent = scentedHex.FoodScent;

            var adjHexes = GetNeighboringHexInfo(scentedHex.Coordinates.x, scentedHex.Coordinates.y);

            if (scent.Strength > 1)
            {
                foreach (var adjHex in adjHexes)
                {
                    if (adjHex.FoodScent.Strength == 0)
                    {
                        _scentedHexes.Add(adjHex);
                    }
                    adjHex.FoodScent.PushScent(scent.Name, scent.Strength - 1);
                }
            }

            if (scent.State == ScentState.Fading)
            {
                scent.Strength--;
            }

            if (scent.Strength == 0)
            {
                scentedHex.FoodScent.Name = null;
                _scentedHexes.Remove(scentedHex);
            }

            scent.State = ScentState.Holding;

            scentedHex = _scentedHexes.FirstOrDefault(x => x.HasActiveScent);
        }

        // set all scents to fading
        foreach (var hex in _scentedHexes)
        {
            hex.FoodScent.State = ScentState.Fading;
        }
    }

    public void HighlightScents()
    {
        // show me the scents!
        var allScents = HexDataDictionary.Values.ToList();
        foreach (var s in allScents)
        {
            var hex = GetHexAt(s.Coordinates).GetComponent<Hex>();
            if (hex.NeverPathable)
                continue;
            var scentColor = hex.NeverPathable ? UnpathableColor : DefaultColor;

            switch (s.FoodScent.Strength)
            {
                case 10:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0F);
                    break;
                case 9:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.111F);
                    break;
                case 8:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.222F);
                    break;
                case 7:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.333F);
                    break;
                case 6:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.444F);
                    break;
                case 5:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.555F);
                    break;
                case 4:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.666F);
                    break;
                case 3:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.777F);
                    break;
                case 2:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 0.888F);
                    break;
                case 1:
                    scentColor = Color.Lerp(Color.red, Color.yellow, 1F);
                    break;
            }

            GameObject o = GetHexAt(s.Coordinates);
            o.GetComponent<Hex>().ChangeColor(scentColor);
        }
    }

    public void RecolorAll()
    {
        var allScents = HexDataDictionary.Values.ToList();
        foreach (var s in allScents)
        {
            var hex = GetHexAt(s.Coordinates).GetComponent<Hex>();
            var color = hex.NeverPathable ? UnpathableColor : DefaultColor;

            GameObject o = GetHexAt(s.Coordinates);
            o.GetComponent<Hex>().ChangeColor(color);
        }
    }

    public void HighlightHex(Vector2Int coords, Color color)
    {
        GameObject o = GetHexAt(coords);
        o.GetComponent<Hex>().ChangeColor(color);
    }

    public void ScentHex()
    {
        string xs = TextX.GetComponentInChildren<UnityEngine.UI.Text>().text;
        string ys = TextY.GetComponentInChildren<UnityEngine.UI.Text>().text;
        
        int x = System.Convert.ToInt32(xs);
        int y = System.Convert.ToInt32(ys);

        var coords = new Vector2Int(x, y);

        Master.MasterFood.CreateFoodStack(coords, 8);
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
        // bounds check
        if (coords.x < 0 || coords.x >= GridWidth)
            return null;
        if (coords.y < 0 || coords.y >= GridHeight)
            return null;

        HexInfo info;

        if (HexDataDictionary.TryGetValue(coords, out info))
        {
            return info;
        }

        var hex = GetHexAt(coords);

        if (hex == null)
            return null;

        var hexInfo = new HexInfo(coords) {IsRock = hex.GetComponent<Hex>().NeverPathable};

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
        return GetNeighboringHexCoordinates(x, y, 1, pathableOnly).Select(GetHexInfoAt).Where(z => z != null).ToList();
    }

    public List<HexInfo> GetNeighborHexInfoRadius2(int x, int y, bool pathableOnly = false)
    {
        return GetNeighboringHexCoordinates(x, y, 2, pathableOnly).Select(GetHexInfoAt).ToList();
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

    public Vector2Int GetRandomPoint(int padding)
    {
        var randPoint = new Vector2Int(UnityEngine.Random.Range(padding, GridWidth - padding),
            UnityEngine.Random.Range(padding, GridHeight - padding));

        return randPoint;
    }
}
