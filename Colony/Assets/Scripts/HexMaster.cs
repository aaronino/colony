using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMaster : MonoBehaviour {

    [SerializeField] int GridWidth;
    [SerializeField] int GridHeight;
    [SerializeField] Vector3 TopLeftPosition;
    [SerializeField] float XSpacing;
    [SerializeField] float YSpacing;
    [SerializeField] GameObject HexTemplate;
    [SerializeField] GameObject GridParent;
    public List<GameObject> CurrentHexGrid;
    public int CurrentX;
    public int CurrentY;

    // temp color helpers
    [SerializeField] public List<Color> GridColors;
    

    public void InitializeHexGrid()
    {
        // Reset values and destroy old grid
        CurrentX = CurrentY = 0;
        if (CurrentHexGrid != null) CurrentHexGrid.ForEach(o => Destroy(o));

        CurrentHexGrid = new List<GameObject>();

        for (int y = 0; y < GridHeight; y++) {
            for (int x = 0; x < GridWidth; x++) {
                GameObject o = Instantiate(HexTemplate, CalculatePosition(x, y), Quaternion.identity, GridParent.transform);
                o.GetComponent<Hex>().InitializeHex(GetRandomColor(), y, x);
            }
        }
            
    }

    public Vector3 CalculatePosition(int x, int y)
    {
        // eve rows need + .5 x spacing
        float extraX = y % 2 == 1 ? XSpacing * .5f : 0;

        return new Vector3(
            TopLeftPosition.x + (x * XSpacing) + extraX,
            TopLeftPosition.y - (y * YSpacing)
        );

    }

    
    public Color GetRandomColor()
    {
        // For now semi-random coloring of grid cells (tie to other properties in the future
        return GridColors[Random.Range(0, 5)];

    }
}
