using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//możliwe stany kart
public enum eCardState
{
    drawpile, //stos kart do pobrania
    table, //karta na stole
    target, //cel
    discard // stos kart odrzuconych
}
//klasa przechowuje informacje o karcie charakterystyczne dla gry. Klasa CardDTC dziedziczy po klasie Card,
// które przechowuje wszystkie podsatwowe informacje o kartach
public class CardDTC : Card
{
    [Header("Def. dynamiczne CardProspector")]
    public eCardState state = eCardState.drawpile;
    public List<CardDTC> hiddenBy = new List<CardDTC>();
    public int layoutID;
    public SlotDef slotDef;
    public bool isGold;

    public override void OnMouseUpAsButton()
    {
        DigTheCard.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
