using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntPosition : MonoBehaviour {

    [SerializeField] public int X;
    [SerializeField] public int Y;

    public void InitializeAnt(int x, int y)
    {
        X = x;
        Y = y;
    }


}
