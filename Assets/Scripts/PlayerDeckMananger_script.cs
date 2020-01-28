﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeckMananger_script : MonoBehaviour
{
    public GameObject CardPrefab;
    public List<int> PlayerDeck;
    private List<int> _activeDeck;
    private Transform _cardSlots;
    private TotalValueTracker_script _totalValueTracker;
    public Color CardColor;
    
    void Start()
    {
        _totalValueTracker = GetComponent<TotalValueTracker_script>();
        _cardSlots = this.transform.GetChild(1);
    }

    public void GenereateDeck()
    {
        _activeDeck = PlayerDeck;
        _activeDeck.Shuffle();
        foreach (Transform t in _cardSlots)
        {
            int v = _activeDeck[0];
            _activeDeck.RemoveAt(0);
            GameObject go = Instantiate(CardPrefab);
            PlayCard_script pc = go.GetComponent<PlayCard_script>();
            pc.PlaceCard(t, false, false);
            pc.Config(_totalValueTracker.PlayerID, v,CardColor, _totalValueTracker.GameController);
        }
    }
}
