using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeatListner : MonoBehaviour
{
    public enum FrequencyType{
        Tops,
        Mids,
        Lows
    }

    private AudioSource _activeAudioSource;
    private GameController _controller;
    private float[] _spectrumArray = new float[1024];
    private float _frequencyStep = 23.43F;

    

    public FrequencyType activeFrequencyType = FrequencyType.Lows;

    private float _currentScaleCoef = 1.0F;

    public float maxScaleDelta = 0.1F;
    public float frequencyWindowStart = 0;
    public float frequencyWindowEnd = 100;

    private void Awake()
    {
        GameObject controllerGameObject = GameObject.FindGameObjectWithTag("GameController");
        if (controllerGameObject != null) {
            this._controller = controllerGameObject.GetComponent<GameController>();
            this._activeAudioSource = this._controller.GetAudioSource();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int frequencySegmentStart = (int) Math.Round(this.frequencyWindowStart/this._frequencyStep);
        int frequencySegmentEnd = (int) Math.Round(this.frequencyWindowStart / this._frequencyStep);

        this._activeAudioSource.GetSpectrumData(this._spectrumArray, 0, FFTWindow.Rectangular);

        int elementCount = frequencySegmentEnd - frequencySegmentStart + 1;
        float[] workingFrequencyiesWindow = new float[elementCount];
        Array.Copy(this._spectrumArray, frequencySegmentStart, workingFrequencyiesWindow, 0, elementCount);

        float avgFrequencyWindowGain = workingFrequencyiesWindow.Sum() / elementCount;

        float curentScale = this.transform.localScale.x / this._currentScaleCoef;

        this._currentScaleCoef = curentScale + curentScale * avgFrequencyWindowGain * this.maxScaleDelta;

        this.transform.localScale = new Vector3(this._currentScaleCoef, this._currentScaleCoef, 0.0F);
    }
}
