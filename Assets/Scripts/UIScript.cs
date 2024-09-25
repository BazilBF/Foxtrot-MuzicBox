using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{

    private GameController _controller;
    private UIDocument _uiDocument;
    public VisualTreeAsset _progressVisualTreeAsset;
    private LevelProgressController _levelProgressController;
    private LevelGoal.LevelGoalType _currentLevelGoalType;
    private Player _player;

    private string[] _uiProgressFieldsList;
    private string _uiProgressFile;
    private VisualElement _progressElement;
    private VisualElement _pauseMenuElement;
    private VisualElement _startMenu;

    private bool _isStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        this._isStarted = false;
        GameObject controllerGameObject = GameObject.FindGameObjectWithTag("GameController");
        if (controllerGameObject != null)
        {
            this._controller = controllerGameObject.GetComponent<GameController>();
            this._levelProgressController = this._controller.GetLevelProgressController();
            this._player = this._controller.GetPlayerInfo();

            this._uiProgressFile = this._levelProgressController.GetUiProgressFile();
            this._uiProgressFieldsList = this._levelProgressController.GetUIProgressFieldsList();
            this._currentLevelGoalType = this._levelProgressController.GetLevelGoalType();

            this._uiDocument = GetComponent<UIDocument>();

            

            this._progressVisualTreeAsset = Resources.Load<VisualTreeAsset>(this._uiProgressFile);

            this._progressElement = this._uiDocument.rootVisualElement.Query("ProgressInfo");
            this._pauseMenuElement = this._uiDocument.rootVisualElement.Query("PauseMenu");
            this._startMenu = this._uiDocument.rootVisualElement.Query("StartMenu");

            Button pauseBtn = this._uiDocument.rootVisualElement.Query<Button>("PauseBtn");
            pauseBtn.RegisterCallback<ClickEvent>(OnPauseBtnClick);

            Button continueBtn = this._uiDocument.rootVisualElement.Query<Button>("ContinueBtn");
            continueBtn.RegisterCallback<ClickEvent>(OnPauseBtnClick);

            Button startLevelBtn = this._uiDocument.rootVisualElement.Query<Button>("StartLevelBtn");
            startLevelBtn.RegisterCallback<ClickEvent>(onStartStartLevelBtnClick);

            Button endBtn = this._uiDocument.rootVisualElement.Query<Button>("EndBtn");
            endBtn.RegisterCallback<ClickEvent>(OnEndTryBtnClick);

            this._progressElement.Add(this._progressVisualTreeAsset.Instantiate());

            string[] tmpPlayerStaffsKeys = this._player.GetStaffsKeys();
            string[] tmpFormatedStaffKeys = tmpPlayerStaffsKeys.Select((keyStr,index) => $"#{index} - \"{keyStr}\"").ToArray();

            string tmpDisplayStr = String.Join(", ", tmpFormatedStaffKeys);

            Label staffKeysLabel = this._uiDocument.rootVisualElement.Query<Label>("StaffKeysLabel");

            staffKeysLabel.text = tmpDisplayStr;


                int skillCnt = this._player.GetSkillCount();
            for (int i=0; i < skillCnt; i++) {
                Button tmpSkillButton = this._uiDocument.rootVisualElement.Query<Button>($"Skill{i}Btn");
                tmpSkillButton.RegisterCallback<ClickEvent, int>(OnToggleSkillClick,i);
            }

        }

        

        
    }

    private void onStartStartLevelBtnClick(ClickEvent inEvent) {
        this._controller.TogglePause();
        this._isStarted = true;
        this._startMenu.style.display = DisplayStyle.None;
    }

    private void OnPauseBtnClick(ClickEvent inEvent) {
        this._controller.TogglePause();
    }

    private void OnToggleSkillClick(ClickEvent clickEvent, int inSkillId)
    {
        this._player.ToggleSkill(inSkillId);
    }

    private void OnEndTryBtnClick(ClickEvent inEvent)
    {
        this._controller.EndTry();
    }

    // Update is called once per frame
    void Update()
    {
        MusicCoordinates tmpMusicCoordinates = this._controller.GetMusicCoordinates();

        int skillCnt = this._player.GetSkillCount();
        for (int i = 0; i < skillCnt; i++)
        {
            Button tmpSkillButton = this._uiDocument.rootVisualElement.Query<Button>($"Skill{i}Btn");
            PlayerSkill.SkillState currentSkillState = this._player.GetSkill(i).CheckState();

            bool buttonNotReadyToEnable = (currentSkillState == PlayerSkill.SkillState.cooldown || currentSkillState == PlayerSkill.SkillState.noEnergy || currentSkillState == PlayerSkill.SkillState.blocked);

            if (tmpSkillButton.enabledSelf && buttonNotReadyToEnable)
            {
                tmpSkillButton.SetEnabled(false);
            }
            else if (!tmpSkillButton.enabledSelf && !buttonNotReadyToEnable) {
                tmpSkillButton.SetEnabled(true);
            }
        }


        string levelStatusCheck = "";


        if (this._isStarted!&&this._controller.CheckIsPaused() && this._pauseMenuElement.style.display != DisplayStyle.Flex) {
            this._pauseMenuElement.style.display = DisplayStyle.Flex;

            int marksCount = this._levelProgressController.GetMarksLength();
            for (int i = 0; i < marksCount; i++) {
                Label markCount = this._pauseMenuElement.Query<Label>($"Mark{i}");
                markCount.text = $"{this._levelProgressController.GetMarksCount(i)}";
            }
            Label totalNotes = this._pauseMenuElement.Query<Label>($"TotalNotes");
            totalNotes.text = $"{this._levelProgressController.GetNoteCount()}";

        }
        else if (!this._controller.CheckIsPaused() && this._pauseMenuElement.style.display != DisplayStyle.None) {
            this._pauseMenuElement.style.display = DisplayStyle.None;
        }

        if (this._levelProgressController != null) {
            float[] currenLeveGoalParams = this._levelProgressController.GetLevelGoalParams();

            Label comboLabel = this._progressElement.Query<Label>("Combo");
            comboLabel.text = $"x{this._levelProgressController.GetComboCount()}/{this._levelProgressController.GetComboChainsCount()}";

            Label scoreLabel = this._progressElement.Query<Label>("Score");
            scoreLabel.text = $"{this._levelProgressController.GetLevelScore()}";

            Label bpmLabel = this._progressElement.Query<Label>("BPM");
            bpmLabel.text = $"{this._controller.GetBpm()}BpM";

            for (int i=0; i<this._uiProgressFieldsList.Length; i++) {
                VisualElement tmpVisualElement = this._progressElement.Query(this._uiProgressFieldsList[i]);


                System.Type elementType = tmpVisualElement.GetType();

                if (elementType.GetProperty("value") != null) {
                    elementType.GetProperty("value").SetValue(tmpVisualElement,currenLeveGoalParams[i]);
                }
            }

        }
    }
}
