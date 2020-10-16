using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//klasa Deck interpretuje dane zawarte w zewnętrznym pliku DeckXML i tworzy talię kart na ich podstawie
public class Deck : MonoBehaviour
{
    [Header("Definiowanie w panelu inspekcyjnym")]
    public bool startFaceUp = false;
    public Sprite suitClub; //trefl
    public Sprite suitDiamont; //karo
    public Sprite suitHeart; //kier serce
    public Sprite suitSpade; //pik

    public Sprite[] faceSprites; //12 sprajtów (po trzy dla każdego koloru kart)
    public Sprite[] rankSprites; //16 sprajtów (liczby + litery)

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    public GameObject prefabCard;
    public GameObject prefabSprite;

    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardsDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    //ta metoda zostanie wywołana przez klasę DigTheCard, gdy będzie ona gotowa do odczytania danych z pliku
    public void InitDeck(string deckXMLText)
    {
        //stwórz nową talię, jeśli jej nie ma
        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        //słownik zwracający sprajt koloru, na podstawie odpowiadającej mu litery
        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", suitClub},
            {"D", suitDiamont },
            {"H", suitHeart },
            {"S", suitSpade }
        };
        ReadDeck(deckXMLText);
        MakeCards();
    }
    public void ReadDeck(string deckXMLText)
    {
        //stwórz nowy obiekt klasy PT_XMLReader oraz wczytanie danych
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);

        //wczytaj elementy dekoracyjne kart
        decorators = new List<Decorator>();
        //znalezienie wszystkich znaczników "decorator" w pierwszym (i jedynym) znaczniku "xml" pliku
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i=0; i<xDecos.Count; i++)
        {
            deco = new Decorator();
            //ustawienie wartości zmiennych klasy Decorator dla elementów dekoracyjnych obecnych na każdej karcie
            //po dwa elementy dekoracyjne na narożnik karty - w sumie 4 elementy deco
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            
            if (xDecos[i].att("scale") != "0")    
            deco.scale = float.Parse(xDecos[i].att("scale"));
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            //dodanie do listy dekoratorów
            decorators.Add(deco);
        }
        //definicja każdej z karty (ranga, położenie elementów Decorator definiujących 
        // rangę karty ('pip') oraz nazwa sprajtu dla kart będących figurami)
        cardsDefs = new List<CardDefinition>();
        //odczytanie wszystkich znaczników "card"
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i=0; i<xCardDefs.Count; i++)
        {
            //dla kazdego znacznika 'card' nowy obiekt CarDDefinition
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            // ilość elementów 'pip' na każdej karcie oznacza ich rangę (np na karcie 'as' jest jeden element 'pip')
            // karty będące figurami nie zawierają elementów 'pip'
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j=0; j<xPips.Count; j++)
                {
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    //dodanie do listy dekoratorów 'pip' danej definicji karty
                    cDef.pips.Add(deco);
                }
            }
            //dla kart będąbych figurami
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardsDefs.Add(cDef);
        }
    }
    //znajdz definicję karty na podstawie numeru 'rank'
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach (CardDefinition cd in cardsDefs)
        {
            if (cd.rank == rnk)
            {
                return (cd);
            }
        }
        return (null);
    }
    public void MakeCards()
    {
        //zdefiniowanie 13 nazw kart dla każdego koloru według schematu C1, C2...
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i=0; i<13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }
        cards = new List<Card>();
        //52 karty, z których każda oznaczona jest numerem
        for (int i=0; i<cardNames.Count; i++)
        {
            cards.Add(MakeCard(i));
        }
    }

    //stwórz zdefiniowaną kartę na podstawie jej numeru
    private Card MakeCard(int cNum)
    {
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        card.name = cardNames[cNum];
        //pierwszy znak nazwy
        card.suit = card.name[0].ToString();
        //drugi znak nazwy to ranga karty
        card.rank = int.Parse(card.name.Substring(1));
        //domyślnie kolor ustawiony jest na czarny
        if(card.suit =="D" || card.suit == "H")
        {
            card.colS = "Red";
            card.color = Color.red;
        }
        //przypisz definicję do karty na podstawie jej numery
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);
        return card;
    }

    //zmienne pomocnicze do wykonania poniższych metod
    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card)
    {
        foreach(Decorator deco in decorators)
        {
            //jeśli to kolor
            if (deco.type == "suit")
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //na podstawie znaku karty otrzymano sprajt
                _tSR.sprite = dictSuits[card.suit];
            }
            else
            {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //jeśli to dekorator będący cyfrą lub literą, pobierz go z tablicy na podstawie rangi 'rank'
                _tSp = rankSprites[card.rank];
                //przypisz go do zmiennej tymczasowej
                _tSR.sprite = _tSp;
                _tSR.color = card.color;
            }
            //sprajty dekoracyjne położone nad obiektami kart
            _tSR.sortingOrder = 1;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = deco.loc;
            //obróć, jeśli trzeba
            if (deco.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (deco.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            _tGO.name = deco.type;
            //dodaj do listy dekoratorów karty
            card.decoGos.Add(_tGO);
        }
    }

    //dodanie elementów dekoracyjnych do kart niebędących figurami
    private void AddPips(Card card)
    {
        foreach(Decorator pip in card.def.pips)
        {
            _tGO = Instantiate(prefabSprite) as GameObject;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = pip.loc;
            if (pip.flip)
            {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (pip.scale != 1)
            {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];
            _tSR.sortingOrder = 1;
            card.pipGOs.Add(_tGO);
        }
    }
    //dodanie obrazków do kart będących figurami
    private void AddFace(Card card)
    {
        if(card.def.face == "")
        {
            return;
        }
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();

        _tSp = GetFace(card.def.face+card.suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }

    //znajdź sprajt na podstawie nazwy
    private Sprite GetFace(string faceS)
    {
        foreach (Sprite _tSP in faceSprites)
        {
            if(_tSP.name == faceS)
            {
                return (_tSP);
            }
        }
        return null;
    }

    //dodaj sprajt będący tyłem karty i przykryj nim przód karty
    private void AddBack(Card card)
    {
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;
        card.FaceUp = startFaceUp;
    }

    //potasuj karty przy użyciu funkcji ref, aby zatosować nowy numer indexu
    static public void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();
        int ndx;
        //tCards =  new List<Card>(); //tymczasowa lista kart
        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }
        oCards = tCards;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
