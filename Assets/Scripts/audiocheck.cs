using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Policy;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class GameController : MonoBehaviour
{

    public GameObject gameFieldPrefab = null;
    public GameObject particleSystem = null;
    public GameObject grid = null;

    private float _particleStartSpeed = 0.0F;

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

    private FieldContainer _fieldContainer;


    private GameObject _activeGameField = null;
    //private int _activeGameFieldBeat32sCount = 0;

    private GameObject _nextGameField = null;
    

    private float _gameWorldWidth = 0.0F;
    private float _gameWorldHeight = 0.0F;


    public Vector2 inGameScreenDownLeft;
    public Vector2 inGameScreenTopRight;

    private Camera _gameCamera;

    private int _bpm = 60;


    private int _beatOffset = 6;

    private bool _beatOffsetPlayed = false;
    private bool _SettingsIsLoaded = false;

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

    private int _maxInstrumentsCount = 0;
    private float _maxInstrumentsGain = 0.0F;

    private Dictionary<string, WaveForm.WaveFormSettings> _waveFormSettings = new Dictionary<string, WaveForm.WaveFormSettings>();

    private AudioSource _audioSource;
    private float _currentGain;

    private float _horizonHeight = 0.7F;

    private UnityEngine.Color[] _gridDefaultColors = new UnityEngine.Color[] { UnityEngine.Color.red, UnityEngine.Color.blue };
    private UnityEngine.Color[] _gridCurrentColors = null;

    private int _lastActivedSkill = -1;

    // Start is called before the first frame update
    void Awake()
    {
        //Screen.orientation = ScreenOrientation.Portrait;
        this._gameCamera = Camera.main;

        this.transform.localPosition = Vector3.zero;

        this._audioSource = gameObject.AddComponent<AudioSource>();
        this._sampleRate = AudioSettings.outputSampleRate;

        this.LoadUserData();

        switch (this._activePlayer.GetDifficulty())
        {
            case Player.Difficulty.Medium: this._beatOffset = 5; break;
            case Player.Difficulty.Hard: this._beatOffset = 4; break;
            default: this._beatOffset = 6; break;
        }



        this._activeLevel = GameData.RythmLevel.LoadSynthTrack(this._activePlayer.GetCurrentLevelFileName());

        StringCollection uniqueInstruments = this._activeLevel.GetUniqueInstruments();
        for (int i = 0; i < uniqueInstruments.Count; i++)
        {
            if (Array.IndexOf(MusicBox.standartInstruments, uniqueInstruments[i]) < 0)
            {
                _waveFormSettings[uniqueInstruments[i]] = WaveForm.LoadInstrumentData(uniqueInstruments[i], this);
            }
        }

        this._maxInstrumentsCount = this._activeLevel.GetMaxInstrumentsCount();
        this._maxInstrumentsGain = this._activeLevel.GetMaxInstrumentsGain();

        this._levelProgressController = new LevelProgressController(this._activePlayer, this._activeLevel);

        this._activePartStaffsData = this._activeLevel.LoadSynth(LevelGoal.LevelGoalState.Starting, this._activePlayer.GetDifficulty());

        this._measure = this._activeLevel.GetMeasure();
        this._length = this._activeLevel.GetLength();
        this.SetBpm(this._activeLevel.GetDefaultBpm());

        this._musicCoordinatesPlayed = new MusicCoordinates(this._length, this._measure, this);

        this.SetGameWorldSize();

        this.particleSystem.transform.localPosition = new Vector3(0, this._gameWorldHeight * this._horizonHeight / 2.0F, 0.0F);
        this._particleStartSpeed = 1.0F;

        this._activeGameField = Instantiate(this.gameFieldPrefab, this.transform);

        if (this.grid != null)
        {
            this.grid.GetComponent<NetRenderer>().SetNetRenderer(40, 0.8F, new Vector2(-1.0F * this._gameWorldWidth / 2.0F, this.GetHorizonLevel()), this._gameWorldWidth, GetHorizonLevel() + (this._gameWorldHeight / 2.0f), UnityEngine.Color.red, UnityEngine.Color.blue, 10.0f * (float)this.GetBeat32LentgthSec());
        }

        FieldContainer activeFieldContainerScript = this._activeGameField.GetComponent<FieldContainer>();
        this._fieldContainer = activeFieldContainerScript.SetPartStaffData(this._activePartStaffsData);
        activeFieldContainerScript.SetState(FieldContainer.CurrentState.Moving);
        this._isPaused = true;
        Time.timeScale = 0.0F;
        this._audioSource.Pause();

        this._audioSource.reverbZoneMix = 0.5F;

    }

    // Update is called once per frame
    void Update()
    {
        if (this._isPaused && Time.timeScale != 0.0F)
        {
            Time.timeScale = 0.0F;
            this._audioSource.Pause();
        }
        else if (!this._isPaused && Time.timeScale != 1.0F)
        {
            Time.timeScale = 1.0F;
            this._audioSource.UnPause();
        }



        this._activePlayer.UpdateRoutine(Time.deltaTime);
        if (this._beatIsSet)
        {
            float beatBarrGain = 0.5F;
            if (this._musicCoordinatesPlayed.GetBeats() == 0)
            {
                beatBarrGain = 0.75F;
                if (!this._roundStarted)
                {
                    this._roundStarted = true;
                    this.SpawnNextGameField();
                }
            }

            this._activeGameField.SendMessage("SpawnBeatBarr", beatBarrGain);

            this._beatIsSet = false;
        }

        if (this._beat32IsSet)
        {
            MusicCoordinates tmpCoordinates = MusicCoordinates.GetCopy(this._musicCoordinatesPlayed);

            if (!this._beatOffsetPlayed && tmpCoordinates.GetTotalBeats() - this.GetBeatOffset() >= 0)
            {
                this._beatOffsetPlayed = true;
            }



            this._activeGameField.SendMessage("SetStaffNotes", new FieldContainer.FieldMessageParams(tmpCoordinates, this._beatOffsetPlayed));

            this._beat32IsSet = false;

            FieldContainer activeFieldScript = this._activeGameField.GetComponent<FieldContainer>();

            bool musicBoxIsReadyToChange = activeFieldScript.CheckIsReadyToChange();
            bool partStaffAllNotePlayed = this._activePartStaffsData.CheckAllNotesPlayed();
            bool readyToChange = musicBoxIsReadyToChange && partStaffAllNotePlayed;




            LevelGoal.LevelGoalState currentLevelGoalState = this._levelProgressController.GetLevelGoalState();
            string currentPartName = this._activePartStaffsData.GetNameCheck();

            if (readyToChange || currentLevelGoalState == LevelGoal.LevelGoalState.Lost)
            {

                if (currentLevelGoalState == LevelGoal.LevelGoalState.Lost && !this._isFailEnding && (this._currentSpeedCoef - this._failSpeedPitchCoef) > Math.Abs(this._deltaChangeSpeedPerSecond))
                {
                    this._isFailEnding = true;
                    this._changeSpeedDuration = this._failChangeSpeedDuration;
                    this._activePlayer.SetPitchCoef(this._failSpeedPitchCoef);
                    this._activePlayer.SetSpeedCoef(this._failSpeedPitchCoef);

                }
                else if ((currentLevelGoalState == LevelGoal.LevelGoalState.Lost && (this._currentSpeedCoef - this._failSpeedPitchCoef) <= Math.Abs(this._deltaChangeSpeedPerSecond)) || (currentLevelGoalState == LevelGoal.LevelGoalState.Won && currentPartName == "Outro"))
                {
                    this._activePlayer.PrepareAndSaveData(this._levelProgressController);
                    this._activePlayer.LoadScene(GameController._resultSceneId);
                }
                else if (this._roundStarted && currentLevelGoalState != LevelGoal.LevelGoalState.Lost)
                {
                    this.ChangeActiveGameField();

                    this._beatOffsetPlayed = false;
                }
            }
        }

        if (this._speedCoef != this._activePlayer.GetGameSpeedCoef())
        {
            this.StartChangingSpeed(this._activePlayer.GetGameSpeedCoef());
        }

        if (this._pitchCoef != this._activePlayer.GetPitchCoef())
        {
            this.StartChangingPitch(this._activePlayer.GetPitchCoef());
        }

        this.ChangeSpeedAndPitch();
        this.ProcessGridColor();
    }

    public void TogglePause()
    {
        if (this._isPaused)
        {
            this._isPaused = false;
        }
        else
        {
            this._isPaused = true;
        }
    }

    public bool CheckIsPaused()
    {
        return this._isPaused;
    }

    public void ToggleSkill(int inSkillIndex)
    {
        bool[] toggleResult = this._activePlayer.ToggleSkill(inSkillIndex);
        if (toggleResult[0] && toggleResult[1])
        {
            this._lastActivedSkill = inSkillIndex;
            this._gridCurrentColors = this._activePlayer.GetGridColor();
            if (this.grid != null)
            {
                this.grid.GetComponent<NetRenderer>().StartChangeColor(this._gridCurrentColors[0], this._gridCurrentColors[1], this._changeSpeedDuration);
            }
        }
    }

    public void ProcessGridColor()
    {
        if (this._lastActivedSkill >= 0)
        {
            if (this._activePlayer.GetLastActivatedSkill() == -1)
            {
                this._lastActivedSkill = -1;
                this._gridCurrentColors = null;
                if (this.grid != null)
                {
                    this.grid.GetComponent<NetRenderer>().StartChangeColor(this._gridDefaultColors[0], this._gridDefaultColors[1], this._changeSpeedDuration);
                }


            }
            else if (this._activePlayer.GetLastActivatedSkill() != this._lastActivedSkill)
            {
                if (this.grid != null)
                {
                    this._lastActivedSkill = this._activePlayer.GetLastActivatedSkill();
                    this._gridCurrentColors = this._activePlayer.GetGridColor();
                    this.grid.GetComponent<NetRenderer>().StartChangeColor(this._gridCurrentColors[0], this._gridCurrentColors[1], this._changeSpeedDuration);
                }
            }
        }
    }

    public Dictionary<string, WaveForm.WaveFormSettings> GetWaveFormSettings()
    {
        return this._waveFormSettings;
    }

    public Vector2 GetWorldSize()
    {
        return new Vector2(this._gameWorldWidth, this._gameWorldHeight);
    }

    public MusicCoordinates GetMusicCoordinates()
    {
        return this._musicCoordinatesPlayed;
    }

    public float GetMaxInstrumentsGain()
    {
        return this._maxInstrumentsGain;
    }


    public GameData.RythmLevel GetActiveRythmLevel()
    {
        return this._activeLevel;
    }

    public float GetGUIOffset()
    {
        return this._offsetGUI;
    }

    public float GetHorizonLevel()
    {
        return (this._gameWorldHeight * this._horizonHeight) / 2.0F;
    }

    public int GetBeatsPerBarr()
    {
        return this._measure * 2;
    }

    public int GetBeatOffset()
    {
        return this._beatOffset;
    }

    public int GetBpm()
    {
        int tmpBpm = (int)(this._bpm * this._currentSpeedCoef);
        return (tmpBpm >= 1 ? tmpBpm : 1);
    }

    public double GetBps()
    {
        double tmpBps = ((double)(this.GetBpm()) / 60.0);
        return (tmpBps >= 1.0 ? tmpBps : 1.0);
    }

    public double GetBeatLentgthSec()
    {
        return 1.0 / this.GetBps();
    }

    public double GetBeat32LentgthSec()
    {
        return this.GetBeatLentgthSec() / (32 / this._length);
    }

    public int GetSamplesPerBeat32()
    {

        return (int)(this._sampleRate / (this.GetBps())) / (32 / this._length);
    }

    public float GetSampleRate()
    {
        return this._sampleRate;
    }

    public float GetMineOffset()
    {
        return this._mineOffset;
    }

    public LevelProgressController GetLevelProgressController()
    {
        return this._levelProgressController;
    }

    public bool GetDemoMode()
    {
        return this._demoMode;
    }

    public float GetBonusProb()
    {
        return this._defaultBonusProb;
    }

    public AudioSource GetAudioSource()
    {
        return this._audioSource;
    }

    public float GetCurrentSpeedCoef()
    {
        return this._currentSpeedCoef;
    }

    public void EndTry()
    {

        this._activePlayer.LoadScene(GameController._mainMenuSceneId);
    }

    public float GetPlayedPhase()
    {
        return (float)this._musicCoordinatesPlayed.GetTotalBeat32s() / (float)this._activePartStaffsData.GetBeat32sCount();
    }

    private void ChangeActiveGameField()
    {

        GameObject dissmisGameField = this._activeGameField;
        FieldContainer dissmisGameFieldScript = dissmisGameField.GetComponent<FieldContainer>();

        this._activePartStaffsData = this._nextPartStaffsData;
        this._activeGameField = this._nextGameField;


        this._fieldContainer = this._activeGameField.GetComponent<FieldContainer>();

        this._fieldContainer.SetState(FieldContainer.CurrentState.Moving);
        this.SpawnNextGameField();

        dissmisGameFieldScript.SetState(FieldContainer.CurrentState.Dismiss);

    }



    private void SpawnNextGameField()
    {

        this._nextPartStaffsData = this._activeLevel.LoadSynth(this._levelProgressController.GetLevelGoalState(), this._activePlayer.GetDifficulty());
        this._nextGameField = Instantiate(this.gameFieldPrefab, this.transform);
        FieldContainer nextFieldContainerScript = this._nextGameField.GetComponent<FieldContainer>();
        nextFieldContainerScript.SetPartStaffData(this._nextPartStaffsData);
        nextFieldContainerScript.SetState(FieldContainer.CurrentState.Orbiting);
        Debug.Log(this._levelProgressController.GetLevelGoalState());

    }

    public static string GetStreamedText(string inpath)
    {
        string path = $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}{inpath}";
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path);
        www.SendWebRequest();
        while (!www.downloadHandler.isDone)
        {
        }
        return www.downloadHandler.text;
    }

    public static byte[] GetStreamedBytes(string inpath)
    {
        string path = $"{Application.streamingAssetsPath}{Path.DirectorySeparatorChar}{inpath}";
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(path);
        www.SendWebRequest();
        while (!www.downloadHandler.isDone)
        {
        }
        return www.downloadHandler.data;
    }



    void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            float metronomeGain = 0.1F;
            if (this._musicCoordinatesPlayed.GetSamples() == 0)
            {
                MusicCoordinates tmpCoordinates = MusicCoordinates.GetCopy(this._musicCoordinatesPlayed);
                if (this._musicCoordinatesPlayed.GetBeat32s() == 0)
                {


                    this._beatIsSet = true;

                }
                this._fieldContainer.NextNoteStroke(tmpCoordinates);
                this._beat32IsSet = true;
            }
            float note = this._fieldContainer.playNotes(this._musicCoordinatesPlayed, this.GetBeat32LentgthSec(), metronomeGain, this._currentPitchCoef);
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
        if (playerObject != null)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            this._activePlayer = playerController.GetActivePlayer();
            this._activePlayer.SetPitchCoef(this._pitchCoef);
            this._activePlayer.SetSpeedCoef(this._speedCoef);
            this._activePlayer.UpdateSkills();
            Debug.Log("Found player");
        }
        else
        {
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

    public float GetCurrentGain()
    {
        return this._currentGain;
    }

    public void SetMusicCoordinates(int inBars, int inBeats, int inBeats32)
    {
        this._musicCoordinatesPlayed = new MusicCoordinates(inBars, inBeats, inBeats32, this._musicCoordinatesPlayed.GetSamples(), this._musicCoordinatesPlayed.GetLength(), this._musicCoordinatesPlayed.GetMeasure(), this);
    }


    private void SetGameWorldSize()
    {

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
        if (this._deltaChangeSpeedPerSecond != 0.0F)
        {
            float newSpeedCoef = this._currentSpeedCoef + this._deltaChangeSpeedPerSecond * Time.deltaTime;
            if (Math.Abs(newSpeedCoef - this._speedCoef) > Math.Abs(this._deltaChangeSpeedPerSecond))
            {
                this._currentSpeedCoef = newSpeedCoef;
            }
            else
            {
                this._deltaChangeSpeedPerSecond = 0.0F;
                this._currentSpeedCoef = this._speedCoef;
            }
            if (this.grid != null)
            {
                this.grid.GetComponent<NetRenderer>().SetDuration(10.0F * (float)this.GetBeat32LentgthSec());
            }
            var psMain = this.particleSystem.GetComponent<ParticleSystem>().main;
            psMain.startSpeed = this._particleStartSpeed * this._currentSpeedCoef;

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

    public void StartChangingSpeed(float inNewSpeedCoef)
    {
        this._deltaChangeSpeedPerSecond = (inNewSpeedCoef - this._speedCoef) / this._changeSpeedDuration;
        this._speedCoef = inNewSpeedCoef;
    }

    public void StartChangingPitch(float inNewPitchCoef)
    {
        this._deltaChangePitchPerSecond = (inNewPitchCoef - this._pitchCoef) / this._changeSpeedDuration;
        this._pitchCoef = inNewPitchCoef;
    }





}


