using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//główna klasa odpowiada za logikę gry oraz wywoływanie przemieszczających się wartości punktowych
public class DigTheCard : MonoBehaviour
{
    static public DigTheCard S;

    public TextAsset deckXML; //zewnętrzny plik zawierający informacje o położeniu sprajtów każej z 52 kart talii, zapisany w języku xml
    public TextAsset layoutXML; // zewnętrzny plik zawierający informacje o każdym slocie karty w grze (położenie, karty przykrywające)
    //public float xOffset = 3;
    //public float yOffset = -2.5f;
    public Vector3 layoutCenter; //punkt centralny rozdania
    //punkty na podstawie których wyznaczany jest ruch przemieszczjącej się wartości punktowej podczas gry
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    //opóźnienie restartu gry po ukończeniu
    public float reloadDelay = 2f;
    public Text gameOverText, roundResultText, highScoreText; // teksty interfejsu
    // sprajty do podmiany w złotych kartach
    public Sprite cardFrontGold;
    public Sprite cardBackGold;

    [Header("Definiowanie dynamiczne")]
    public Deck deck;
    public Layout layout;
    public Transform layoutAnchor; // rodzic wszystkich kart
    public CardDTC target; //cel
    public List<CardDTC> drawPile; // lista kart na stosie kart do pobrania
    public List<CardDTC> table; //lista kart na stosie gracza
    public List<CardDTC> discardPile; //stos kart odrzuconych (karty znajdujące się pod celem)
    public FloatingScore fsRun; //przemieszczająca się wartość punktowa


