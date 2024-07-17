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

    private void OnCloseButtonClicked(ClickEvent inEvent) {
        this._startLevelMenuElement.style.display = DisplayStyle.None;
    }

    private void OnLevelButtonClicked(ClickEvent inEvent, int inIn) {
        Label levelName = this._startLevelMenuElement.Query<Label>("LevelName");
        levelName.text = this._activePlayer.GetLevelName(inIn);

        Label levelGoal = this._startLevelMenuElement.Query<Label>("LevelGoal");
        levelGoal.text = this._activePlayer.GetLevelGoalType(inIn);

        Label levelDesc = this._startLevelMenuElement.Query<Label>("LevelDesc");
        levelDesc.text = this._activePlayer.GetLevelDesc(inIn);


        this._currentLevel = inIn;
        RadioButtonGroup difficultyRadioGroup = this._startLevelMenuElement.Query<RadioButtonGroup>("DifficultyRadioGroup");
        difficultyRadioGroup.RegisterValueChangedCallback(OnDifficultyChanged);
        difficultyRadioGroup.value = 0;
        

        this._startLevelMenuElement.style.display = DisplayStyle.Flex;
    }

    private void OnDifficultyChanged(ChangeEvent<int> inEvent) {
        


        int[] maxScores = this._activePlayer.GetLevelScores(this._currentLevel);
        int[] playsCnt = this._activePlayer.GetLevelPlaysCnt(this._currentLevel);
        int[] winsCnt = this._activePlayer.GetLevelWinsCnt(this._currentLevel);

        Label statsValue = this._startLevelMenuElement.Query<Label>("StatsValue");
        statsValue.text = $"{winsCnt[inEvent.newValue]}/{playsCnt[inEvent.newValue]}";
        Label scoreValue = this._startLevelMenuElement.Query<Label>("ScoreValue");
        scoreValue.text = $"{maxScores[inEvent.newValue]}";

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
