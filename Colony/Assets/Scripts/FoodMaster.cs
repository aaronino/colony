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
    public int MinDistance = 25;
    private int _stackScentStrength = 8;
    
    public int _pelletScentStrength = 3;

    public int StackScentStrength
    {
        get { return _stackScentStrength;}
        set
        {
            if (value >= 0 && value <= 10)
            {
                _stackScentStrength = value;
            }
        }
    }

    public int PelletScentStrength
    {
        get { return _pelletScentStrength;}
        set
        {
            if (value >= 0 && value <= 10)
            {
                _pelletScentStrength = value;
            }
        }
    }

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
        spawnPoint.x -= 15;
        spawnPoint.y -= 15;
        if (spawnPoint.x < 0) spawnPoint.x = 0;
        if (spawnPoint.y < 0) spawnPoint.y = 0;
        CreateFoodStack(spawnPoint, 15);
        spawnPoint.x += 18;
        spawnPoint.y += 18;
        CreateFoodStack(spawnPoint, 5);
    }

    public int GetTotalFood()
    {
        return CurrentFood.Count() + CurrentStacks.Sum(x => x.Value.Size);
    }

    public void SpawnFood()
    {
        
        var foodCount = GetTotalFood();

        if (foodCount < MinFood)
        {
            
            var spawnPoint = Master.MasterHex.GetRandomPoint(0);
            while (Vector2Int.Distance(spawnPoint, Master.MasterAnt.ColonyLocation) < MinDistance)
            {
                spawnPoint = Master.MasterHex.GetRandomPoint(0);
            }
            var spawnSize = UnityEngine.Random.Range(MinFood, MaxFood);
            CreateFoodStack(spawnPoint, spawnSize);
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
        GameObject o = Instantiate(FoodPelletTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);

        var foodHex = Master.MasterHex.GetHexInfoAt(coords);
        foodHex.HasPellet = true;

        CurrentFood.Add(coords,o);
        
        return o;
    }

    public GameObject CreateFoodStack(Vector2Int coords, int size)
    {
        GameObject o = Instantiate(FoodStackTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);

        var stackInfo = new FoodStackInfo(coords, size, o);

        CurrentStacks.Add(coords, stackInfo);

        var stackHex = Master.MasterHex.GetHexInfoAt(coords);

        stackHex.HasFoodStack = true;

        return o;
    }

}
