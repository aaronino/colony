using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntInfo : MonoBehaviour {

    public Vector2Int Location;
    public long LastTurn;
    public GameObject Ant;
    public GameObject Food;
    public int Energy;
    public int MaxEnergy;
    public bool HasFood;

    public ChemInfo NearFood;
    public ChemInfo NearColony;

    public void InitializeAnt(GameObject ant, Vector2Int coords, long turn)
    {
        Ant = ant;
        Location = coords;
        LastTurn = turn;
        MaxEnergy = 100;
        Energy = 100;
        NearColony = new ChemInfo() {Distance = 1, Location = coords };
        NearFood = new ChemInfo() {Distance = 0, Location = coords};
    }
}
