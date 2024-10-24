using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static GameData;
using UnityEngine.Networking;
using UnityEngine.Windows;

public class WaveForm : SynthInstrument
{
    enum WaveZeroPosition { 
        Top,
        Middle,
        Bottom,
    }

    [System.Serializable]
    public class WaveFormSettings {
        public Wave waveType = Wave.Saw; //wave type for current instrument
        public Wave mWaveType = Wave.Sin; //wave type for modulating
        public float modulatingGaing = 0.0F;
        public float mFrequency = 7; //frequency for modulating
        public float fadeLength = 2.0F; //length of Fade Phase as a part of Beat
        public bool distortionFlg = true; // apply distortion to frequency
        public float distortionGain = 0.3F;
        public bool canSustain = false;
        public float[] fadeGain;
        public float[] fadeWave;
        public float[] wave;
        public float lineColorTreshold = 0.1F;
        public float fadeWaveBase;

        public static readonly string synthMusicFolder = $"SynthInstruments{Path.DirectorySeparatorChar}";

        public void GetStreamedWave(GameController inGameController, string inInstrumentPath)
        {

            this.wave = this.GetTextureAndWaveData($"{inInstrumentPath}Wave.png","Wave", WaveZeroPosition.Middle);
            this.fadeWave = this.GetTextureAndWaveData($"{inInstrumentPath}FadeWave.png","FadeWave", WaveZeroPosition.Middle);

            this.fadeGain = this.GetTextureAndWaveData($"{inInstrumentPath}FadeGain.png","FadeGain", WaveZeroPosition.Bottom);

            Debug.Log("Test");


        }

        private float[] GetTextureAndWaveData(string inPath, string inType, WaveZeroPosition inWaveZeroPosition)
        {
            float[] resultArray = null ;
            Texture2D resultTexture = new Texture2D(2,2);

            byte[] resultBuffer = GameController.GetStreamedBytes(inPath);
            bool loadResult = ImageConversion.LoadImage(resultTexture, resultBuffer);

            if (loadResult)
            {
                resultArray = WaveForm.ReadTextureWaveToArray(resultTexture, inWaveZeroPosition, this.lineColorTreshold);
                
            }
            else {
                throw new InvalidWaveInstrumentDataException($"no data:{inPath}");
            }
            return resultArray;
        }

        public bool CheckLoaded() {
            return this.wave != null && this.fadeWave != null && this.fadeGain != null;
        }
    }


    public class InvalidWaveInstrumentDataException : Exception
    {
        public InvalidWaveInstrumentDataException() : base() { }
        public InvalidWaveInstrumentDataException(string message) : base(message) { }
        public InvalidWaveInstrumentDataException(string message, Exception inner) : base(message, inner) { }
    }

    

    
    private readonly float[] _fadeGain;
    private readonly float[] _fadeWave;
    private readonly float[] _wave;
    private readonly float _fadeWaveBase;
    private readonly float _lineColorTreshold = 0.1F;

    public WaveForm(WaveFormSettings inWaveFormSettings, float inSampleRate, float ininstrumentGainCoef) : base(inSampleRate, ininstrumentGainCoef)
    {
        this.waveType = inWaveFormSettings.waveType;
        this.mWaveType = inWaveFormSettings.mWaveType;
        this.modulatingGaing = inWaveFormSettings.modulatingGaing;
        this.mFrequency = inWaveFormSettings.mFrequency;
        this.fadeLength = inWaveFormSettings.fadeLength;
        this.distortionFlg = inWaveFormSettings.distortionFlg;
        this.canSustain = inWaveFormSettings.canSustain;
        this._fadeGain = inWaveFormSettings.fadeGain;
        this._fadeWave = inWaveFormSettings.fadeWave;
        this._wave = inWaveFormSettings.wave;
        this._fadeWaveBase = inWaveFormSettings.fadeWaveBase;
        
    }

    

    protected override float GetAmplitude()
    {
        int i = (int)Mathf.Floor((float)(this._phase * this._wave.Length));
        return this._wave[i];
    }

    protected override float FaidGain(double inFadePhase)
    {
        int i = (int)Mathf.Floor((float)(inFadePhase * this._fadeWave.Length));
        this._frequencyModifier = this._fadeWave[i]*this._fadeWaveBase;
        i = (int)Mathf.Floor((float)(inFadePhase * this._fadeGain.Length));
        return this._fadeGain[i];
    }

    public static WaveFormSettings LoadInstrumentData(string inSyntInstrument, GameController inGameController) {

        WaveFormSettings returnWaveFormSettings = null;
        string instrumentPath = $"{WaveFormSettings.synthMusicFolder}{inSyntInstrument}{Path.DirectorySeparatorChar}";

        string dataString = GameController.GetStreamedText($"{instrumentPath}InstrumentsSettings.json");
        returnWaveFormSettings = JsonUtility.FromJson<WaveFormSettings>(dataString);

        returnWaveFormSettings.GetStreamedWave(inGameController,instrumentPath);        
        
        return returnWaveFormSettings;
    }

    private static float[] ReadTextureWaveToArray(Texture2D inTexture, WaveForm.WaveZeroPosition inWaveFormZero, float inLineColorTreshold)
    {
        float[] returnValue = new float[inTexture.width];
        float zeroPosition = 0.0F;
        if (inWaveFormZero == WaveZeroPosition.Middle)
        {
            zeroPosition = 0.5F;
        }
        else if (inWaveFormZero == WaveZeroPosition.Bottom) {
            zeroPosition = 1.0F;
        }

        float prevValue = 0.0F;
        for (int i=0; i < returnValue.Length; i++) {
            returnValue[i] = prevValue;
            for (int y=0; y < inTexture.height; y++) {
                UnityEngine.Color pixelColor = inTexture.GetPixel(i, y);
                
                if (pixelColor.r < inLineColorTreshold && pixelColor.g < inLineColorTreshold && pixelColor.b < inLineColorTreshold) {
                    float yPos = (float)y / (float)inTexture.height;
                    returnValue[i] = (zeroPosition - yPos)*(inWaveFormZero == WaveZeroPosition.Middle? -2.0F:1.0F);
                    break;
                }
            }
        }
        return returnValue;
    }
}
