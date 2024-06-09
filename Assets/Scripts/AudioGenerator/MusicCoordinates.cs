using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicCoordinates
{
    private int bars;
    private int beats;
    private int beat32s;
    private int samples;

    private readonly int length;
    private readonly int measure;
    private readonly int beat32PerBeat;

    private GameController _controller;

    public MusicCoordinates( int inBars, int inBeats, int inBeat32s, int inSamples, int inLength, int inMeasure, GameController inGameController)
    {

        this.bars = 0;
        this.beats = 0;
        this.beat32s = 0;
        this.samples = 0;
        this.length = inLength;
        this.measure = inMeasure;

        this.beat32PerBeat = 32 / inLength;
        this._controller = inGameController;

        int totalBeats = (inBars * this.measure + inBeats);
        int totalBeats32 = totalBeats * this.beat32PerBeat + inBeat32s;
        int totalSamples = totalBeats32 * this._controller.GetSamplesPerBeat32() + inSamples;

        this.AddSamples(totalSamples);
        
    }

    public MusicCoordinates(int inLength, int inMeasure, GameController inGameController)
    {
        this.bars = 0;
        this.beats = 0;
        this.beat32s = 0;
        this.samples = 0;
        this.length = inLength;
        this.measure = inMeasure;
        this._controller = inGameController;


        this.beat32PerBeat = 32 / inLength;
    }

    public MusicCoordinates(int inSamples, int inLength, int inMeasure, GameController inGameController)
    {
        this.length = inLength;
        this.measure = inMeasure;

        this.beat32PerBeat = 32 / inLength;

        this._controller = inGameController;

        int samplesLeft = inSamples % this._controller.GetSamplesPerBeat32();
        this.samples = samplesLeft;

        int newBeats32Count = (inSamples / this._controller.GetSamplesPerBeat32());

        int beat32sLeft = newBeats32Count % beat32PerBeat;
        this.beat32s = beat32sLeft;

        int newBeatCount = (newBeats32Count / this.beat32PerBeat);
        int beatsLeft = newBeatCount % (this.measure);
        this.beats = beatsLeft;

        this.bars = newBeatCount / (this.measure);
    }



    public void AddMetrics(int inBars, int inBeats, int inBeat32s, int inSamples) {
        int totalBeats = (inBars * this.measure + inBeats);
        int totalBeats32 = totalBeats * this.beat32PerBeat + inBeat32s;
        int totalSamples = totalBeats32 * this._controller.GetSamplesPerBeat32() + inSamples;

        this.AddSamples(totalSamples);
    }

    public void AddSamples(int inAddSamples) {

        int newSamplesCount = this.samples + inAddSamples;
        this.samples = newSamplesCount % this._controller.GetSamplesPerBeat32();
        
        int newBeats32Count = this.beat32s + (newSamplesCount / this._controller.GetSamplesPerBeat32());
        this.beat32s = newBeats32Count % beat32PerBeat;

        int newBeatCount = this.beats + (newBeats32Count / this.beat32PerBeat);
        this.beats = newBeatCount % (this.measure);

        this.bars += newBeatCount / (this.measure);

        /*if (this.samples==0) {
            Debug.Log($"Coordinatex {this.bars}x{this.beats}x{this.beat32s}");
        }*/
    }

    public static MusicCoordinates GetCopy(MusicCoordinates inMusicCoordinatesCopy)
    {
        MusicCoordinates MusicCoordinatesCopy = new MusicCoordinates(inMusicCoordinatesCopy.GetBars(), inMusicCoordinatesCopy.GetBeats(), inMusicCoordinatesCopy.GetBeat32s(), inMusicCoordinatesCopy.GetSamples(), inMusicCoordinatesCopy.GetLength(), inMusicCoordinatesCopy.GetMeasure(), inMusicCoordinatesCopy.GetGameController());
        //Debug.Log("check");
        return MusicCoordinatesCopy;
    }

    public int GetBars() { return this.bars;}

    public int GetBeats() { return this.beats; }

    public int GetBeat32s() { return this.beat32s;}

    public int GetSamples() { return this.samples; }

    public int GetLength() { return this.length;}
    public int GetMeasure() { return this.measure;}

    public GameController GetGameController() {
        return this._controller;
    }

    public int GetTotalBeats() { return this.bars * this.measure + this.beats; }
    
    public int GetTotalBeat32s() { return this.GetTotalBeats() * this.beat32PerBeat + this.beat32s; }

    public int GetTotalSamples() { return this.GetTotalBeat32s() * this._controller.GetSamplesPerBeat32() + this.samples; }

    public static MusicCoordinates GetDelta(MusicCoordinates MusicCoordinates1, MusicCoordinates MusicCoordinates2) {

        int samples1 = MusicCoordinates1.GetTotalSamples();
        int samples2 = MusicCoordinates2.GetTotalSamples();


        int samplesDelta = samples1 - samples2;

        //Debug.Log($"{samples1}{samples2} = {samplesDelta}");

        return new MusicCoordinates(samplesDelta,MusicCoordinates1.GetLength(),MusicCoordinates1.GetMeasure(),MusicCoordinates1.GetGameController());
    }
}
