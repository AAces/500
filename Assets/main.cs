﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using UnityEngine.UI;

public class main : MonoBehaviour
{

    public Button cardSelectSubmitButton, cardSelectRemoveButton, setButton, runButton;

    public Button[] playerButtons, actionButtons, numberButtons, spades, hearts, diamonds, clubs; //0=spades, 1=hearts, 2=diamonds, 3=clubs

    private Button[][] cardButtons;

    public Text selectedCardsText, expectedCardsText, discardPileText, tableMeldsText;

    public Text[] handTexts;

    private List<Meld> meldsOnTable, storedMelds;

    private Hand[] hands;

    private Card storedCard;

    public Card[][] cards;

    private List<Card> selectedCards, discardPile;

    private int activePlayer, playerCount, expectedCards;

    public bool disableAuto;

    private bool drawFromDeck, turnOver, playingOnExisting, playingNewMeld, drawingFromDiscard, firstCardEntry, discarding, min, selectingCards;

    void Start()
    {
        init();
    }

    void init()
    {
        meldsOnTable = new List<Meld>();
        discardPile = new List<Card>();
        selectedCards = new List<Card>();
        firstCardEntry = false;
        discarding = false;
        drawingFromDiscard = false;
        playingOnExisting = false;
        playingNewMeld = false;
        selectingCards = false;
        drawFromDeck = false;
        turnOver = false;
        min = false;
        selectedCardsText.text = "";
        expectedCardsText.text = "";
        tableMeldsText.text = "";
        discardPileText.text = "";
        expectedCardsText.gameObject.SetActive(false);
        selectedCardsText.gameObject.SetActive(false);
        discardPileText.gameObject.SetActive(false);
        tableMeldsText.gameObject.SetActive(false);
        setupCards();
        setupButtons();
        foreach (var v in handTexts)
        {
            v.gameObject.SetActive(false);
            v.text = "";
        }
        foreach (var v in playerButtons)
        {
            v.gameObject.SetActive(true);
        }
    }

    void turn()
    {
        foreach (var t in handTexts)
        {
            t.color = Color.black;
        }
        handTexts[activePlayer].color = Color.red;

        if (activePlayer == 0 && !disableAuto)
        {
            //TODO: AI Turn 

            //Draw:

            //Play melds:

            //Play on melds:

            //Discard:

            var vals = new float[hands[0].getKnownCards().Count];
            Debug.Log("Card values (Higher=hold it, Lower=Discard):");
            for (var i = 0; i < hands[0].getKnownCards().Count; i++)
            {
                vals[i] = hands[0].getKnownCards()[i].getValueInHand(hands, meldsOnTable, discardPile);
                Debug.Log(hands[0].getKnownCards()[i].asString()+": " +vals[i]);
            }


        }
        else
        {
            actionButtons[0].gameObject.SetActive(true);
            actionButtons[1].gameObject.SetActive(true);
            actionButtons[2].gameObject.SetActive(false);
            actionButtons[3].gameObject.SetActive(false);
            actionButtons[4].gameObject.SetActive(false);
        }
    }

    void setupCards()
    {
        cards = new Card[4][];
        for (var s = 0; s < 4; s++) //0=spades, 1=hearts, 2=diamonds, 3=clubs
        {
            cards[s] = new Card[13];
            for (var r = 0; r < 13; r++)
            {
                cards[s][r] = new Card(s, r+1);
            }
        }

        foreach (var v in cards)
        {
            foreach (var c in v)
            {
                c.setList(cards);
            }
        }
    }

