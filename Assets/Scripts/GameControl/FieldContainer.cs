using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FieldContainer : MonoBehaviour
{

    public enum CurrentState
    {
        Moving,
        Playing,
        Orbiting,
        Dismiss
    }

    public class FieldMessageParams {
        private MusicCoordinates _musicCoordinates;
        private bool _beatOffsetPlayed;

        public FieldMessageParams(MusicCoordinates inMusicCoordinates, bool inBeatOffsetPlayed) { 
            this._beatOffsetPlayed = inBeatOffsetPlayed;
            this._musicCoordinates = inMusicCoordinates;
        }

        public MusicCoordinates GetMusicCoordinates() { return _musicCoordinates; }
        public bool GetBeatOffsetPlayed() {  return _beatOffsetPlayed; }
    }

    public GameObject background = null;
    public GameObject InstrumentStaff = null;

    private string _name;

    private GameObject[] _playableStaffs;
    private SynthInstrument[] _unPlayableInstruments;

    private GameController _controller;

    public GameData.PartStaffsData _partStaffData;

    private float _sampleRate;

    private float _staffHeight = 0.0F;
    private float _staffWidth = 0.0F;

    private float _staffTopOffset = 0.2F;
    private float _spawnWidthCoef = 0.3F;

    private Vector2 _worldSize;

    private Vector3[] _startOrbitingCoordinates;
    private Vector3[] _playingCoordinates;

    private float[] _startAngle;
    private Vector3[] _startPosition;
    private Vector2[] _movingDistance;
    private float _currentTime = 0.0F;
    private float _winDistanceCoef = 0.25F;
    private float _phase = 1.0F;
    
    private int _beatsPerBarr;

    private int[] _mineCoolDown;
    private int[] _mineCoolDownLimit;
    
    private int[] _noteCount;

    private readonly System.Random _rand = new System.Random();

    private Player _activePlayer;
    private Player.Difficulty _currentDifficulty;

    public CurrentState _currentState = CurrentState.Orbiting;

    // Start is called before the first frame update
    void Awake()
    {

        GameObject controllerGameObject = GameObject.FindGameObjectWithTag("GameController");
        if (controllerGameObject != null)
        {
            this._controller = controllerGameObject.GetComponent<GameController>();

            this._activePlayer = this._controller.GetPlayerInfo();
            this._currentDifficulty = this._activePlayer.GetDifficulty();

            this._sampleRate = this._controller.GetSampleRate();
            this._beatsPerBarr = this._controller.GetBeatsPerBarr();

            SpriteRenderer backGroundRenderer = background.GetComponent<SpriteRenderer>();

            this._worldSize = this._controller.GetWorldSize();
            float GUIOffset = this._controller.GetGUIOffset();
            float verticalGUIOffset = 0.0F;
            float horizontalGUIOffset = 0.0F;

            if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            {
                horizontalGUIOffset = this._worldSize.x * GUIOffset * 2.0F;
            }
            else
            {
                verticalGUIOffset = this._worldSize.y * GUIOffset;
            }
            float newScaleHeight = (this._worldSize.y / backGroundRenderer.sprite.bounds.size.y) - verticalGUIOffset;
            float newScaleWidth = (this._worldSize.x / backGroundRenderer.sprite.bounds.size.x) - horizontalGUIOffset;

            backGroundRenderer.color = new Color(0.3f, 0.4f, 0.6f, 0.3f);
            backGroundRenderer.sortingLayerName = "Background";
            background.transform.localScale = new Vector3(newScaleWidth, newScaleHeight, -0.01F);
            background.transform.localPosition = new Vector3(0, verticalGUIOffset / 2.0F, 0);
        }


        
    }

    // Update is called once per frame
    void Update()
    {

        this._phase = 1.0F;

        if (this._currentState == CurrentState.Orbiting) {

            MovingAlongStaff movingAlongStaffScript = this.GetComponent<MovingAlongStaff>();
            if (movingAlongStaffScript != null) { 
                this._phase = movingAlongStaffScript.GetPhase();
            }
            this._currentTime += Time.deltaTime;
            float orbitingPhase = Mathf.Lerp((float)(-Math.PI),(float)(Math.PI),(float)(this._currentTime/(this._controller.GetBeatLentgthSec() * this._beatsPerBarr)));
            if (Math.Round(orbitingPhase,4)==Math.Round(Math.PI,4)) {
                this._currentTime = 0.0f;
            }

            for (int i=0; i<this._playableStaffs.Length; i++) {
                
                float newX = this._phase * (float)Math.Sin(this._startAngle[i] + orbitingPhase);
                float newY = this._phase * (float)Math.Cos(this._startAngle[i] + orbitingPhase);

                this._playableStaffs[i].transform.position = new Vector3(newX, newY, 0.0F);
            }
        }        

        if (this._currentState == CurrentState.Moving || this._currentState == CurrentState.Dismiss) {

            int speedCoeff = (this._currentState == CurrentState.Moving ? this._partStaffData.GetBridgeOffsetBeats() : 1);

            this._phase = Mathf.Lerp(0.0f, 1.0f, this._currentTime / (float)(this._controller.GetBeatLentgthSec() * speedCoeff));
            this._currentTime += Time.deltaTime;

            for (int i = 0; i < this._playableStaffs.Length; i++)
            {
                float newX = this._startPosition[i].x + this._phase * this._movingDistance[i].x;
                float newY = this._startPosition[i].y + this._phase * this._movingDistance[i].y;

                this._playableStaffs[i].transform.localPosition = new Vector3(newX, newY, 0.0F);

            }

            if (this._currentState == CurrentState.Dismiss && this._phase == 1.0F)
            {
                Destroy(this.gameObject);
            }


            if (this._phase == 1.0F)
            {
                this.SetState(CurrentState.Playing);
            }

            
        }

        if (this._currentState == CurrentState.Playing) {

            this.ProcessInput();
        }
    }

    private void ProcessInput() {
        string[] staffsKeys = this._activePlayer.GetStaffsKeys();
        for (int i=0; i < this._playableStaffs.Length; i++) {
            if (Input.GetKeyDown(staffsKeys[i])) {
                this._playableStaffs[i].GetComponent<StaffController>().PlayerTaped();
            }
        }

        string[] skillsKeys = this._activePlayer.GetSkillsKeys();
        for (int i = 0; i < skillsKeys.Length; i++)
        {
            if (Input.GetKeyDown(skillsKeys[i]))
            {
                this._activePlayer.ToggleSkill(i) ;
            }
        }

        string pauseKey = this._activePlayer.GetPauseKey();
        if (Input.GetKeyDown(pauseKey))
        {
            this._controller.TogglePause();
        }


        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                

                Vector3 touchPosWorld = Camera.main.ScreenToWorldPoint(touch.position);

                Vector2 touchPosWorld2D = new Vector2(touchPosWorld.x, touchPosWorld.y);

                RaycastHit2D hitInformation = Physics2D.Raycast(touchPosWorld2D, Camera.main.transform.forward);

                if (hitInformation.collider != null)
                {
                    //We should have hit something with a 2D Physics collider!
                    GameObject touchedObject = hitInformation.transform.gameObject;
                    StaffController touchedStaff = touchedObject.GetComponent<StaffController>();
                    Debug.Log($"[Touch Staff][Collided {touchedObject.transform.name}]");
                    if (touchedStaff != null)
                    {
                        Debug.Log($"[Touch Staff][This is staff]");
                        touchedStaff.PlayerTaped();
                    }
                }
            }
        }
    }

    public double GetBeatLentgthSec() {
        return this._controller.GetBeatLentgthSec();
    }

    public MusicBox SetPartStaffData(GameData.PartStaffsData inPartStaffData) {
        this._partStaffData = inPartStaffData;
        this._name = this._partStaffData.GetPartStaffDataName();
        return this.SetStaffs();
    }

    public string getPartStaffData() {
        return this._name;
    }

    public GameController GetGameController() {
        return this._controller;
    }

    public float GetWinDistanceCoef() {

        float returnWinDistanceCoef;
        switch (this._currentDifficulty)
        {
            case Player.Difficulty.Easy:
                {
                    returnWinDistanceCoef = this._winDistanceCoef * 1.5F;
                    break;
                }
            case Player.Difficulty.Medium:
                {
                    returnWinDistanceCoef = this._winDistanceCoef;
                    break;
                }
            case Player.Difficulty.Hard:
                {
                    returnWinDistanceCoef = this._winDistanceCoef * 0.5F;
                    break;
                }
            default: returnWinDistanceCoef = this._winDistanceCoef; break;
        }

        return returnWinDistanceCoef;
    }

    public float GetPhase() {
        return this._phase;
    }

    public bool GetDemoMode() {
        return this._controller.GetDemoMode();
    }

    public int AddScore(float inWinDeltaDistance, bool inIsBonus) {
        LevelProgressController levelProgressController = this._controller.GetLevelProgressController();

        return levelProgressController.AddScore(inWinDeltaDistance, inIsBonus);
    }

    public int AddDamage() {
        LevelProgressController levelProgressController = this._controller.GetLevelProgressController();

        return levelProgressController.AddDamage();
    }

    public void SetState(CurrentState inCurrentState) {
        this._currentState = inCurrentState;
        this._currentTime = 0.0F;
        if (this._currentState == CurrentState.Orbiting) {
            MovingAlongStaff movingScript = this.GetComponent<MovingAlongStaff>();
            if (movingScript == null)
            {
                movingScript = this.transform.AddComponent<MovingAlongStaff>();
            }

            for (int i = 0; i < this._playableStaffs.Length; i++)
            {
                StaffController currentStaffController = this._playableStaffs[i].GetComponent<StaffController>();
                currentStaffController.SetState(StaffController.CurrentState.Orbiting);
            }

            this.transform.localPosition = new Vector3(0.0F, 1.0F, 0.0F);

            movingScript.SetObjectMove(new Vector3(1.0F, 1.0F, 0.0F), Vector2.zero, this._partStaffData.GetBeat32sCount() * (float)this._controller.GetBeat32LentgthSec(),this._controller);
        }
        if (this._currentState == CurrentState.Dismiss) {
            MovingAlongStaff movingScript = this.GetComponent<MovingAlongStaff>();
            if (movingScript != null)
            {
                Destroy(this.transform.GetComponent<MovingAlongStaff>());
            }

            for (int i = 0; i < this._playableStaffs.Length; i++)
            {
                Vector3 currentPosition = this._playableStaffs[i].transform.localPosition;
                this._startPosition[i] = new Vector3(currentPosition.x, currentPosition.y, 0.0F);
                this._movingDistance[i] = new Vector2(0.0F, -6.0F);

                StaffController currentStaffController = this._playableStaffs[i].GetComponent<StaffController>();
                currentStaffController.SetState(StaffController.CurrentState.Moving);
            }

        }
        if (this._currentState == CurrentState.Moving) {

            MovingAlongStaff movingScript = this.GetComponent<MovingAlongStaff>();
            if (movingScript != null) {
                Destroy(this.transform.GetComponent<MovingAlongStaff>());
            }
            this.transform.localScale = new Vector3(1.0F, 1.0F, 0.0F);

            for (int i = 0; i < this._playableStaffs.Length; i++)
            {
                Vector3 currentPosition = this._playableStaffs[i].transform.position;
                this._startPosition[i] = new Vector3(currentPosition.x, currentPosition.y, 0.0F);
                this._movingDistance[i] = new Vector2(this._playingCoordinates[i].x - currentPosition.x, this._playingCoordinates[i].y - currentPosition.y);

                StaffController currentStaffController = this._playableStaffs[i].GetComponent<StaffController>();
                currentStaffController.SetState(StaffController.CurrentState.Moving);
            }

            this._controller.SetMusicCoordinates(0, 0, 0);
        }
        if (this._currentState == CurrentState.Playing) {

            MovingAlongStaff movingScript = this.GetComponent<MovingAlongStaff>();
            if (movingScript != null)
            {
                Destroy(this.transform.GetComponent<MovingAlongStaff>());
            }
            this.transform.localScale = new Vector3(1.0F, 1.0F, 0.0F);
        }
    }

    private MusicBox SetStaffs()
    {

        SpriteRenderer staffSpriteRenderer = this.background.GetComponent<SpriteRenderer>();
        float backGroundWidth = staffSpriteRenderer.sprite.bounds.size.x * this.background.transform.localScale.x;
        float backGroundHeight = staffSpriteRenderer.sprite.bounds.size.y * this.background.transform.localScale.y;

        List<GameData.ScoreStaff> playableStaffs = this._partStaffData.GetPlayableStaffs();
        List<GameData.ScoreStaff> unPlayableStaffs = this._partStaffData.GetUnPlayableStaffs();

        int playableIntsrumentsCount = playableStaffs.Count;
        int unPlayableIntsrumentsCount = unPlayableStaffs.Count;


        this._playableStaffs = new GameObject[playableIntsrumentsCount];
        this._unPlayableInstruments = new SynthInstrument[unPlayableIntsrumentsCount];

        int instrumentsCount = playableIntsrumentsCount + unPlayableIntsrumentsCount;

        SynthInstrument[] allSynthIntstruments = new SynthInstrument[instrumentsCount];
        float[] allInstrumentsGaings = new float[instrumentsCount];

        //setting with playable staffs, which is shown on the screen

        this._staffHeight = backGroundHeight * (1 - this._staffTopOffset);
        this._staffWidth = backGroundWidth / playableIntsrumentsCount;
        float staffPlayY = this.background.transform.localPosition.y + (backGroundHeight * this._staffTopOffset) / -2.0F;
        float staffPlayX = 0.0F;
        float staffSpawnX = 0.0F;

        float deltaAngle = 360.0F / playableIntsrumentsCount;

        this._startOrbitingCoordinates = new Vector3[playableIntsrumentsCount];
        this._playingCoordinates = new Vector3[playableIntsrumentsCount];
        this._startAngle = new float[playableIntsrumentsCount];
        this._startPosition = new Vector3[playableIntsrumentsCount];
        this._movingDistance = new Vector2[playableIntsrumentsCount];
        this._mineCoolDown = new int[playableIntsrumentsCount];
        this._mineCoolDownLimit = new int[playableIntsrumentsCount];
        this._noteCount = new int[playableIntsrumentsCount];

        for (int i = 0; i < playableIntsrumentsCount; i++)
        {

            float instrumentGain = playableStaffs[i].GetInstrumentGain();
            string instrument = playableStaffs[i].GetInstrument();
            allSynthIntstruments[i] = MusicBox.GetSynthInstrument(instrument, this._sampleRate, instrumentGain);
            allInstrumentsGaings[i] = instrumentGain;

            this._mineCoolDownLimit[i] = 0;
            this._mineCoolDown[i] = 0;
            this._noteCount[i]= 0;


            float spawnWidthX = this._staffWidth * this._spawnWidthCoef;

            if (i == 0)
            {
                staffPlayX = -1 * (playableIntsrumentsCount * this._staffWidth) / 2.0F + this._staffWidth / 2.0F;
                staffSpawnX = -1 * (playableIntsrumentsCount * spawnWidthX) / 2.0F + spawnWidthX / 2.0F;
            }
            else

            {
                staffPlayX += this._staffWidth;
                staffSpawnX += spawnWidthX;
            }

            Vector3 globalSpawnPoint = new Vector3(staffSpawnX, staffPlayY, 0);

            this._playingCoordinates[i] = new Vector3(staffPlayX, staffPlayY, 0);

            this._startAngle[i] = (float)Math.PI*deltaAngle * i/180.0F;

            float staffOrbitStartX = (float)Math.Sin(this._startAngle[i]);
            float staffOrbitStartY = (float)Math.Cos(this._startAngle[i]);

            this._startOrbitingCoordinates[i] = new Vector3(staffOrbitStartX, staffOrbitStartY);

            GameObject newStaff = Instantiate(this.InstrumentStaff, new Vector3(staffPlayX, staffPlayY, 0), Quaternion.identity, this.transform);
            StaffController script = newStaff.GetComponent<StaffController>();

            float maxBeatButtonWidth;

            if (Screen.orientation == ScreenOrientation.LandscapeLeft || SystemInfo.operatingSystemFamily != OperatingSystemFamily.Other)
            {
                maxBeatButtonWidth = this._worldSize.y * 0.15F;
            }
            else
            {
                maxBeatButtonWidth = this._worldSize.x * 0.15F;
            }


            script.SetStaff(this._staffWidth, this._staffHeight, maxBeatButtonWidth, globalSpawnPoint, allSynthIntstruments[i],i);
            this._playableStaffs[i] = newStaff;
        }
        for (int i = 0; i < unPlayableIntsrumentsCount; i++)
        {
            float instrumentGain = unPlayableStaffs[i].GetInstrumentGain();
            string instrument = unPlayableStaffs[i].GetInstrument();
            int yIterator = playableIntsrumentsCount + i;

            SynthInstrument unPlayableIntstrument = MusicBox.GetSynthInstrument(instrument, this._sampleRate, instrumentGain);
            _unPlayableInstruments[i] = unPlayableIntstrument;
            allSynthIntstruments[yIterator] = unPlayableIntstrument;
            allInstrumentsGaings[yIterator] = instrumentGain;
        }

            return new MusicBox(allSynthIntstruments, this._controller.GetMaxInstrumentsGain(), this._sampleRate);
    }

    public void SpawnBeatBarr(float inBeatBarrGain) {
        if (this._currentState == CurrentState.Playing && !this._partStaffData.CheckAllNotesPlayed()) {
            for (int i = 0; i < this._playableStaffs.Length; i++)
            {
                double beatOffsetLength = this._controller.GetBeatOffset() * this._controller.GetBeatLentgthSec();
                StaffController currentStaffController = this._playableStaffs[i].GetComponent<StaffController>();
                currentStaffController.SpawnBeatBarr(inBeatBarrGain, beatOffsetLength);
            }
        }
    }

    public void SetStaffNotes(FieldMessageParams infieldMessageParams) {
        MusicCoordinates msgMusicCoordinates = infieldMessageParams.GetMusicCoordinates();
        bool justPlayNote = !infieldMessageParams.GetBeatOffsetPlayed();

        if ((this._currentState == CurrentState.Playing || this._currentState == CurrentState.Moving) && !this._partStaffData.CheckAllNotesPlayed())
        {
            double beatOffsetLength = this._controller.GetBeatOffset() * this._controller.GetBeatLentgthSec();

            for (int i = 0; i < this._unPlayableInstruments.Length; i++) {
                
                SynthNote currentNote = this._partStaffData.GetLevelNote(i, false, msgMusicCoordinates);
                if (currentNote != null)
                {
                    this._unPlayableInstruments[i].KeyStroke(currentNote, msgMusicCoordinates, true);
                }
            }

            if (this._playableStaffs.Length > 0)
            {
                int startIndex = this._rand.Next(1, this._playableStaffs.Length);
                bool noteIsSet = false;

                for (int i = 0; i < this._playableStaffs.Length; i++)
                {

                    int currentIndex = (startIndex + i) % this._playableStaffs.Length;

                    MusicCoordinates tmpMusicCoordinates;
                    if (!justPlayNote)
                    {
                        tmpMusicCoordinates = MusicCoordinates.GetCopy(msgMusicCoordinates);
                        tmpMusicCoordinates.AddMetrics(0, this._controller.GetBeatOffset(), 0, 0);
                    }
                    else
                    {
                        tmpMusicCoordinates = msgMusicCoordinates;

                    }

                    StaffController currentStaffController = this._playableStaffs[currentIndex].GetComponent<StaffController>();
                    SynthNote currentNote = this._partStaffData.GetLevelNote(currentIndex, true, tmpMusicCoordinates);



                    if (currentNote != null)
                    {
                        //Debug.Log(currentNote);
                        if (!justPlayNote && this._currentState == CurrentState.Playing)
                        {
                            noteIsSet = this.PrepareNSpawnBeatPuck(tmpMusicCoordinates, currentNote, currentStaffController, beatOffsetLength, noteIsSet);
                        }
                        else
                        {

                            currentStaffController.KeyStroke(currentNote, tmpMusicCoordinates, true);
                        }

                    }
                    else if (msgMusicCoordinates.GetBeat32s() == 0 && this._currentState == CurrentState.Playing)
                    {

                        float deltaDistance = currentStaffController.GetDeltaDistance();
                        float mineOffset = this._controller.GetMineOffset();

                        if (
                                this._currentDifficulty != Player.Difficulty.Easy &&
                                deltaDistance < mineOffset &&
                                this._rand.NextDouble() < this._partStaffData.GetMineSpawnChance() / (this._currentDifficulty == Player.Difficulty.Hard ? 1.0F : 2.0F) &&
                                !this._partStaffData.CheckAllNotesPlayed()

                            )
                        {
                            currentStaffController.SpawnBeatPuck(null, beatOffsetLength, BeatPuck.PuckType.Mine);
                            this._mineCoolDown[currentIndex] = 0;
                            this._mineCoolDownLimit[currentIndex] = this._controller.GetBeatOffset() + this._rand.Next(-1, 1);
                        }
                        else
                        {
                            this._mineCoolDown[currentIndex]++;
                        }
                    }
                }
            }
        }
    }

    private bool PrepareNSpawnBeatPuck(MusicCoordinates inMusicCoordinates, SynthNote inCurrentNote, StaffController inCurrentStaffController, double inBeatOffsetLength, bool inNoteIsSet) {
        bool noteIsSet = inNoteIsSet;
        BeatPuck.PuckType spawnBeatPuckType;
        LevelProgressController tmpLevelController = this._controller.GetLevelProgressController();

        if (tmpLevelController.GetLevelGoalState() != LevelGoal.LevelGoalState.Bonus && tmpLevelController.GetLevelGoalState() != LevelGoal.LevelGoalState.Won)
        {
            switch (this._currentDifficulty)
            {
                case Player.Difficulty.Easy:
                    {
                        spawnBeatPuckType = (inMusicCoordinates.GetBeat32s() == 0 && !noteIsSet ? (this._rand.NextDouble() > this._controller.GetBonusProb() ? BeatPuck.PuckType.Note : BeatPuck.PuckType.Bonus) : BeatPuck.PuckType.Invisible);
                        break;
                    }
                case Player.Difficulty.Medium:
                    {
                        spawnBeatPuckType = (inMusicCoordinates.GetBeat32s() == 0 ? (this._rand.NextDouble() > this._controller.GetBonusProb() / 2.0F ? BeatPuck.PuckType.Note : BeatPuck.PuckType.Bonus) : BeatPuck.PuckType.Invisible);
                        break;
                    }
                case Player.Difficulty.Hard:
                    {
                        spawnBeatPuckType = BeatPuck.PuckType.Note;
                        break;
                    }
                default: spawnBeatPuckType = BeatPuck.PuckType.Note; break;
            }
        }
        else
        {
            spawnBeatPuckType = BeatPuck.PuckType.Bonus;
        }
        noteIsSet = true;

        if (spawnBeatPuckType != BeatPuck.PuckType.Mine && spawnBeatPuckType != BeatPuck.PuckType.Invisible)
        {

            tmpLevelController.AddNoteCount();
        }

        inCurrentStaffController.SpawnBeatPuck(inCurrentNote, inBeatOffsetLength, spawnBeatPuckType);

        return noteIsSet;
    }

    public bool CheckBeatsPucks()
    {
        bool hasSpawnedPucks = false;
        for (int i = 0; i < this._playableStaffs.Length;i++) {
            StaffController staffController = this._playableStaffs[i].GetComponent<StaffController>();
            if (staffController.CheckBeatsPucks())
            {
                hasSpawnedPucks = true; break;
            }
        }
        
        return hasSpawnedPucks;
    }
}
