using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodMaster : MonoBehaviour {

    [SerializeField] GameObject FoodPelletTemplate;
    [SerializeField] GameObject FoodStackTemplate;

    [SerializeField] Gameplay Master;

    public Dictionary<Vector2Int, GameObject> CurrentFood;
    public Dictionary<Vector2Int, FoodStackInfo> CurrentStacks;
    public List<FoodStackInfo> DespawnStacks;

    public void InitializeFood()
    {
        CurrentFood = new Dictionary<Vector2Int, GameObject>();
        CurrentStacks = new Dictionary<Vector2Int, FoodStackInfo>();
        DespawnStacks = new List<FoodStackInfo>();
    }

    public void SpawnFood()
    {
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

        foreach (var stack in DespawnStacks)
        {
            CurrentStacks.Remove(stack.Coordinates);
            Destroy(stack.Stack);
        }

        DespawnStacks.Clear();
    }

    public GameObject CreateFoodPellet(Vector2Int coords)
    {
        GameObject o = Instantiate(FoodPelletTemplate, Master.MasterHex.CalculatePosition(coords.x, coords.y), Quaternion.identity, Master.MasterHex.GridParent.transform);

        var foodHex = Master.MasterHex.GetHexInfoAt(coords);
        foodHex.HasFood = true;

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

        //var pellets = Master.MasterHex.GetNeighboringHexInfo(coords.x, coords.y, true);

        //foreach (var hex in pellets)
        //{
        //    CreateFoodPellet(hex.Coordinates);
        //}

        return o;
    }

}
