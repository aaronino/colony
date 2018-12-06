using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AntMaster : MonoBehaviour {

    [SerializeField] GameObject AntTemplate;
    [SerializeField] Gameplay Master;
    public List<GameObject> CurrentAnts;

    public GameObject CreateAntAt(int x, int y)
    {
        GameObject o = Instantiate(AntTemplate, Master.MasterHex.CalculatePosition(x, y), Quaternion.identity, Master.MasterHex.GridParent.transform);
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
            var info = Master.MasterHex.GetNeighboringHexInfo(curX, curY).Where(o => !o.HasAnt).ToList();
//            Debug.Log("Trying to move ant at " + curX + ", " + curY + ", there are " + o.Count + " neighboring hexes.");
            if (info.Count == 0) {
                Debug.Log("No neighbors found");
            }
            else {
                var newXY = info[Random.Range(0, info.Count)];
 //               Debug.Log("We are moving to " + newXY.X + ", " + newXY.Y);
                MoveAnt(ant, System.Convert.ToInt32(newXY.X), System.Convert.ToInt32(newXY.Y));
            }
        }
    }

    public void MoveAnt(GameObject o, int x, int y) 
    {
        var ant = o.GetComponent<AntPosition>();
        o.transform.position = Master.MasterHex.CalculatePosition(x, y);
        ant.X = x;
        ant.Y = y;
    }

    public bool CheckForAnt(int x, int y)
    {
        return CurrentAnts.Where(o => (o.GetComponent<AntPosition>().X == x && o.GetComponent<AntPosition>().Y == y)).Count() > 0;
    }
}
