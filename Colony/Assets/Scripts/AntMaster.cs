using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Schema;
using DG.Tweening;

public class AntMaster : MonoBehaviour {

    [SerializeField] GameObject AntTemplate;
    [SerializeField] GameObject AntHillTemplate;
    [SerializeField] GameObject AntHillCenterTemplate;

    [SerializeField] Gameplay Master;

    [SerializeField] public int FoodStored;
    [SerializeField] public int Population;
    [SerializeField] int IdleOccupancy;
    [SerializeField] int RestingOccupany;

    public int MaxAntEnergy = 1000;

    public Vector2Int ColonyLocation;
    public bool RandomColonyLocation = true;
    public int ColonyRadius = 2;

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
        
        Master.MasterHex.ClearHex(hillHex);
        Instantiate(AntHillCenterTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);
        hillHex.IsColony = true;

        var radius = 0; // radius of zero would be a single hex colony

        var range = Master.MasterHex.GetNeighboringHexInfo(coords.x, coords.y);

        while (radius <= ColonyRadius)
        {
            radius++;

            foreach (var hex in range)
            {
                Master.MasterHex.ClearHex(hex);

                if (radius <= ColonyRadius)
                {
                    Instantiate(AntHillTemplate,
                        Master.MasterHex.CalculatePosition(hex.Coordinates.x, hex.Coordinates.y),
                        Quaternion.identity, Master.MasterHex.GridParent.transform);
                    hex.IsColony = true;
                }
            }

            var range2 = new List<HexInfo>();
            foreach (var hex in range)
            {
                range2.AddRange(Master.MasterHex.GetNeighboringHexInfo(hex.Coordinates.x, hex.Coordinates.y)
                    .Where(x => !x.IsColony));
            }

            range.Clear();
            foreach (var hex2 in range2)
            {
                if (!range.Contains(hex2))
                {
                    range.Add(hex2);
                }
            }
        }

        // colony entrance is every hex adjacent to a colony hex
        ColonyEntrance.AddRange(range);
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
        // birth new ants by spending surplus food
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

        foreach (var ant in CurrentAnts.Where(x => x.LastTurn != Master.GameTurn))
        {
            // even if an ant was unable to act they lose energy to hunger
            ant.Energy--;
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
        if (ant.IsHeld)
            return;

        var myHex = ant.Hex;
        var myLocation = myHex.Coordinates;
        var adjacentHexes = Master.MasterHex.GetNeighboringHexInfo(myLocation.x, myLocation.y, false);
        var preferHexes = adjacentHexes.Where(x => x.IsPathable).ToList();

        HexInfo moveTarget = null;

        // GENERAL
        // are we standing next to food?
        var foodHex = adjacentHexes.FirstOrDefault(x => x.HasPellet);
        if (foodHex != null)
        {
            myHex.FoodInfo.Distance = 1;
            myHex.FoodInfo.Coordinates = foodHex.Coordinates;
            myHex.FoodInfo.LastUpdated = Master.GameTurn;
        }
        else
        {
            var bestFoodHex = adjacentHexes.Where(x => x.FoodInfo.Distance > 0
                                                       && x.FoodInfo.Coordinates != myHex.Coordinates
                                                       && (myHex.FoodInfo.Distance == 0 || myHex.FoodInfo.Distance > x.FoodInfo.Distance) )
                .OrderBy(x => x.FoodInfo.Distance)
                .FirstOrDefault();

            myHex.FoodInfo.Distance = bestFoodHex == null ? 0 : bestFoodHex.FoodInfo.Distance + 1;
            myHex.FoodInfo.Coordinates = bestFoodHex == null ? myHex.Coordinates : bestFoodHex.Coordinates;
            myHex.FoodInfo.LastUpdated = Master.GameTurn;
        }

        // can we drop off food?
        if (ant.HasFood && adjacentHexes.Any(x => x.IsColony))
        {
            // made it home! store the food
            Destroy(ant.Food);
            ant.Food = null;
            ant.HasFood = false;
            FoodStored++;
        }

        // are we standing next to the colony?
        var homeHex = adjacentHexes.FirstOrDefault(x => x.IsColony);
        if (homeHex != null)
        {
            myHex.HomeInfo.Distance = 1;
            myHex.HomeInfo.Coordinates = homeHex.Coordinates;
            myHex.HomeInfo.LastUpdated = Master.GameTurn;
        }
        else
        {
            var bestHomeHex = adjacentHexes.Where(x => x.HomeInfo.Distance > 0
                                                       && x.HomeInfo.Coordinates != myHex.Coordinates
                                                       && (myHex.HomeInfo.Distance == 0 || myHex.HomeInfo.Distance > x.HomeInfo.Distance))
                .OrderBy(x => x.HomeInfo.Distance)
                .FirstOrDefault();

            myHex.HomeInfo.Distance = bestHomeHex == null ? 0 : bestHomeHex.HomeInfo.Distance + 1;
            myHex.HomeInfo.Coordinates = bestHomeHex == null ? myHex.Coordinates : bestHomeHex.Coordinates;
            myHex.HomeInfo.LastUpdated = Master.GameTurn;
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
                    // move anywhere!
                    MoveToAnyTarget(ant, adjacentHexes);
                }
            }
        }
    }

    private HexInfo FindFoodTarget(List<HexInfo> area)
    {
        if (!area.Any())
            return null;

        // prefer spaces with recent food trails
        var nearFood = area.Where(x => x.FoodInfo.Distance > 0)
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

        if (!target.IsPathable || !target.IsEmpty || ant.Hex == target 
            || Vector2Int.Distance(ant.Hex.Coordinates, target.Coordinates) >= 2)                     
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

        var nearTargets = ant.Hex.AdjacentSpaces.AdjacentTo(target.Coordinates);

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

        var farTargets = target.AdjacentSpaces.NonAdjacentTo(target.Coordinates);

        foreach (var space in farTargets)
        {
            var hex = Master.MasterHex.GetHexInfoAt(space);
            if (hex == null) continue;
            if (MoveToTarget(ant, hex))
                return true;
        }

        return false;
    }

    private bool MoveToAnyTarget(AntInfo ant, List<HexInfo> area)
    {
        var hex = area.Where(x => x.IsPathable && x.IsEmpty).RandomElement();

        return MoveToTarget(ant, hex);
    }

    public void MoveAnt(AntInfo ant, HexInfo hex)
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

        hex.LastTouched = Master.GameTurn;
    }
}
