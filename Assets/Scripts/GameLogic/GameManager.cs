﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private PlayerInputManager inputManager;
    
    public Loader loader;
    public LevelSelector levelSelector;
    public AchievementChecker achievementChecker;
    public ControlSchemeSpriteHandler controlSchemeSpriteHandler;
    public int playerCount = 0;
    public Color[] playerColours;

    [HideInInspector] public List<PlayerStats> playerStats = new List<PlayerStats>();
    [HideInInspector] public List<PlayerInput> playerInputs = new List<PlayerInput>();
    [HideInInspector] public List<UIController> uIControllers = new List<UIController>();

    private BaseGamemode selectedGamemode;

    public BaseGamemode SelectedGamemode
    {
        get { return selectedGamemode; }
    }

    private FreeForAllGamemode freeForAllGamemode;
    private ExtractionGamemode extractionGamemode;

    private Player[] players = new Player[4];
    private bool firstKeyboardPlayerHasJoined;
    private bool secondKeyboardPlayerHasJoined;

    // Sets up this class as a singleton
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        inputManager = GetComponent<PlayerInputManager>();
        freeForAllGamemode = GetComponent<FreeForAllGamemode>();
        extractionGamemode = GetComponent<ExtractionGamemode>();
    }


    private void Start()
    {
        inputManager.EnableJoining();
        SceneManager.activeSceneChanged += OnSceneChange;
    }

    public int AssignPlayerNumber (Player player)
    {
        int num = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (num > 0)
            {
                continue;
            }
            if (players[i] == null)
            {
                players[i] = player;
                num = i + 1;
            }
        }
        return num;
    }


    // This prevents players from being able to join the game whilst a match is underway
    // Players can only join in the main menu
    private void OnSceneChange(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            inputManager.EnableJoining();
        }
        else
        {
            inputManager.DisableJoining();
        }
    }

    // called by player input manager when new device enters
    // increments player count so that the next player who joins gets the appropriate player number
    void OnPlayerJoined()
    {
        playerCount++;
        playerStats.Add(new PlayerStats(playerCount));        
    }

    // called by player input manager when device leaves
    // Removes player stats ui for the player that left and changes the count of players so that new players can join
    void OnPlayerLeft()
    {
        print("Player Left");
        for (int i = 0; i < playerStats.Count; i++)
        {
            bool playerIsStillConnected = false;
            for (int j = 0; j < players.Length; j++)
            {
                if (players[j] == null)
                {
                    continue;
                }
                if (playerStats[i].playerNumber == players[j].playerNumber)
                {
                    playerIsStillConnected = true;
                }
            }

            if (playerIsStillConnected == false)
            {
                playerStats.Remove(playerStats[i]);
            }
        }
        playerCount--;
        
    }

    // Called by game starter to start the match
    public void StartMatch(GameMode gameMode)
    {

        // sets the selected gamemode and starts the match
        switch (gameMode)
        {
            case GameMode.FreeForAll:
                selectedGamemode = freeForAllGamemode;
                break;
            case GameMode.Elimination:
                break;
            case GameMode.Extraction:
                selectedGamemode = extractionGamemode;
                break;
            case GameMode.Climb:
                break;
            default:
                break;
        }

        // Goes to a random level from the playlist of the selected gamemode
        levelSelector.GoToLevel(gameMode, selectedGamemode.StartMatch);
    }

    // Called when the match is over or the player leaves
    // Returns back to the main menu scene and respawns all the characters 
    // sets time scale back to 1 incase the players left through pause menu
    public void EndMatch()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            return;
        }

        selectedGamemode.Exit();
        selectedGamemode = null;
        SceneManager.LoadScene(0);        
        UnPause();
        StartCoroutine("CoEndMatch");
    }

    IEnumerator CoEndMatch()
    {
        yield return new WaitForSeconds(0.1f);
        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            player.CreateNewCharacter();
        }
    }

    // Called when a player presses the pause button
    // This then tells all the players to switch controls to UI
    // Also tells the UI manager to bring up the pause menu and cursors
    public void Pause ()
    {
        if (UIManager.instance.CurrentUIState == UIState.MatchEnd)
        {
            return;
        }

        Time.timeScale = 0;

        foreach (PlayerInput player in playerInputs)
        {
            player.SwitchCurrentActionMap("UI");
        }


        UIManager.instance.Pause(uIControllers);       

    }

    // Tells all players to switch controls back to player
    public void UnPause ()
    {
        if (UIManager.instance.CurrentUIState == UIState.MatchEnd)
        {
            return;
        }
        Time.timeScale = 1;

        foreach (PlayerInput player in playerInputs)
        {
            player.SwitchCurrentActionMap(player.GetComponent<Player>().currentPlayerActionMap);
        }

        UIManager.instance.UnPause();
    }

    public void AwardKill(int playerNumber)
    {
        foreach (PlayerStats player in playerStats)
        {
            if (player.playerNumber == playerNumber)
            {
                player.playerKills++;
            }
        }

        if (selectedGamemode != null)
        {
            selectedGamemode.AwardPlayerKill(playerNumber);
        }
    }

    public void AwardMatchWin(int playerNumber)
    {
        foreach (PlayerStats player in playerStats)
        {
            if (player.playerNumber == playerNumber)
            {
                player.matchWins++;
            }
        }
    }

    private void Update()
    {
        if (secondKeyboardPlayerHasJoined == false)
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                secondKeyboardPlayerHasJoined = true;
                inputManager.JoinPlayer(playerCount, playerCount,"SecondKeyboard", Keyboard.current);
            }
        }
        if (firstKeyboardPlayerHasJoined == false)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                firstKeyboardPlayerHasJoined = true;
                inputManager.JoinPlayer(playerCount, playerCount, "Keyboard & Mouse", Keyboard.current);
            }
        }
    }

}
