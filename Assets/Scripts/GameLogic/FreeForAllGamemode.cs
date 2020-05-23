﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeForAllGamemode : BaseGamemode
{
    
    [HideInInspector] public int playersEliminated;

    


    public override void StartMatch ()
    {
        base.StartMatch();
        Debug.Log("Free for all start match");
        playersStillAliveThisRound = numOfPlayers;

        foreach (Player player in players)
        {
            player.CharacterDied(false);
        }

        StartRound();
    }

    protected override void EndMatch ()
    {
        List<PlayerStats> playerStats = UIManager.instance.playerStats;
        int highestWins = 0;
        List<int> currentBestPlayers = new List<int>() {0};
        foreach (PlayerStats player in playerStats)
        {
            if (player.roundWins > highestWins)
            {
                currentBestPlayers.Clear();
                currentBestPlayers.Add(player.playerNumber);
                highestWins = player.roundWins;
            }
            else if (player.roundWins == highestWins)
            {
                currentBestPlayers.Add(player.playerNumber);
            }
        }

        print(currentBestPlayers);

        UIManager.instance.EndMatch(currentBestPlayers);
        StartCoroutine("DelayAtEndOfMatch");
            
    }

    public void StartRound()
    {
        if (roundNumber == 1)
        {
            foreach (var player in players)
            {
                UIManager.instance.CreateNewPlayerStats(player.playerNumber);
            }
        }

        playersStillAliveThisRound = numOfPlayers - playersEliminated;
        UIManager.instance.StartNewRound(roundNumber);

        foreach (Player player in players)
        {
            player.CreateNewCharacter();
        }

    }

    public override void EndRound (int winningPlayerNumber)
    {
        if (roundNumber >= numberOfRounds)
        {
            EndMatch();
        }
        else
        {
            UIManager.instance.EndRound(winningPlayerNumber , roundNumber);
            roundNumber++;
            print("End Round");
            StartCoroutine("DelayBetweenRounds");
        }

    }



    public override void PlayerDied ()
    {
        base.PlayerDied();

        if (playersStillAliveThisRound == 1)
        {
            int winningPlayerNumber = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].isStillAlive)
                {
                    winningPlayerNumber = players[i].playerNumber;
                }
            }
            EndRound(winningPlayerNumber);
        }
        else if (playersStillAliveThisRound < 1)
        {
            EndRound(0);
        }
    }


    IEnumerator DelayBetweenRounds ()
    {
        yield return new WaitForSeconds(3);
        StartRound();
        GameManager.instance.levelSelector.GoToLevel(GameMode.FreeForAll);
    }

    IEnumerator DelayAtEndOfMatch ()
    {
        yield return new WaitForSeconds(3);

        for (int i = 1; i <= numOfPlayers; i++)
        {
            UIManager.instance.RemovePlayerStats(i);
        }

        GameManager.instance.EndMatch();
    }
}