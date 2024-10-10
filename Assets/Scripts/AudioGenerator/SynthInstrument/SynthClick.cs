using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SynthClick : SynthInstrument
{

    public SynthClick(float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        this.fadeLength = 0.8F;
        this.waveType = Wave.WhiteNoise;
        this.modulatingGaing = 0;
        this.distortionFlg = false;

    }
    protected override float FaidGain(double inFadePhase)
    {
        double checkPhase = inFadePhase + 1;
        return 1 / Mathf.Pow((float)checkPhase,10);
    }

}
