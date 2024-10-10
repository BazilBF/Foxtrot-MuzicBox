using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public enum Wave
{
    Pulse,
    Square,
    Saw,
    Triangle,
    Sin,
    WhiteNoise
}

public class SynthInstrument
{
    //Tweaks for inhereited instruments
    protected Wave waveType = Wave.Saw; //wave type for current instrument

    protected Wave mWaveType = Wave.Sin; //wave type for modulating
    protected float modulatingGaing = 0.0F;
    protected float mFrequency = 7; //frequency for modulating

    protected float fadeLength = 2.0F; //length of Fade Phase as a part of Beat

    protected static float pulseLength = 0.1F; // lehgth of the pulse

    protected bool distortionFlg = true; // apply distortion to frequency
    protected float distortionGain = 0.3F;

    protected bool canSustain = false;

    //Internal Settings
    private SynthNote _noteToPlay;
    private SynthNote _nextNoteToPlay;
    private bool _trueNote = false;

    private int _beats; //Length of the note in seconds
    private readonly float _sampleRate;
    private float _instrumentGainCoef;
    private MusicCoordinates _musicCoordinatesStart;

    private float _currentAmplitude; 
    

    //Runtime parametrs
    private readonly static System.Random _randObj = new System.Random();

    protected int _samplesPlayed = 0;
    protected float _phase = 0; //phase for carrying sound

    protected float _mPhase = 0; //phase for modulating sound

    protected bool _isPlayingFlg = false; //flag that note is playing

    protected bool _isBass = false;

    public SynthInstrument(float inSampleRate, float ininstrumentGainCoef){
        this._sampleRate = inSampleRate;
        this._instrumentGainCoef = ininstrumentGainCoef; 
    }

    public float PlayNote(MusicCoordinates inMusicCoordinates, double inBeatLentgth, float inPitchCoef = 1.0F)
    {
        
        float amplitude = 0F;
        if(this._isPlayingFlg){
            
            
            double currentLength = this._samplesPlayed / (double) this._sampleRate;
            double fadeLength = inBeatLentgth * this.fadeLength;
            double wholeNoteLength = this._beats * inBeatLentgth;
            double currentPlayPhase;

            amplitude = this.GetAmplitude();

            if (this.distortionFlg)
            {
                amplitude = this.GetdistortionGain(amplitude);
            }

            if (!this._trueNote)
            {
                amplitude = amplitude * (0.8F + 0.2F * (float)SynthInstrument._randObj.NextDouble());
            }
            
            ChangePhase(inPitchCoef);
            _samplesPlayed++;


            if (this.canSustain && currentLength <= wholeNoteLength)
            {
                currentPlayPhase = currentLength / wholeNoteLength;
                amplitude *= this.SustainGain(currentPlayPhase);
            }
            else if (currentLength <= wholeNoteLength + fadeLength)
            {

                currentPlayPhase = (currentLength - (this.canSustain ? wholeNoteLength : 0.0)) / fadeLength;
                amplitude *= this.FaidGain(currentPlayPhase);
            }
            else {
                this._isPlayingFlg = false;
                this._currentAmplitude = 0.0F;
            }
            this._currentAmplitude = amplitude;


        }

        return amplitude;
    }

    public float GetCurrentAmplitude() {
        return this._currentAmplitude;
    }

    private void ResetPlayParams() {

        this._samplesPlayed = 0;
        this._isPlayingFlg = false;
        this._noteToPlay = null;
        this._nextNoteToPlay = null;
        this._beats = 0;
        this._trueNote = false;
        this._phase = 0;

    }

    public float GetInstrumentGain() {
        return this._instrumentGainCoef;
    }

    public void SetInstrumentGain(float inNewGain)
    {
        this._instrumentGainCoef = inNewGain;
    }

    protected virtual float FaidGain(double inFadePhase){
        
        return inFadePhase<1.0F ? 1.0F - (float)inFadePhase : 0.0F;
    }

    protected virtual float SustainGain(double inSustainPhase) {
        return 1.0F;
    }

    protected virtual float GetdistortionGain(float inAmplitude) {
        float distortedAmplitude = inAmplitude;
        //distortedAmplitude = inAmplitude * (float) Math.Pow(inAmplitude,3);
        distortedAmplitude = inAmplitude * (float)Math.Tanh(inAmplitude);
        return distortedAmplitude;
    }

    protected virtual float GetAmplitude(){
        float amplitude=0.0F;
        try
        {
            amplitude = SynthInstrument.GetAmplitudeByWave(this._phase, this.waveType) * this.Modulate() * this._instrumentGainCoef * this._noteToPlay.GetGain();

            if (amplitude > 1.0F)
            {
                Debug.Log("wat");
            }
        }
        catch {
            Debug.Log("Wat");
        }
        //Debug.Log($"Amplitude = {this.phase}/{amplitude}");
        return amplitude;
    }

