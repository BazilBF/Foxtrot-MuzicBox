using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Saw : SynthInstrument
{

    public Saw(float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        this.waveType = Wave.Saw;
        this.canSustain = true;
        this.modulatingGaing = 0.0F;
        

    }
}