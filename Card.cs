using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//klasa Decorator przechowuje informacje o elementach dekoracyjnych na kartach (znak, liczbę)
[System.Serializable]
public class Decorator
{
    public string type;
    public Vector3 loc;
    public bool flip = false; //czy symbol ma być odwrócony
    public float scale = 1f;
}

//ta klasa przechowuje informacje o randze klasy
[System.Serializable]
public class CardDefinition
{
    public string face;
    public int rank;
    //lista obiektów dekoracyjnych na kartach, które nie są figurami
    //karty będące figurami również posiadają elementy dekoracyjne (w narożnikach),
    //nie są jednak przechowywane w klasie CardDefinition, ponieważ dla każdej karty mają to samo położenie
    public List<Decorator> pips = new List<Decorator>();
    //public bool isGold = false;
}

//klasa Card przechowuje wszystkie informacje o karcie, a także jej definicję. Zarządza również warstwami 
// komponentu SpriteRenderer
public class Card : MonoBehaviour
{
    //definiowanie każdej karty następuje w metodzie klasy Deck - ReadTheDeck()
    [Header("definiowanie dynamiczne")]
    public string suit;
    public int rank;
    public Color color = Color.black;
    public string colS = "Black";

    //obiekty dekoracyjne na każdej karcie (kolor i litera (lub cyfra))
    public List<GameObject> decoGos = new List<GameObject>();
    //elementy dekoracyjne na kartach niebędących figurami
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; //tylna strona kart

    public CardDefinition def;

    public SpriteRenderer[] spriteRenderers;
    //czy karta ma być odsłonięta
    public bool FaceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }
    //wirtualna metoda umożliwia nadpisanie override w klasie dziedziczącej
    virtual public void OnMouseUpAsButton()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        SetSortOrder(0);
    }

    //metoda pobierająca wszystkie komponenty SpriteRenderer karty w celu ustawienia warstwy
    public void PopulateSpriteRenderers()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }
    //metoda ustawiająca nazwawnie warstwy sortujacej wszystkich sprajtów karty
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();
        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }
    // ustawienie numeru warstwy wszystkim komponentom SpriteRenderer
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();
        foreach(SpriteRenderer tSR in spriteRenderers)
        {
                if (tSR.gameObject == this.gameObject)
                {
                    tSR.sortingOrder = sOrd;
                    continue;
                }
            switch (tSR.gameObject.name)
            {
                //element "back" zakrywa karty
                case "back":
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "face":
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
