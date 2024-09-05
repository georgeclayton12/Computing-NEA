using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Vector3 PlayerPosition;


    public GameData() 
    {

        PlayerPosition = Vector3.zero;//value if no data is found 
    }
}
