using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayThroughLevelGoal : LevelGoal
{

    
    private float _playerCurrentHealth;
    private float _playerMaxHealth;
    private float _currentHeal = 0.0F;
    private int _currentTimesPlayed = 0;
    private float[] _enemyMineDamage;
    private readonly float _baseHeal = 5.0F;

    private readonly System.Random _rand = new System.Random();

    public PlayThroughLevelGoal(Player inPlayerInfo, GameData.RythmLevel inRythmLevel) : base(inPlayerInfo, inRythmLevel)
    {
        this._playerCurrentHealth = 100.0F * inPlayerInfo.GetPlayerLevel();
        this._playerMaxHealth = 100.0F * inPlayerInfo.GetPlayerLevel();
        this._currentHeal = this._baseHeal * inPlayerInfo.GetPlayerLevel();
        this._enemyMineDamage = inRythmLevel.GetMineDamage();

        this._levelGoalType = LevelGoalType.DestroyLevelGoal;
        this._uiProgressFile = "UIModules/UILevelGoals/PlayLevelGoalProgress";
        string[] tmpUIProgressFieldsList = { "PlayerHealth" };
        this._uiProgressFieldsList = tmpUIProgressFieldsList;
    }

    protected override int ProcessAndReturnScore(int inScoreMarkIndex, bool inIsBonus)
    {
        

        if (inIsBonus && this._playerCurrentHealth < 1.0)
        {
            this._playerCurrentHealth += this._baseHeal;
        }
        return base.ProcessAndReturnScore(inScoreMarkIndex, inIsBonus);
    }

    protected override int ProcessAndReturnDamage()
    {
        float damageDealed = this._enemyMineDamage[0] + (float)this._rand.NextDouble() * (this._enemyMineDamage[1] - this._enemyMineDamage[0]);
        this._playerCurrentHealth -= damageDealed;

        if (this._playerCurrentHealth <= 0.0F)
        {
            this.SetLevelGoalState(LevelGoal.LevelGoalState.Lost);
        }

        return (int)damageDealed;
    }

    public override float[] GetLevelGoalParams()
    {
        float[] returnParams = { 100 * (this._playerCurrentHealth / this._playerMaxHealth)};
        return returnParams;
    }

    public override LevelGoalState GetLevelGoalState()
    {
        if (this._rythmLevel.GetTimesPlayed() > 0)
        {
            this.SetLevelGoalState(LevelGoalState.Won);
        }
        return base.GetLevelGoalState();
    }
}
