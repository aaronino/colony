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

    // behaviors
    public bool AllowedEat;
    public bool AllowedGather;
    public bool AllowedExplore;
    public bool IsHeld;

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
    }

    public bool IsHungry
    {
        get { return Energy < MaxEnergy * .6; }
    }
}
