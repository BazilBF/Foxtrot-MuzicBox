using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Square : SynthInstrument
{

    public Square(float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        this.waveType = Wave.Square;
        this.modulatingGaing = 0.0F;
        

    }
}