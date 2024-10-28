using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatButtonScript : MonoBehaviour
{
    private UnityEngine.Color _defaultColor = new UnityEngine.Color(1.0F, 1.0F, 1.0F);
    private UnityEngine.Color _currentColor = new UnityEngine.Color(1.0F, 1.0F, 1.0F);

    private SpriteRenderer _beatButtonSpriteRender;
    
    private Vector2 _beatButtonTargetPosition;
    private float _beatButtonOffset;
    private float _beatButtonScale;
    private float _beatButtonWidth;

    private SynthInstrument _staffInstrument;

    public GameObject particleSystem = null;
    public float beatButtonMaxScale = 0.1F;

    private float _phase = 1.0F;
    private float _speedPerSecond = 0.0F;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this._speedPerSecond > 0.0F)
        {
            ProcessBeatButton();
            this._phase += Time.deltaTime * this._speedPerSecond;
            if (this._phase > 1.0F)
            {
                this.ResetBeatButton();
            }
        }
    }

    private void ResetBeatButton() {
        this._speedPerSecond = 0.0F;
        this._phase = 1.0F;
        this.transform.localPosition = this._beatButtonTargetPosition;
        this._beatButtonSpriteRender.color = this._defaultColor;

        ParticleSystem tmpParticleSystem = this.particleSystem.GetComponent<ParticleSystem>();
        tmpParticleSystem.Stop();
        
    }

    private void ProcessBeatButton()
    {

        float currentAmplitude = this._staffInstrument.GetCurrentAmplitude();
        this.transform.localPosition = new Vector3(0, this._beatButtonTargetPosition.y + currentAmplitude * this.beatButtonMaxScale * this._beatButtonScale, 0);

        float colorCoef = 1.0F - this._phase;

        float newColorR = this._defaultColor.r - (this._defaultColor.r - this._currentColor.r) * colorCoef;
        float newColorG = this._defaultColor.g - (this._defaultColor.g - this._currentColor.g) * colorCoef;
        float newColorB = this._defaultColor.b - (this._defaultColor.b - this._currentColor.b) * colorCoef;

        this._beatButtonSpriteRender.color = new UnityEngine.Color(newColorR, newColorG, newColorB);


    }

    public void SetBeatButton(float inBeatButtonWidth, float inBeatButtonY, SynthInstrument inSynthInstrument) {
        this._beatButtonSpriteRender = this.GetComponent<SpriteRenderer>();
        this._beatButtonSpriteRender.sortingLayerName = "Pucks";
        this._beatButtonSpriteRender.color = this._defaultColor;
        this._beatButtonSpriteRender.sortingOrder = 3;
        this._staffInstrument = inSynthInstrument;
        this._beatButtonWidth = inBeatButtonWidth;
        this._beatButtonTargetPosition = new Vector2(0, inBeatButtonY);
        this.transform.localPosition = Vector3.zero;

        

        this._beatButtonScale = 0.30F * (this._beatButtonWidth / this._beatButtonSpriteRender.sprite.bounds.size.x);
        this.transform.localScale = new Vector3(this._beatButtonScale, this._beatButtonScale, 0);
    }

    public void SetBeatAlpha(float inAlpha) {
        UnityEngine.Color tmpBeatColor = this._beatButtonSpriteRender.color;
        tmpBeatColor.a = inAlpha;
        this._beatButtonSpriteRender.color = tmpBeatColor;
    }

    public void SetCurrentColor(UnityEngine.Color inColor) { 
        this._currentColor = inColor;
    }

    public void ProcessMove(float inPhase) {
        float newY = inPhase * this._beatButtonTargetPosition.y;
        this.transform.localPosition = new Vector3(0.0F, newY, 0.0F);
    }

    public Vector2 GetBeatButtonTargetPosition() {
        return this._beatButtonTargetPosition;
    }

    public void StartReacting(float inDurationSec, UnityEngine.Color inColor, bool isTrue) {
        this._phase = 0.0F;
        this._speedPerSecond = 1.0F / inDurationSec;
        this._currentColor = inColor;
        if (isTrue)
        {
            ParticleSystem tmpParticleSystem = this.particleSystem.GetComponent<ParticleSystem>();
            var mainParticleSystem = tmpParticleSystem.main;
            mainParticleSystem.startColor = inColor;
            tmpParticleSystem.Play();
        }
    }
}

