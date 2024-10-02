using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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
    private SpriteRenderer _beatButtonSpriteRender = null;

    private float _beatButtonOffset = 0.1F;
    private float _beatButtonWidth = 0.0F;
    private float _beatButtonScale = 1.0F;
    private float _beatButtonMaxScale = 0.1F;
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
    private Vector2 _beatButtonTargetPosition;

    private float _winDistance = 0.1F;

    private int _staffNumber = 0;

    public void SetStaff(float inStaffWidth, float inStaffHeight, float inBeatButtonMaxWidth, Vector3 inGlobalXSpawnPosition, SynthInstrument inInstrument, int inStaffNumber) {
        this._staffWidth = inStaffWidth;
        this._staffHeight = inStaffHeight;
        //Vector3 localXSpawnPosition = this.transform.InverseTransformPoint(inGlobalXSpawnPosition);
        this._spawnPosition = this.transform.InverseTransformPoint(inGlobalXSpawnPosition);
        this._staffInstrument = inInstrument;
        this._parentScript = this.transform.parent.GetComponent<FieldContainer>();
        this._demoModeFlag = this._parentScript.GetDemoMode();
        this._staffNumber = inStaffNumber;

        this._staffCollider = this.transform.GetComponent<BoxCollider2D>();
        if (this._staffCollider != null) {
            this._staffCollider.size = new Vector2(inStaffWidth, inStaffHeight);
            this._staffCollider.enabled = false;
        }

        this._beatLentgthSec = this._parentScript.GetBeatLentgthSec();

        float newBeatButtonWidth = this._staffWidth * (1.0F - this._beatButtonOffset);
        this._beatButtonWidth = (1 - this._beatButtonOffset) * Mathf.Min(newBeatButtonWidth, inBeatButtonMaxWidth);


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

        this._beatButtonSpriteRender = beatButton.GetComponent<SpriteRenderer>();
        this._beatButtonSpriteRender.sortingLayerName = "Pucks";
        this._beatButtonSpriteRender.sortingOrder = 3;
        

        float beatButtonY = -(1 - this._beatButtonOffset) * this._staffHeight / 2.0F + this._beatButtonWidth / 2.0F;
        
        this.beatButton.transform.parent = this.gameObject.transform;
        this._beatButtonScale = 0.30F * (this._beatButtonWidth / this._beatButtonSpriteRender.sprite.bounds.size.x);

        beatButton.transform.localScale = new Vector3(this._beatButtonScale, this._beatButtonScale, 0);
        this._beatButtonTargetPosition = new Vector2(0, beatButtonY);
        this.beatButton.transform.localPosition = Vector3.zero;
        
        this._totalDistance = Vector3.Distance(this._beatButtonTargetPosition, this._spawnPosition);
        this._winDistance = this._totalDistance * this._parentScript.GetWinDistanceCoef();
        Debug.Log($"win distance{this._winDistance} total distance {this._totalDistance} coef {this._parentScript.GetWinDistanceCoef()}");
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
        movingScript.SetObjectMove(new Vector2(inBarrGain*(1.0F - this._spriteOffset) * this._staffWidth, this._staffWidth * this._spriteOffset * inBarrGain),this._beatButtonTargetPosition, (float) inDuration, this._parentScript.GetGameController());

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

        movingScript.SetObjectMove(new Vector2((1.0F - this._beatPuckOffset) * this._beatButtonWidth, 0.95F*(1.0F - this._beatPuckOffset) * this._beatButtonWidth), this._beatButtonTargetPosition, (float)inDuration, this._parentScript.GetGameController());

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

    public void PlayerTaped()
    {
        Debug.Log($"[Touch Staff][Staff #{this._staffNumber}][{this._currentState}]");
        if (this._currentState == CurrentState.Playing)
        {
            GameObject nextPuck;
            bool hasPuckToPeek = this._currentVisiblePucks.TryPeek(out nextPuck);

            float nextPuckDistance = this.GetNextPuckDistance(1);

            Debug.Log($"[Touch Staff][Staff #{this._staffNumber}][{hasPuckToPeek}][{nextPuckDistance <= this._totalDistance * 0.95F}]");

            if (hasPuckToPeek && nextPuckDistance <= this._totalDistance * 0.95F)
            {
                
                BeatPuck nextPuckScript = nextPuck.GetComponent<BeatPuck>();

                if (nextPuckDistance <= this._winDistance && !this._playerTapped && nextPuckScript.GetPuckType() != BeatPuck.PuckType.Mine)
                {
                    float deltaWinDistance = nextPuckDistance / this._winDistance;
                    this._nextDeltaWinDistance = deltaWinDistance;
                    nextPuckScript.SetTrueNote(deltaWinDistance);
                }
                else {

                    nextPuckScript.SetFalseNote();
                }
                nextPuckScript.SetPlayerTapped();
            }
        }
    }

    public float GetDeltaDistance(int inPuckVisibilityFlag = 1) {
        return this.GetNextPuckDistance(inPuckVisibilityFlag) / this._totalDistance;
    }

    // Update is called once per frame
    void Update()
    {

        UnityEngine.Color tmpBeatColor = this._beatButtonSpriteRender.color;

        if (this._currentState == CurrentState.Playing) {

            beatButton.transform.localPosition = new Vector3(0, this._beatButtonTargetPosition.y + +this._staffInstrument.GetCurrentAmplitude() * this._beatButtonMaxScale * this._beatButtonScale, 0);

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
                float nextBarrDistance = Vector3.Distance(this._beatButtonTargetPosition, nextBeatBarr.transform.localPosition);
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
            

            float newY = phase * this._beatButtonTargetPosition.y;

            float check = phase * this._beatButtonTargetPosition.y;

            this.beatButton.transform.localPosition = new Vector3(0.0F, newY, 0.0F);

            float newScaleX = phase * this._backgroundTargetScale.x;
            float newScaleY = phase * this._backgroundTargetScale.y;

            this.staffBackground.transform.localScale = new Vector3(newScaleX, newScaleY, 0.0F);

            if (phase == 1.0F)
            {
                this.SetState(CurrentState.Playing);
            }

            tmpBeatColor.a = 1.0F;
            this._beatButtonSpriteRender.color = tmpBeatColor;

            UnityEngine.Color tmpStaffColor = this._staffSpriteRenderer.color;

            if (tmpStaffColor.a > 0.0F)
            {
                tmpStaffColor.a -= Time.deltaTime;
                this._staffSpriteRenderer.color = tmpStaffColor;
            }

        }

        if (this._currentState == CurrentState.Orbiting) {
            tmpBeatColor.a = 0.4F;
            this._beatButtonSpriteRender.color = tmpBeatColor;
        }
    }

    private void ProcessBeatPuck(GameObject inNextPuck) {
        float nextPuckDistance = Vector3.Distance(this._beatButtonTargetPosition, inNextPuck.transform.localPosition);
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
                    this._parentScript.AddScore(this._nextDeltaWinDistance,(nextPuckType == BeatPuck.PuckType.Bonus));
                    this._nextDeltaWinDistance = -1.0F;
                }
                else
                {
                    if ((nextPuckType == BeatPuck.PuckType.Mine && nextPuckIsTapped) || nextPuckType == BeatPuck.PuckType.Note)
                    {
                        this._parentScript.AddDamage();
                    }
                    else if (nextPuckType != BeatPuck.PuckType.Invisible ) {
                        this._parentScript.BreakComboChain();
                    }                    
                }
            }
            if (nextPuckType != BeatPuck.PuckType.Mine)
            {
                //Debug.Log($"note true tone {trueNote}");
                this.QueueNote(nextNote, trueNote);
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

                if (beatPuckScript != null)
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
            
            this.topLeftBone.transform.localPosition = new Vector3(this._spawnPosition.x/this.staffBackground.transform.localScale.x - this._spriteOffset / 2.0F, this._spawnPosition.y / this.staffBackground.transform.localScale.y,0.0F);
            this.topRightBone.transform.localPosition = new Vector3(this._spawnPosition.x / this.staffBackground.transform.localScale.x + this._spriteOffset / 2.0F, this._spawnPosition.y / this.staffBackground.transform.localScale.y, 0.0F); ;
        }
        else {
            this._staffCollider.enabled = false;
        }
    }
}
