using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SynthTap : SynthInstrument
{

    public SynthTap(float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        
        this.waveType = Wave.Pulse;
        this.modulatingGaing = 0;
        this.noiseLevel = 0.8F;
        this.fadeLength = 2.0F;
        this.pulseLength = 0.7F;

    }
    

}
