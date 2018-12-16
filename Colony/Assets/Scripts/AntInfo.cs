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

    public ChemInfo KnownFood;
    public ChemInfo KnownHome;

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
        KnownHome = new ChemInfo() {Distance = 1, Location = hex.Coordinates };
        KnownFood = new ChemInfo() {Distance = 0, Location = hex.Coordinates};
    }

    public bool IsHungry
    {
        get { return Energy < MaxEnergy * .6; }
    }
}
