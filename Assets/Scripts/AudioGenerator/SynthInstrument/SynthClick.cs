using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SynthClick : SynthInstrument
{

    public SynthClick(float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        this.atackLength = 0.2F;
        this.fadeLength = 0.8F;
        this.waveType = Wave.WhiteNoise;
        this.modulatingGaing = 0;
        this.distortionFlg = false;

    }
    protected override void ModifyAttack(float inAttackPhase)
    {
        float checkPhase = inAttackPhase + 1;
        this._currentGainCoef = 1 / Mathf.Pow(checkPhase,10);
    }

    protected override void ModifyFade(float inFadePhase)
    {
        this._currentGainCoef = 0;
    }

    protected override void ModifySustain()
    {
        //this.currentGainCoef = 0;
    }

}
