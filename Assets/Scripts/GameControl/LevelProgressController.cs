
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum LevelProgressState { 
    LevelInProgress,
    LevelWinned,
    LevelLost
}

public class LevelProgressController
{

    private Player _playerInfo;
    private GameData.RythmLevel _rythmLevel;
    private LevelGoal _targetGoal;

    private LevelProgressState _curentLevelState;

    private int _comboCount = 0;
    private int _maxComboCount = 0;
    private int _comboChains = 0;
    private bool _comboActive = false;
    private int _notePlayedCount = 0;

    private int _levelScore = 0;

    private int[] _marksCount = new int[] { 0, 0, 0, 0, 0, 0};

    public LevelProgressController(Player inPlayerInfo, GameData.RythmLevel inRythmLevel) {
        this._playerInfo = inPlayerInfo;
        this._rythmLevel = inRythmLevel;



        
        string getGoalType = inRythmLevel.GetGoalType();
        switch (getGoalType) {
            case "Destroy": this._targetGoal = new DestroyLevelGoal(inPlayerInfo, inRythmLevel); break;
            case "PlayThrough": this._targetGoal = new PlayThroughLevelGoal (inPlayerInfo, inRythmLevel); break;
            default: this._targetGoal = new LevelGoal(inPlayerInfo, inRythmLevel); break;
        }

    }

    public void AddNoteCount() { 
        this._notePlayedCount++;
    }

    public int GetNoteCount() {
        return this._notePlayedCount;
    }

    public int GetMarksCount(int inMarkIndex) {
        return this._marksCount[inMarkIndex];
    }

    public int[] GetMarksCount() {
        return this._marksCount;
    }

    public int GetComboCount() {
        return this._comboCount;
    }

    public int GetComboChainsCount() {
        return this._comboChains;
    }

    public int AddScore(float inWinDeltaDistance, bool inIsBonus) {

        this.ProcessComboChain();

        int scoreMarkIndex = this.CalculateMark(inWinDeltaDistance);
        this._marksCount[scoreMarkIndex]++;


        int scoreToAdd = (this._comboCount + 1) * this._targetGoal.AddScore(scoreMarkIndex, this._comboCount, inIsBonus);

        this._playerInfo.AddScore(scoreToAdd);
        this._levelScore += scoreToAdd;

        return scoreMarkIndex;
    }

    public int GetLevelScore() {
        return this._levelScore;
    }

    public int CalculateLevelMark() {
        float[] markModifiers = LevelGoal.GetMarkModifiers();
        float totalPossibleLevelPoints = this._notePlayedCount * markModifiers[0];

        float totalLevelPoints = 0.0F;
        for (int i = 0; i < this._marksCount.Length; i++) { 
            totalLevelPoints += this._marksCount[i] * markModifiers[i];
        }

        return this.CalculateMark(totalLevelPoints/totalPossibleLevelPoints);
    }

    private int CalculateMark(float inWinDeltaDistance) {
        Debug.Log(inWinDeltaDistance);
        int returnMark = 5;
        if (inWinDeltaDistance > 0.8F)
        {
            returnMark = 0;
        }
        else if (inWinDeltaDistance >= 0.7F) {
            returnMark = 1;
        }
        else if (inWinDeltaDistance >= 0.5F)
        {
            returnMark = 2;
        }
        else if (inWinDeltaDistance >= 0.3F)
        {
            returnMark = 3;
        }
        else if (inWinDeltaDistance >= 0.1F)
        {
            returnMark = 4;
        }

        

        return returnMark;
    }

    public int GetMarksLength() {
        return this._marksCount.Length;
    }

    public int GetMaxComboCount() {
        return this._maxComboCount;

    }

    private void ProcessComboChain() {
        if (this._comboActive)
        {
            this._comboCount++;
            if (this._comboCount > this._maxComboCount) {
                this._maxComboCount = this._comboCount;
            }
        }
        else {
            this._comboCount = 0;
            this._comboChains++;
            this._comboActive = true;
        }
    }

    public int AddDamage() {
        this.BreakComboChain();
        return this._targetGoal.AddDamage();
    }

    public void BreakComboChain() {
        this._comboCount = 0;
        this._comboActive = false;
    }

    public LevelGoal.LevelGoalState GetLevelGoalState() {
        return this._targetGoal.GetLevelGoalState();
    }

    public void ProcessLevelFlow() {

    }

    public float[] GetLevelGoalParams() { 
        return this._targetGoal.GetLevelGoalParams();
    }

    public LevelGoal.LevelGoalType GetLevelGoalType() {
        return this._targetGoal.GetLevelGoalType();
    }

    public string GetUiProgressFile() { 
        return this._targetGoal.GetUiProgressFile();
    }

    public string[] GetUIProgressFieldsList()
    {
        return this._targetGoal.GetUIProgressFieldsList();
    }
}
