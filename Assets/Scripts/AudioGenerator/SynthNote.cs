using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;


public class SynthNote
{
    private static IDictionary<string, float> scale = new Dictionary<string, float>();
    private static IDictionary<string, FalseNote> falseScale = new Dictionary<string, FalseNote>();

    private readonly System.Random _randObj = new System.Random();

    private readonly string noteStr;
    private readonly int octave;
    private readonly int beat32s;
    private readonly float gain;

    private class FalseNote {
        private float[][] _falseScales;
        private readonly System.Random _randObj = new System.Random();

        public FalseNote(float[][] inFalseScales) {
            this._falseScales = inFalseScales;
        }

        public float[] GetFalseScale() {
            int returnIndex = this._randObj.Next(this._falseScales.Length);
            return this._falseScales[returnIndex];
        }


    }

    //Setting scale for our instrument at the 0st octave
    static SynthNote()
    {
        scale.Add("C", 16.35F);
        scale.Add("C#", 17.32F);
        scale.Add("D", 18.35F);
        scale.Add("D#", 19.45F);
        scale.Add("E", 20.6F);
        scale.Add("F", 21.83F);
        scale.Add("F#", 23.12F);
        scale.Add("G", 24.5F);
        scale.Add("G#", 25.96F);
        scale.Add("A", 27.5F);
        scale.Add("A#", 29.14F);
        scale.Add("B", 30.87F);

        falseScale.Add("C",  new FalseNote(new float[2][] { new float[2] { 17.32F, 0 }, new float[2] { 23.12F, 0 } }));
        falseScale.Add("C#", new FalseNote(new float[2][] { new float[2] { 18.35F, 0 }, new float[2] { 24.5F, 0 } }));
        falseScale.Add("D",  new FalseNote(new float[2][] { new float[2] { 19.45F, 0 }, new float[2] { 25.96F, 0 } }));
        falseScale.Add("D#", new FalseNote(new float[2][] { new float[2] { 20.6F, 0 },  new float[2] { 27.5F, 0 } }));
        falseScale.Add("E",  new FalseNote(new float[2][] { new float[2] { 21.83F, 0 }, new float[2] { 29.14F, 0 } }));
        falseScale.Add("F",  new FalseNote(new float[2][] { new float[2] { 23.12F, 0 }, new float[2] { 30.87F, 0 } }));
        falseScale.Add("F#", new FalseNote(new float[2][] { new float[2] { 24.5F, 0 },  new float[2] { 16.35F, 1 } }));
        falseScale.Add("G",  new FalseNote(new float[2][] { new float[2] { 25.96F, 0 }, new float[2] { 17.32F, 1 } }));
        falseScale.Add("G#", new FalseNote(new float[2][] { new float[2] { 27.5F, 0 },  new float[2] { 18.35F, 1 } }));
        falseScale.Add("A",  new FalseNote(new float[2][] { new float[2] { 29.14F, 0 }, new float[2] { 19.45F, 1 } }));
        falseScale.Add("A#", new FalseNote(new float[2][] { new float[2] { 30.87F, 0 }, new float[2] { 20.6F, 1 } }));
        falseScale.Add("B",  new FalseNote(new float[2][] { new float[2] { 16.35F, 1 }, new float[2] { 21.83F, 1 } }));
    }

    public SynthNote(string inNoteStr, int inOctave, int inBeats, float inGain) { 
        this.noteStr = inNoteStr;
        this.octave = inOctave;
        this.beat32s = inBeats;
        this.gain = (inGain != null && inGain > 0.0F ? inGain : 1.0F);
    }

    public SynthNote(string inNoteStr, int inOctave, float inMeasure, int inLength, float inGain)
    {
        this.noteStr = inNoteStr;
        this.octave = inOctave;
        this.beat32s = (int)(inMeasure*(32.0F/(float)inLength));
        this.gain = (inGain != null && inGain > 0.0F ? inGain : 1.0F);
    }


    public string GetNote()
    {
        return this.noteStr;
    }

    public int GetOctave()
    {
        return this.octave;
    }

    public int GetBeat32s()
    {
        return this.beat32s;
    }

    public float GetGain() {
        return this.gain;
    }

    public float GetFrequency(bool inTrueNoteFlg) { 
        float frequency = 0f;
        if (inTrueNoteFlg)
        {
            frequency = SynthNote.scale[this.noteStr] * (float)Math.Pow(2, this.octave) + this._randObj.Next(-2,+2);
        }
        else
        {
            FalseNote falseNote = SynthNote.falseScale[this.noteStr];
            float[] falseNoteData = falseNote.GetFalseScale();

            frequency = (falseNoteData[0] + this._randObj.Next(-5, 5)) * (float)Math.Pow(2, this.octave + falseNoteData[1]);
        }
        return frequency;
    }


}
