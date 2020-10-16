using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//rodzaje zdarzeń związanych z zarządzaniem wynikiem
public enum eScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}
//klasa odpowiedzialna za zarządzanie wynikiem gracza oraz przechowywanie go w słowniku PlayerPrefs
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;
    static public int SCORE_FROM_PREV_ROUND = 0; // 
    static public int HIGH_SCORE;

    [Header("Definiowanie dynamiczne")]
    public int chain = 0; // łańcuch punktów zwiększa się o 1 gdy gracz pozbędzie się kolejnej karty ze stołu w serii (za pierwszą kartę 1 punkt, za drugą 2 punktu...)
    // gdy gracz wyciągnie kartę ze stosu, łańcuch zeruje się
    public int scoreRun = 0; // zgromadzone punkty w serii
    public int score; // punkty wyświetlane

    void Awake()
    {
       if(S == null)
        {
            S = this;
        }
        else
        {
            Debug.LogError("BŁąd");
        }
       //pobranie wartości najwyższego wyniku
       if (PlayerPrefs.HasKey("A"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("A");
        }
        score = 0;
        SCORE_FROM_PREV_ROUND = 0;
    }
    //zabezpieczenie działania programu
    static public void EVENT(eScoreEvent evt)
    {
        try
        {
            S.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager Event wywołany gdy S=null");
        }
    }
    void Event(eScoreEvent evt)
    {
        switch (evt)
        {
            case eScoreEvent.draw: //pobieranie karty
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;
            case eScoreEvent.gameWin: // wygrana
            case eScoreEvent.gameLoss: //przegrana
                chain = 0;
                //score += scoreRun;
                scoreRun = 0;
                break;
            case eScoreEvent.mine: //usun kartę z tableau
                print("chain"+chain);
                chain++;
                scoreRun += chain;
                break;
            case eScoreEvent.mineGold:
                chain++;
                scoreRun += 2*chain;
                break;
        }
        switch (evt) 
        {
            case eScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                break;
            case eScoreEvent.gameLoss:
                if (HIGH_SCORE <= score)
                {
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("A", score);
                }
                break;
            default:
                break;
        }
    }
    //statyczne właściwości do odczytu pól
    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
