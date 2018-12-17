using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodMaster : MonoBehaviour {

    [SerializeField] GameObject FoodPelletTemplate;
    [SerializeField] GameObject FoodStackTemplate;

    [SerializeField] Gameplay Master;

    public int MaxFood = 125;
    public int MinFood = 20;
    public int MinStacks = 3;
    public int MinDistance = 25;
    public int StackScentStrength = 8;
    public int PelletScentStrength = 3;

    internal Dictionary<Vector2Int, GameObject> CurrentFood;
    internal Dictionary<Vector2Int, FoodStackInfo> CurrentStacks;
    private List<FoodStackInfo> DespawnStacks;

    public void InitializeFood()
    {
        CurrentFood = new Dictionary<Vector2Int, GameObject>();
        CurrentStacks = new Dictionary<Vector2Int, FoodStackInfo>();
        DespawnStacks = new List<FoodStackInfo>();

        // spawn initial food near colony
        var spawnPoint = Master.MasterAnt.ColonyLocation;
        spawnPoint.x -= Master.MasterAnt.ColonyRadius + 15;
        spawnPoint.y -= Master.MasterAnt.ColonyRadius + 15;
        spawnPoint.Clamp(Master.MasterHex.MinPosition, Master.MasterHex.MaxPosition);

        CreateFoodStack(spawnPoint, MinFood / 2);

        spawnPoint.x += Master.MasterAnt.ColonyRadius + 20;
        spawnPoint.y += Master.MasterAnt.ColonyRadius + 20;
        spawnPoint.Clamp(Master.MasterHex.MinPosition, Master.MasterHex.MaxPosition);

        CreateFoodStack(spawnPoint, MinFood / 2);
    }

    public int GetTotalFood()
    {
        return CurrentFood.Count() + CurrentStacks.Sum(x => x.Value.Size);
    }

    public void SpawnFood()
    {
        
        var foodCount = GetTotalFood();

        if (foodCount < MinFood || CurrentStacks.Count < MinStacks)
        {
            var spawnPoint = Master.MasterHex.GetRandomPoint(0);
            while (Vector2Int.Distance(spawnPoint, Master.MasterAnt.ColonyLocation) < MinDistance)
            {
                spawnPoint = Master.MasterHex.GetRandomPoint(0);
            }
            var spawnSize = UnityEngine.Random.Range(MinFood, MaxFood);
            CreateFoodStack(spawnPoint, spawnSize > 6 ? spawnSize : 7 ); // no stack should be less than 7 size
        }

        // Create pellets around current stacks
        foreach (var stack in CurrentStacks)
        {
            var stackCoords = stack.Key;
            var adjacentHexes = Master.MasterHex.GetNeighboringHexInfo(stackCoords.x, stackCoords.y, true);

            var emptyHexes = adjacentHexes.Where(x => x.IsEmpty).ToList();

            foreach (var hex in emptyHexes)
            {
                CreateFoodPellet(hex.Coordinates);
                stack.Value.Size--;

                if (stack.Value.Size == 0)
                {
                    DespawnStacks.Add(stack.Value);
                    CreateFoodPellet(stackCoords);
                    break;
                }
            }
        }

        // Destroy empty stacks
        foreach (var stack in DespawnStacks)
        {
            var stackHex = Master.MasterHex.GetHexInfoAt(stack.Coordinates);
            stackHex.HasFoodStack = false;
            CurrentStacks.Remove(stack.Coordinates);
            Destroy(stack.Stack);
        }

        DespawnStacks.Clear();
    }

    public GameObject CreateFoodPellet(Vector2Int coords)
    {
        var foodHex = Master.MasterHex.GetHexInfoAt(coords);

        if (foodHex == null)
            return null;

        GameObject o = Instantiate(FoodPelletTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);
        foodHex.HasPellet = true;

        CurrentFood.Add(coords,o);
        
        return o;
    }

    public void CreateFoodStack(Vector2Int coords, int size)
    {
        var stackHex = Master.MasterHex.GetHexInfoAt(coords);

        if (stackHex == null)
            return;

        if (stackHex.HasFoodStack)
        {
            var stackInfo = CurrentStacks[coords];
            stackInfo.Size += size;
        }
        else
        {
            GameObject o = Instantiate(FoodStackTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);
            var stackInfo = new FoodStackInfo(coords, size, o);

            if (stackHex.HasPellet)
            {
                var food = Master.MasterFood.CurrentFood[coords];
                Master.MasterFood.CurrentFood.Remove(coords);
                stackHex.HasPellet = false;
                Destroy(food);
                stackInfo.Size++; // give back this one pellet
            }

            CurrentStacks.Add(coords, stackInfo);
            stackHex.HasFoodStack = true;
        }
    }

}
