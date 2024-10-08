using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Player _activePlayer;
    public static readonly string synthMusicFolder = $"SynthMusic{Path.DirectorySeparatorChar}";

    private void Awake()
    {
        if (this._activePlayer == null)
        {
            this._activePlayer = new Player();
            DontDestroyOnLoad(this.gameObject);
        }
        this._activePlayer.CheckAndLoadAvaibleLevelMetadata(synthMusicFolder);
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("PlayerLoaded");
        
        

        

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Player GetActivePlayer() {
        return this._activePlayer;
    }
}
