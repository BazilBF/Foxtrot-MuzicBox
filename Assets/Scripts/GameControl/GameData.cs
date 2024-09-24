using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class GameData
{

    public string gameName;
    public string gameAuthor;
    public float version;


    [System.Serializable]
    public class RythmLevel
    {
        public string name;
        public string enemyMug;
        public string goalType;
        public int maxPlaybleMelody;
        public int maxPlaybleBeat;
        public float enemyHealth;
        public string enemyType;
        public int surviveTime;
        public int goalScore;
        public float mineSpawnChance;
        public float mineDamageMin;
        public float mineDamageMax;
        public string synthTrack;
        public List<string> levelStructure = new List<string>();

        public int measure;
        public int length;
        public int defaultBpm;

        public List<ScorePart> couplets = new List<ScorePart>();
        public List<ScorePart> choruses = new List<ScorePart>();
        public List<ScorePart> solos = new List<ScorePart>();
        public List<ScorePart> intros = new List<ScorePart>();
        public List<ScorePart> outros = new List<ScorePart>();
        public List<ScorePart> bridges = new List<ScorePart>();

        private readonly System.Random rand = new System.Random();

        //private PartStaffsData currentPartInstruments = null;

        private int _partsCount = 0;
        private int _timesPlayed = 0;
        private int _currentPart = -1;

        public static RythmLevel LoadSynthTrack(string inSynthTrackName)
        {
            string musicFolder = $"{GameController.synthMusicFolder}{inSynthTrackName}.json";
            string data = GameController.GetStreamedText(musicFolder);
            RythmLevel returnLevel = JsonUtility.FromJson<RythmLevel>(data);

            return returnLevel;

        }

        public int[] getScoreParametrs()
        {
            int[] returnParametrs = { this.measure, this.length, this.defaultBpm };
            return returnParametrs;
        }

        public int GetDefaultBpm()
        {
            return this.defaultBpm;
        }

        public int GetMeasure()
        {
            return this.measure;
        }

        public int GetLength()
        {
            return this.length;
        }

        public int GetTimesPlayed()
        {
            return this._timesPlayed;
        }

        public int GetMaxInstrumentsCount() { 
            int maxInstrumentsCount = 0;
            for (int i= 0; i<this.levelStructure.Count;i++) {
                ScorePart tmpScorePart = this.GetScorePart(this.levelStructure[i]);
                int tmpInstrumentCount = tmpScorePart.GetInstrumentsCount(ScorePart.StaffTypes.AllStaffs);
                if (tmpInstrumentCount> maxInstrumentsCount) {
                    maxInstrumentsCount = tmpInstrumentCount;
                }
            }
            return maxInstrumentsCount;
        }

        public float GetMaxInstrumentsGain() {
            float maxInstrumentsGain = 0;
            for (int i = 0; i < this.levelStructure.Count; i++)
            {
                ScorePart tmpScorePart = this.GetScorePart(this.levelStructure[i]);
                float tmpInstrumentGain = tmpScorePart.GetTotalInstrumentsGain(ScorePart.StaffTypes.AllStaffs);
                if (tmpInstrumentGain > maxInstrumentsGain)
                {
                    maxInstrumentsGain = tmpInstrumentGain;
                }
            }
            return maxInstrumentsGain;
        }

        public ScorePart GetScorePart(string inScorePartType)
        {
            ScorePart returnScorePart = null;
            List<ScorePart> ScorePartList = null;
            switch (inScorePartType)
            {
                case "Couplet": ScorePartList = this.couplets; break;
                case "Chorus": ScorePartList = this.choruses; break;
                case "Solo": ScorePartList = this.solos; break;
                case "Intro": ScorePartList = this.intros; break;
                case "Outro": ScorePartList = this.outros; break;
                case "Bridge": ScorePartList = this.bridges; break;
            }

            int returnIndex = 0;
            if (ScorePartList.Count > 0)
            {
                returnIndex = this.rand.Next(ScorePartList.Count);
                returnScorePart = ScorePartList[returnIndex];
                returnScorePart.ResetScoreStaff();
            }

            return returnScorePart;
        }

        public string GetLevelSynthTrack()
        {
            return this.synthTrack;
        }

        public string GetGoalType() {
            return this.goalType;
        }

        public int GetTotalPartCount() {
            return this.levelStructure.Count;
        }

        public int GetPartsCount() {
            return this._partsCount;
        }

        public PartStaffsData LoadSynth(int inMovingBeat32sCount, LevelGoal.LevelGoalState inLevelGoalState) {

            PartStaffsData newPartStaffData = null;


            bool playeblblePartFound = false;
            int partsChecked = 0;

            string currentPartStr = null;
            ScorePart currentScorePartObj = null;

            if (inLevelGoalState == LevelGoal.LevelGoalState.Starting)
            {
                currentPartStr = "Intro";
                currentScorePartObj = this.GetScorePart("Intro");

            }
            else if (inLevelGoalState == LevelGoal.LevelGoalState.Won) {
                currentPartStr = "Outro";
                currentScorePartObj = this.GetScorePart("Outro");
            }

            if (currentScorePartObj != null)
            {
                playeblblePartFound = true;
            }
            else
            {

                do
                {
                    if (this._currentPart++ >= this.levelStructure.Count - 1)
                    {
                        this._currentPart = 0;
                        this._timesPlayed++;
                    }
                    partsChecked++;

                    currentPartStr = this.levelStructure[this._currentPart];

                    currentScorePartObj = this.GetScorePart(currentPartStr);
                    if (currentScorePartObj != null)
                    {
                        playeblblePartFound = true;
                    }

                    bool check = !playeblblePartFound;
                    bool check2 = partsChecked < this.levelStructure.Count;
                    bool check3 = check && check2;

                } while (!playeblblePartFound && partsChecked < this.levelStructure.Count);
            }

            if (playeblblePartFound)
            {
                Debug.Log(currentPartStr);
                this._partsCount++;
                newPartStaffData = new PartStaffsData(currentPartStr,this, currentScorePartObj.GetBrigeOffsetBeats());
                List<ScoreStaff> melodyStaffs = currentScorePartObj.GetScoreStaffList("Melody");

                melodyStaffs = this.ShuffleStaffs(melodyStaffs);

                int playbleMelodyStaffCount = 0;

                for (int i = 0; i < melodyStaffs.Count; i++)
                {

                   
                    if (playbleMelodyStaffCount < this.maxPlaybleMelody && melodyStaffs[i].GetPlayable())
                    {
                        newPartStaffData.SetPlayableStaff(melodyStaffs[i]);
                        playbleMelodyStaffCount++;
                    }
                    else
                    {
                        newPartStaffData.SetUnPlayableStaff(melodyStaffs[i]);
                    }
                }

                int playbleBeatStaffCount = 0;

                List<ScoreStaff> beatStaffs = currentScorePartObj.GetScoreStaffList("Beat");
                beatStaffs = this.ShuffleStaffs(beatStaffs);

                for (int i = 0; i < beatStaffs.Count; i++)
                {
                    if (playbleBeatStaffCount < this.maxPlaybleBeat && beatStaffs[i].GetPlayable())
                    {
                        newPartStaffData.SetPlayableStaff(beatStaffs[i]);
                        playbleBeatStaffCount++;
                    }
                    else
                    {
                        newPartStaffData.SetUnPlayableStaff(beatStaffs[i]);
                    }
                }

                newPartStaffData.SetTotalBeat32sCount(inMovingBeat32sCount);
            }
            else
            {
                // ToDo: add exception 
            }

            

            return newPartStaffData;
        }

        

        public float GetMineSpawnChance() {
            return this.mineSpawnChance;
        }        

        

        public int[] GetScoreMetrics()
        {
            int[] returnMetrics = { this.GetMeasure(), this.GetLength(), this.GetDefaultBpm() };
            return returnMetrics;
        }

        public List<ScoreStaff> ShuffleStaffs(List<ScoreStaff> listToShuffle)
        {
            for (int i = listToShuffle.Count - 1; i > 0; i--)
            {
                int k = rand.Next(i + 1);
                ScoreStaff value = listToShuffle[k];
                listToShuffle[k] = listToShuffle[i];
                listToShuffle[i] = value;
            }
            return listToShuffle;
        }

        public float GetEnemyHealth() {
            return this.enemyHealth;
        }

        public string GetEnemyMug() {
            return this.enemyMug;
        }

        public string GetEnemyType() {
            return this.enemyType;
        }

        public float[] GetMineDamage()
        {
            float[] mineDamage = { this.mineDamageMin, this.mineDamageMax };
            return mineDamage;
        }
    }

    [System.Serializable]
    public class ScoreNote
    {
        public int bar;
        public int beat;
        public int beat32;

        public float measure;
        public int length;

        public string note;
        public int octave;
        public float gain;

        public int[] GetNoteCoordinates()
        {
            int[] returnCoordinates = { this.bar, this.beat, this.beat32 };
            return returnCoordinates;
        }

        public string GetNote()
        {
            return this.note;
        }

        public float GetMeasure()
        {
            return this.measure;
        }

        public int GetLength()
        {
            return this.length;
        }
        public int GetOctave()
        {
            return this.octave;
        }

        public float GetGain()
        {
            return this.gain;
        }

    }

    [System.Serializable]
    public class ScoreStaff
    {

        public string instrument;
        public float instrumentGain;
        public bool playable;

        public List<ScoreNote> notes = new List<ScoreNote>();

        private bool _allNotePlayed = false;
        private int _lastNoteSet = 0;

        public ScoreNote GetNote(int[] inNoteCoordinates, bool inDoNotSet = false)
        {
            ScoreNote returnScoreNote = null;

            for (int i = (inDoNotSet ? 0 : this._lastNoteSet); i < this.notes.Count; i++)
            {
                int[] tmpNoteCoordinate = this.notes[i].GetNoteCoordinates();
                if (tmpNoteCoordinate[0] == inNoteCoordinates[0] && tmpNoteCoordinate[1] == inNoteCoordinates[1] && tmpNoteCoordinate[2] == inNoteCoordinates[2])
                {
                    if (!inDoNotSet)
                    {
                        this._lastNoteSet = i;
                    }
                    returnScoreNote = this.notes[i];
                    break;
                }
                else if (!inDoNotSet && tmpNoteCoordinate[0] > inNoteCoordinates[0] && tmpNoteCoordinate[1] > inNoteCoordinates[1] && tmpNoteCoordinate[2] > inNoteCoordinates[2])
                {
                    break;
                }
            }

            int[] lastNoteCoordinate = this.GetLastNoteCoordinates();
            if (!inDoNotSet && !this._allNotePlayed && lastNoteCoordinate[0] <= inNoteCoordinates[0] && lastNoteCoordinate[1] <= inNoteCoordinates[1] && lastNoteCoordinate[2] <= inNoteCoordinates[2]) {
                this._allNotePlayed = true;
            }

            return returnScoreNote;
        }

        public int[] GetNoteCoordinatesByIbdex(int inIndex)
        {
            return (inIndex < this.notes.Count ? this.notes[inIndex].GetNoteCoordinates() : null);
        }

        public int GetNotesCount()
        {
            return this.notes.Count;
        }

        public int[] GetLastNoteCoordinates()
        {
            return this.notes[this.notes.Count - 1].GetNoteCoordinates();
        }

        public int[] FinalNoteCoordinates()
        {
            ScoreNote finalNote = this.notes[this.notes.Count - 1];
            return finalNote.GetNoteCoordinates();
        }

        public bool CheckAllNotesPlayed()
        {
            return (this._allNotePlayed || this.GetNotesCount()==0);
        }

        public bool GetPlayable()
        {
            return this.playable;
        }

        public string GetInstrument()
        {
            return this.instrument;
        }

        public float GetInstrumentGain()
        {
            return this.instrumentGain;
        }

        public void ResetScoreStaff()
        {
            this._allNotePlayed = false;
            this._lastNoteSet = 0;
        }
    }

    [System.Serializable]
    public class ScorePart
    {
        public enum StaffTypes{ 
            AllStaffs,
            BeatStaffs,
            MelodyStaffs
        }

        public List<ScoreStaff> ScoreMelodyStaffs = new List<ScoreStaff>();
        public List<ScoreStaff> ScoreBeatStaffs = new List<ScoreStaff>();
        public int brigeOffsetBeats = 1;

        public List<ScoreStaff> GetScoreStaffList(string inScoreStaffType)
        {
            List<ScoreStaff> returnScoteStaff = null;
            switch (inScoreStaffType)
            {
                case "Melody": returnScoteStaff = this.ScoreMelodyStaffs; break;
                case "Beat": returnScoteStaff = this.ScoreBeatStaffs; break;
            }
            return returnScoteStaff;
        }

        public int GetBrigeOffsetBeats()
        {
            return this.brigeOffsetBeats;
        }

        public void ResetScoreStaff()
        {
            for (int i = 0; i < this.ScoreMelodyStaffs.Count; i++)
            {
                this.ScoreMelodyStaffs[i].ResetScoreStaff();
            }
            for (int i = 0; i < this.ScoreBeatStaffs.Count; i++)
            {
                this.ScoreBeatStaffs[i].ResetScoreStaff();
            }
        }

        public int GetInstrumentsCount (StaffTypes inStaffType)
        {
            int returnCount = 0;

            if (inStaffType == StaffTypes.AllStaffs || inStaffType == StaffTypes.MelodyStaffs) {
                returnCount += this.ScoreMelodyStaffs.Count;
            }
            if (inStaffType == StaffTypes.AllStaffs || inStaffType == StaffTypes.BeatStaffs)
            {
                returnCount += this.ScoreBeatStaffs.Count;
            }

            return returnCount;
        }

        public float GetTotalInstrumentsGain(StaffTypes inStaffType) { 
            float returnGain = 0;
            if (inStaffType == StaffTypes.AllStaffs || inStaffType == StaffTypes.MelodyStaffs)
            {
                for (int i = 0; i < this.ScoreMelodyStaffs.Count; i++) {
                    returnGain += this.ScoreMelodyStaffs[i].GetInstrumentGain();
                }
            }
            if (inStaffType == StaffTypes.AllStaffs || inStaffType == StaffTypes.BeatStaffs)
            {
                for (int i = 0; i < this.ScoreBeatStaffs.Count; i++)
                {
                    returnGain += this.ScoreBeatStaffs[i].GetInstrumentGain();
                }
            }
            return returnGain;
        }
    }


    public class PartStaffsData{
        private List<ScoreStaff> _playableStaffs = new List<ScoreStaff>();
        private List<ScoreStaff> _unPlayableStaffs = new List<ScoreStaff>();

        private List<int> _playbleNoteCount = new List<int>();
        private List<int> _unPlaybleNoteCount = new List<int>();

        private RythmLevel _parentRythmLevel;

        private bool _allNotesPlayed = false;
        private int _bridgeOffsetBeats;

        private int _totalBeat32sCount = 0;

        private int _currentBeats32sCount = 0;

        private int _notesPlayed;
        private string _nameCheck;

        public PartStaffsData(string inName, RythmLevel inParentRythmLevel, int inBridgeOffsetBeats) { 
            this._nameCheck = inName;
            this._parentRythmLevel = inParentRythmLevel;
            this._bridgeOffsetBeats = inBridgeOffsetBeats;
        }

        public string GetNameCheck() {
            return this._nameCheck;
        }

        public void SetUnPlayableStaff(ScoreStaff inMelodyStaffs) {
            this._unPlayableStaffs.Add(inMelodyStaffs);
            this._unPlaybleNoteCount.Add(0);
        }

        public void SetPlayableStaff(ScoreStaff inMelodyStaffs)
        {
            this._playableStaffs.Add(inMelodyStaffs);
            this._playbleNoteCount.Add(0);
        }

        public int GetBridgeOffsetBeats() {
            return this._bridgeOffsetBeats;
        }

        public string GetPartStaffDataName() {
            return this._nameCheck;
        }

        public float GetMineSpawnChance() { 
            return this._parentRythmLevel.GetMineSpawnChance();
        }

        public List<ScoreStaff> GetPlayableStaffs()
        {
            return this._playableStaffs;
        }
        public List<ScoreStaff> GetUnPlayableStaffs()
        {
            return this._unPlayableStaffs;
        }
        public int GetBeat32sCount()
        {
            return this._totalBeat32sCount;
        }

        public int GetCurrentBeats32sCount() {
            return this._currentBeats32sCount;
        }

        public void SetTotalBeat32sCount(int inTotalBeat32sCount) {
            this._totalBeat32sCount = inTotalBeat32sCount;
        }

        public int GetPartBeat32sCount(int inMeasure, int inLength) {

            int lastBeat32sCount = 0;

            int scoreLength = inLength;
            int scoreMeasure = inMeasure;

            for (int i = 0; i < this._playableStaffs.Count; i++)
            {

                if (this._playableStaffs[i].GetNotesCount() > 0)
                {
                    int[] tmpNotesCoordinates = this._playableStaffs[i].GetLastNoteCoordinates();
                    ScoreNote tmpNote = this._playableStaffs[i].GetNote(tmpNotesCoordinates, true);

                    int noteLength = tmpNote.GetLength();
                    float noteMeasure = tmpNote.GetMeasure();

                    int totalBeats32s = (tmpNotesCoordinates[0] * scoreMeasure + tmpNotesCoordinates[1]) * (32 / scoreLength) + (int)(noteMeasure * (32.0F / (float)noteLength));

                    if (lastBeat32sCount == 0 || totalBeats32s > lastBeat32sCount)
                    {
                        lastBeat32sCount = totalBeats32s;
                    }
                }
            }

            for (int i = 0; i < this._unPlayableStaffs.Count; i++)
            {
                if (this._unPlayableStaffs[i].GetNotesCount() > 0)
                {
                    int[] tmpNotesCoordinates = this._unPlayableStaffs[i].GetLastNoteCoordinates();
                    ScoreNote tmpNote = this._unPlayableStaffs[i].GetNote(tmpNotesCoordinates, true);

                    int noteLength = tmpNote.GetLength();
                    float noteMeasure = tmpNote.GetMeasure();

                    int totalBeats32s = (tmpNotesCoordinates[0] * scoreMeasure + tmpNotesCoordinates[1]) * (32 / scoreLength) + +(int)(noteMeasure * (32.0F / (float)noteLength));

                    if (lastBeat32sCount == 0 || totalBeats32s > lastBeat32sCount)
                    {
                        lastBeat32sCount = totalBeats32s;
                    }
                }
            }

            this.SetTotalBeat32sCount(lastBeat32sCount);

            return lastBeat32sCount;
        }

        public int[] GetNextNoteCoordinates(int inStaffIndex, bool inIsPlayable, int inNoteIndex) {
            List<ScoreStaff> tmpStaffList = (inIsPlayable ? this._playableStaffs : this._unPlayableStaffs);
            return tmpStaffList[inStaffIndex].GetNoteCoordinatesByIbdex(inNoteIndex);
        }

        public int[] GetNextNoteCoordinates(int inStaffIndex, bool inIsPlayable)
        {
            List<int> tmpNoteCount = (inIsPlayable ? this._playbleNoteCount : this._unPlaybleNoteCount);
            return this.GetNextNoteCoordinates(inStaffIndex,inIsPlayable, tmpNoteCount[inStaffIndex] + 1);
        }

        public SynthNote GetLevelNote(int inStaffIndex, bool inIsPlayable, MusicCoordinates inMusicCoordinates)
        {
            SynthNote returnNote = null;

            this._currentBeats32sCount = inMusicCoordinates.GetTotalBeat32s();

            

            int[] currentNoteCoordinates = { inMusicCoordinates.GetBars() + 1, inMusicCoordinates.GetBeats() + 1, inMusicCoordinates.GetBeat32s() + 1 };
            
            List<ScoreStaff> tmpStaffList = (inIsPlayable? this._playableStaffs : this._unPlayableStaffs);
            List<int> tmpNoteCount = (inIsPlayable ? this._playbleNoteCount : this._unPlaybleNoteCount);

            if (tmpStaffList[inStaffIndex].GetNotesCount() > 0)
            {
                ScoreNote checkScoreNote = tmpStaffList[inStaffIndex].GetNote(currentNoteCoordinates);

                if (checkScoreNote != null)
                {
                    returnNote = new SynthNote(checkScoreNote.GetNote(), checkScoreNote.GetOctave(), checkScoreNote.GetMeasure(), checkScoreNote.GetLength(), checkScoreNote.GetGain());
                    tmpNoteCount[inStaffIndex]++;
                }
            }

            bool allPlayableNotesPlayed = true;
            for (int i=0; i <this._playbleNoteCount.Count;i++) {
                bool checkPlayed = this._playableStaffs[i].CheckAllNotesPlayed();
                if (!checkPlayed) {
                    allPlayableNotesPlayed = false; break;
                }
            }

            bool allUnPlayableNotesPlayed = true;
            for (int i = 0; i < this._unPlayableStaffs.Count; i++)
            {
                bool checkPlayed = this._unPlayableStaffs[i].CheckAllNotesPlayed();
                if (!checkPlayed)
                {
                    allUnPlayableNotesPlayed = false; break;
                }
            }


            this._allNotesPlayed = allPlayableNotesPlayed && allUnPlayableNotesPlayed;

            return returnNote;
        }

        public bool CheckAllNotesPlayed()
        {
            return this._allNotesPlayed;
        }
    }




}
