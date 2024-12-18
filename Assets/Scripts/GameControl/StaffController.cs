using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using Unity.VisualScripting;
using UnityEngine;



public class StaffController : MonoBehaviour
{

    public enum PuckType
    {
        Note,
        Mine
    }

    public enum CurrentState
    {
        Moving,
        Playing,
        Orbiting,
        Dismiss
    }

    private float _staffWidth = 0.0F;
    private float _staffHeight = 0.0F;

    private float _spawnPointYPrc = 0.0F;


    public GameObject beatButton = null;
    public GameObject beatPuck = null;
    public GameObject staffBackground = null;
    public GameObject beat32Bar = null;

    public GameObject topLeftBone = null;
    public GameObject topRightBone = null;
    public GameObject bottomBone = null;

    private SpriteRenderer _staffSpriteRenderer = null;
    
    private BeatButtonScript _beatButtonScript = null;

    private float _beatButtonOffset = 0.1F;
    private float _beatButtonWidth = 0.0F;

    private float _spriteOffset = 0.02F;
    private float _beatPuckOffset = 0.1F;
    private float _beatMarkerScale = 0.0F;
    private double _beatLentgthSec;

    private FieldContainer _parentScript;

    private Vector3 _spawnPosition = Vector3.zero;
    private Vector3 globalSpawn = Vector3.zero;

    private SynthInstrument _staffInstrument;
    private bool _trueNote = false;
    private bool _playerTapped = false;

    private bool _demoModeFlag = false;

    private Queue<GameObject> _currentBeatBarrs = new Queue<GameObject>();
    private Queue<GameObject> _currentVisiblePucks = new Queue<GameObject>();
    private Queue<GameObject> _currentInVisiblePucks = new Queue<GameObject>();

    private int _currentBeatPucksCount = 0;

    private float _totalDistance;
    private float _nextVisiblePuckDistance;
    private float _nextInvisiblePuckDistance;

    private BoxCollider2D _staffCollider;

    private float _nextDeltaWinDistance = -1.0F;

    private CurrentState _currentState = CurrentState.Orbiting;
    private Vector3 _backgroundTargetScale;
    

    private float _winDistance = 0.1F;

    private int _staffNumber = 0;

    private GameController _controller;
    private float _spawnWidthCoef;
    private float _spawnOffset = 0.1F;



    public UnityEngine.Color[] winColors = new UnityEngine.Color[]{
                                                new UnityEngine.Color(0.0F,0.66F,1.0F),
                                                new UnityEngine.Color(0.0F,0.11F,1.0F),
                                                new UnityEngine.Color(0.94F,1.0F,0.0F),
                                                new UnityEngine.Color(1.0F,0.9F,0.0F),
                                                new UnityEngine.Color(0.45F,0.93F,0.8F),
                                                new UnityEngine.Color(0.3F,0.93F,0.91F)
                                            };

    public void SetStaff(float inStaffWidth, float inStaffHeight, float inBeatButtonMaxWidth, Vector3 inGlobalXSpawnPosition, SynthInstrument inInstrument, int inStaffNumber, GameController inGameController, float inSpawnWidthCoef) {
        this._staffWidth = inStaffWidth;
        this._staffHeight = inStaffHeight;
        //Vector3 localXSpawnPosition = this.transform.InverseTransformPoint(inGlobalXSpawnPosition);
        this._spawnPosition = this.transform.InverseTransformPoint(inGlobalXSpawnPosition);
        this._staffInstrument = inInstrument;
        this._parentScript = this.transform.parent.GetComponent<FieldContainer>();
        this._demoModeFlag = this._parentScript.GetDemoMode();
        this._staffNumber = inStaffNumber;
        this._controller = inGameController;
        this._spawnWidthCoef = inSpawnWidthCoef;

        this._staffCollider = this.transform.GetComponent<BoxCollider2D>();
        if (this._staffCollider != null) {
            this._staffCollider.size = new Vector2(inStaffWidth, inStaffHeight);
            this._staffCollider.enabled = false;
        }

        this._beatLentgthSec = this._parentScript.GetBeatLentgthSec();
        
        this._staffSpriteRenderer = staffBackground.GetComponent<SpriteRenderer>();
        float newScaleWidth = (1 - this._spriteOffset) * this._staffWidth / this._staffSpriteRenderer.sprite.bounds.size.x;
        float newScaleHeight = this._staffHeight / this._staffSpriteRenderer.sprite.bounds.size.y;

        UnityEngine.Color tmpColor = this._staffSpriteRenderer.color;

        tmpColor.a = 0.0F;

        this._staffSpriteRenderer.color = tmpColor;
        this._staffSpriteRenderer.sortingLayerName = "Staffs";
        this._staffSpriteRenderer.sortingOrder = 0;

        this._backgroundTargetScale = new Vector3(newScaleWidth, newScaleHeight, 0.0F);
        staffBackground.transform.localScale = Vector3.zero;
        staffBackground.transform.localPosition = Vector3.zero;

        float newBeatButtonWidth = this._staffWidth * (1.0F - this._beatButtonOffset);
        float beatButtonY = -(1 - this._beatButtonOffset) * this._staffHeight / 2.0F + this._beatButtonWidth / 2.0F;

        this._beatButtonWidth = (1 - this._beatButtonOffset) * Mathf.Min(newBeatButtonWidth, inBeatButtonMaxWidth);

        this.beatButton.transform.parent = this.gameObject.transform;
        this._beatButtonScript = this.beatButton.GetComponent<BeatButtonScript>();
        this._beatButtonScript.SetBeatButton(this._beatButtonWidth, beatButtonY, this._staffInstrument);

        this._totalDistance = Vector3.Distance(this._beatButtonScript.GetBeatButtonTargetPosition(), this._spawnPosition);
        this._winDistance = this._totalDistance * this._parentScript.GetWinDistanceCoef();
        
    }

