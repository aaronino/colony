using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class AntMaster : MonoBehaviour {

    [SerializeField] GameObject AntTemplate;
    [SerializeField] GameObject AntHillTemplate;
    [SerializeField] Gameplay Master;

    [SerializeField] int FoodStored;
    [SerializeField] int Population;
    [SerializeField] int IdleOccupancy;
    [SerializeField] int RestingOccupany;
    [SerializeField] Vector2Int ColonyLocation;

    public GameObject AntHill;
    private List<HexInfo> ColonyEntrance;
    public List<AntInfo> CurrentAnts;
    public List<AntInfo> DespawnAnts;

    public GameObject CreateAntAt(Vector2Int coords)
    {
        int x = coords.x;
        int y = coords.y;

        GameObject o = Instantiate(AntTemplate, Master.MasterHex.CalculatePosition(x, y), Quaternion.identity, Master.MasterHex.GridParent.transform);
        AntInfo ant = o.GetComponent<AntInfo>();
        ant.InitializeAnt(o, ColonyLocation, Master.GameTurn);
        CurrentAnts.Add(ant);

        var antHex = Master.MasterHex.GetHexInfoAt(coords);
        MoveAnt(ant, antHex);
        return o;
    }

    public GameObject CreateAntHillAt(Vector2Int coords)
    {
        GameObject o = Instantiate(AntHillTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);

        var hillHex = Master.MasterHex.GetHexInfoAt(coords);

        hillHex.IsColony = true;

        return o;
    }

    public void InitializeAnts()
    {
        CurrentAnts = new List<AntInfo>();
        DespawnAnts = new List<AntInfo>();

        AntHill = CreateAntHillAt(ColonyLocation);
        ColonyEntrance = new List<HexInfo>();

        ColonyEntrance = Master.MasterHex.GetNeighboringHexInfo(ColonyLocation.x, ColonyLocation.y, true);
    }

    public void ColonyAct()
    {
        BirthAnts(); 

        SpawnAnts();

        RestAnts();
    }

    private void SpawnAnts()
    {
        // spawn ants
        while (IdleOccupancy > 0 && ColonyEntrance.Any(x => !x.HasAnt))
        {
            CreateAntAt(ColonyEntrance.First(x => !x.HasAnt).Coordinates);
            IdleOccupancy--;
        }
    }

    private void BirthAnts()
    {
        // birth new ants
        if (FoodStored > Population)
        {
            FoodStored--;
            Population++;
            IdleOccupancy++;
        }
    }

    private void RestAnts()
    {
        // rest ants by feeding them
        FoodStored -= RestingOccupany;
        if (FoodStored < 0)
        {
            // not enough food, kill starved ants
            RestingOccupany += FoodStored; 
            Population += FoodStored;
            FoodStored = 0;
        }

        IdleOccupancy += RestingOccupany;
        RestingOccupany = 0;
    }

    public void AllAntsAct()
    {
        int readyAnts = CurrentAnts.Count(x => x.LastTurn != Master.GameTurn);

        int prevReadyAnts = 0;

        while (readyAnts != prevReadyAnts)
        {
            prevReadyAnts = readyAnts;

            foreach (var ant in CurrentAnts.Where(x => x.LastTurn != Master.GameTurn))
            {
                AntAct(ant);
            }

            foreach (var ant in DespawnAnts)
            {
                Destroy(ant.Ant);
                CurrentAnts.Remove(ant);
                var hex = Master.MasterHex.GetHexInfoAt(ant.Location);
                hex.HasAnt = false;
            }

            readyAnts = CurrentAnts.Count(x => x.LastTurn != Master.GameTurn);
        }
    }

    public void KillExhaustedAnts()
    {
        List<AntInfo> exhaustedAnts = CurrentAnts.Where(x => x.Energy <= 0).ToList();

        foreach (var ant in exhaustedAnts)
        {
            Destroy(ant.Ant);
            CurrentAnts.Remove(ant);
            var hex = Master.MasterHex.GetHexInfoAt(ant.Location);
            hex.HasAnt = false;
            Population--;
        }
    }

    private void AntAct(AntInfo ant)
    {
        var adjacentHexes = Master.MasterHex.GetNeighboringHexInfo(ant.Location.x, ant.Location.y, true);

        // DEFEND
        
        // REST
        if (ant.Energy <= ant.MaxEnergy / 2)
        {
            if (adjacentHexes.Any(x => x.IsColony))
            {
                // made it home! de-spawn
                DespawnAnts.Add(ant);
                RestingOccupany++;
            }
            else
            {
                // move towards the colony (this action does not spend energy)
                var nearColony = adjacentHexes.Where(x => x.IsEmpty && x.NearColony.Distance > 0).OrderBy(x => x.NearColony.Distance).FirstOrDefault();
                if (nearColony != null)
                {
                    MoveAnt(ant, nearColony);
                    ant.LastTurn = Master.GameTurn;
                }
            }

            return; // take no further action if rest is priority
        }

        // ATTACK


        // GATHER


        // EXPLORE
        // move randomly for now
        var target = adjacentHexes.Where(x => x.IsEmpty).RandomElement();
        if (target != null)
        {
            MoveAnt(ant, target);
            ant.LastTurn = Master.GameTurn;
            ant.Energy--;
        }






        

    }

    public void MoveAnt(AntInfo ant, HexInfo newLocation)
    {
        var oldLocation = Master.MasterHex.GetHexInfoAt(ant.Location);

        oldLocation.HasAnt = false;
        newLocation.HasAnt = true;
        
        ant.Ant.transform.DOMove(Master.MasterHex.CalculatePosition(newLocation.Coordinates.x, newLocation.Coordinates.y), Master.CoreGameLoopFrequency).SetEase(Ease.Linear);
        // ant.Ant.transform.position = Master.MasterHex.CalculatePosition(newLocation.Coordinates.x, newLocation.Coordinates.y);
        ant.Location = newLocation.Coordinates;

        // chem trails
        ant.NearColony.Location = oldLocation.Coordinates;
        ant.NearColony.Distance++;

        if (newLocation.NearColony.Distance == 0 || newLocation.NearColony.Distance > ant.NearColony.Distance)
        {
            newLocation.NearColony.Location = oldLocation.Coordinates;
            newLocation.NearColony.Distance = ant.NearColony.Distance;
        }
    }

    public bool CheckForAnt(Vector2Int coords)
    {
        return CurrentAnts.Any(x => x.Location == coords);
    }
}
