using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyLevelGoal : LevelGoal
{

    private float _playerCurrentHealth;
    private float _playerMaxHealth;
    private readonly float _enemyMaxHealth;
    private float _enemyCurrentHealth;
    private float[] _enemyMineDamage;
    private string _enemyMug;
    private readonly System.Random _rand = new System.Random();
    private bool _playerHasWon = false;

    private readonly float _baseAtack = 5.0F;
    private readonly float _baseHeal = 5.0F;

    private float _currentHeal = 0.0F;
    private int _currentTimesPlayed = 0;

    

    public DestroyLevelGoal(Player inPlayerInfo, GameData.RythmLevel inRythmLevel) : base(inPlayerInfo, inRythmLevel)
    {
        this._enemyMaxHealth = inRythmLevel.GetEnemyHealth();
        this._enemyCurrentHealth = inRythmLevel.GetEnemyHealth();
        this._enemyMug = inRythmLevel.GetEnemyMug();
        this._playerCurrentHealth = 100.0F * inPlayerInfo.GetPlayerLevel();
        this._playerMaxHealth = 100.0F * inPlayerInfo.GetPlayerLevel();
        this._currentHeal = this._baseHeal* inPlayerInfo.GetPlayerLevel();
        this._enemyMineDamage = inRythmLevel.GetMineDamage();

        this._levelGoalType = LevelGoalType.DestroyLevelGoal;
        this._uiProgressFile = "UIModules/UILevelGoals/DestroyLevelGoalProgress";
        string[] tmpUIProgressFieldsList = { "PlayerHealth", "EnemyHealth" };
        this._uiProgressFieldsList = tmpUIProgressFieldsList;
    }

    protected override int ProcessAndReturnScore(int inScoreMarkIndex, bool inIsBonus)
    {
        this._enemyCurrentHealth -= this._baseAtack * DestroyLevelGoal._MarksModifier[inScoreMarkIndex];

        if (this._enemyCurrentHealth <= 0.0F) {
            this.SetLevelGoalState(LevelGoal.LevelGoalState.Bonus);
            this._playerHasWon = true;
            this._currentTimesPlayed = this._rythmLevel.GetTimesPlayed();
        }

        if (inIsBonus && this._playerCurrentHealth<1.0) {
            this._playerCurrentHealth += this._baseHeal;
        }
        return base.ProcessAndReturnScore(inScoreMarkIndex, inIsBonus);
    }

    protected override int ProcessAndReturnDamage()
    {
        float damageDealed = this._enemyMineDamage[0] + (float)this._rand.NextDouble() * (this._enemyMineDamage[1] - this._enemyMineDamage[0]);
        this._playerCurrentHealth -= damageDealed;

        if (this._playerCurrentHealth <= 0.0F) {
            this.SetLevelGoalState(LevelGoal.LevelGoalState.Lost);
        }

        return (int)damageDealed;
    }

    protected override void ProcessLevelGoal(int inCurrentBeat32sCount)
    {
        base.ProcessLevelGoal(inCurrentBeat32sCount);

        this.levelProgress = this._enemyCurrentHealth / this._enemyMaxHealth;
    }

    public override float[] GetLevelGoalParams()
    {
        float[] returnParams = { 100*(this._playerCurrentHealth/ this._playerMaxHealth), 100*(this._enemyCurrentHealth / this._enemyMaxHealth) };
        return returnParams;
    }

    public override LevelGoalState GetLevelGoalState()
    {
        if (this._playerHasWon && this._rythmLevel.GetTimesPlayed() > this._currentTimesPlayed) {
            this.SetLevelGoalState(LevelGoalState.Won);
        }
        return base.GetLevelGoalState();
    }
}
