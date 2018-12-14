using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hex : MonoBehaviour {

    [SerializeField] public bool NeverPathable;
    [SerializeField] public int Row;
    [SerializeField] public int Column;

    public void InitializeHex(Color backColor, int row, int column)
    {
        GetComponent<SpriteRenderer>().color = backColor;
        Row = row;
        Column = column;
        gameObject.name = "Hex " + row + "-" + column;
        NeverPathable = false;
    }

    public void ChangeColor(Color backColor)
    {
        GetComponent<SpriteRenderer>().color = backColor;
    }
}
