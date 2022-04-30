using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class main : MonoBehaviour
{

    public Button cardSelectSubmitButton, cardSelectRemoveButton, setButton, runButton;

    public Button[] playerButtons, actionButtons, numberButtons, spades, hearts, diamonds, clubs;

    private Button[][] cardButtons;

    private TextMesh selectedCardsText;

    private List<Play> playsOnTable, storedPlays;

    private Hand[] hands;

    private Card storedCard;

    private Card[][] cards;

    private List<Card> selectedCards, discardPile;

    private int activePlayer, playerCount, expectedCards;

    private bool turnOver, playingOnExisting, playingNewPlay, drawingFromDiscard, firstCardEntry, discarding, min, selectingCards;

    void Start()
    {
        init();
    }

    void init()
    {
        playsOnTable = new List<Play>();
        discardPile = new List<Card>();
        selectedCards = new List<Card>();
        firstCardEntry = false;
        discarding = false;
        drawingFromDiscard = false;
        playingOnExisting = false;
        playingNewPlay = false;
        selectingCards = false;
        turnOver = false;
        min = false;
        selectedCardsText.text = "";
        setupButtons();
        hideCardButtons();

        foreach (var v in playerButtons)
        {
            v.gameObject.SetActive(true);
        }
    }

    void turn()
    {
        if (activePlayer == 0)
        {
            //TODO: AI Turn 
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

    void setupButtons() //TODO: Delegates
    {
        cardButtons = new[] { spades, hearts, diamonds, clubs }; //Potentially change order
        hideCardButtons();

        foreach (var v in actionButtons)
        {
            v.gameObject.SetActive(false);
        }

        foreach (var v in playerButtons)
        {
            v.gameObject.SetActive(false);
        }

        setButton.gameObject.SetActive(false);
        runButton.gameObject.SetActive(false);
    }

    void actionButtonPress(int a) //0=draw from pile, 1=draw from discard, 2=play new play, 3=play on existing play, 4=discard 
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
                hands[activePlayer].blindDraw();  
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
            case 2: //play new play
                playingNewPlay = true;
                promptCardEntry(3, true);
                break;
            case 3: //play on existing play
                playingOnExisting = true;
                promptCardEntry(1, false);
                break;
            case 4: //discard
                discarding = true;
                promptCardEntry(1,false);
                break;
        }
    }

    void promptCardEntry(int e, bool m)
    {
        expectedCards = e;
        min = m;
        selectedCardsText.text = "";
        selectedCards.Clear();
        showCardButtons();
        selectingCards = true;
    }

    void promptFirstCardEntry()
    {
        firstCardEntry = true;
        promptCardEntry(7,false);
    }

    void cardButtonPress(int s, int r)
    {
        var c = cards[s][r];
        selectedCards.Add(c);
        selectedCardsText.text += " " + c.asString();
        cardButtons[s][r].gameObject.SetActive(false);
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
            //TODO: Set who goes first
            turn();
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

            drawingFromDiscard = false;
        } else if (playingNewPlay)
        {
            if (!validPlay(selectedCards)) return;
            var p = new Play(selectedCards);
            hands[activePlayer].playPlay(p);
            playsOnTable.Add(p);
            playingNewPlay = false;
        } else if (playingOnExisting)
        {
            var c = selectedCards[0];
            var pp = playsOnTable.Where(p => p.canPlay(c)).ToList();
            switch (pp.Count)
            {
                case 1:
                    hands[activePlayer].playCard(c, pp[0]);
                    break;
                case 2:
                    setOrRun(c, pp);
                    break;
            }
        }
    }

    void setOrRun(Card c, List<Play> pp)
    {
        storedCard = c;
        storedPlays = pp;
        setButton.gameObject.SetActive(true);
        runButton.gameObject.SetActive(true);
    }

    void setButtonPress()
    {
        if (storedPlays[0].getPlayType() == 0)
        {
            hands[activePlayer].playCard(storedCard, storedPlays[0]);
        }
        else
        {
            hands[activePlayer].playCard(storedCard, storedPlays[1]);
        }
    }

    void cardSelectRemovePress()
    {
        if (selectedCards.Count == 0) return;
        var c = selectedCards[selectedCards.Count - 1];
        selectedCards.Remove(c);
        cardButtons[c.getSuit()][c.getRank()-1].gameObject.SetActive(true);
    }

    void showCardButtons()
    {
        cardSelectRemoveButton.gameObject.SetActive(true);
        cardSelectSubmitButton.gameObject.SetActive(true);
        selectedCardsText.gameObject.SetActive(true);
        foreach (var v in cardButtons)
        {
            foreach (var c in v)
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    void hideCardButtons()
    {
        cardSelectRemoveButton.gameObject.SetActive(false);
        cardSelectSubmitButton.gameObject.SetActive(false);
        selectedCardsText.gameObject.SetActive(false);
        foreach (var v in cardButtons)
        {
            foreach (var c in v)
            {
                c.gameObject.SetActive(false);
            }
        }
    }

    void pressPlayerCount(int n)
    {
        hands = new Hand[n];
        playerCount = n;
        for (int i = 0; i < n; i++)
        {
            hands[i] = new Hand(i);
        }
        foreach (var v in playerButtons)
        {
            v.gameObject.SetActive(false);
        }
        promptFirstCardEntry();
    }

    bool validPlay(List<Card> cards) //TODO: Check validitiy
    {
        return true;
    }

}

public class Card
{
    private int suit, rank;

    private bool seen;

    public Card(int suit, int rank)
    {
        this.suit = suit;
        this.rank = rank;
        this.seen = false;
    }

    public string asString()
    {
        return "";
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

    public int getValueInContext(Hand[] a, Play[] t, Card[] d)
    {

        return getRawValue();
    }

    public int getRank()
    {
        return this.rank;
    }

    public int getSuit()
    {
        return this.suit;
    }
}

public class Hand
{
    private int size, value, player;
    private List<Card> cards, tableCards;
    public Hand(int p)
    {
        this.size = 7;
        this.player = p;
        this.cards = new List<Card>();
    }

    public void setHand(List<Card> h)
    {
        cards = h;
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

    public bool playCard(Card c, Play p)
    {
        if (!discard(c)) return false;
        tableCards.Add(c);
        p.playCard(c);
        return true;
    }

    public void playPlay(Play p)
    {
        foreach (var c in p.getCards())
        {
            discard(c);
            tableCards.Add(c);
        }
    }

    public int getTablePoints()
    {
        return tableCards.Sum(c => c.getRawValue());
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

public class Play
{
    private int playType; //0=set, 1=run

    private List<Card> cards, playbleCards;

    public Play(List<Card> cards)
    {
        this.cards = cards;
        this.playType = cards[0].getSuit() == cards[1].getSuit() ? 1 : 0;
        this.updatePlayableCards();
    }

    public bool canPlay(Card c)
    {
        return playbleCards.Contains(c);
    }

    public int getPlayType()
    {
        return this.playType;
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

    private void updatePlayableCards()
    {
        if (playType == 0)
        {

        }
        else
        {

        }
    }
}