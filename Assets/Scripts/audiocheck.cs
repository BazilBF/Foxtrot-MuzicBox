using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject gameFieldPrefab = null;

    private LevelProgressController _levelProgressController;


    public static readonly string synthMusicFolder = $"SynthMusic{Path.DirectorySeparatorChar}";

    private readonly static int _mainMenuSceneId = 0;
    private readonly static int _resultSceneId = 2;
    private bool _isPaused = false;

    private float _speedCoef = 1.0F;
    private float _pitchCoef = 1.0F;
    
    private float _deltaChangeSpeedPerSecond = 0.0F;
    private float _deltaChangePitchPerSecond = 0.0F;
    private float _currentSpeedCoef = 0.0F;
    private float _currentPitchCoef = 1.0F;

    private float _changeSpeedDuration = 2.0F;
    private readonly float _failChangeSpeedDuration = 5.0F;
    private readonly float _failSpeedPitchCoef = 0.01F;

    private bool _isFailEnding = false; 
    

    private Player _activePlayer;

    private GameData.RythmLevel _activeLevel;

    private GameData.PartStaffsData _activePartStaffsData;
    private GameData.PartStaffsData _nextPartStaffsData;

    private MusicBox _activeGameMusicBox;
    private MusicBox _nextGameMusicBox;

    private GameObject _activeGameField = null;
    private int _activeGameFieldBeat32sCount = 0;

    private GameObject _nextGameField = null;

    private float _gameWorldWidth = 0.0F;
    private float _gameWorldHeight = 0.0F;


    public Vector2 inGameScreenDownLeft;
    public Vector2 inGameScreenTopRight;

    private Camera _gameCamera;

    private int _bpm = 60;
    

    public int beatOffset = 6;

    private float _offsetGUI = 0.15F;
    private int _measure = 3;
    private int _length = 4;

    private float _sampleRate;


    private bool _beatIsSet = false;
    private bool _beat32IsSet = false;
    private bool _roundStarted = false;

    

    private float _mineOffset = 0.1F;

    private bool _demoMode = false;

    private float _defaultBonusProb = 0.3F;

    private MusicCoordinates _musicCoordinatesPlayed;

    

    private AudioSource _audioSource;
    private float _currentGain;

    // Start is called before the first frame update
    void Awake()
    {
        //Screen.orientation = ScreenOrientation.Portrait;
        this._gameCamera = Camera.main;

        this.transform.localPosition = Vector3.zero;

        this._audioSource = gameObject.AddComponent<AudioSource>();
        this._sampleRate = AudioSettings.outputSampleRate;

        this.LoadUserData();

        switch (this._activePlayer.GetDifficulty()) {
            case Player.Difficulty.Medium: this.beatOffset = 5; break;
            case Player.Difficulty.Hard: this.beatOffset = 4; break;
            default: this.beatOffset = 6; break;
        }

        

        this._activeLevel = GameData.RythmLevel.LoadSynthTrack(this._activePlayer.GetCurrentLevelFileName());
        this._levelProgressController = new LevelProgressController(this._activePlayer, this._activeLevel);

        this._activePartStaffsData = this._activeLevel.LoadSynth(0,LevelGoal.LevelGoalState.Starting);
        this._activeGameFieldBeat32sCount = this._activePartStaffsData.GetPartBeat32sCount(this._measure,this._length);



        this._measure = this._activeLevel.GetMeasure();
        this._length = this._activeLevel.GetLength();
        this.SetBpm(this._activeLevel.GetDefaultBpm());

        this._musicCoordinatesPlayed = new MusicCoordinates(this._length, this._measure, this);

        this.SetGameWorldSize();

        this._activeGameField = Instantiate(this.gameFieldPrefab, this.transform);

        FieldContainer activeFieldContainerScript = this._activeGameField.GetComponent<FieldContainer>();
        this._activeGameMusicBox = activeFieldContainerScript.SetPartStaffData(this._activePartStaffsData);
        activeFieldContainerScript.SetState(FieldContainer.CurrentState.Moving);

    }

    // Update is called once per frame
    void Update()
    {
        if (this._isPaused && Time.timeScale != 0.0F) {
            Time.timeScale = 0.0F;
            this._audioSource.Pause();
        }
        else if (!this._isPaused && Time.timeScale != 1.0F) {
            Time.timeScale = 1.0F;
            this._audioSource.UnPause();
        }

        this._activePlayer.UpdateRoutine(Time.deltaTime);
        if (this._beatIsSet)
        {
            float beatBarrGain = 0.75F;
            if (this._musicCoordinatesPlayed.GetBeats() == 0)
            {
                beatBarrGain = 1.0F;
                if (!this._roundStarted)
                {
                    this._roundStarted = true;
                    this.SpawnNextGameField();
                }
            }

            this._activeGameField.SendMessage("SpawnBeatBarr", beatBarrGain);
            
            this._beatIsSet = false;
        }

        if (this._beat32IsSet) {
            MusicCoordinates tmpCoordinates = MusicCoordinates.GetCopy(this._musicCoordinatesPlayed);


            this._activeGameField.SendMessage("SetStaffNotes", tmpCoordinates);

            this._beat32IsSet = false;

            FieldContainer activeFieldScript = this._activeGameField.GetComponent<FieldContainer>();

            bool musicBoxIsPlaying = this._activeGameMusicBox.CheckIsPlaying();
            bool partStaffAllNotePlayed = this._activePartStaffsData.CheckAllNotesPlayed();
            bool fieldHasBeatPucks = activeFieldScript.CheckBeatsPucks();
            bool readyToChange = !musicBoxIsPlaying && partStaffAllNotePlayed && !fieldHasBeatPucks;

            LevelGoal.LevelGoalState currentLevelGoalState = this._levelProgressController.GetLevelGoalState();
            string currentPartName = this._activePartStaffsData.GetNameCheck();

            //Debug.Log($"!MusicBoxIsPlaying = {!MusicBoxIsPlaying} && PartStaffAllNotePlayed = {PartStaffAllNotePlayed} && !FieldHasBeatPucks = {!FieldHasBeatPucks}");

            if (readyToChange || currentLevelGoalState == LevelGoal.LevelGoalState.Lost)
            {

                if (currentLevelGoalState == LevelGoal.LevelGoalState.Lost && !this._isFailEnding  && (this._currentSpeedCoef - this._failSpeedPitchCoef) > Math.Abs(this._deltaChangeSpeedPerSecond))
                {
                    this._isFailEnding = true;
                    this._changeSpeedDuration = this._failChangeSpeedDuration;
                    this._activePlayer.SetPitchCoef(this._failSpeedPitchCoef);
                    this._activePlayer.SetSpeedCoef(this._failSpeedPitchCoef);

                }
                else if ((currentLevelGoalState == LevelGoal.LevelGoalState.Lost  && (this._currentSpeedCoef - this._failSpeedPitchCoef) <= Math.Abs(this._deltaChangeSpeedPerSecond)) || (currentLevelGoalState == LevelGoal.LevelGoalState.Won && currentPartName == "Outro"))
                {
                    this._activePlayer.PrepareAndSaveData(this._levelProgressController);                    
                    this._activePlayer.LoadScene(GameController._resultSceneId);
                }
                else if (this._roundStarted && currentLevelGoalState != LevelGoal.LevelGoalState.Lost)
                {
                    //Debug.Log($"Active: {this._activePartStaffsData.GetPartStaffDataName()} | Next {this._nextPartStaffsData.GetPartStaffDataName()}");
                    this.ChangeActiveGameField();
                }
            }
        }

        if (this._speedCoef != this._activePlayer.GetGameSpeedCoef()) { 
            this.StartChangingSpeed(this._activePlayer.GetGameSpeedCoef());
        }

        if (this._pitchCoef != this._activePlayer.GetPitchCoef())
        {
            this.StartChangingPitch(this._activePlayer.GetPitchCoef());
        }

        this.ChangeSpeedAndPitch();
    }

    public void TogglePause() {
        if (this._isPaused) {
            this._isPaused = false;
        }
        else {
            this._isPaused = true;
        }
    }

    public bool CheckIsPaused() {
        return this._isPaused;
    }

    public void SetMusicBox(MusicBox inMusicBox) {
        this._activeGameMusicBox = inMusicBox;
    }

    public Vector2 GetWorldSize() {
        return new Vector2(this._gameWorldWidth, this._gameWorldHeight);
    }

    public MusicCoordinates GetMusicCoordinates() {
        return this._musicCoordinatesPlayed;
    }

    

    public GameData.RythmLevel GetActiveRythmLevel() {
        return this._activeLevel;
    }

    public float GetGUIOffset() {
        return this._offsetGUI;
    }

    public int GetBeatsPerBarr() {
        return this._measure * 2;
    }

    public int GetBeatOffset() {
        return this.beatOffset;
    }

    public int GetBpm() {
        int tmpBpm = (int)(this._bpm * this._currentSpeedCoef);
        return (tmpBpm >= 1 ? tmpBpm : 1);
    }

    public double GetBps() {
        double tmpBps = ((double)(this.GetBpm()) / 60.0);
        return (tmpBps >= 1.0 ? tmpBps : 1.0);
    }

    public double GetBeatLentgthSec()
    {
        return 1.0/this.GetBps();
    }

    public double GetBeat32LentgthSec() {
        return this.GetBeatLentgthSec() / (32 / this._length);
    }

    public int GetSamplesPerBeat32() {
        
        return (int)(this._sampleRate / (this.GetBps())) / (32 / this._length);
    }

    public float GetSampleRate() {
        return this._sampleRate;
    }

    public float GetMineOffset() {
        return this._mineOffset;
    }

    public LevelProgressController GetLevelProgressController() {
        return this._levelProgressController;
    }

    public bool GetDemoMode() {
        return this._demoMode;
    }

    public float GetBonusProb() {
        return this._defaultBonusProb;
    }

    public AudioSource GetAudioSource() {
        return this._audioSource;
    }

    public float GetCurrentSpeedCoef() {
        return this._currentSpeedCoef;
    }

    public void EndTry() {
        
        this._activePlayer.LoadScene(GameController._mainMenuSceneId);
    }

    

    private void ChangeActiveGameField (){

        GameObject dissmisGameField = this._activeGameField;
        FieldContainer dissmisGameFieldScript = dissmisGameField.GetComponent<FieldContainer>();

        this._activePartStaffsData = this._nextPartStaffsData;
        this._activeGameField = this._nextGameField;
        this._activeGameMusicBox = this._nextGameMusicBox;
        this._activeGameFieldBeat32sCount = this._activePartStaffsData.GetBeat32sCount();

        FieldContainer activeFieldContainerScript = this._activeGameField.GetComponent<FieldContainer>();

        activeFieldContainerScript.SetState(FieldContainer.CurrentState.Moving);
        this.SpawnNextGameField();

        dissmisGameFieldScript.SetState(FieldContainer.CurrentState.Dismiss);

    }

    

    private void SpawnNextGameField() {

        this._nextPartStaffsData = this._activeLevel.LoadSynth(this._activeGameFieldBeat32sCount,this._levelProgressController.GetLevelGoalState());
        this._nextGameField = Instantiate(this.gameFieldPrefab, this.transform);
        FieldContainer nextFieldContainerScript = this._nextGameField.GetComponent<FieldContainer>();
        this._nextGameMusicBox = nextFieldContainerScript.SetPartStaffData(this._nextPartStaffsData);
        nextFieldContainerScript.SetState(FieldContainer.CurrentState.Orbiting);

    }

    public static string GetStreamedText(string inpath) {
        var _path = $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}{inpath}";
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(_path);
        www.SendWebRequest();
        while (!www.downloadHandler.isDone)
        {
        }
        return www.downloadHandler.text;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels) {
            float metronomeGain = 0.1F;
            if (this._musicCoordinatesPlayed.GetSamples() == 0)
            {
                MusicCoordinates tmpCoordinates = MusicCoordinates.GetCopy(this._musicCoordinatesPlayed);
                if (this._musicCoordinatesPlayed.GetBeat32s() == 0)
                {
                    this._activeGameMusicBox.beatStroke(tmpCoordinates);
                    if (this._musicCoordinatesPlayed.GetBeats() == 0)
                    {
                        metronomeGain = 1.0F;
                    }
                    else if (this._musicCoordinatesPlayed.GetBeats() % this._measure == 0)
                    {
                        metronomeGain = 0.5F;
                    }

                    this._beatIsSet = true;
                    
                }
                this._activeGameMusicBox.NextNoteStroke(tmpCoordinates);
                this._beat32IsSet = true;
            }
            float note = this._activeGameMusicBox.playNotes(this._musicCoordinatesPlayed, this.GetBeat32LentgthSec(), metronomeGain, this._currentPitchCoef);
            this._currentGain = note;

            for (int j = 0; j < channels; j++)
            {
                data[i + j] = note;
            }
            this._musicCoordinatesPlayed.AddSamples(1);
        }
    }



    private void LoadUserData()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            this._activePlayer = playerController.GetActivePlayer();
            this._activePlayer.SetPitchCoef(this._pitchCoef);
            this._activePlayer.SetSpeedCoef(this._speedCoef);
            this._activePlayer.UpdateSkills();
            Debug.Log("Found player");
        }
        else {
            this._activePlayer = new Player();
            this._activePlayer.CheckAndLoadAvaibleLevelMetadata(GameController.synthMusicFolder);
            Debug.Log("New player");
        }
        this._currentPitchCoef = this._activePlayer.GetPitchCoef();
        this._currentSpeedCoef = this._activePlayer.GetGameSpeedCoef();
     

    }

    public Player GetPlayerInfo()
    {
        return this._activePlayer;
    }

    public float GetCurrentGain() {
        return this._currentGain;
    }

    public void SetMusicCoordinates(int inBars, int inBeats, int inBeats32) {
        this._musicCoordinatesPlayed = new MusicCoordinates(inBars, inBeats, inBeats32, this._musicCoordinatesPlayed.GetSamples(),this._musicCoordinatesPlayed.GetLength(),this._musicCoordinatesPlayed.GetMeasure(), this);
    }


    private void SetGameWorldSize() {

        Debug.Log($"Screen: {Screen.width} x {Screen.height}");

        this.inGameScreenDownLeft = this._gameCamera.ScreenToWorldPoint(new Vector3(0, 0, this._gameCamera.transform.position.z));
        this.inGameScreenTopRight = this._gameCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, this._gameCamera.transform.position.z));

        Debug.Log($"Calculated scrren {this.inGameScreenTopRight} | {Screen.width}x{Screen.height}");

        this._gameWorldHeight = this.inGameScreenTopRight.y - this.inGameScreenDownLeft.y;
        this._gameWorldWidth = this.inGameScreenTopRight.x - this.inGameScreenDownLeft.x;

        

        //Instantiate(this.backGround, new Vector3(0, 0, 0), Quaternion.identity);

    }

    private void SetBpm(int inBpm)
    {
        this._bpm = inBpm;
    }

    private void ChangeSpeedAndPitch()
    {
        if (this._deltaChangeSpeedPerSecond != 0.0F) {
            float newSpeedCoef = this._currentSpeedCoef + this._deltaChangeSpeedPerSecond * Time.deltaTime;
            if (Math.Abs(newSpeedCoef - this._speedCoef) > Math.Abs(this._deltaChangeSpeedPerSecond))
            {
                this._currentSpeedCoef = newSpeedCoef;
            }
            else {
                this._deltaChangeSpeedPerSecond = 0.0F;
                this._currentSpeedCoef = this._speedCoef;
            }
            
        }

        if (this._deltaChangePitchPerSecond != 0.0F)
        {
            float newPitchCoef = this._currentPitchCoef + this._deltaChangePitchPerSecond * Time.deltaTime;
            if (Math.Abs(newPitchCoef - this._pitchCoef) > Math.Abs(this._deltaChangePitchPerSecond))
            {
                this._currentPitchCoef = newPitchCoef;
            }
            else
            {
                this._deltaChangePitchPerSecond = 0.0F;
                this._currentPitchCoef = this._pitchCoef;
            }

        }
    }

    public void StartChangingSpeed(float inNewSpeedCoef) {
        this._deltaChangeSpeedPerSecond = (inNewSpeedCoef - this._speedCoef)/this._changeSpeedDuration;
        this._speedCoef = inNewSpeedCoef;
    }

    public void StartChangingPitch(float inNewPitchCoef)
    {
        this._deltaChangePitchPerSecond = (inNewPitchCoef - this._pitchCoef) / this._changeSpeedDuration;
        this._pitchCoef = inNewPitchCoef;
    }





}


