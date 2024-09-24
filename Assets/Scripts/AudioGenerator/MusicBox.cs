using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox 
{
    private readonly float _sampleRate;

    private SynthInstrument[] _musicBoxInstruments;
    
    private readonly SynthInstrument _musicBoxMethronome;
    private readonly SynthNote _beatNote = new SynthNote("C", 5, 1, 1.0F);
    private float _totalGain = 0.0F;

    private int _instrumentsCount = 0;

    public MusicBox(string[] inInstruments, float inTotalInstrumentsGain, float inSampleRate)
    {
        this._sampleRate = inSampleRate;

        this._instrumentsCount = inInstruments.Length;
        this._musicBoxInstruments = new SynthInstrument[_instrumentsCount];

        this._musicBoxMethronome = new SynthClick(this._sampleRate, 1.0F);

        this._totalGain = inTotalInstrumentsGain + 1.0F; //1.0F - for methronome
    }

    public MusicBox(SynthInstrument[] inInstruments, float inTotalInstrumentsGain, float inSampleRate)
    {
        this._sampleRate = inSampleRate;
        this._instrumentsCount = inInstruments.Length;
        this._musicBoxInstruments = inInstruments;
                
        this._totalGain += inTotalInstrumentsGain + 1.0F; //1.0F - for methronome

        this._musicBoxMethronome = new SynthClick(this._sampleRate, 1.0F);

    }

    public static SynthInstrument GetSynthInstrument(string inInstrument, float inSampleRate, float inInstrumentsGain) {
        SynthInstrument returnSynthInstrument;
        switch (inInstrument)
        {
            case "Click":
                returnSynthInstrument = new SynthClick(inSampleRate, inInstrumentsGain); break;
            case "Saw":
                returnSynthInstrument = new Saw(inSampleRate, inInstrumentsGain); break;
            case "Sin":
                returnSynthInstrument = new Sin(inSampleRate, inInstrumentsGain); break;
            case "Square":
                returnSynthInstrument = new Square(inSampleRate, inInstrumentsGain); break;
            default:
                returnSynthInstrument = new SynthInstrument(inSampleRate, inInstrumentsGain); break;
        }
        return returnSynthInstrument;
    }

    

    public bool CheckIsPlaying() {
        bool isPlaying = false;
        for (int i=0; i < this._instrumentsCount; i++) {
            
            if (this._musicBoxInstruments[i].CheckIsPlaying()) {
                isPlaying = true; break;
            }
        }
        return isPlaying;
    }

    public void ResetMusicBox(SynthInstrument[] inInstruments) {
        this._musicBoxInstruments = inInstruments;
    }

    public void SetInstruments(string[] inIntsruments, float[] inInstrumentsGain) {
        this._instrumentsCount = inIntsruments.Length;
        this._musicBoxInstruments = new SynthInstrument[_instrumentsCount];

        for (int i = 0; i < this._instrumentsCount; i++)
        {
            SynthInstrument newInstrument;
            switch (inIntsruments[i])
            {
                case "Click":
                    newInstrument = new SynthClick(this._sampleRate, inInstrumentsGain[i]); break;
                default:
                    newInstrument = new SynthInstrument(this._sampleRate, inInstrumentsGain[i]); break;
            }

            this._musicBoxInstruments[i] = newInstrument;
        }
    }

    public float playNotes(MusicCoordinates inMusicCoordinates, double inBeatLentgth, float inMetronomeGain, float inPitchCoef = 1.0F) {
        
        float amplitude = 0.0F;

        for (int i = 0;i < this._instrumentsCount;i++)
        {
            float instrumentAmplitude = this._musicBoxInstruments[i].PlayNote(inMusicCoordinates, inBeatLentgth, inPitchCoef);

            if (Math.Abs(instrumentAmplitude) > 0) {
                amplitude += instrumentAmplitude;
            }
        }

        amplitude += this._musicBoxMethronome.PlayNote(inMusicCoordinates, inBeatLentgth) * inMetronomeGain;

        return amplitude / this._totalGain;
    }

    public void beatStroke(MusicCoordinates inMusicCoordinates) {
        this._musicBoxMethronome.KeyStroke(this._beatNote, inMusicCoordinates, true);
    }

    public void NextNoteStroke(MusicCoordinates inMusicCoordinates) {
        for (int i = 0; i < this._instrumentsCount; i++)
        {
            if (this._musicBoxInstruments[i].HasNextNote())
            {
                this._musicBoxInstruments[i].KeyStrokeNextNote(inMusicCoordinates);
            }
        }
    }

    public SynthInstrument getMusicBoxInstrument(int i) {
        return this._musicBoxInstruments[i];
    }
}
