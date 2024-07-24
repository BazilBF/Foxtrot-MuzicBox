using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        PlayerController playerControllerScript;
        if (playerObject == null)
        {
            playerObject = new GameObject();
            playerControllerScript = playerObject.AddComponent<PlayerController>();
            playerObject.tag = "Player";
            playerObject.name = "PlayerContainer";

        }
        else {
            playerControllerScript = playerObject.GetComponent<PlayerController>();
        }

        GameObject mainMenuUI = GameObject.FindGameObjectWithTag("GameController");
        mainMenuUI.SendMessage("InititMainMenu", playerControllerScript.GetActivePlayer());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