    public void SetNextPuckDistance(float inNextPuckDistance, bool inPuckIsVisible) {
        if (inPuckIsVisible) {
            this._nextVisiblePuckDistance = inNextPuckDistance;
        }
        else {
            this._nextInvisiblePuckDistance = inNextPuckDistance;
        }
    }

    public float GetNextPuckDistance(int inPuckVisibilityFlag) {
        float returnDistance = -1;
        switch (inPuckVisibilityFlag) {
            case 0: returnDistance =(this._nextInvisiblePuckDistance > this._nextVisiblePuckDistance? this._nextInvisiblePuckDistance : this._nextVisiblePuckDistance);break;
            case 1: returnDistance = this._nextVisiblePuckDistance;break;
            case 2: returnDistance = this._nextInvisiblePuckDistance;break;
        }
        return returnDistance;
    }



    public void SpawnBeatBarr (float inBarrGain, double inDuration){

        GameObject newBeatBarr = Instantiate(this.beat32Bar);
        SpriteRenderer barrSpriteRenderer = newBeatBarr.transform.GetComponent<SpriteRenderer>();
        newBeatBarr.transform.parent = this.transform;
        
        barrSpriteRenderer.color = new UnityEngine.Color(1.0f, 1.0f, 1.0f, inBarrGain);
        barrSpriteRenderer.sortingLayerName = "Staffs";
        barrSpriteRenderer.sortingOrder = 2;

        this._currentBeatBarrs.Enqueue(newBeatBarr);

        newBeatBarr.transform.localPosition = this._spawnPosition;

        MovingAlongStaff movingScript = newBeatBarr.GetComponent<MovingAlongStaff>();
        movingScript.SetObjectMove(new Vector2(inBarrGain*(1.0F - this._spriteOffset) * this._staffWidth, this._staffWidth * this._spriteOffset * inBarrGain), this._beatButtonScript.GetBeatButtonTargetPosition(), (float) inDuration, this._parentScript.GetGameController());

    }

