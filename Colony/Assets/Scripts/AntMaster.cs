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

    public int MaxAntEnergy = 1000;

    public Vector2Int ColonyLocation;
    public bool RandomColonyLocation = true;

    private List<HexInfo> ColonyEntrance;
    public List<AntInfo> CurrentAnts;
    public List<AntInfo> DespawnAnts;

    public GameObject CreateAntAt(Vector2Int coords)
    {
        int x = coords.x;
        int y = coords.y;

        GameObject o = Instantiate(AntTemplate, Master.MasterHex.CalculatePosition(x, y), Quaternion.identity, Master.MasterHex.GridParent.transform);
        AntInfo ant = o.GetComponent<AntInfo>();

        HexInfo hex = Master.MasterHex.GetHexInfoAt(coords);
        ant.InitializeAnt(o, hex, Master.GameTurn, MaxAntEnergy);
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

        if (RandomColonyLocation)
            ColonyLocation = Master.MasterHex.GetRandomPoint(5);

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
            CreateAntAt(ColonyEntrance.Where(x => x.IsPathable && x.IsEmpty).RandomElement().Coordinates);
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
                var hex = ant.Hex;
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
            var hex = ant.Hex;
            hex.HasAnt = false;
            if (ant.HasFood)
            {
                hex.HasPellet = true;
                Master.MasterFood.CurrentFood.Add(hex.Coordinates, ant.Food);
            }
            Population--;
        }
    }

    private void AntAct(AntInfo ant)
    {
        var myHex = ant.Hex;
        var myLocation = myHex.Coordinates;
        var adjacentHexes = Master.MasterHex.GetNeighboringHexInfo(myLocation.x, myLocation.y, false);
        var preferHexes = adjacentHexes.Where(x => x.IsPathable && x.Coordinates != ant.LastLocation).ToList();

        HexInfo moveTarget = null;

        // GENERAL
        // are we standing next to food?
        var foodHex = adjacentHexes.FirstOrDefault(x => x.HasPellet);
        if (foodHex != null)
        {
            ant.KnownFood.Distance = 1;
            ant.KnownFood.Location = foodHex.Coordinates;
            myHex.FoodInfo.Distance = 1;
            myHex.FoodInfo.Location = foodHex.Coordinates;
        }

        // can we drop off food?
        if (ant.HasFood && adjacentHexes.Any(x => x.IsColony))
        {
            // made it home! store the food
            Destroy(ant.Food);
            ant.Food = null;
            ant.HasFood = false;
            FoodStored++;
            ant.KnownFood.Distance = 0;
        }

        // EAT
        if (ant.AllowedEat && ant.IsHungry && FoodStored > 0)
        {
            if (adjacentHexes.Any(x => x.IsColony))
            {
                // made it home! de-spawn
                DespawnAnts.Add(ant);
                RestingOccupany++;
                return; // this ant is done
            }
            moveTarget = FindHomeTarget(preferHexes);
        }
        

        // GATHER
        if (ant.AllowedGather && moveTarget == null)
        {
            // can we pickup food?
            if (!ant.HasFood && foodHex != null)
            {
                // Pickup the food
                var food = Master.MasterFood.CurrentFood[foodHex.Coordinates];
                Master.MasterFood.CurrentFood.Remove(foodHex.Coordinates);

                ant.Food = food;
                ant.HasFood = true;
                foodHex.HasPellet = false;

                ant.Food.transform.DOMove(Master.MasterHex.CalculatePosition(ant.Hex.Coordinates.x, ant.Hex.Coordinates.y), 0F)
                    .SetEase(Ease.Linear);
            }
            
            // are we carrying food?
            if (ant.HasFood)
            {
                moveTarget = FindHomeTarget(preferHexes);
            }

            // are we looking for food?
            if (!ant.HasFood)
            {
                moveTarget = FindFoodTarget(preferHexes);

                if (moveTarget != null && myHex.FoodInfo.Distance > 0 && moveTarget.FoodInfo.Distance > 0
                && moveTarget.FoodInfo.Distance > myHex.FoodInfo.Distance)
                {
                    moveTarget = null; // prefer exploration instead of backtracking for food
                }
            }
        }
        

        // EXPLORE
        if (ant.AllowedExplore && moveTarget == null)
        {
            moveTarget = FindSearchTarget(preferHexes);
        }


        // ANARCHY
        if (moveTarget == null)
        {
            moveTarget = preferHexes.RandomElement();
        }

        // attempt to move to target
        if (!MoveToTarget(ant, moveTarget))
        {
            // attempt to move near target
            if (!MoveToNearTarget(ant, moveTarget))
            {
                // move far from target
                if (!MoveToFarTarget(ant, moveTarget))
                {
                    // last choice: move back
                    MoveToLastTarget(ant);
                }
            }
        }
    }

    private HexInfo FindFoodTarget(List<HexInfo> area)
    {
        if (!area.Any())
            return null;

        // prefer spaces with recent food trails
        var nearFood = area.Where(x => x.FoodInfo.Distance > 0 && Master.GameTurn - x.FoodInfo.LastUpdated < 50)
            .OrderBy(x => x.FoodInfo.Distance).FirstOrDefault();

        if (nearFood != null)
        {
            // found a space with a food trail
            return nearFood;
        }

        // next check spaces with food scent
        var nearScent = area.Where(x => x.FoodScent.Strength > 0).OrderByDescending(x => x.FoodScent.Strength).FirstOrDefault();
        
        return nearScent;
    }

    private HexInfo FindHomeTarget(List<HexInfo> area)
    {
        if (!area.Any())
            return null;

        // prefer spaces with home trail
        var nearHome = area.Where(x => x.HomeInfo.Distance > 0).OrderBy(x => x.HomeInfo.Distance).FirstOrDefault();
        
        return nearHome;
    }

    private HexInfo FindSearchTarget(List<HexInfo> area)
    {
        if (!area.Any())
            return null;

        // prefer unexplored scented spaces
        var unexploredScented = area.Where(x => x.HomeInfo.Distance == 0 && x.FoodScent.Strength > 0).RandomElement();

        if (unexploredScented != null)
            return unexploredScented;

        // next check for unexplored spaces
        var unexplored = area.Where(x => x.HomeInfo.Distance == 0).RandomElement();

        if (unexplored != null)
            return unexplored;

        // finally just search the oldest explored space
        var oldest = area.OrderBy(x => x.LastTouched).First();

        return oldest;
    }

    private bool MoveToTarget(AntInfo ant, HexInfo target)
    {
        if (target == null)
            return false;

        if (!target.IsPathable || !target.IsEmpty || ant.Hex == target)
        {
            return false;
        }

        MoveAnt(ant, target);

        return true;
    }

    private bool MoveToNearTarget(AntInfo ant, HexInfo target)
    {
        if (target == null)
            return false;

        var nearTargets = ant.Hex.AdjacentSpaces.AdjacentTo(target.Coordinates).Where(x => x != ant.LastLocation);

        foreach (var space in nearTargets)
        {
            var hex = Master.MasterHex.GetHexInfoAt(space);
            if (hex == null) continue;
            if (MoveToTarget(ant, hex))
                return true;
        }

        return false;
    }

    private bool MoveToFarTarget(AntInfo ant, HexInfo target)
    {
        if (target == null)
            return false; 

        var farTargets = target.AdjacentSpaces.NonAdjacentTo(target.Coordinates).Where(x => x != ant.LastLocation);

        foreach (var space in farTargets)
        {
            var hex = Master.MasterHex.GetHexInfoAt(space);
            if (hex == null) continue;
            if (MoveToTarget(ant, hex))
                return true;
        }

        return false;
    }

    private bool MoveToLastTarget(AntInfo ant)
    {
        var lastTarget = ant.LastLocation;

        if (lastTarget == ant.Hex.Coordinates)
            return false;
        
        var hex = Master.MasterHex.GetHexInfoAt(lastTarget);
        if (hex == null) return false;

        if (MoveToTarget(ant, hex))
            return true;

        return false;
    }

    private void MoveAnt(AntInfo ant, HexInfo hex)
    {
        var oldHex = ant.Hex;

        oldHex.HasAnt = false;
        hex.HasAnt = true;
        ant.Hex = hex;
        ant.LastLocation = oldHex.Coordinates;

        // move ant
        ant.Ant.transform.DOMove(Master.MasterHex.CalculatePosition(hex.Coordinates.x, hex.Coordinates.y), Master.CoreGameLoopFrequency).SetEase(Ease.Linear);
        
        // move food if ant is carrying
        if (ant.Food != null)
        {
            ant.Food.transform.DOMove(Master.MasterHex.CalculatePosition(hex.Coordinates.x, hex.Coordinates.y), Master.CoreGameLoopFrequency).SetEase(Ease.Linear);
        }

        // uncomment this if you want to see where the ants have been
        //Master.MasterHex.HighlightHex(hex.Coordinates, Color.green);

        ant.Energy--; // spend energy
        ant.LastTurn = Master.GameTurn;

        // ant knowledge

        ant.KnownHome.Location = oldHex.Coordinates;
        ant.KnownHome.Distance++;

        if (ant.KnownFood.Distance > 0)
        {
            ant.KnownFood.Location = oldHex.Coordinates;
            ant.KnownFood.Distance++;
        }

        // chem trails
        if (hex.HomeInfo.Distance == 0 || hex.HomeInfo.Distance > ant.KnownHome.Distance)
        {
            hex.HomeInfo.Location = oldHex.Coordinates;
            hex.HomeInfo.Distance = ant.KnownHome.Distance;
            hex.HomeInfo.LastUpdated = Master.GameTurn;
        }

        if (hex.FoodInfo.Distance == 0 || hex.FoodInfo.Distance > ant.KnownFood.Distance)
        {
            hex.FoodInfo.Location = oldHex.Coordinates;
            hex.FoodInfo.Distance = ant.KnownFood.Distance;
            hex.FoodInfo.LastUpdated = Master.GameTurn;
        }

        hex.LastTouched = Master.GameTurn;
    }
}
