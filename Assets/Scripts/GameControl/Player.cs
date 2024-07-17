using System.Collections;
using System.Collections.Generic;

using System.IO;
using static Player;
using System.Dynamic;
using System.Linq;
using System;
using static LevelGoal;
using UnityEngine;

using UnityEngine.SceneManagement;


public class Player
{

    private static readonly string defaultSettings= @"
        {
	        ""playerName"":""TestSubject"",
	        ""playerXP"":0,
	        ""playerLevel"":1,
	        ""playerScore"":0,
	        ""currentLevel"":0,
	        ""levelMetaData"":[],
	        ""skillStatus"":[true,true],
	        ""difficulty"":2,
            ""staffsKeys"":[ ""a"", ""s"", ""d"", ""f"", ""g"" ],
            ""skillsKeys"" : [ ""1"", ""2"" ],
            ""pauseKey"":""p""
        }
        ";
    [Serializable]
    private class PlayerDataToSave {

        public string playerName;
        public int playerXP;
        public int playerLevel;
        public int playerScore;
        

        public string[] staffsKeys;
        public string[] skillsKeys;
        public string pauseKey;

        public List<LevelMetaData> levelMetadata = new List<LevelMetaData>();

        public List<bool> skillStatus = new List<bool>();
    }
    [Serializable]
    private class LevelMetaData {
        public string fileName;
        public string name;
        public string goalType;
        public string descritprion;
        public List<string> unlocksLevels = new List<string>();

        public int[][] marksCounts = new int[][]{
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0}
        };