    private void Awake()
    {
        S = this;
    }
    //metoda aktualizująca elementy interfejsu
    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "Najlepszy wynik: " + ScoreManager.HIGH_SCORE;
        go.GetComponent<Text>().text = hScore;

        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }

        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }
        ShowResultsUI(false);
    }
    //zaprezentowanie wyniku rundy, po zakończeniu gry
    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }
    // Start is called before the first frame update
    void Start()
    {
        SetUpUITexts();
        Scoreboard.S.score = ScoreManager.SCORE;
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);//tasowanie przy użyciu referencji, a nie kopii listy deck.cards
        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text); // wczytaj położenie kart
        drawPile = ConvertListCardsToListCardDTC(deck.cards); 
        LayoutGame(); //rozmieść karty
        ChangeCardsToGold();
    }
    //funkcja przyporządkującą kartom losowo status złotej, a następnie podmieniającą ich obiekt Sprite.
    public void ChangeCardsToGold()
    {
        foreach (CardDTC card in table)
        {
            //karty odkryte na start gry nie mogą być złote
            if (card.hiddenBy.Count == 0)
            {
                return;
            }
            float rand = Random.RandomRange(0f, 1f);
            if (rand < 0.5 && card.hiddenBy.Count != 0)
                {
                card.isGold = true;
                SpriteRenderer spriteR = card.GetComponent<SpriteRenderer>();
                spriteR.sprite = cardFrontGold;
                SpriteRenderer spriteBR = GameObject.Find(card.gameObject.name+"/back").GetComponent<SpriteRenderer>();
                spriteBR.sprite = cardBackGold;
                spriteBR.sortingOrder = 2;
                }
        }
    }
    // konwersja obiektów Card na obiekty CardDTC
    // zasosowana ponieważ klasa Deck inicjuje obiekty Card, a do zarządzania grą potrzebna jest klasa dziedzicząca CardDTC
    List<CardDTC> ConvertListCardsToListCardDTC(List<Card> lCD)
    {
        List<CardDTC> lCP = new List<CardDTC>();
        CardDTC tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardDTC;
            lCP.Add(tCP);
        }
        return (lCP);
    }
    // fukncja zwracająca następną w kolejności kartę ze stosu kart do pobrania
    CardDTC Draw()
    {
        CardDTC cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }
    // rozmieszczenie kart w odpowiednich miejscach
    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        CardDTC cp;
        table = new List<CardDTC>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.FaceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);

            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.table;
            cp.SetSortingLayerName(tSD.layerName);
            table.Add(cp);
        }
        foreach (CardDTC tCP in table) //ustalenie które karty zasłaniają inne
        {
            //gdy istnieją numery kart zasłaniających daną kartę, znajdź je po identyfikatorze warstwy i dodaj do listy
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }
        MoveToTarget(Draw());
        UpdateDrawPile();
    }
    //znajdń karty po identyfikatorze wartstwy
    CardDTC FindCardByLayoutID(int layoutID)
    {
        foreach (CardDTC tCP in table)
        {
            if (tCP.layoutID == layoutID) {
                return (tCP);
            }
        }
        return (null);
    }
    // ustal które karty należy zasłonić
    void SetTableFaces()
    {
        foreach (CardDTC cd in table)
        {
            bool faceUp = true;
            foreach (CardDTC cover in cd.hiddenBy)
            {
                if (cover.state == eCardState.table)
                {
                    faceUp = false;
                }
            }
            cd.FaceUp = faceUp;
        }
    }
    //funkcja przemieszczająca wybraną kartę (w tym wypadku cel) na stos kart odrzuconych. Wywoływana w chwili wybrania przez gracza 
    //nowego celu
    void MoveToDiscard(CardDTC cd)
    {
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.FaceUp = true;

        cd.SetSortingLayerName(layout.discardPile.layerName);
        // każda kolejna karta jest położona nad poprzednią
        cd.SetSortOrder(-100 + discardPile.Count);
    }
    //funkcja przemieszczajaca zagraną przez gracza kartę na miejsce celu
    void MoveToTarget(CardDTC cd)
    {
        if (target != null) MoveToDiscard(target);
        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);

        cd.FaceUp = true;

        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }
    void UpdateDrawPile() //rozmieszczenie kart na stosie do pobrania oraz nadanie im podstawowych wartości
    {
        CardDTC cd;
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;
            //rozmieszczenie kart w pewnej odległości względem siebie
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);

            cd.FaceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            //każda kolejna karta ma o 10 mniejszą kolejność sortowania, przez co jest pod poprzednią i nad następną
            cd.SetSortOrder(-10 * i);
        }
    }
    //funkcja wywoływana po kliknięciu na którąś z kart
    public void CardClicked(CardDTC cd)
    {
        switch (cd.state)
        {
            case eCardState.target:
                break;

            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;

            case eCardState.table:
                bool validMatch = true;
                if (!cd.FaceUp)
                {
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }
                if (!validMatch) return;

                table.Remove(cd);
                MoveToTarget(cd);
                SetTableFaces();
                if(cd.isGold == true)
                {
                    ScoreManager.EVENT(eScoreEvent.mineGold);
                    FloatingScoreHandler(eScoreEvent.mineGold);
                }
                if(cd.isGold != true)
                {
                    ScoreManager.EVENT(eScoreEvent.mine);
                    FloatingScoreHandler(eScoreEvent.mine);
                }
                break;
        }
        CheckForGameOver();
    }
    //sprawdzenie czy nastąpił koniec gry po każdym zagraniu gracza
    void CheckForGameOver()
    {
        if (table.Count == 0)
        {
            GameOver(true); //wygrana
            return;
        }
        if (drawPile.Count > 0)
        {
            return;
        }
        //sprawdzenie czy można wykonać ruch którąkolwiek z kart
        foreach (CardDTC cd in table)
        {
            if (AdjacentRank(cd, target)) 
            {
                return;
            }
        }
        GameOver(false);//przegrana
    }
    void GameOver(bool won)
    {
        //zapisanie punktów niezależnie od wygranej lub przegranej
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Koniec rundy";
            roundResultText.text = "Wygrałeś rundę, twój wynik: " + score;
            ShowResultsUI(true);
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            ShowResultsUI(true);
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
            gameOverText.text = "Koniec gry";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "Rekord! Twój wynik: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Twój wynik końcowy: " + score;
            }
        }
        //restart gry
        Invoke("ReloadLevel", reloadDelay);
    }
    void ReloadLevel()
    {
        SceneManager.LoadScene("__Prospector_Scene_1_GoldCard");
    }
    //sprawczenie czy mozliwy jest jakikolwiek ruch
    public bool AdjacentRank(CardDTC c0, CardDTC c1)
    {
        //jeśli choć jedna z kart jest zakryta, ruch nie jest możliwy
        if (!c0.FaceUp || !c1.FaceUp) return (false);
        // wartość bezwzględna z różnicy rangi wybranej karty oraz celu (c1)
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }
        // zagranie asa na 2, oraz 2 na asa jest możliwe
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);
        return false;
    }
    //funkcja odpowiadająca za wywoływanie przemieszczającej się wartości punktowej (na podstawie prefabrykantu)
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // dla poniższych trzech przypadków, zmienna porusza się od wartości punktowej aktualnej kombinacji
            // do całkowitego wyniku
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
            case eScoreEvent.draw:
                if (fsRun != null)
                {
                    fsPts = new List<Vector2>();
                    // wcześniej zdefiniowane pozycje
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    //zdefiniowanie parametrów przemieszczającej się wartości punktowej
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;
                }
                break;
            case eScoreEvent.mineGold:
                FloatingScore fsg;
                //punktem początkowym jest aktualna pozycja myszki
                Vector2 p0g = Input.mousePosition;
                p0g.x /= Screen.width;
                p0g.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0g);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                //w przypadku kopania złotej karty, łańcuch punktów liczony jest dwukrotnie
                fsg = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN * 2, fsPts);
                fsg.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fsg;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fsg.reportFinishTo = fsRun.gameObject;
                }
                break;
            case eScoreEvent.mine:
                FloatingScore fs;
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);

                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

        

    // Update is called once per frame
    void Update()
    {
    }
}
