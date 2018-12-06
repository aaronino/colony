using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour {

    [SerializeField] HexMaster HexMast;
    [SerializeField] AntMaster AntMast;
    [SerializeField] float CoreGameLoopFrequency;

	// Use this for initialization
	void Start () {
        HexMast.InitializeHexGrid();
        AntMast.InitializeAnts();
        InvokeRepeating("CoreGameLoop", 0f, CoreGameLoopFrequency);
	}
	
    /// <summary>
    /// This is the primary place for action to take place
    /// </summary>
    void CoreGameLoop()
    {
        AntMast.MoveAllAnts();
    }

	// Update is called once per frame
	void Update () {
		
	}
    
}
