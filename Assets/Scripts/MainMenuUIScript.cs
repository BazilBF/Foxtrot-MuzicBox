using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUIScript : MonoBehaviour
{
    private UIDocument _uiDocument;
    private VisualElement _startLevelMenuElement;
    private VisualElement _loadLevelElement;
    private Player _activePlayer;
    private readonly static int _levelScene = 1;

    private int _currentLevel;

    private int[] _maxScores;
    private int[] _playsCnt;
    private int[] _winsCnt;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("UiLoaded");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InititMainMenu(Player inPlayer) {
        this._activePlayer = inPlayer;
        Debug.Log("GUI Init");

        this._uiDocument = GetComponent<UIDocument>();
        this._startLevelMenuElement = this._uiDocument.rootVisualElement.Query("StartLevelMenu");
        this._startLevelMenuElement.style.display = DisplayStyle.None;

        this._loadLevelElement = this._uiDocument.rootVisualElement.Query("LoadLevel");
        this._loadLevelElement.style.display = DisplayStyle.None;

        Button backBtn = this._startLevelMenuElement.Query<Button>("BackBtn");
        backBtn.RegisterCallback<ClickEvent>(OnCloseButtonClicked);


        Button startBtn = this._startLevelMenuElement.Query<Button>("StartBtn");
        startBtn.RegisterCallback<ClickEvent>(OnStartButtonClicked);

        Button exitBtn = this._uiDocument.rootVisualElement.Query<Button>("QuitBtn");
        exitBtn.RegisterCallback<ClickEvent>(OnExitButtonClicked);

        VisualTreeAsset menuElement = Resources.Load<VisualTreeAsset>("UIModules/MenuElement");
        VisualElement chooseLevelElement = this._uiDocument.rootVisualElement.Query("ChooseLevel");

        int avaibleLevelCount = this._activePlayer.GetAvaibleLevelCount();
        for (int i = 0; i < avaibleLevelCount; i++) {
            TemplateContainer newMenuElement = menuElement.Instantiate();
            chooseLevelElement.Add(newMenuElement);
            Label levelNameLabel = newMenuElement.Query<Label>("LevelNameLabel");
            levelNameLabel.text = this._activePlayer.GetLevelName(i);

            Label levelTypeLabel = newMenuElement.Query<Label>("LevelTypeLabel");
            levelTypeLabel.text = this._activePlayer.GetLevelGoalType(i);

            
            Button openButton = newMenuElement.Query<Button>("OpenButton");
            openButton.RegisterCallback<ClickEvent, int>(OnLevelButtonClicked, i);
        }


    }

    private void OnExitButtonClicked(ClickEvent inEvent) {
        Application.Quit();
    }

    private void OnCloseButtonClicked(ClickEvent inEvent) {
        this._startLevelMenuElement.style.display = DisplayStyle.None;
    }

    private void OnLevelButtonClicked(ClickEvent inEvent, int inIn) {

        this._currentLevel = inIn;

        Label levelName = this._startLevelMenuElement.Query<Label>("LevelName");
        levelName.text = this._activePlayer.GetLevelName(this._currentLevel);

        Label levelGoal = this._startLevelMenuElement.Query<Label>("LevelGoal");
        levelGoal.text = this._activePlayer.GetLevelGoalType(this._currentLevel);

        Label levelDesc = this._startLevelMenuElement.Query<Label>("LevelDesc");
        levelDesc.text = this._activePlayer.GetLevelDesc(this._currentLevel);
               

        this._maxScores = this._activePlayer.GetLevelScores(this._currentLevel);
        this._playsCnt = this._activePlayer.GetLevelPlaysCnt(this._currentLevel);
        this._winsCnt = this._activePlayer.GetLevelWinsCnt(this._currentLevel);

        RadioButtonGroup difficultyRadioGroup = this._startLevelMenuElement.Query<RadioButtonGroup>("DifficultyRadioGroup");
        difficultyRadioGroup.RegisterValueChangedCallback(OnDifficultyChanged);
        this.UpdateScores(0);
        difficultyRadioGroup.value = 0;
        

        this._startLevelMenuElement.style.display = DisplayStyle.Flex;
    }

    private void OnDifficultyChanged(ChangeEvent<int> inEvent) {
        this.UpdateScores(inEvent.newValue);
    }

    private void UpdateScores(int inDifficulty) {
        Label statsValue = this._startLevelMenuElement.Query<Label>("StatsValue");
        statsValue.text = $"{this._winsCnt[inDifficulty]}/{this._playsCnt[inDifficulty]}";
        Label scoreValue = this._startLevelMenuElement.Query<Label>("ScoreValue");
        scoreValue.text = $"{this._maxScores[inDifficulty]}";
    }

    private void OnStartButtonClicked(ClickEvent inEvent) {
        RadioButtonGroup difficultyRadioGroup = this._startLevelMenuElement.Query<RadioButtonGroup>("DifficultyRadioGroup");
        this._activePlayer.SetCurrentLevel(this._currentLevel);
        this._activePlayer.SetDifficulty(difficultyRadioGroup.value);
        this._startLevelMenuElement.style.display = DisplayStyle.None;
        this._loadLevelElement.style.display = DisplayStyle.Flex;
        this._activePlayer.LoadScene(MainMenuUIScript._levelScene);
    }

    


}
