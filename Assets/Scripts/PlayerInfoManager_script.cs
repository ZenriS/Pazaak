﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerInfoManager_script : MonoBehaviour
{
    public string Name; //name of the player
    public int Credits; //Amount of money

    [System.Serializable]
    public class DeckInventroyClass
    {
        public string CardInfo;
        public int CardAmount;
    }

    public List<DeckInventroyClass> PlayerDeck;

    public List<string> ActiveDeck; //All cards the player has selected to play with, max 12

    //avatar info
    public List<int> PartIndex;
    public List<Sprite> AvatarParts; //avatar sprite

    public List<int> ColorIndex;
    public List<Color> PartColors;

    //options info
    public float MusicVolume;
    public float SFXVolume;    
    
    //Level? //used to unlock AI opponets?

    public int Wins, Loses, Played;
    private float _winRate; //wins/gamesPlayed

    public int BetAmount;

    public GenerateSelectionCards_script CardSelection;
    public UIManager_script UiManager;
    public CharacterCreator_script CharacterCreator;
    public Options_script Options;
    
    void Update()
    {
        //Debugging
        if (Input.GetKeyDown(KeyCode.F2)) //cheat in money
        {
            ModifyCredits(99999);
        }
    }

    public void CalcWinRate()
    {
        _winRate = (float)Wins / (float)Played;
    }

    public void GameWon()
    {
        Wins++;
        Played++;
        CalcWinRate();
    }

    public void GameLost()
    {
        Loses++;
        Played++;
        CalcWinRate();
    }

    public void ModifyCredits(int credits)
    {
        Credits += credits;
        UiManager.UpdatePlayerInfoBar();
        SavePlayerInfo();
    }

    public void AddNewCard(string cardInfo, int price)
    {
        Credits -= price;
        int index = 999;
        for (int i = 0; i < PlayerDeck.Count; i++)
        {
            if (PlayerDeck[i].CardInfo == cardInfo)
            {
                index = i;
                break;
            }
        }
        if (index == 999)
        {
            DeckInventroyClass newCard = new DeckInventroyClass();
            newCard.CardAmount = 1;
            newCard.CardInfo = cardInfo;
            PlayerDeck.Add(newCard);
        }
        else
        {
            PlayerDeck[index].CardAmount++;
        }

        SavePlayerInfo();
    }
    
    //New game, save and load stuff
    public void LoadPlayerInfo()
    {
        PlayerData_script playerData = SaveSystem_script.LoadPlayer();
        Name = playerData.Name;

        PartIndex = new List<int>(playerData.PartIndex);
        ColorIndex = new List<int>(playerData.ColorIndex);

        CharacterCreator.GetAvatarInfo(PartIndex, ColorIndex, out AvatarParts, out PartColors);
        
        UiManager.SetPlayerGraphics(AvatarParts, PartColors);

        Credits = playerData.Credits;
        
        CombinePlayerDeck(playerData.CardString, playerData.CardAmount);

        Wins = playerData.Wins;
        Loses = playerData.Loses;
        Played = playerData.Played;

        MusicVolume = playerData.MusicVolume;
        SFXVolume = playerData.SFXVolume;

        Options.LoadVolumeSettings(MusicVolume, SFXVolume);

        CalcWinRate();
    }

    void CombinePlayerDeck(List<string> cardInfo, List<int> cardAmount)
    {
        if (cardInfo == null)
        {
            return;
        }
        PlayerDeck = new List<DeckInventroyClass>();
        for (int i = 0; i < cardInfo.Count; i++)
        {
            DeckInventroyClass newCard = new DeckInventroyClass();
            newCard.CardInfo = cardInfo[i];
            newCard.CardAmount = cardAmount[i];

            PlayerDeck.Add(newCard);
        }
    }

    public void SavePlayerInfo()
    {
        SaveSystem_script.SaveData(this);
    }

    public void SetDefaultValues()
    {
        Credits = 1000;
        Wins = 0;
        Loses = 0;
        Played = 0;

        SetPlayerAvatar();

        SavePlayerInfo();
    }

    public void SetPlayerName(string s)
    {
        Name = s;
    }

    public void SetPlayerAvatar()
    {
        CharacterCreator.GetIndexValues(out PartIndex, out ColorIndex);
        CharacterCreator.GetAvatarInfo(PartIndex, ColorIndex, out AvatarParts, out PartColors);

        UiManager.SetPlayerGraphics(AvatarParts, PartColors);
    }
}