    public void SpawnBeatPuck(SynthNote inNoteToPlay, double inDuration, BeatPuck.PuckType inPuckType) {

        GameObject newPuck = Instantiate(this.beatPuck, this.transform);

        BeatPuck beatPuckScript = newPuck.GetComponent<BeatPuck>();
        beatPuckScript.SetBeatPuck(inNoteToPlay);
        beatPuckScript.SetPuckType(inPuckType);

        if (inPuckType != BeatPuck.PuckType.Mine) {
            this._currentBeatPucksCount++;
        }

        if (inPuckType == BeatPuck.PuckType.Invisible)
        {
            this._currentInVisiblePucks.Enqueue(newPuck);
        }
        else {
            this._currentVisiblePucks.Enqueue(newPuck);
        }
        

        newPuck.transform.localPosition = this._spawnPosition;

        MovingAlongStaff movingScript = newPuck.GetComponent<MovingAlongStaff>();

        movingScript.SetObjectMove(new Vector2((1.0F - this._beatPuckOffset) * this._beatButtonWidth, 0.95F*(1.0F - this._beatPuckOffset) * this._beatButtonWidth), this._beatButtonScript.GetBeatButtonTargetPosition(), (float)inDuration, this._parentScript.GetGameController());

        bool visiblePuck = true;
        if (inPuckType == BeatPuck.PuckType.Invisible)
        {
            visiblePuck = false;
        }

        this.SetNextPuckDistance(this._totalDistance,visiblePuck);


    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void QueueNote(SynthNote inNextNote, bool inTrueNote) {
        this._staffInstrument.QueueNote(inNextNote, inTrueNote);
    }

    public void KeyStroke(SynthNote inNoteToPlay, MusicCoordinates inMusicCoordinates, bool inTrueNote) {
        this._staffInstrument.KeyStroke(inNoteToPlay, inMusicCoordinates, inTrueNote);
    }

    public BeatPuck PlayerTaped()
    {

        BeatPuck returnNote = null;
        if (this._currentState == CurrentState.Playing)
        {
            GameObject nextPuck;
            bool hasPuckToPeek = this._currentVisiblePucks.TryPeek(out nextPuck);

            float nextPuckDistance = this.GetNextPuckDistance(1);

            

            if (hasPuckToPeek && nextPuckDistance <= this._totalDistance * 0.95F)
            {
                
                BeatPuck nextPuckScript = nextPuck.GetComponent<BeatPuck>();
                BeatPuck.PuckType nextPuckType = nextPuckScript.GetPuckType();

                if (!nextPuckScript.GetPlayerTapped())
                {
                    if (nextPuckDistance <= this._winDistance && nextPuckScript.GetPuckType() != BeatPuck.PuckType.Mine)
                    {
                        float deltaWinDistance = 1.0F - (nextPuckDistance / this._winDistance);
                        this._nextDeltaWinDistance = deltaWinDistance;
                        int colorIndex = this._parentScript.AddScore(this._nextDeltaWinDistance, (nextPuckType == BeatPuck.PuckType.Bonus));
                        UnityEngine.Color tmpCurrentColor = this.winColors[colorIndex];
                        nextPuckScript.SetTrueNote(deltaWinDistance, tmpCurrentColor);
                    }
                    else
                    {
                        nextPuckScript.SetFalseNote();
                    }
                    nextPuckScript.SetPlayerTapped();

                    returnNote = nextPuckScript;
                }
            }
        }
        return returnNote; 
    }

    public float GetDeltaDistance(int inPuckVisibilityFlag = 1) {
        return this.GetNextPuckDistance(inPuckVisibilityFlag) / this._totalDistance;
    }

    

    // Update is called once per frame
    void Update()
    {
        
        

        if (this._currentState == CurrentState.Playing) {

           

            UnityEngine.Color tmpStaffColor = this._staffSpriteRenderer.color;

            if (tmpStaffColor.a < 0.7F)
            {
                tmpStaffColor.a += Time.deltaTime;
                this._staffSpriteRenderer.color = tmpStaffColor;
            }

            GameObject nextVisiblePuck;
            bool hasVisiblePuckToPeek = this._currentVisiblePucks.TryPeek(out nextVisiblePuck);
            if (hasVisiblePuckToPeek) {
                this.ProcessBeatPuck(nextVisiblePuck);
            }

            GameObject nextInvisiblePuck;
            bool hasInVisiblePuckToPeek = this._currentInVisiblePucks.TryPeek(out nextInvisiblePuck);
            if (hasInVisiblePuckToPeek)
            {
                this.ProcessBeatPuck(nextInvisiblePuck);
            }

            GameObject nextBeatBarr;
            bool hasBarrToPeak = this._currentBeatBarrs.TryPeek(out nextBeatBarr);

            if (hasBarrToPeak) {
                float nextBarrDistance = Vector3.Distance(this._beatButtonScript.GetBeatButtonTargetPosition(), nextBeatBarr.transform.localPosition);
                nextBarrDistance = (float)Math.Round((double)nextBarrDistance, 4, MidpointRounding.ToEven);
                if (nextBarrDistance == 0.0F)
                {
                    GameObject barrToDelete = this._currentBeatBarrs.Dequeue();
                    DestroyImmediate(barrToDelete);
                }
            }
        }

        if (this._currentState == CurrentState.Moving)
        {

            float phase = this._parentScript.GetPhase();
            
            this._beatButtonScript.ProcessMove(phase);
            

            float newScaleX = phase * this._backgroundTargetScale.x;
            float newScaleY = phase * this._backgroundTargetScale.y;

            this.staffBackground.transform.localScale = new Vector3(newScaleX, newScaleY, 0.0F);

            if (phase == 1.0F)
            {
                this.SetState(CurrentState.Playing);
            }

            
            this._beatButtonScript.SetBeatAlpha(1.0F);

            UnityEngine.Color tmpStaffColor = this._staffSpriteRenderer.color;

            if (tmpStaffColor.a > 0.0F)
            {
                tmpStaffColor.a -= Time.deltaTime;
                this._staffSpriteRenderer.color = tmpStaffColor;
            }

        }

        if (this._currentState == CurrentState.Orbiting) {

            this._beatButtonScript.SetBeatAlpha(0.4F);
        }
    }

    private void ProcessBeatPuck(GameObject inNextPuck) {
        float nextPuckDistance = Vector3.Distance(this._beatButtonScript.GetBeatButtonTargetPosition(), inNextPuck.transform.localPosition);
        nextPuckDistance = (float)Math.Round((double)nextPuckDistance, 4, MidpointRounding.ToEven);
        

        BeatPuck nextPuckScript = inNextPuck.GetComponent<BeatPuck>();
        SynthNote nextNote = nextPuckScript.GetSynthNote();
        BeatPuck.PuckType nextPuckType = nextPuckScript.GetPuckType();

        bool PuckIsVisible = true;
        if (nextPuckType == BeatPuck.PuckType.Invisible) {
            PuckIsVisible = false;
        }
        this.SetNextPuckDistance(nextPuckDistance, PuckIsVisible);

        if (nextPuckDistance <= 0.01F)
        {

            bool trueNote = nextPuckScript.GetTrueTone() || this._demoModeFlag;

            bool nextPuckIsTapped = nextPuckScript.GetPlayerTapped();
            
            

            if (!this._demoModeFlag)
            {
                if (trueNote && nextPuckIsTapped && nextPuckType != BeatPuck.PuckType.Invisible && nextPuckType != BeatPuck.PuckType.Mine)
                {

                    this._nextDeltaWinDistance = -1.0F;
                }
                else
                {
                    if ((nextPuckType == BeatPuck.PuckType.Mine && nextPuckIsTapped) || nextPuckType == BeatPuck.PuckType.Note)
                    {
                        this._parentScript.AddDamage();
                        this._beatButtonScript.SetCurrentColor(UnityEngine.Color.red);
                        
                    }
                    else if (nextPuckType != BeatPuck.PuckType.Invisible)
                    {
                        this._parentScript.BreakComboChain();
                    }
                }

            }
            
            if (nextPuckType != BeatPuck.PuckType.Mine)
            {
                //Debug.Log($"note true tone {trueNote}");
                this.QueueNote(nextNote, trueNote);
                float durationSec = nextPuckScript.GetSynthNote().GetBeat32s() * (float)this._controller.GetBeat32LentgthSec();
                this._beatButtonScript.StartReacting(durationSec, nextPuckScript.GetPuckColor(), trueNote);
            }

            


            this.SetNextPuckDistance(this._totalDistance, PuckIsVisible);
        }
        if (nextPuckDistance == 0.0F)
        {
            GameObject puckToDelete;

            if (nextPuckType == BeatPuck.PuckType.Invisible)
            {
                puckToDelete = this._currentInVisiblePucks.Dequeue();
            }
            else {
                puckToDelete = this._currentVisiblePucks.Dequeue();
            }

            if (nextPuckType != BeatPuck.PuckType.Mine)
            {
                this._currentBeatPucksCount--;
            }

            DestroyImmediate(puckToDelete);
        }
        else
        {
            BeatPuck beatPuckScript = inNextPuck.GetComponent<BeatPuck>();
            float currentBeatMarkerScale;
            float puckLeftDistance;
            if (nextPuckDistance >= this._winDistance)
            {

                puckLeftDistance = (nextPuckDistance - this._winDistance) / (this._totalDistance - this._winDistance);
                currentBeatMarkerScale = (1.0F - puckLeftDistance);

                if (beatPuckScript != null)
                {
                    beatPuckScript.SetBeatMarkerScale(currentBeatMarkerScale);
                }
            }
            else {
                puckLeftDistance = nextPuckDistance / this._winDistance;
                currentBeatMarkerScale = (1.0F - puckLeftDistance);

                if (beatPuckScript != null && !beatPuckScript.GetPlayerTapped())
                {
                    beatPuckScript.SetReadyToSet();
                    beatPuckScript.SetBeatMarkerScale(currentBeatMarkerScale);
                }
            }

        }
    }

    public bool CheckBeatsPucks() {
        bool hasSpawnedPucks = false;
        if (this._currentBeatPucksCount>0) {
            hasSpawnedPucks = true;
        }
        return hasSpawnedPucks;
    }

    public void SetState(CurrentState inCurrentState)
    {
        this._currentState = inCurrentState;
        if (inCurrentState == CurrentState.Playing)
        {
            this._staffCollider.enabled = true;

            float topWidth = this._spawnWidthCoef - (this._spawnWidthCoef * this._spawnOffset);


            this.topLeftBone.transform.localPosition = new Vector3((this._spawnPosition.x/this.staffBackground.transform.localScale.x) - topWidth / 2.0F, this._spawnPosition.y / this.staffBackground.transform.localScale.y,0.0F);
            this.topRightBone.transform.localPosition = new Vector3((this._spawnPosition.x / this.staffBackground.transform.localScale.x) + topWidth / 2.0F, this._spawnPosition.y / this.staffBackground.transform.localScale.y, 0.0F); ;
        }
        else {
            this._staffCollider.enabled = false;
        }
    }
}
