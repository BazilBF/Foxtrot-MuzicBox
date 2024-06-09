
using static GameData;


public class LevelGoal
{

    protected float levelProgress = 0;
    protected readonly Player _playerInfo;
    protected readonly GameData.RythmLevel _rythmLevel;
    protected LevelGoalType _levelGoalType;

    protected string _uiProgressFile;
    protected string[] _uiProgressFieldsList;

    private LevelGoalState _levelGoalCurrentState = LevelGoalState.InProgress;
    protected readonly static float[] _MarksModifier = new float[] { 2.0F, 1.5F, 1.0F, 0.75F, 0.5F, 0.25F };

    public enum LevelGoalState {
        Starting,
        InProgress,
        Won,
        Bonus,
        Lost
    }

    public static float[] GetMarkModifiers() {
        return LevelGoal._MarksModifier;
    }

    public enum LevelGoalType {
        DestroyLevelGoal

    }

    public string[] GetUIProgressFieldsList() {
        return this._uiProgressFieldsList;
    }

    public string GetUiProgressFile() {
        return this._uiProgressFile;
    }

    public LevelGoal(Player inPlayerInfo, GameData.RythmLevel inRythmLevel) {
        this._rythmLevel = inRythmLevel;
        this._playerInfo = inPlayerInfo;
    }

    public int AddScore(int inScoreMarkIndex, int inComboCount, bool inIsBonus) {
        return this.ProcessAndReturnScore(inScoreMarkIndex, inIsBonus);

    }

    public int AddDamage() {
        return this.ProcessAndReturnDamage();
    }

    virtual public void ProcessLevelFlow() {

    }

    virtual protected int ProcessAndReturnScore(int inScoreMarkIndex, bool inIsBonus) {
        return 100;
    }

    virtual protected int ProcessAndReturnDamage()
    {
        return 100;
    }

    virtual protected void ProcessLevelGoal(int inCurrentBeat32sCount) {

    }

    protected void SetLevelGoalState(LevelGoalState inLevelGoalState) {
        this._levelGoalCurrentState = inLevelGoalState;
    }

    virtual public LevelGoalState GetLevelGoalState() {
        return this._levelGoalCurrentState;
    }

    public LevelGoalType GetLevelGoalType() {
        return this._levelGoalType;
    }

    public virtual float[] GetLevelGoalParams() {
        return null;
    }

}