        public int[] maxLevelScores = new int[] {0, 0, 0 };
        public int[] playsCnt = new int[] { 0, 0, 0 };
        public int[] winsCnt = new int[] { 0, 0, 0 };
        public int[] maxLevelMark = new int[] { 0, 0, 0 };
        public int[] maxComboChainsCount = new int[] { 0, 0, 0 };
        public int[] maxComboLength = new int[] { 0, 0, 0 };
        public bool isLocked = false;

    }
    [Serializable]
    private class LevelList
    {
        public List<string> levelDataFiles = new List<string>();
    }

    private PlayerDataToSave _currentDataToSave;

    private float _gameSpeedCoef = 1.0F;
    private float _pitchCoef = 1.0F;

    private float _energy = 100;
    private float _energyMax = 100;
    private float _fullEnergyRegen = 180F;
    private int _currentLevel = 0;
    private int _difficulty = 0;

    private List<PlayerSkill> _avaibleSkills = new List<PlayerSkill>();
    private LevelGoal.LevelGoalState _levelGoalState;

    private int _previousMaxScore;
    private int _previousMaxLevelMark;
    private int _previousMaxComboChainsCount;
    private int _previousMaxComboLength;

    private int[] _currentMarksCounts;
    private int _currentLevelMark;
    private int _currentComboChainsCount;
    private int _currentMaxComboLength;
    private int _currentLevelScore;
    private int _currentNoteCount;

    private bool[] _progressImproveRate;


    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Player() {
        this.LoadSettings();
        this.UpdateSkills();

        this._energy = this._energyMax;
    }


    public void AddLevelMarksCounts(LevelProgressController inLevelProgressController) {
        for (int i = 0; i < 6; i++) { 
            this._currentDataToSave.levelMetadata[this._currentLevel].marksCounts[i][(int)this.GetDifficulty()] += inLevelProgressController.GetMarksCount(i);
        }
        this._currentDataToSave.levelMetadata[this._currentLevel].marksCounts[6][(int)this.GetDifficulty()] += inLevelProgressController.GetNoteCount();
    }

    public int[][] GetLevelMarksCount(int inIndex) {
        int[][] marksCount= null;
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        if (tmpLevelMetaData != null) {
            marksCount = tmpLevelMetaData.marksCounts;
        }
        return marksCount;
    }

    public bool LoadSettings() {
        Debug.Log($"Default settings: {Player.defaultSettings}");
        string tmpSettingsStr = PlayerPrefs.GetString("PlayerSettings", Player.defaultSettings);
        Debug.Log($"Loaded settings: {tmpSettingsStr}");
        this._currentDataToSave = JsonUtility.FromJson<PlayerDataToSave>(tmpSettingsStr);
        return (tmpSettingsStr != Player.defaultSettings);
    }

    public void CheckAndLoadAvaibleLevelMetadata(string inPath) {


        string data = GameController.GetStreamedText($"{inPath}LevelList.json");
        LevelList levelListObj = JsonUtility.FromJson<LevelList>(data);

        if (levelListObj.levelDataFiles.Count != this._currentDataToSave.levelMetadata.Count) {
            //TODO: add proper processes - add in metadata levelID check that all levels are present in metadata
            this._currentDataToSave.levelMetadata.Clear();
            for (int i = 0; i < levelListObj.levelDataFiles.Count; i++) {
                string fileName = levelListObj.levelDataFiles[i];
                data = GameController.GetStreamedText($"{inPath}{fileName}.json");

                LevelMetaData tmpLevelMetadata = JsonUtility.FromJson<LevelMetaData>(data);
                tmpLevelMetadata.fileName = fileName;
                this._currentDataToSave.levelMetadata.Add(tmpLevelMetadata);

                Debug.Log($"Level:{this._currentDataToSave.levelMetadata[i].name}");
            }
        }
    }



    public int GetAvaibleLevelCount() {
        return this._currentDataToSave.levelMetadata.Count;
    }
    public LevelGoal.LevelGoalState GetLevelGoalState()
    {
        return this._levelGoalState;
    }

    public string GetLevelName(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.name : "NA");

    }



    public string GetFileName(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.fileName : "NA");

    }

    public string GetCurrentLevelFileName() {
        return this.GetFileName(this._currentLevel);
    }

    public string GetLevelGoalType(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.goalType : "NA");

    }

    public string GetLevelDesc(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.descritprion : "NA");

    }

    public int[] GetLevelScores(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.maxLevelScores : new int[] { 0, 0, 0 });

    }

    public int[] GetLevelPlaysCnt(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.playsCnt : new int[] { 0, 0, 0 });

    }

    public int[] GetLevelWinsCnt(int inIndex)
    {
        LevelMetaData tmpLevelMetaData = this._currentDataToSave.levelMetadata.ElementAtOrDefault(inIndex);
        return (tmpLevelMetaData != null ? tmpLevelMetaData.winsCnt : new int[] { 0, 0, 0 });

    }

    public int[][] GetTotalMarksCount() {
        int[][] resultMarksCount = new int[][]{
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0},
            new int[] { 0, 0, 0}
        };

        for (int i = 0; i < this._currentDataToSave.levelMetadata.Count; i++ )
        {
            for (int y=0; y < resultMarksCount.Length; y++) {
                resultMarksCount[y][0] += this._currentDataToSave.levelMetadata[i].marksCounts[y][0];
                resultMarksCount[y][1] += this._currentDataToSave.levelMetadata[i].marksCounts[y][1];
                resultMarksCount[y][2] += this._currentDataToSave.levelMetadata[i].marksCounts[y][2];
            }
        }

        return resultMarksCount;
    }

    public void SaveSettings() {
        string tmpSettingsStr = JsonUtility.ToJson(this._currentDataToSave);
        PlayerPrefs.SetString("PlayerSettings", tmpSettingsStr);
        PlayerPrefs.Save();
    }

    public void PrepareAndSaveData(LevelProgressController inLeverProgressController) {
        this._previousMaxScore = this._currentDataToSave.levelMetadata[this._currentLevel].maxLevelScores[(int)this._difficulty];
        this._previousMaxComboLength = this._currentDataToSave.levelMetadata[this._currentLevel].maxComboLength[(int)this._difficulty];
        this._previousMaxComboChainsCount = this._currentDataToSave.levelMetadata[this._currentLevel].maxComboChainsCount[(int)this._difficulty];
        this._previousMaxLevelMark = this._currentDataToSave.levelMetadata[this._currentLevel].maxLevelMark[(int)this._difficulty];

        this._levelGoalState = inLeverProgressController.GetLevelGoalState();

        int marksCount = inLeverProgressController.GetMarksLength();
        this._currentLevelMark = inLeverProgressController.CalculateLevelMark();
        bool[] result = new bool[] { false, false, false, false};
        bool levelWon = this._levelGoalState == LevelGoal.LevelGoalState.Won;

        this._currentMarksCounts = inLeverProgressController.GetMarksCount();
        this._currentNoteCount = inLeverProgressController.GetNoteCount();
        if (levelWon)
        {
            for (int i = 0; i < marksCount; i++)
            {
                this._currentDataToSave.levelMetadata[this._currentLevel].marksCounts[i][(int)this._difficulty] += this._currentMarksCounts[i];
            }
            this._currentDataToSave.levelMetadata[this._currentLevel].marksCounts[marksCount][(int)this._difficulty] += this._currentNoteCount;
        }

        this._currentLevelScore = inLeverProgressController.GetLevelScore();
        if (this._previousMaxScore < this._currentLevelScore) {
            result[0] = true;
            if (levelWon) {
                this._currentDataToSave.levelMetadata[this._currentLevel].maxLevelScores[(int)this._difficulty] = this._currentLevelScore;
            }
        }

        this._currentMaxComboLength = inLeverProgressController.GetMaxComboCount();
        if (this._previousMaxComboLength < this._currentMaxComboLength)
        {
            result[1] = true; 
            if (levelWon)
            {
                this._currentDataToSave.levelMetadata[this._currentLevel].maxComboLength[(int)this._difficulty] = this._currentMaxComboLength;
            }
        }

        this._currentComboChainsCount = inLeverProgressController.GetComboChainsCount();
        if (this._previousMaxComboChainsCount < this._currentComboChainsCount)
        {
            result[2] = true;
            if (levelWon) { 
                this._currentDataToSave.levelMetadata[this._currentLevel].maxComboChainsCount[(int)this._difficulty] = this._currentComboChainsCount;
            }
        }
        if (this._previousMaxLevelMark > this._currentLevelMark)
        {
            result[3] = true;
            if (levelWon)
            {
                this._currentDataToSave.levelMetadata[this._currentLevel].maxLevelMark[(int)this._difficulty] = this._currentLevelMark;

            }
        }

        if (levelWon) {
            this._currentDataToSave.levelMetadata[this._currentLevel].winsCnt[(int)this._difficulty]++;
        }
        this._currentDataToSave.levelMetadata[this._currentLevel].playsCnt[(int)this._difficulty]++;

        this._progressImproveRate = result;

        this.SaveSettings();
    }

    public void UpdateSkills() {
        Debug.Log("Updating skills");
        this._avaibleSkills.Clear();
        for (int i=0; i < this._currentDataToSave.skillStatus.Count; i++) {
            PlayerSkill tmpSkill = PlayerSkill.GetSkill(this, i, this._currentDataToSave.skillStatus[i]);
            Debug.Log($"Processing skill {tmpSkill.GetSkillName()}");
            this._avaibleSkills.Add(tmpSkill);
        }
    }

    public void UpdateRoutine(float inDeltaTime) {
        this.SkillControl(inDeltaTime);
        this.RegenEnergy(inDeltaTime);
    }

    public void SkillControl(float inDeltaTime) {
        for (int i = 0; i < this._avaibleSkills.Count; i++) {
            if (this._avaibleSkills[i].CheckAvailble()) {
                this._avaibleSkills[i].SkillControl(inDeltaTime);
            } 
        }
    }

    public int GetSkillCount() {
        return this._avaibleSkills.Count;
    }

    public bool[] ToggleSkill(int inSkillIndex) {
        PlayerSkill currentSkill = this._avaibleSkills[inSkillIndex];
        
        bool skillIsToggled = false;

        Debug.Log($"Toggle skill {currentSkill.GetSkillName()}");

        if (!currentSkill.CheckActive())
        {
            bool skillIsBlocked = this.CheckSkillIsBlocked(inSkillIndex);
            if (!skillIsBlocked)
            {
                skillIsToggled = currentSkill.ActivateSkill();
                
            }
        }
        else {
            skillIsToggled = currentSkill.DeactivateSkill();
        }

        return new bool[] { skillIsToggled, currentSkill.CheckActive()};
    }

    public PlayerSkill GetSkill(int inSkillIndex) {
        return this._avaibleSkills[inSkillIndex];
    }

    public bool CheckSkillIsBlocked(int inSkillIndex) {
        PlayerSkill currentSkill = this._avaibleSkills[inSkillIndex];
        int[] blockingSkills = currentSkill.GetBlockingSkills();

        bool skillIsBlocked = false;

        for (int i = 0; i < blockingSkills.Length; i++)
        {
            if (this._avaibleSkills[blockingSkills[i]].CheckActive())
            {
                skillIsBlocked = true;
                break;
            }
        }
        return skillIsBlocked;
    }

    public int GetCurrentLevel()
    {
        return this._currentLevel;
    }

    public void SetCurrentLevel(int inLevelIndex) { 
        this._currentLevel = inLevelIndex;
    }

    public string GetPlayerName()
    {
        return this._currentDataToSave.playerName;
    }

    public Difficulty GetDifficulty()
    {
        return (Difficulty)this._difficulty;

    }

    public void SetDifficulty(int inDifficulty) {
        this._difficulty = inDifficulty;
    }

    public int[] GetCurrentMarksCounts() {
        return this._currentMarksCounts;
    }

    public int GetCurrentLevelMark()
    {
        return this._currentLevelMark;
    }

    public int GetCurrentComboChainsCount()
    {
        return this._currentComboChainsCount;
    }

    public int GetCurrentMaxComboLength()
    {
        return this._currentMaxComboLength;
    }

    public int GetCurrentLevelScore()
    {
        return this._currentLevelScore;
    }

    public int GetCurrentNoteCount()
    {
        return this._currentLevelScore;
    }

    public int GetPlayerXP()
    {
        return this._currentDataToSave.playerXP;
    }

    public int GetPlayerLevel()
    {
        return this._currentDataToSave.playerLevel;
    }

    public int GetPlayerScore()
    {
        return this._currentDataToSave.playerScore;
    }

    public string[] GetStaffsKeys()
    {
        return this._currentDataToSave.staffsKeys;
    }

    public string[] GetSkillsKeys()
    {
        return this._currentDataToSave.skillsKeys;
    }

    public string GetPauseKey() {
        return this._currentDataToSave.pauseKey;
    }

    public void AddScore(int inNewScore)
    {
        this._currentDataToSave.playerScore += inNewScore;
    }

    public void LoadScene(int inSceneId) {
        SceneManager.LoadScene(inSceneId);

    }


    public void SpendEnergy(float inEnergyToSpend) {
        if (this._energy > inEnergyToSpend)
        {
            this._energy -= inEnergyToSpend;
        }
        else {
            this._energy = 0;
        }
    }

    public void RegenEnergy(float inDeltaTime) {
        if (this._energy < this._energyMax) { 
            float newEnergyValue = inDeltaTime*(this._energyMax/this._fullEnergyRegen) + this._energy;
            this._energy = (newEnergyValue < this._energyMax ? newEnergyValue : this._energyMax);
        }
    }

    public float GetEnergyValue() {
        return this._energy;
    }

    public bool CheckActiveSkill(int inSkillId) {
        return this._avaibleSkills[inSkillId].CheckAvailble() ;
    }

    public float GetGameSpeedCoef() {
        return this._gameSpeedCoef;
    }

    public float GetPitchCoef() {
        return this._pitchCoef;
    }

    public void SetSpeedCoef(float inNewCoef) { 
        this._gameSpeedCoef = inNewCoef;
    }

    public void SetPitchCoef(float inNewCoef) {
        this._pitchCoef = inNewCoef;
    }

}
