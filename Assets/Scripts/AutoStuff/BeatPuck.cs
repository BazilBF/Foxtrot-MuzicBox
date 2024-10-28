using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BeatPuck : MonoBehaviour
{

    public enum PuckType
    {
        Mine,
        Note,
        Bonus,
        Invisible,
        NightCore
    }

    private SynthNote _thisSynthNote;
    private bool _playerTapped = false;
    private bool _trueNote = false;
    private SpriteRenderer _puckSpriteRenderer;
    private PuckType _puckType;
    private float _deltaWinDistance;
    private float _beatMarkerScale;
    private bool _readyToSet = false;
    public GameObject particleSystem = null;

    private SpriteRenderer _beatMarkSpriteRenderer;

    public GameObject beatMarker;

    

    // Start is called before the first frame update
    void Awake()
    {
        this._puckSpriteRenderer = this.transform.GetComponent<SpriteRenderer>();
        this._puckSpriteRenderer.sortingLayerName = "Pucks";
        this._puckSpriteRenderer.sortingOrder = 1;

        this._beatMarkSpriteRenderer = this.beatMarker.GetComponent<SpriteRenderer>();
        this.beatMarker.transform.localScale = Vector3.zero;
        this._beatMarkSpriteRenderer.sortingLayerName = "Pucks";
        this._beatMarkSpriteRenderer.sortingOrder = 2;

        this.SetBeatMarkerColor(new UnityEngine.Color(0.0f, 0.5f, 0.5f, 1.0f));

    }

    public void SetPuckType(PuckType inPuckType) {
        this._puckType = inPuckType;
        switch (this._puckType) {
            case PuckType.Mine:
                this._puckSpriteRenderer.color = new UnityEngine.Color(1.0f, 0.08f, 0.08f, 1.0f);
                break;
            case PuckType.Invisible:
                this._puckSpriteRenderer.color = new UnityEngine.Color(0.5f, 0.5f, 0.5f, 0.1f);
                break;
            case PuckType.Note: 
                this._puckSpriteRenderer.color = new UnityEngine.Color(0.85f, 0.85f, 0.85f, 1.0f); 
                break;
            case PuckType.Bonus:
                this._puckSpriteRenderer.color = new UnityEngine.Color(1.0f, 0.71f, 0.1f, 1.0f);
                break;
            case PuckType.NightCore: break;
        }
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBeatPuck(SynthNote inSynthNote) { 
        this._thisSynthNote = inSynthNote;        
    }

    public void SetBeatMarkerScale(float inScale) {
        if ((this._puckType == PuckType.Note || this._puckType == PuckType.Bonus)&&(!this._playerTapped))
        {
            this._beatMarkerScale = this._puckSpriteRenderer.sprite.bounds.size.x * (this._puckSpriteRenderer.sprite.bounds.size.x / _beatMarkSpriteRenderer.sprite.bounds.size.x) * inScale;
            this.beatMarker.transform.localScale = new Vector3(this._beatMarkerScale, this._beatMarkerScale, 0.0F);
            
        }
    }

    public void SetBeatMarkerColor(UnityEngine.Color inColor) {
        this._beatMarkSpriteRenderer.color = inColor;
    }

    public void SetPuckColor(UnityEngine.Color inColor)
    {
        this._puckSpriteRenderer.color = inColor;
    }

    public SynthNote GetSynthNote() { 
        return this._thisSynthNote;
    }

    public PuckType GetPuckType() {
        return this._puckType;
    }

    public float GetDeltaWinDistance() {
        return this._deltaWinDistance;
    }

    public void SetPlayerTapped() {
        this._playerTapped = true;
    }

    public void SetReadyToSet() {
        if (!this._readyToSet && (this._puckType == PuckType.Note || this._puckType == PuckType.Bonus))
        {
            this._readyToSet = true;
            this.SetPuckColor(new UnityEngine.Color(0.0f, 0.5f, 0.5f, 1.0f));
            this.SetBeatMarkerColor(new UnityEngine.Color(0.1f, 0.1f, 1.0f, 1.0f));
        }
    }

    public void SetTrueNote(float inDeltaWinDistance, UnityEngine.Color inColor) {
        if (!this._playerTapped) {
            this._trueNote = true;
            this._deltaWinDistance = inDeltaWinDistance;
            this.SetPuckColor(new UnityEngine.Color(0.0f, 0.8f, 0.0f, 1.0f));
            this.SetBeatMarkerColor(inColor);

            ParticleSystem tmpParticleSystem = this.particleSystem.GetComponent<ParticleSystem>();
            var emitParams = new ParticleSystem.EmitParams();
            emitParams.startColor = inColor;
            emitParams.startSize = 0.2f;
            tmpParticleSystem.Emit(emitParams,10);
        }
    }

    public void SetFalseNote() {
        if (!this._playerTapped)
        {
            
            this.SetPuckColor (new UnityEngine.Color(0.8f, 0.0f, 0.0f, 1.0f));
            this.SetBeatMarkerColor(new UnityEngine.Color(1.0f, 0.0f, 0.0f, 1.0f));
        }
    }

    public UnityEngine.Color GetPuckColor() { 
        return this._beatMarkSpriteRenderer.color;
    }

    public bool GetPlayerTapped() {
        return this._playerTapped;
    }

    public bool GetTrueTone() {
        return this._puckType == PuckType.Invisible || this._puckType == PuckType.Bonus || this._trueNote;
    }

}
