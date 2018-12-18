using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntInfo : MonoBehaviour
{

    public HexInfo Hex;
    public Vector2Int LastLocation;
    public long LastTurn;
    public GameObject Ant;
    public GameObject Food;
    public int Energy;
    public int MaxEnergy;
    public bool HasFood;
    public short ColorIndex;

    // behaviors
    public bool AllowedEat;
    public bool AllowedGather;
    public bool AllowedExplore;

    public void InitializeAnt(GameObject ant, HexInfo hex, long turn, int energy)
    {
        AllowedExplore = true;
        AllowedEat = true;
        AllowedGather = true;
        Ant = ant;
        Hex = hex;
        LastLocation = hex.Coordinates;
        LastTurn = turn;
        MaxEnergy = energy;
        Energy = energy;

        var rend = ant.GetComponent<SpriteRenderer>();
        switch (ColorIndex)
        {
            case 1:
                rend.color = Color.red;
                break;
            case 2:
                rend.color = Color.blue;
                break;
            case 3:
                rend.color = Color.magenta;
                break;
            case 4:
                rend.color = Color.green;
                break;
            default:
                rend.color = Color.black;
                break;
        }
    }

    public bool IsHungry
    {
        get { return Energy < MaxEnergy * .6; }
    }
}
