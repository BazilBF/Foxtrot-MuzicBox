using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayTroughLevelGoal : LevelGoal
{

    private float _playerHealth = 100;

    public PlayTroughLevelGoal(Player inPlayerInfo, GameData.RythmLevel inRythmLevel) : base(inPlayerInfo, inRythmLevel)
    {
        
    }
}
