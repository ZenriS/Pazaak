﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController_script : MonoBehaviour
{
    public string Difficulty;
    public int MaxValue;
    public int ActivePlayer;
    public GameObject MainCanvas;
    public TotalValueTracker_script LeftBoard, RightBoard;
    private PlayerDeckMananger_script _leftDeck, _rightDeck;
    private UIManager_script _uiManager;
    private GlobalDeckManager_script _globalDeckManager;
    public AIMananger_script AiMananger;
    public float SwitchDelay;
    private Coroutine _switchPlayer;
    public bool RoundDone;
    public int GameStage; //used to control controls
    public SoundManager_script SoundManager;
    public AudioClip RoundWin, RoundLose;
    private PlayerInfoManager_script _playerInfoManager;

    void Awake()
    {
        //LeftBoard = MainCanvas.transform.GetChild(1).GetChild(2).GetChild(4).GetComponent<TotalValueTracker_script>();
        //RightBoard = MainCanvas.transform.GetChild(1).GetChild(2).GetChild(5).GetComponent<TotalValueTracker_script>();
        _leftDeck = LeftBoard.GetComponent<PlayerDeckMananger_script>();
        _rightDeck = RightBoard.GetComponent<PlayerDeckMananger_script>();

        _uiManager = GetComponent<UIManager_script>();
        _globalDeckManager = GetComponent<GlobalDeckManager_script>();
        _playerInfoManager = _uiManager.PlayerInfoManager;
    }

    void Start()
    {
        //GameStage = 1;
        //LeftBoard.TogglePlayer(false);
        //RightBoard.TogglePlayer(false);
        //Invoke("GenerateDecks", 2f);
        //Invoke("StartGame", 4f);
    }

    public void GenerateDecks()
    {
        LeftBoard.GenerateDeck();
        RightBoard.GenerateDeck();
        _globalDeckManager.GenerateGlobalDeck();
        _uiManager.SetPlayerNames();
    }

    void DetermingStartingPlayer()
    {
        int left = _leftDeck.FindHighstCard();
        int right = _rightDeck.FindHighstCard();
        if (left > right)
        {
            ActivePlayer = 1;
        }
        else if (left < right)
        {
            ActivePlayer = 0;
        }
        else if (left == right)
        {
            float r = Random.Range(0f, 1f);
            if (r < 0.5f)
            {
                ActivePlayer = 1;
            }
            else
            {
                ActivePlayer = 0;
            }
        }
    }

    public void StartGame()
    {
        RoundDone = false;
        SwitchPlayer();
    }

    public void SwitchPlayer()
    {
        _switchPlayer = StartCoroutine(DoSwitchPlayer());
        //Debug.Log("Switch Player: ActivePlayer - " + ActivePlayer);
    }

    IEnumerator DoSwitchPlayer()
    {
        if (RoundDone)
        {
            Debug.Log("Round Over");
            //StopCoroutine(_switchPlayer);
            yield break;
        }

        LeftBoard.TogglePlayer(false);
        RightBoard.TogglePlayer(false);
        if (LeftBoard.PlayerDone && RightBoard.PlayerDone)
        {
            //CompareScore();
            StartCoroutine("CompareScore");
            yield break;
        }
        yield return new WaitForSeconds(SwitchDelay);
        //Debug.Log("Do Switch Player");
        if (LeftBoard.PlayerDone && !RightBoard.PlayerDone)
        {
            ActivePlayer = 1;
        }
        else if (RightBoard.PlayerDone && !LeftBoard.PlayerDone)
        {
            ActivePlayer = 0;
        }
        else if((LeftBoard.ActiveValue > MaxValue || RightBoard.ActiveValue > MaxValue))
        {
            RoundDone = true;
            //CompareScore();
            StartCoroutine("CompareScore");
            //StopCoroutine(_switchPlayer);
            yield break;
        }
        else
        {
            ActivePlayer = (ActivePlayer == 0) ? 1 : 0;
        }
        _uiManager.SwitchPlayer(ActivePlayer);
        _globalDeckManager.PlaceGlobalCard(ActivePlayer);
        yield return new WaitForSeconds(_globalDeckManager.MoveTime);
        if (ActivePlayer == 0)
        {
            LeftBoard.TogglePlayer(true);
            RightBoard.TogglePlayer(false);
        }
        else
        {
            RightBoard.TogglePlayer(true);
            LeftBoard.TogglePlayer(false);
        }
        if (ActivePlayer == AiMananger.AIBoard.PlayerID)
        {
            AiMananger.DeterminPlay();
        }
    }

    IEnumerator CompareScore()
    {
        Debug.Log("Compare score");
        string t;
        string s;
        string leftScore = LeftBoard.PlayerName + "\n\n Score \n" + LeftBoard.ActiveValue + " of " + MaxValue;
        string rightScore = RightBoard.PlayerName + "\n\n Score \n" + RightBoard.ActiveValue + " of " + MaxValue;
        RoundDone = true;
        AiMananger.RoundOver();
        yield return new WaitForSeconds(0.5f);
        if ((LeftBoard.ActiveValue > RightBoard.ActiveValue && LeftBoard.ActiveValue <= MaxValue) || (LeftBoard.ActiveValue <= MaxValue && RightBoard.ActiveValue > MaxValue)) //left wins
        {
            LeftBoard.Wins++;
            _uiManager.Invoke("UpdateLeftRoundCounter",1.1f);
            Debug.Log("GameController_script: CompareScore: Player 1(left) Wins");
            SoundManager.PlayEffetDelay(RoundWin, 0.5f);
            if (LeftBoard.Wins == 3) //player wins
            {
                t = "Game Over";
                s = LeftBoard.PlayerName + " Wins";
                _uiManager.ToggleEndScreen(true,false, t, s, leftScore, rightScore);
                _playerInfoManager.GameWon();
                int winnings = Mathf.RoundToInt(AiMananger.AIStates.Odds * _playerInfoManager.BetAmount);
                _playerInfoManager.ModifyCredits(winnings);
                yield break;
            }
            t = "Round Over";
            s = LeftBoard.PlayerName + " Wins";
            ActivePlayer = 1;
            _uiManager.ToggleEndScreen(true,true, t, s, leftScore, rightScore);
        }
        else if ((RightBoard.ActiveValue > LeftBoard.ActiveValue && RightBoard.ActiveValue <= MaxValue) || (RightBoard.ActiveValue <= MaxValue && LeftBoard.ActiveValue > MaxValue)) //right wins
        {
            RightBoard.Wins++;
            _uiManager.Invoke("UpdateRightRoundCounter",1.1f);
            Debug.Log("GameController_script: CompareScore: Player 2(left) Wins");
            SoundManager.PlayEffetDelay(RoundLose, 0.5f);
            if (RightBoard.Wins == 3) //ai wins
            {
                t = "Game Over";
                s = RightBoard.PlayerName + " Wins";
                _uiManager.ToggleEndScreen(true,false, t, s, leftScore, rightScore);
                _playerInfoManager.GameLost();
                _playerInfoManager.ModifyCredits(-_playerInfoManager.BetAmount);
                yield break;
            }
            t = "Round Over";
            s = RightBoard.PlayerName + " Wins";
            ActivePlayer = 0;
            _uiManager.ToggleEndScreen(true,true, t, s, leftScore, rightScore);
        }
        else if(LeftBoard.ActiveValue == RightBoard.ActiveValue || (LeftBoard.ActiveValue > MaxValue && RightBoard.ActiveValue > MaxValue)) //draw
        {
            t = "Round Over";
            if (LeftBoard.TieBreaker && !RightBoard.TieBreaker)
            {
                s = "Tie break \n " + LeftBoard.PlayerName + " Wins!";
                LeftBoard.Wins++;
            }
            else if (!LeftBoard.TieBreaker && RightBoard.TieBreaker)
            {
                s = "Tie break \n " + RightBoard.PlayerName + " Wins!";
                RightBoard.Wins++;
            }
            else if (LeftBoard.TieBreaker && RightBoard.TieBreaker)
            {
                s = "Double Tie Break\nDraw";
            }
            else
            {
                s = "Draw";
            }
            _uiManager.ToggleEndScreen(true,true, t, s, leftScore, rightScore);
        }
    }

    public void NewRound() //ui button
    {
        Debug.Log("New round");
        StopAllCoroutines();
        StartCoroutine(StartNewRound());
    }

    IEnumerator StartNewRound()
    {
        _uiManager.ToggleEndScreen(false, false);
        List<PlayCard_script> pcs = new List<PlayCard_script>();
        pcs.AddRange(LeftBoard.transform.GetChild(0).GetComponentsInChildren<PlayCard_script>());
        pcs.AddRange(RightBoard.transform.GetChild(0).GetComponentsInChildren<PlayCard_script>());
        //sound effect
        foreach (PlayCard_script pc in pcs)
        {
           pc.RemoveCard();
        }
        LeftBoard.ResetValues(false);
        RightBoard.ResetValues(false);
        yield return new WaitForSeconds(1.1f);
        //screen transition etc
        StartGame();
    }

    public void NewGame() //ui button
    {
        GameStage = 1;
        //Debug.Log("New game");
        StopAllCoroutines();
        StartCoroutine(StartNewGame());
    }

    IEnumerator StartNewGame()
    {
        _uiManager.ToggleLoadingScreen(true);
        yield return new WaitForSeconds(1f);

        LeftBoard.TogglePlayer(false);
        RightBoard.TogglePlayer(false);
        _uiManager.PreGameScreen.SetActive(false);
        _uiManager.GameBoardScreen.SetActive(true);
        LeftBoard.ResetValues(true);
        RightBoard.ResetValues(true);
        List<PlayCard_script> pcs = new List<PlayCard_script>();
        pcs.AddRange(LeftBoard.DiscardPile.GetComponentsInChildren<PlayCard_script>());
        pcs.AddRange(LeftBoard._mainCardBoard.GetComponentsInChildren<PlayCard_script>());
        pcs.AddRange(LeftBoard._handCardBoard.GetComponentsInChildren<PlayCard_script>());
        pcs.AddRange(RightBoard._mainCardBoard.GetComponentsInChildren<PlayCard_script>());
        pcs.AddRange(RightBoard._handCardBoard.GetComponentsInChildren<PlayCard_script>());
        foreach (PlayCard_script pc in pcs)
        {
            pc.DestroyCard();
        }
        //screen transition etc
        yield return new WaitForSeconds(1f);
        GenerateDecks();
        yield return new WaitForSeconds(1f);
        DetermingStartingPlayer();
        _uiManager.ToggleEndScreen(false, false);
        _uiManager.ResetUI();
        GameStage = 1;
        StartGame();
        _uiManager.ToggleLoadingScreen(false);
    }

}
