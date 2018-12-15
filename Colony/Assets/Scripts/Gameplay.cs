using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using TMPro;

public class Gameplay : MonoBehaviour {

    [SerializeField] public HexMaster MasterHex;
    [SerializeField] public AntMaster MasterAnt;
    [SerializeField] public FoodMaster MasterFood;
    [SerializeField] public float CoreGameLoopFrequency;
    [SerializeField] public Camera MainCamera;
    [SerializeField] public TextMeshProUGUI TurnText;
    [SerializeField] public TextMeshProUGUI PopulationText;
    [SerializeField] public TextMeshProUGUI FoodStoredText;

    public long GameTurn;

	// Use this for initialization
	void Start () {
        MasterHex.InitializeHexGrid();
        MasterAnt.InitializeAnts();
	    MasterFood.InitializeFood();
        TurnText.text = "Turn " + GameTurn;
	    //StartLoop(CoreGameLoopFrequency);
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
        GameTurn++;
        
        // spawn food
        MasterFood.SpawnFood();

        // propagate scents
        MasterHex.PropagateScents();

        // perform ant actions
        MasterAnt.AllAntsAct();

        MasterAnt.KillExhaustedAnts();

        // perform colony actions
        MasterAnt.ColonyAct();

        //MasterHex.HighlightScents();

        TurnText.text = "Turn: " + GameTurn;
        PopulationText.text = "Ants: " + MasterAnt.CurrentAnts.Count;
        FoodStoredText.text = "Food: " + MasterAnt.FoodStored;
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

    public void Next()
    {
        CoreGameLoop();
    }

    // Update is called once per frame
    void Update ()
    {
    }
}
