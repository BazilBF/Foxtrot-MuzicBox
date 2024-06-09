using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BeatPuck : MonoBehaviour
{

    private SynthNote _thisSynthNote;
    private bool _playerTapped = false;
    private bool _trueNote = false;
    private SpriteRenderer _puckSpriteRenderer;
    private PuckType _puckType;
    private float _deltaWinDistance;
    private float _beatMarkerScale;

    private SpriteRenderer _beatMarkSpriteRenderer;

    public GameObject beatMarker;

    public enum PuckType
    {
        Mine,
        Note,
        Bonus,
        Invisible,
        NightCore
    }

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

    }

    public void SetPuckType(PuckType inPuckType) {
        this._puckType = inPuckType;
        switch (this._puckType) {
            case PuckType.Mine:
                this._puckSpriteRenderer.color = new UnityEngine.Color(0.3f, 0.0f, 0.0f, 0.65f);
                break;
            case PuckType.Invisible:
                this._puckSpriteRenderer.color = new UnityEngine.Color(1.0f, 1.0f, 1.0f, 0.05f);
                break;
            case PuckType.Note: 
                this._puckSpriteRenderer.color = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 0.65f); 
                break;
            case PuckType.Bonus:
                this._puckSpriteRenderer.color = new UnityEngine.Color(0.85f, 0.70f, 0.35f, 0.65f);
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
        if (this._puckType == PuckType.Note || this._puckType == PuckType.Bonus)
        {
            this._beatMarkerScale = this._puckSpriteRenderer.sprite.bounds.size.x * (this._puckSpriteRenderer.sprite.bounds.size.x / _beatMarkSpriteRenderer.sprite.bounds.size.x) * inScale;
            this.beatMarker.transform.localScale = new Vector3(this._beatMarkerScale, this._beatMarkerScale, 0.0F);
            if (inScale == 1.0F && !this._playerTapped) {
                this._beatMarkSpriteRenderer.color = new UnityEngine.Color(0.85f, 0.70f, 0.35f, 1.0f);
            }
        }
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

    public void SetTrueNote(float inDeltaWinDistance) {
        if (!this._playerTapped) {
            this._trueNote = true;
            this._deltaWinDistance = inDeltaWinDistance;
            this._beatMarkSpriteRenderer.color = new UnityEngine.Color(0.0f, 0.8f, 0.0f, 1.0f);
        }
    }

    public void SetFalseNote() {
        if (!this._playerTapped)
        {
            this._beatMarkSpriteRenderer.color = new UnityEngine.Color(1.0f, 0.0f, 0.0f, 1.0f);
            this._puckSpriteRenderer.color = new UnityEngine.Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public bool GetPlayerTapped() {
        return this._playerTapped;
    }

    public bool GetTrueTone() {
        return this._puckType == PuckType.Invisible || this._puckType == PuckType.Bonus || this._trueNote;
    }

}
