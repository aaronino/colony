using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodMaster : MonoBehaviour {

    [SerializeField] GameObject FoodPelletTemplate;
    [SerializeField] GameObject FoodStackTemplate;

    [SerializeField] Gameplay Master;

    public int MaxFood = 1000;
    public int MinFood = 50;
    public int MinStacks = 3;
    public int MinDistance = 3;
    public int StackScentStrength = 8;
    public int PelletScentStrength = 3;

    internal Dictionary<Vector2Int, GameObject> CurrentFood;
    internal Dictionary<Vector2Int, FoodStackInfo> CurrentStacks;
    private List<FoodStackInfo> DespawnStacks;
    private short NextColorIndex;

    public void InitializeFood()
    {
        CurrentFood = new Dictionary<Vector2Int, GameObject>();
        CurrentStacks = new Dictionary<Vector2Int, FoodStackInfo>();
        DespawnStacks = new List<FoodStackInfo>();

        // spawn initial food near colony
        var spawnPoint = Master.MasterAnt.ColonyLocation;
        spawnPoint.x -= (Master.MasterAnt.ColonyRadius + MinDistance);
        spawnPoint.y -= (Master.MasterAnt.ColonyRadius + MinDistance);
        spawnPoint.Clamp(Master.MasterHex.MinPosition, Master.MasterHex.MaxPosition);

        CreateFoodStack(spawnPoint, MinFood / 2);

        spawnPoint.x = Master.MasterAnt.ColonyLocation.x + (Master.MasterAnt.ColonyRadius + MinDistance);
        spawnPoint.y = Master.MasterAnt.ColonyLocation.y + (Master.MasterAnt.ColonyRadius + MinDistance);
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
                CreateFoodPellet(hex.Coordinates, stack.Value.ColorIndex);
                stack.Value.Size--;

                if (stack.Value.Size == 0)
                {
                    DespawnStacks.Add(stack.Value);
                    CreateFoodPellet(stackCoords, stack.Value.ColorIndex);
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

    public GameObject CreateFoodPellet(Vector2Int coords, short colorIndex)
    {
        var foodHex = Master.MasterHex.GetHexInfoAt(coords);

        if (foodHex == null)
            return null;

        GameObject o = Instantiate(FoodPelletTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);

        var rend = o.GetComponent<SpriteRenderer>();
        switch (colorIndex)
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
                rend.color = Color.white;
                break;
        }

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

            var rend = o.GetComponent<SpriteRenderer>();
            switch (NextColorIndex)
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
                    rend.color = Color.white;
                    break;
            }

            var stackInfo = new FoodStackInfo(coords, size, NextColorIndex, o);

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
            NextColorIndex++;

            if (NextColorIndex > 4)
            {
                NextColorIndex = 0;
            }
        }
    }

}
