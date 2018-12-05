using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameplay : MonoBehaviour {

    [SerializeField] HexMaster HexMast;
    

	// Use this for initialization
	void Start () {
        HexMast.InitializeHexGrid();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
}
