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
        StartLoop(CoreGameLoopFrequency);
        
	}

    public void StartLoop(float frequency)
    {
        CancelInvoke();
        if (frequency < .1f) frequency = .1f;
        CoreGameLoopFrequency = frequency;
        InvokeRepeating("CoreGameLoop", 0f, frequency);
    }
	
    /// <summary>
    /// This is the primary place for action to take place
    /// </summary>
    void CoreGameLoop()
    {
        MasterAnt.AllAntsAct();
    }

    public void SpeedUp()
    {
        StartLoop(CoreGameLoopFrequency - .1f);
    }

    public void SlowDown()
    {
        StartLoop(CoreGameLoopFrequency + .1f);
    }

    public void Pause()
    {
        CancelInvoke();
    }

    public void Play()
    {
        StartLoop(CoreGameLoopFrequency);
    }

	// Update is called once per frame
	void Update () {
		
	}
    
}
