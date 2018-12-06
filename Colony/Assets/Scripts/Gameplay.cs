using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour {

    [SerializeField] public HexMaster MasterHex;
    [SerializeField] public AntMaster MasterAnt;
    [SerializeField] float CoreGameLoopFrequency;

	// Use this for initialization
	void Start () {
        MasterHex.InitializeHexGrid();
        MasterAnt.InitializeAnts();
        InvokeRepeating("CoreGameLoop", 0f, CoreGameLoopFrequency);
	}
	
    /// <summary>
    /// This is the primary place for action to take place
    /// </summary>
    void CoreGameLoop()
    {
        MasterAnt.MoveAllAnts();
    }

	// Update is called once per frame
	void Update () {
		
	}
    
}
