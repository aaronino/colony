using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntMaster : MonoBehaviour {

    [SerializeField] GameObject AntTemplate;
    [SerializeField] HexMaster MasterHex;
    public List<GameObject> CurrentAnts;

    public GameObject CreateAntAt(int x, int y)
    {
        GameObject o = Instantiate(AntTemplate, MasterHex.CalculatePosition(x, y), Quaternion.identity, MasterHex.GridParent.transform);
        o.GetComponent<AntPosition>().InitializeAnt(x, y);
        CurrentAnts.Add(o);
        return o;
    }

    public void InitializeAnts()
    {
        CurrentAnts = new List<GameObject>();
        CreateAntAt(5, 5);
        CreateAntAt(10, 10);

        CreateAntAt(15, 15);
        CreateAntAt(25, 25);
        CreateAntAt(35, 35);
        CreateAntAt(20, 20);
        CreateAntAt(20, 20);
    }

    public void MoveAllAnts()
    {
        foreach(GameObject ant in CurrentAnts) {
            int curX = ant.GetComponent<AntPosition>().X;
            int curY = ant.GetComponent<AntPosition>().Y;
            var o = MasterHex.GetNeighboringHexCoordinates(curX, curY);
            Debug.Log("Trying to move ant at " + curX + ", " + curY + ", there are " + o.Count + " neighboring hexes.");
            if (o.Count == 0) {
                Debug.Log("No neighbors found");
            }
            else {
                var newXY = o[Random.Range(0, o.Count - 1)];
                Debug.Log("We are moving to " + newXY.x + ", " + newXY.y);
                MoveAnt(ant, System.Convert.ToInt32(newXY.x), System.Convert.ToInt32(newXY.y));
            }
        }
    }

    public void MoveAnt(GameObject o, int x, int y) 
    {
        var ant = o.GetComponent<AntPosition>();
        o.transform.position = MasterHex.CalculatePosition(x, y);
        ant.X = x;
        ant.Y = y;
    }
}
