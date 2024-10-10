using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Sin : SynthInstrument
{

    public Sin(float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        this.waveType = Wave.Sin;
        this.canSustain = true;
        this.modulatingGaing = 0.2F;
        this.distortionFlg = false;


    }
}