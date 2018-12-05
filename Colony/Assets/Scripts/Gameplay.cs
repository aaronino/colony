using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour {

    [SerializeField] HexMaster MasterHex;
    

	// Use this for initialization
	void Start () {
        MasterHex.InitializeHexGrid();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
}
