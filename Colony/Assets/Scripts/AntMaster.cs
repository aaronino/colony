using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class AntMaster : MonoBehaviour {

    [SerializeField] GameObject AntTemplate;
    [SerializeField] GameObject AntHillTemplate;
    [SerializeField] GameObject AntHillCenterTemplate;

    [SerializeField] Gameplay Master;

    [SerializeField] public int FoodStored;
    [SerializeField] int Population;
    [SerializeField] int IdleOccupancy;
    [SerializeField] int RestingOccupany;
    [SerializeField] Vector2Int ColonyLocation;

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

    public void CreateAntHillAt(Vector2Int coords)
    {
        var hillHex = Master.MasterHex.GetHexInfoAt(coords);
        
        Master.MasterHex.ClearHex(coords);
        Instantiate(AntHillCenterTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);
        hillHex.IsColony = true;

        var hillAdj = Master.MasterHex.GetNeighboringHexInfo(coords.x, coords.y, false);

        foreach (var hex in hillAdj)
        {
            Master.MasterHex.ClearHex(hex.Coordinates);
            Instantiate(AntHillTemplate, Master.MasterHex.CalculatePosition(hex.Coordinates.x, hex.Coordinates.y), Quaternion.identity, Master.MasterHex.GridParent.transform);
            hex.IsColony = true;
        }

        foreach (var hex in hillAdj)
        {
            ColonyEntrance.AddRange(Master.MasterHex.GetNeighboringHexInfo(hex.Coordinates.x, hex.Coordinates.y, true).Where(x => !x.IsColony));
        }
    }

    

    public void InitializeAnts()
    {
        CurrentAnts = new List<AntInfo>();
        DespawnAnts = new List<AntInfo>();
        ColonyEntrance = new List<HexInfo>();

        CreateAntHillAt(ColonyLocation);
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
            DespawnAnts.Clear();

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
        var emptyHexes = adjacentHexes.Where(x => x.IsEmpty);

        // EAT
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

        // GATHER
        if ( !ant.HasFood && adjacentHexes.Any(x => x.HasFood))
        {
            // Pickup the food
            var foodHex = adjacentHexes.First(x => x.HasFood);
            var food = Master.MasterFood.CurrentFood[foodHex.Coordinates];
            Master.MasterFood.CurrentFood.Remove(foodHex.Coordinates);

            ant.Food = food;
            ant.HasFood = true;
            ant.NearFood.Distance = 1;
            ant.NearFood.Location = foodHex.Coordinates;

            foodHex.HasFood = false;

            ant.Food.transform.DOMove(Master.MasterHex.CalculatePosition(ant.Location.x, ant.Location.y), 0F).SetEase(Ease.Linear);
        }

        if (ant.HasFood)
        {
            if (adjacentHexes.Any(x => x.IsColony))
            {
                // made it home! store the food
                Destroy(ant.Food);
                ant.Food = null;
                ant.HasFood = false;
                FoodStored++;
            }
            else
            {
                // move towards the colony
                var nearColony = adjacentHexes.Where(x => x.IsEmpty && x.NearColony.Distance > 0).OrderBy(x => x.NearColony.Distance).FirstOrDefault();
                if (nearColony != null)
                {
                    MoveAnt(ant, nearColony);
                    ant.LastTurn = Master.GameTurn;
                }
                else
                {
                    var nextBestHex = adjacentHexes.Where(x => x.IsEmpty).RandomElement();

                    if (nextBestHex != null)
                    {
                        MoveAnt(ant, nextBestHex);
                        ant.LastTurn = Master.GameTurn;
                    }
                }

                return;
            }
        }
        
        if (adjacentHexes.Any(x => x.NearFood.Distance > 0))
        {
            var nearFood = adjacentHexes.Where(x => x.IsEmpty && x.NearFood.Distance > 0).OrderBy(x => x.NearFood.Distance).FirstOrDefault();
            if (nearFood != null)
            {
                MoveAnt(ant, nearFood);
                ant.LastTurn = Master.GameTurn;

                return;
            }
        }
        if (adjacentHexes.Any(x => x.GetFoodScent() > 0))
        {
            var scentFood = adjacentHexes.Where(x => x.IsEmpty).OrderByDescending(x => x.GetFoodScent()).FirstOrDefault();
            if (scentFood != null)
            {
                MoveAnt(ant, scentFood);
                ant.LastTurn = Master.GameTurn;
                return;
            }
        }

        // EXPLORE

        if (emptyHexes.Any())
        {
            var targetHexes = emptyHexes.Unexplored();

            if (!targetHexes.Any())
            {
                targetHexes = emptyHexes.Scented();
            }

            if (!targetHexes.Any())
            {
                targetHexes = emptyHexes;
            }
            
            var target = targetHexes.RandomElement();
            if (target != null)
            {
                MoveAnt(ant, target);
                ant.LastTurn = Master.GameTurn;
                ant.Energy--;
            }
        }


    }

    public void MoveAnt(AntInfo ant, HexInfo newLocation)
    {
        var oldLocation = Master.MasterHex.GetHexInfoAt(ant.Location);

        oldLocation.HasAnt = false;
        newLocation.HasAnt = true;
        
        ant.Ant.transform.DOMove(Master.MasterHex.CalculatePosition(newLocation.Coordinates.x, newLocation.Coordinates.y), Master.CoreGameLoopFrequency).SetEase(Ease.Linear);
        if (ant.Food != null)
        {
            ant.Food.transform.DOMove(Master.MasterHex.CalculatePosition(newLocation.Coordinates.x, newLocation.Coordinates.y), Master.CoreGameLoopFrequency).SetEase(Ease.Linear);
        }

        ant.Location = newLocation.Coordinates;

        // ant knowledge

        ant.NearColony.Location = oldLocation.Coordinates;
        ant.NearColony.Distance++;

        if (ant.NearFood.Distance > 0)
        {
            ant.NearFood.Location = oldLocation.Coordinates;
            ant.NearFood.Distance++;
        }

        // chem trails

        if (newLocation.NearColony.Distance == 0 || newLocation.NearColony.Distance > ant.NearColony.Distance)
        {
            newLocation.NearColony.Location = oldLocation.Coordinates;
            newLocation.NearColony.Distance = ant.NearColony.Distance;
        }

        if (newLocation.NearFood.Distance == 0 || newLocation.NearFood.Distance > ant.NearFood.Distance)
        {
            newLocation.NearFood.Location = oldLocation.Coordinates;
            newLocation.NearFood.Distance = ant.NearFood.Distance;
        }
    }

    public bool CheckForAnt(Vector2Int coords)
    {
        return CurrentAnts.Any(x => x.Location == coords);
    }
}