    void setupButtons() 
    {
        cardButtons = new[] { spades, hearts, diamonds, clubs }; //0=spades, 1=hearts, 2=diamonds, 3=clubs
        hideCardButtons();

        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 13; j++)
            {
                var s = i;
                var r = j;
                cardButtons[s][r].onClick.RemoveAllListeners();
                cardButtons[s][r].onClick.AddListener(delegate { cardButtonPress(s, r); });
            }
        }

        foreach (var v in actionButtons)
        {
            v.gameObject.SetActive(false);
        }

        for (var j = 0; j < 5; j++)
        {
            var a = j;
            actionButtons[a].onClick.RemoveAllListeners();
            actionButtons[a].onClick.AddListener(delegate { actionButtonPress(a); });
        }

        foreach (var v in playerButtons)
        {
            v.gameObject.SetActive(false);
        }

        for (var j = 0; j < 4; j++)
        {
            var a = j;
            playerButtons[a].onClick.RemoveAllListeners();
            playerButtons[a].onClick.AddListener(delegate { pressPlayerCount(a); });
        }
        
        foreach (var v in numberButtons)
        {
            v.gameObject.SetActive(false);
        }

        for (var j = 0; j < 4; j++)
        {
            var a = j;
            numberButtons[a].onClick.RemoveAllListeners();
            numberButtons[a].onClick.AddListener(delegate { pressPlayerSelect(a); });
        }

        setButton.gameObject.SetActive(false);
        runButton.gameObject.SetActive(false);

        setButton.onClick.RemoveAllListeners();
        setButton.onClick.AddListener(setButtonPress);

        runButton.onClick.RemoveAllListeners();
        runButton.onClick.AddListener(runButtonPress);

        cardSelectRemoveButton.onClick.RemoveAllListeners();
        cardSelectRemoveButton.onClick.AddListener(cardSelectRemovePress);

        cardSelectSubmitButton.onClick.RemoveAllListeners();
        cardSelectSubmitButton.onClick.AddListener(cardSelectSubmitPress);
    }

    void pressPlayerSelect(int p)
    {
        activePlayer = p;
        foreach (var v in numberButtons)
        {
            v.gameObject.SetActive(false);
        }
        turn();
    }

    void actionButtonPress(int a) //0=draw from pile, 1=draw from discard, 2=play new meld, 3=play on existing meld, 4=discard 
    {
        if (selectingCards) return;
        switch (a)
        {
            case 0: //draw from pile
                actionButtons[0].gameObject.SetActive(false);
                actionButtons[1].gameObject.SetActive(false);
                actionButtons[2].gameObject.SetActive(true);
                actionButtons[3].gameObject.SetActive(true);
                actionButtons[4].gameObject.SetActive(true);
                if (activePlayer == 0 && disableAuto)
                {
                    drawFromDeck = true;
                    promptCardEntry(1,false);
                }
                else
                {
                    hands[activePlayer].blindDraw();
                } 
                updateHandTexts();
                break;
            case 1: //draw from discard
                actionButtons[0].gameObject.SetActive(false);
                actionButtons[1].gameObject.SetActive(false);
                actionButtons[2].gameObject.SetActive(true);
                actionButtons[3].gameObject.SetActive(true);
                actionButtons[4].gameObject.SetActive(true);
                drawingFromDiscard = true;
                promptCardEntry(1, false);
                break;
            case 2: //play new meld
                playingNewMeld = true;
                promptCardEntry(3, true);
                break;
            case 3: //play on existing meld
                playingOnExisting = true;
                promptCardEntry(1, false);
                break;
            case 4: //discard
                discarding = true;

                //TODO: Remove this-------------------------------------------------------------------------
                if (activePlayer==0)
                {
                    var vals = new float[hands[0].getKnownCards().Count];
                    Debug.Log("Card values (Higher=hold it, Lower=Discard):");
                    for (var i = 0; i < hands[0].getKnownCards().Count; i++)
                    {
                        vals[i] = hands[0].getKnownCards()[i].getValueInHand(hands, meldsOnTable, discardPile);
                        Debug.Log(hands[0].getKnownCards()[i].asString() + ": " + vals[i]);
                        
                    }
                    hands[0].debugUsefulCards(meldsOnTable);
                }
                //TODO:--------------------------------------------------------------------------------------
                
                promptCardEntry(1,false);
                break;
        }
    }

    void promptCardEntry(int e, bool m)
    {
        expectedCards = e;
        min = m;
        selectedCardsText.text = "";
        expectedCardsText.text = "";
        if (firstCardEntry)
        {
            expectedCardsText.text = "Enter starting hand (7)";
        }
        if (turnOver)
        {
            expectedCardsText.text = "Enter flipped-over card (1)";
        }
        if (discarding)
        {
            expectedCardsText.text = "Enter discarded card (1)";
        }
        if (playingNewMeld)
        {
            expectedCardsText.text = "Enter all cards in meld (3+)";
        }
        if (playingOnExisting)
        {
            expectedCardsText.text = "Enter card played (1)";
        }
        if (drawingFromDiscard)
        {
            expectedCardsText.text = "Enter card picked up from (1)";
        }
        
        selectedCards.Clear();
        showCardButtons();
        selectingCards = true;
    }

    void promptFirstCardEntry()
    {
        firstCardEntry = true;
        Debug.Log("Awaiting first hand");
        promptCardEntry(7,false);
    }

    void cardButtonPress(int s, int r)
    {
        var c = cards[s][r];
        selectedCards.Add(c);
        selectedCardsText.text = stringOfSelectedCards();
        cardButtons[s][r].gameObject.SetActive(false);
    }

    void updateDiscardPile()
    {
        discardPileText.text = "";
        foreach (var c in discardPile)
        {
            discardPileText.text += c.asString();
            discardPileText.text += " ";
        }
    }

    void updateMeldsText()
    {
        tableMeldsText.text = "";
        foreach (var m in meldsOnTable)
        {
            tableMeldsText.text += m.asText();
            tableMeldsText.text += "\n";
        }
    }

    string stringOfSelectedCards()
    {
        var temp = "";
        foreach (var v in selectedCards)
        {
            temp += v.asString();
            temp += " ";
        }

        return temp;
    }

    void cardSelectSubmitPress()
    {
        if (min)
        {
            if (selectedCards.Count < expectedCards) return;
        }
        else
        {
            if (selectedCards.Count != expectedCards) return;
        }
        hideCardButtons();
        selectingCards = false;
        if (firstCardEntry)
        {
            hands[0].setHand(selectedCards);
            firstCardEntry = false;
            turnOver = true;
            promptCardEntry(1,false);
        } else if (turnOver)
        {
            var c = selectedCards[0];
            c.markAsSeen();
            discardPile.Add(c);
            turnOver = false;
            for (var i = 0; i < playerCount; i++)
            {
                numberButtons[i].gameObject.SetActive(true);
            }
            updateDiscardPile();
        }
        else if (discarding)
        {
            var c = selectedCards[0];
            hands[activePlayer].discard(c);
            discardPile.Add(c);
            c.markAsSeen();
            discarding = false;
            activePlayer++;
            if (activePlayer >= playerCount)
            {
                activePlayer = 0;
            }
            updateDiscardPile();
            turn();
        } else if (drawingFromDiscard)
        {
            var c = selectedCards[0];
            var i = discardPile.IndexOf(c);
            for (var j = discardPile.Count-1; j >= i; j--)
            {
                hands[activePlayer].drawCard(discardPile[j]);
                discardPile.RemoveAt(j);
            }
            updateDiscardPile();
            drawingFromDiscard = false;
        } else if (playingNewMeld)
        {
            if (!validMeld(selectedCards)) return;
            var t = new List<Card>();
            foreach (var c in selectedCards)
            {
                t.Add(c);
            }
            var p = new Meld(t, cards);
            hands[activePlayer].playMeld(p);
            meldsOnTable.Add(p);
            Debug.Log("There are " + meldsOnTable.Count + " melds on the table.");
            for (var i = 0; i < meldsOnTable.Count; i++)
            {
                Debug.Log(i);
                Debug.Log(".asText: " + meldsOnTable[i].asText());
                Debug.Log("Meld type: " + meldsOnTable[i].getMeldType());
                Debug.Log("First card in list: " + meldsOnTable[i].getCards()[0].asString());
            }
            playingNewMeld = false;
            updateMeldsText();
        } else if (playingOnExisting)
        {
            var c = selectedCards[0];
            var m = meldsOnTable.Where(p => p.canPlay(c)).ToList();
            switch (m.Count)
            {
                case 1:
                    hands[activePlayer].playCard(c, m[0]);
                    break;
                case 2:
                    if (m[0].getMeldType() == m[1].getMeldType())
                    {
                        hands[activePlayer].playCard(c, m[0]);
                    }
                    else
                    {
                        setOrRun(c, m);
                    }
                    break;
                case 3:
                    if (m[0].getMeldType() == m[1].getMeldType())
                    {
                        setOrRun(c, new List<Meld> { m[0], m[2] });
                    } else
                    {
                        setOrRun(c, new List<Meld> { m[0], m[1] });
                    }
                    break;
            }
            updateMeldsText();
            playingOnExisting = false;
        } else if (drawFromDeck)
        {
            var c = selectedCards[0];
            hands[activePlayer].drawCard(c);
            drawFromDeck = false;
        }
        updateHandTexts();
    }

    void setOrRun(Card c, List<Meld> pp)
    {
        storedCard = c;
        storedMelds = pp;
        setButton.gameObject.SetActive(true);
        runButton.gameObject.SetActive(true);
    }

    void setButtonPress()
    {
        hands[activePlayer].playCard(storedCard, storedMelds[0].getMeldType() == 0 ? storedMelds[0] : storedMelds[1]);
        setButton.gameObject.SetActive(false);
        runButton.gameObject.SetActive(false);
    }

    void runButtonPress()
    {
        hands[activePlayer].playCard(storedCard, storedMelds[0].getMeldType() == 0 ? storedMelds[1] : storedMelds[0]);
        setButton.gameObject.SetActive(false);
        runButton.gameObject.SetActive(false);
    }

    void cardSelectRemovePress()
    {
        if (selectedCards.Count == 0) return;
        var c = selectedCards[selectedCards.Count - 1];
        selectedCards.Remove(c);
        cardButtons[c.getSuit()][c.getRank()-1].gameObject.SetActive(true);
        selectedCardsText.text = stringOfSelectedCards();
    }

    void showCardButtons()
    {
        cardSelectRemoveButton.gameObject.SetActive(true);
        cardSelectSubmitButton.gameObject.SetActive(true);
        selectedCardsText.gameObject.SetActive(true);
        expectedCardsText.gameObject.SetActive(true);
        foreach (var v in cardButtons)
        {
            foreach (var c in v)
            {
                c.gameObject.SetActive(true);
            }
        }

        foreach (var v in cards)
        {
            foreach (var c in v)
            {
                if ((drawFromDeck || turnOver) && (hands.Any(h => h.getKnownCards().Contains(c)) || meldsOnTable.Any(m=>m.getCards().Contains(c)) || discardPile.Contains(c)))
                {
                    cardButtons[c.getSuit()][c.getRank()-1].gameObject.SetActive(false);
                }

                if (drawingFromDiscard && (disableAuto || activePlayer != 0))
                {
                    if (!discardPile.Contains(c))
                    {
                        cardButtons[c.getSuit()][c.getRank() - 1].gameObject.SetActive(false);
                    }
                }

                if (playingNewMeld||discarding) 
                {
                    if (discardPile.Contains(c) || meldsOnTable.Any(m=>m.getCards().Contains(c)))
                    {
                        cardButtons[c.getSuit()][c.getRank() - 1].gameObject.SetActive(false);
                    }
                    else
                    {
                        for (var i = 0; i < playerCount; i++)
                        {
                            if (i == activePlayer) continue;
                            if (hands[i].getKnownCards().Contains(c))
                            {
                                cardButtons[c.getSuit()][c.getRank() - 1].gameObject.SetActive(false);
                            }
                        }
                    }
                }

                if (playingOnExisting && !meldsOnTable.Any(m => m.canPlay(c)))
                {
                    cardButtons[c.getSuit()][c.getRank() - 1].gameObject.SetActive(false);
                }
            }
        }
        foreach (var v in handTexts)
        {
            v.gameObject.SetActive(false);
        }
        discardPileText.gameObject.SetActive(false);
        tableMeldsText.gameObject.SetActive(false);
    }

    void hideCardButtons()
    {
        cardSelectRemoveButton.gameObject.SetActive(false);
        cardSelectSubmitButton.gameObject.SetActive(false);
        selectedCardsText.gameObject.SetActive(false);
        expectedCardsText.gameObject.SetActive(false);
        foreach (var v in cardButtons)
        {
            foreach (var c in v)
            {
                c.gameObject.SetActive(false);
            }
        }
        for (var i = 0; i < playerCount; i++)
        {
            handTexts[i].gameObject.SetActive(true);
        }
        discardPileText.gameObject.SetActive(true);
        tableMeldsText.gameObject.SetActive(true);
    }

    void pressPlayerCount(int n)
    {
        Debug.Log((n+1)+" players selected.");
        hands = new Hand[n+1];
        playerCount = n+1;
        for (var i = 0; i < n+1; i++)
        {
            hands[i] = new Hand(i, cards);
        }
        foreach (var v in playerButtons)
        {
            v.gameObject.SetActive(false);
        }
        promptFirstCardEntry();
    }

    private bool validMeld(List<Card> c)
    { //0=spades, 1=hearts, 2=diamonds, 3=clubs
        if (c.Count != c.Distinct().Count() || c.Count < 3) return false;
        if (c[0].getSuit() == c[1].getSuit())
        {//run
            var s = c[0].getSuit();
            var t = c.Select(k => k.getRank()).ToList();
            var M = t.Max();
            var m = t.Min();
            if (c.Any(j => j.getSuit() != s))
            {
                return false;
            }

            if (m == 1 && M == 13)
            {
                if (t.Contains(2) && c.Count == 3) return false;
                if (c.Count == 13) return true;
                var temp = new List<Card>();
                temp.AddRange(c);
                temp.Remove(c.First(k => k.getRank() == 13));

                var i = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.First(j => !c.Any(k => k.getRank() == j));
                var l = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Last(j => !c.Any(k => k.getRank() == j));

                for (var r = 12; i > l; r--)
                {
                    if (!c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
                for (var r = 2; r < i; r++)
                {
                    if (!c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
                for (var r = i; r <= l; r++)
                {
                    if (c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (var i = m; i < M + 1; i++)
                {
                    if (!c.Any(k => k.getRank() == i))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {//set
            var r = c[0].getRank();
            if (c.Any(j => j.getRank() != r))
            {
                return false;
            }
        }
        return true;
    }

    void updateHandTexts()
    {
        for (var i = 0; i < playerCount; i++)
        {
            handTexts[i].text = hands[i].toText();
        }
    }

}

public class Card
{
    private int suit, rank;

    private bool seen;

    private Card[][] list;

    public Card(int suit, int rank)
    {
        this.suit = suit; //0=spades ♠, 1=hearts ♥, 2=diamonds ♦, 3=clubs ♣
        this.rank = rank;
        this.seen = false;
    }

    public void setList(Card[][] l)
    {
        this.list = l;
    }

    public string asString()
    {
        var r = "";
        var s = "";

        if (rank == 1)
        {
            r = "A";
        } else if (rank < 11)
        {
            r = rank.ToString();
        }
        else
        {
            switch (rank)
            {
                case 11:
                    r = "J";
                    break;
                case 12:
                    r = "Q";
                    break;
                case 13:
                    r = "K";
                    break;
            }
        }

        switch (suit)
        {
            case 0:
                s = "♠";
                break;
            case 1:
                s = "♥";
                break;
            case 2:
                s = "♦";
                break;
            case 3:
                s = "♣";
                break;
        }

        return r+s;
    }

    public bool hasBeenSeen()
    {
        return this.seen;
    }

    public void markAsSeen()
    {
        this.seen = true;
    }

    public int getRawValue()
    {
        if (rank == 1)
        {
            return 15;
        }
        return rank < 10 ? 5 : 10;
    }

    public bool equals(Card c)
    {
        return c.getRank() == this.getRank() && c.getSuit() == this.getSuit();
    }

    public float getValueInHand(Hand[] allHands, List<Meld> tableMelds, List<Card> discardCards)
    {
        return getValueOfHolding(allHands,tableMelds,discardCards) - getValueOfDiscarding(allHands, tableMelds, discardCards);
    }

    private float getValueOfHolding(Hand[] allHands, List<Meld> tableMelds, List<Card> discardCards)
    {
        var raw = getRawValue();

        var potentialValue = 0f;

        var potentialLoss = (float) raw;

        var h = allHands.First(j => j.getKnownCards().Contains(this));

        if (tableMelds.Any(m => m.canPlay(this))) {
            if (allHands[1].getKnownCards().Count == 1)
            {
                potentialValue += h.getValue();
            }
            else
            {
                potentialValue += 1.5f * raw;
            }
        }

        if (discardCards.Count > 1)
        {
            potentialValue += discardCards.Select((t2, i) => (from t1 in discardCards.Where((t1, j) => j != i) select new List<Card> {t2, t1, this} into t where validMeld(t) select new Meld(t, list) into nt select nt.getValue()).Sum()).Sum()*1.5f;    
        }

        var temp = 0f;
        foreach (var (c1, c2) in list.SelectMany(s => discardCards.SelectMany(c1 => s.Select(c2 => (c1, c2)))))
        {
            if (discardCards.Contains(c2) || c2.equals(this) || tableMelds.Any(m => m.getCards().Contains(c2))) continue;
            var m = new List<Card> { this, c1, c2 };
            if (validMeld(m))
            {
                temp += ((float)new Meld(m, list).getValue()) / (h.getKnownCards().Contains(c2) ? 1f : (allHands.Any(h => h.getKnownCards().Contains(c2)) ? 0.75f : 2f));
            }
        }
        potentialValue += temp;

        if (h.getKnownCards().Count > 2)
        {
            float tem = (from c1 in h.getKnownCards() where c1 != this from c2 in h.getKnownCards() where c2 != this && c2 != c1 select new List<Card> {c1, c2, this} into t where validMeld(t) select new Meld(t, list).getValue()).Sum();

            tem/=2;
            foreach (var (s, c1) in list.SelectMany(s => h.getKnownCards().Select(c1 => (s, c1))))
            {
                if (c1.equals(this)) continue;
                foreach (var c2 in s)
                {
                    if (h.getKnownCards().Contains(c2) || discardCards.Contains(c2) || tableMelds.Any(m => m.getCards().Contains(c2))) continue;
                    var m = new List<Card> { this, c1, c2 };
                    if (validMeld(m))
                    {
                        tem += ((float) new Meld(m, list).getValue()) / (allHands.Any(h => h.getKnownCards().Contains(c2)) ? 1.5f : 2f);
                    }
                }
            }

            potentialValue += tem;
        }

        if (seen) potentialLoss += 2;

        return potentialValue-potentialLoss;
    }

    private float getValueOfDiscarding(Hand[] allHands, List<Meld> tableMelds, List<Card> discardCards)
    {
        var raw = getRawValue();

        var potentialValue = 0f;

        var potentialLoss = 0f;

        var h = allHands.First(j => j.getKnownCards().Contains(this));
        
        var u = h.usefulCards(tableMelds);

        if (tableMelds.Any(m => m.canPlay(this))) potentialLoss += raw;

        foreach (var hand in allHands)
        {
            if (hand.getKnownCards().Contains(this)||hand.getKnownCards().Count<2) continue;
            foreach (var c1 in hand.getKnownCards())
            {

                foreach (var c2 in hand.getKnownCards())
                {
                    if (c1.equals(c2)) continue;
                    var m = new List<Card> { c1, c2, this };
                    if (validMeld(m))
                    {
                        var j = new Meld(m, list);
                        if (hand.getSize() < 3 + hand.getPlayer())
                        {
                            potentialLoss += h.getValue() + j.getValue();
                        }
                        else
                        {
                            potentialLoss += j.getValue();
                        }
                    }
                }
            }
        }
        foreach(var hand in allHands)
        {
            if (hand.getKnownCards().Contains(this)||hand.getKnownCards().Count<1) continue;
            foreach(var c1 in hand.getKnownCards())
            {
                if (c1.getRank() != getRank() && c1.getSuit() != getSuit()) continue;
                foreach(var s in list)
                {
                    foreach(var c2 in s)
                    {
                        if (c1.getRank() != c2.getRank() && c1.getSuit() != c2.getSuit()) continue;
                        if (hand.getKnownCards().Contains(c2) || equals(c2) || discardCards.Contains(c2)) continue;
                        var m = new List<Card> { c1, c2, this };
                        if (validMeld(m))
                        {
                            var j = new Meld(m, list);
                            potentialLoss += j.getValue() / 2f;
                        }
                    }
                }
            }
        }

        if (u.Any(c => c.c.getRank() == getRank()))
        {
            potentialLoss += (from us in u where us.c.getRank() == getRank() select us.v).Sum()/3f;
        }
        
        return potentialValue - potentialLoss;
    }

    public int getRank()
    {
        return this.rank;
    }

    public int getSuit()
    {
        return this.suit;
    }

    private bool validMeld(List<Card> c)
    { //0=spades, 1=hearts, 2=diamonds, 3=clubs
        if (c.Count != c.Distinct().Count() || c.Count < 3) return false;
        if (c[0].getSuit() == c[1].getSuit())
        {//run
            var s = c[0].getSuit();
            var t = c.Select(k => k.getRank()).ToList();
            var M = t.Max();
            var m = t.Min();
            if (c.Any(j => j.getSuit() != s))
            {
                return false;
            }

            if (m == 1 && M == 13)
            {
                if (t.Contains(2) && c.Count == 3) return false;
                if (c.Count == 13) return true;
                var temp = new List<Card>();
                temp.AddRange(c);
                temp.Remove(c.First(k=>k.getRank()==13));

                var i = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.First(j => !c.Any(k => k.getRank() == j));
                var l = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Last(j => !c.Any(k => k.getRank() == j));

                for (var r = 12; i > l; r--)
                {
                    if (!c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
                for (var r = 2; r < i; r++)
                {
                    if (!c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
                for (var r = i; r <= l; r++)
                {
                    if(c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (var i = m; i < M + 1; i++) 
                {
                    if (!c.Any(k => k.getRank() == i))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {//set
            var r = c[0].getRank();
            if (c.Any(j => j.getRank() != r))
            {
                return false;
            }
        }
        return true;
    }
}

public class Hand
{
    private int size, value, player;
    private List<Card> cards, tableCards;
    private Card[][] list;
    public Hand(int p, Card[][] list)
    {
        this.size = 7;
        this.player = p;
        this.cards = new List<Card>();
        this.tableCards = new List<Card>();
        this.list = list;
    }

    public void setHand(List<Card> h)
    {
        cards.AddRange(h);
    }

    public List<(Card c,float v)> usefulCards(List<Meld> tableMelds)
    {
        var l = new List<(Card c, float v)>();
        foreach(var s in list)
        {
            foreach(var c1 in s)
            {
                if (getKnownCards().Contains(c1)||tableMelds.Any(m=>m.getCards().Contains(c1))) continue;
                foreach(var c2 in getKnownCards())
                {
                    foreach(var c3 in getKnownCards())
                    {
                        if (c2.equals(c3)) continue;
                        var t = new List<Card> { c1, c2, c3 };
                        if (validMeld(t))
                        {
                            var m = new Meld(t, list);
                            var v = ((float) m.getValue())/2;
                            if (l.Any(i => i.c.equals(c1)))
                            {
                                v += l.First(i => i.c.equals(c1)).v;
                                l.Remove(l.First(i => i.c.equals(c1)));
                            }
                            l.Add((c1, v));
                        }
                    }
                }
            }
        }
        return l;
    }

    public void debugUsefulCards(List<Meld> tableMelds)
    {
        foreach(var (c,v) in usefulCards(tableMelds))
        {
            Debug.Log("The card " + c.asString() + " is useful with value " + v + ".");
        }
    }

    private bool validMeld(List<Card> c)
    { //0=spades, 1=hearts, 2=diamonds, 3=clubs
        if (c.Count != c.Distinct().Count() || c.Count < 3) return false;
        if (c[0].getSuit() == c[1].getSuit())
        {//run
            var s = c[0].getSuit();
            var t = c.Select(k => k.getRank()).ToList();
            var M = t.Max();
            var m = t.Min();
            if (c.Any(j => j.getSuit() != s))
            {
                return false;
            }

            if (m == 1 && M == 13)
            {
                if (t.Contains(2) && c.Count == 3) return false;
                if (c.Count == 13) return true;
                var temp = new List<Card>();
                temp.AddRange(c);
                temp.Remove(c.First(k => k.getRank() == 13));

                var i = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.First(j => !c.Any(k => k.getRank() == j));
                var l = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }.Last(j => !c.Any(k => k.getRank() == j));

                for (var r = 12; i > l; r--)
                {
                    if (!c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
                for (var r = 2; r < i; r++)
                {
                    if (!c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
                for (var r = i; r <= l; r++)
                {
                    if (c.Any(k => k.getRank() == r))
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (var i = m; i < M + 1; i++)
                {
                    if (!c.Any(k => k.getRank() == i))
                    {
                        return false;
                    }
                }
            }
        }
        else
        {//set
            var r = c[0].getRank();
            if (c.Any(j => j.getRank() != r))
            {
                return false;
            }
        }
        return true;
    }

    public string toText()
    {
        var temp = "";
        foreach (var v in cards)
        {
            temp += v.asString();
            temp += " ";
        }

        if (size > cards.Count)
        {
            temp += "(" + cards.Count + "/" + size + "), (-?/+" + getTablePoints() + ")";
        }
        else
        {
            temp += "(" + size + "), (-" + calculateValue() + "/+" + getTablePoints() + "=" + (getTablePoints()-calculateValue()) + ")";
        }

        
        return temp;
    }

    public void blindDraw()
    {
        this.size++;
    }

    public bool discard(Card c)
    {
        if (player == 0)
        {
            if (!cards.Contains(c)) return false;
            cards.Remove(c);
            size--;
            return true;
        }
        else
        {
            size--;
            if (cards.Contains(c)) cards.Remove(c);
            return true;
        }
    }

    public bool playCard(Card c, Meld p)
    {
        if (!discard(c)) return false;
        tableCards.Add(c);
        p.playCard(c);
        return true;
    }

    public void playMeld(Meld p)
    {
        foreach (var c in p.getCards())
        {
            discard(c);
            tableCards.Add(c);
        }
    }

    public int getTablePoints()
    {
        return 0+tableCards.Sum(c => c.getRawValue());
    }

    public void drawCard(Card c)
    {
        cards.Add(c);
        size++;
    }

    public List<Card> getKnownCards()
    {
        return this.cards;
    }

    public int getPlayer()
    {
        return this.player;
    }

    public int getSize()
    {
        return this.size;
    }

    public int getValue()
    {
        return calculateValue();
    }

    private int calculateValue()
    {
        return cards.Sum(c => c.getRawValue());
    }
}

public class Meld
{
    private int meldType; //0=set, 1=run

    private List<Card> cards, playbleCards;

    private Card[][] list;

    public Meld(List<Card> cards, Card[][] c)
    {
        this.cards = cards;
        this.meldType = cards[0].getSuit() == cards[1].getSuit() ? 1 : 0;
        this.playbleCards = new List<Card>();
        list = c;
        this.updatePlayableCards();
    }

    public bool canPlay(Card c)
    {
        return playbleCards.Contains(c);
    }

    public int getMeldType()
    {
        return this.meldType;
    }

    public List<Card> getCards()
    {
        return this.cards;
    }

    public void playCard(Card c)
    {
        cards.Add(c);
        updatePlayableCards();
    }

    public string asText()
    {
        var temp = "";
        foreach (var c in cards)
        {
            temp += c.asString();
            temp += " ";
        }
        return temp;
    }

    public int getValue()
    {
        return cards.Sum(c => c.getRawValue());
    }

    private void updatePlayableCards()
    {
        if (meldType == 0)
        {
            playbleCards = (from v in list from c in v where (c.getRank() == cards[0].getRank()&&!cards.Contains(c)) select c).ToList();
        }
        else
        {
            var t = cards.Select(c => c.getRank()).ToList();
            var M = t.Max();
            var m = t.Min();
            playbleCards.Clear();
            if (M == 13)
            {
                if (m == 1)
                {
                    return;
                }

                if (cards.Contains(list[cards[0].getSuit()][11]))
                {
                    playbleCards.Add(list[cards[0].getSuit()][0]);
                    playbleCards.Add(list[cards[0].getSuit()][m-2]);
                }
                else
                {
                    playbleCards.Add(list[cards[0].getSuit()][11]);
                    playbleCards.Add(list[cards[0].getSuit()][m]);
                }

            } else if (m == 1)
            {
                if (cards.Contains(list[cards[0].getSuit()][1]))
                {
                    playbleCards.Add(list[cards[0].getSuit()][12]);
                    playbleCards.Add(list[cards[0].getSuit()][M]);
                }
                else
                {
                    playbleCards.Add(list[cards[0].getSuit()][1]);
                    playbleCards.Add(list[cards[0].getSuit()][m-2]);
                }
            }
            else
            {
                
                if (cards.Contains(list[cards[0].getSuit()][M-2]))
                {
                    playbleCards.Add(list[cards[0].getSuit()][m-2]);
                    playbleCards.Add(list[cards[0].getSuit()][M]);
                }
                else
                {
                    playbleCards.Add(list[cards[0].getSuit()][m]);
                    playbleCards.Add(list[cards[0].getSuit()][M-2]);
                }
            }
        }
    }
}