    protected virtual float Modulate(){
        float mCoef=1;
        if (this.modulatingGaing>0 && this.modulatingGaing <1){
            float modulatingGaingCalc = 1.0F - this.modulatingGaing;
            float modulateAmplitude = 0.5F + SynthInstrument.GetAmplitudeByWave(this._mPhase, this.mWaveType)/2;
            mCoef = modulatingGaingCalc + this.modulatingGaing * modulateAmplitude;
        }
        
        return mCoef;
    }

    public void QueueNote(SynthNote inNoteToPlay, bool inTrueNote) {
        this._nextNoteToPlay = inNoteToPlay;
        this._trueNote = inTrueNote;
    }

    public void KeyStrokeNextNote(MusicCoordinates inMusicCoordinates) {
        if (this._nextNoteToPlay != null)
        {
            this.KeyStroke(this._nextNoteToPlay, inMusicCoordinates, this._trueNote);
            this._nextNoteToPlay = null;
        }
    }

    public bool HasNextNote() {
        return (this._nextNoteToPlay == null ? false : true);
    }

    public void KeyStroke(SynthNote inNoteToPlay, MusicCoordinates inMusicCoordinates, bool inTrueNote)
    {
        this.ResetPlayParams();
        this._noteToPlay = inNoteToPlay;
        this._beats = inNoteToPlay.GetBeat32s();
        this._isPlayingFlg = true;
        this._musicCoordinatesStart = inMusicCoordinates;
        this._trueNote = inTrueNote;
    }
    
    private void ChangePhase(float inPitchCoef)
    {
        //double phaseTime = inCurrentLength - Math.Truncate(inCurrentLength);

        float frequencyCalc = this._noteToPlay.GetFrequency(_trueNote) * (this._isBass ? 1.0F : inPitchCoef);
        float mFrequencyCalc = this.mFrequency;

        this._phase += (float)(frequencyCalc/ this._sampleRate);
        this._mPhase += (float)(mFrequencyCalc / this._sampleRate);
        if (this._phase >= 1)
        {
            this._phase = 0;
        }
        
        if (this._mPhase >= 1)
        {
            this._mPhase = 0;
        }
    }

    public static float GetSaw(float inPhase){
        float amplitude = 0.0F;// 1.45F*(2.0F * inPhase - 1.0F)*(1.0F - (float)Math.Pow((2 * inPhase-1),8));
        
         
        if(inPhase<0.5F && inPhase>=0){
            amplitude = inPhase;
        }
        else{
            amplitude = inPhase - 1;
        }
        return amplitude * 2;
        //return amplitude;
    }

    public static float GetSquare(float inPhase){ 
        float amplitude = 0;
        if(inPhase < 0.5F && inPhase >= 0){
            amplitude = 1;
        }
        else{
            amplitude = -1;
        }
        return amplitude;
    }

    public static float GetPulse(float inPhase){
        float amplitude = 0;
        if (inPhase < SynthInstrument.pulseLength && inPhase >= 0)
        {
            amplitude = 1;
        }
        else
        {
            amplitude = -1;
        }
        return amplitude;
    }

    public static float GetTriangle(float inPhase){

        float amplitude = 4 * Math.Abs(inPhase-0.5F) - 1;

        return amplitude;
    
    }

    public static float GetSin(float inPhase){
        float amplitude = Mathf.Sin(2 * Mathf.PI * inPhase) ;
        return amplitude;
    }

    public static float GetWhiteNoise(float inPhase) {
        return (float)(SynthInstrument._randObj.NextDouble() * 2.0 - 1.0);
    }

    public bool CheckIsPlaying() {
        return this._isPlayingFlg;
    }

    public static float GetAmplitudeByWave(float inPhase, Wave inWaveType){
        float amplitude = 0;
        switch(inWaveType){
            case Wave.Pulse:
                amplitude=SynthInstrument.GetPulse(inPhase);
                break;
            case Wave.Square:
                amplitude=SynthInstrument.GetSquare(inPhase);
                break;
            case Wave.Saw:
                amplitude=SynthInstrument.GetSaw(inPhase);
                break;
            case Wave.Triangle:
                amplitude=SynthInstrument.GetTriangle(inPhase);
                break;
            case Wave.Sin:
                amplitude=SynthInstrument.GetSin(inPhase);
                break;
            case Wave.WhiteNoise:
                amplitude = SynthInstrument.GetWhiteNoise(inPhase);
                break;
        };
        return amplitude;
    }
    

}
