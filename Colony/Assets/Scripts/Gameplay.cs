﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using TMPro;
using System;
public class Gameplay : MonoBehaviour {

    [SerializeField] public HexMaster MasterHex;
    [SerializeField] public AntMaster MasterAnt;
    [SerializeField] public FoodMaster MasterFood;
    [SerializeField] public float CoreGameLoopFrequency;
    [SerializeField] public Camera MainCamera;
    [SerializeField] public TextMeshProUGUI TurnText;
    [SerializeField] public TextMeshProUGUI PopulationText;
    [SerializeField] public TextMeshProUGUI FoodStoredText;
    public TextMeshProUGUI PauseButton;
    bool _paused;

    public long GameTurn;

	// Use this for initialization
	void Start () {
        MasterHex.InitializeHexGrid();
        MasterAnt.InitializeAnts();
	    MasterFood.InitializeFood();
        TurnText.text = "Turn " + GameTurn;
        _paused = true;
	    PauseOrPlay();
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
        if (MasterAnt.FoodStored == 0 && MasterAnt.Population == 0)
            return; // GAME OVER

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

        TurnText.text = ConvertToString(GameTurn);
        PopulationText.text = string.Format("Ants: {0}  High: {1}", MasterAnt.Population, MasterAnt.HighPopulation);
        FoodStoredText.text = string.Format("Food: {0}  Avail: {1}", MasterAnt.FoodStored, MasterFood.GetTotalFood());
    }

    public static string ConvertToString(long gameTurn) {
        // gameturn = total minutes
        // 1440 minutes per day
        // 
        long Day = Convert.ToInt32(Math.Floor(gameTurn / 1440m));
        long Hour = Convert.ToInt32(Math.Floor(gameTurn / 60m) - (Day * 24m));
        long Minute = gameTurn % 60;
        return "Day " + (Day + 1) + " " + Hour.ToString("00") + ":" + Minute.ToString("00");
    }

    public void SpeedUp()
    {
        StartLoop(CoreGameLoopFrequency - .1f);
    }

    public void SlowDown()
    {
        StartLoop(CoreGameLoopFrequency + .1f);
    }

    public void PauseOrPlay()
    {
        if (_paused) {
            StartLoop(CoreGameLoopFrequency);
            PauseButton.text = "Pause";
        }
        else {
            CancelInvoke();
            PauseButton.text = "Play";
        }
        _paused = !_paused;
    }

    public void Play()
    {
        
    }

    public void Next()
    {
        CoreGameLoop();
    }

    // Update is called once per frame
    void Update()
    {
        if (_paused && Input.GetKeyDown(KeyCode.Space))
        {
            Next();
        }
    }
}
