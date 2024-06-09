using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ResultScreenUiScript : MonoBehaviour
{
    private Player _activePlayer;
    private readonly static int _mainMenuSceneId = 0;
    private readonly static int _levelScene = 2;

    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        
        if (playerObject != null)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            this._activePlayer = playerController.GetActivePlayer();
            
            int currentLevel = this._activePlayer.GetCurrentLevel();
            int currenDifficulty = (int)this._activePlayer.GetDifficulty();

            Debug.Log("Found player");

            UIDocument uiDocument = GetComponent<UIDocument>();
            

            LevelGoal.LevelGoalState currentState = this._activePlayer.GetLevelGoalState();

            Label playResultLabel = uiDocument.rootVisualElement.Query<Label>("PlayResultLabel");
            string levelResult;
            if (currentState == LevelGoal.LevelGoalState.Won) {
                levelResult = "LEVEL CLEARED";
            }
            else {
                levelResult = "LEVEL FAILED";
            }
            playResultLabel.text = levelResult;

            Label tryCountLabel = uiDocument.rootVisualElement.Query<Label>("TryCountLabel");
            string tryCountString;

            int[] tryCount = this._activePlayer.GetLevelPlaysCnt(currentLevel);
            int[] winCount = this._activePlayer.GetLevelWinsCnt(currentLevel);

            string winCountStr;
            switch (winCount[currenDifficulty]%10)
            {
                case 1: winCountStr = $"{winCount[currenDifficulty]}st"; break;
                case 2: winCountStr = $"{winCount[currenDifficulty]}nd"; break;
                case 3: winCountStr = $"{winCount[currenDifficulty]}rd"; break;
                default: winCountStr = $"{winCount[currenDifficulty]}th"; break;
            }

            if (currentState == LevelGoal.LevelGoalState.Won)
            {
                
                
                if (tryCount[currenDifficulty] == 1)
                {
                    tryCountString = $"Success on the first try? O,..,o";
                }
                else if (tryCount[currenDifficulty] == winCount[currenDifficulty])
                {
                    tryCountString = $"{winCount[currenDifficulty]} of {tryCount[currenDifficulty]}. Cheater or Beast -,..,-?";
                }
                else
                {
                    tryCountString = $"The {winCountStr} success of {tryCount[currenDifficulty]}. ^,..,^";
                }
            }
            else
            {
                tryCountString = $"It could be the {winCountStr} success of {tryCount[currenDifficulty]}. ^,..,-";
            }
            tryCountLabel.text = tryCountString;

            int[] levelMarksCounts = this._activePlayer.GetCurrentMarksCounts();
            for (int i=0; i < levelMarksCounts.Length;i++) {
                Label tmpMarkLabel = uiDocument.rootVisualElement.Query<Label>($"Mark{i}");
                tmpMarkLabel.text = levelMarksCounts[i].ToString();
            }

            Label totalNotesLabel = uiDocument.rootVisualElement.Query<Label>("TotalNotes");
            int notesCount = this._activePlayer.GetCurrentNoteCount();
            totalNotesLabel.text = notesCount.ToString();

            Label LevelMarkLabel = uiDocument.rootVisualElement.Query<Label>("TotalNotes");
            int levelMark = this._activePlayer.GetCurrentLevelMark();
            string levelMarkStr;
            switch (levelMark) { 
                case 0: levelMarkStr = "S";break;
                case 1: levelMarkStr = "A"; break;
                case 2: levelMarkStr = "B"; break;
                case 3: levelMarkStr = "C"; break;
                case 4: levelMarkStr = "D"; break;
                default: levelMarkStr = "E"; break;
            }
            LevelMarkLabel.text = levelMarkStr;

            Label LevelScoreLabel = uiDocument.rootVisualElement.Query<Label>("LevelScore");
            int levelScore = this._activePlayer.GetCurrentLevelScore();
            LevelScoreLabel.text = levelScore.ToString();

            Label comboCntLabel = uiDocument.rootVisualElement.Query<Label>("ComboCntLabel");
            int comboCnt = this._activePlayer.GetCurrentComboChainsCount();
            comboCntLabel.text = comboCnt.ToString();

            Label comboLgthLabel = uiDocument.rootVisualElement.Query<Label>("ComboLgthLabel");
            int maxComboLength = this._activePlayer.GetCurrentMaxComboLength();
            LevelScoreLabel.text = levelScore.ToString();

            Button returnBtn = uiDocument.rootVisualElement.Query<Button>("ReturnBtn");
            returnBtn.RegisterCallback<ClickEvent>(OnReturnBtnClick);

            Button retryBtn = uiDocument.rootVisualElement.Query<Button>("RetryBtn");
            returnBtn.RegisterCallback<ClickEvent>(OnRetryBtnClick);
        }

        
    }

    private void OnReturnBtnClick(ClickEvent inEvent) {
        this._activePlayer.LoadScene(ResultScreenUiScript._mainMenuSceneId);
    }

    private void OnRetryBtnClick(ClickEvent inEvent)
    {
        this._activePlayer.LoadScene(ResultScreenUiScript._levelScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
