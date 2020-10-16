using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//możliwe stany przemieszczającej się wartości wartości
public enum eFSState
{
    idle,
    pre,
    active,
    post
}
//ta klasa odpowiada za przemieszczające się po ekranie wartości punktowe
public class FloatingScore : MonoBehaviour
{
    [Header("definiowanie dynamiczne")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    public int score
    {
        get
        {
            return (_score);
        }
        set {
            _score = value;
            scoreString = _score.ToString("N0");//separatory do wyniku
            GetComponent<Text>().text = scoreString;
            }
    }
    public List<Vector2> bezierPts; //punkty wykorzystane do zakrzywienia ruchu zmiennej
    public List<float> fontSizes; //punkty do obsługi zmiany rozmiaru czcionki podczas ruchu
    public float timeStart = -1f;
    public float timeDuration = 1f; //czas przemieszczania się wartości
    public string easingCurve = Easing.InOut; // Wykorzystanie klasy Easing pozwala na wygładzenie ruchu przemieszczającej się wartości punktowej (zdefiniowana w klasie Utils)

    public GameObject reportFinishTo = null; //obiekt, któremu zmienna przekaże informację o zakończeniu przemieszczania (przez funkcję SendMessage())
    private RectTransform rectTrans;
    private Text txt;

    //inicjacja przemieszczającej się zmiennej
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);
        //jeśli jest to tylko jeden punkt, przemieść się do niego
        if (ePts.Count == 1)
        {
            transform.position = ePts[0];
            return;
        }
        //jeśli czas startu jest ustawiony na 0, nastąpi bezzwłoczna inicjalizacja
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        // gotowa do przemieszczania
        state = eFSState.pre;
    }
    // dodanie punktów do obiektu
    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == eFSState.idle) return;

        //inicjalizacja zmiennych do przemieszczania się
        float u = (Time.time - timeStart) / timeDuration;
        float uC = Easing.Ease(u, easingCurve);
        //jeśli u < 0, zmienna nie porusza się
        if (u < 0)
        {
            state = eFSState.pre;
            txt.enabled = false;
        }
        // jeśli u>= 1, zatrzymaj ruch wartości, zniszcz ją i dodaj punkty do tablicy
        else
        {
            if (u >= 1)
            {
                uC = 1;
                state = eFSState.post;
                if (reportFinishTo != null)
                {
                    reportFinishTo.SendMessage("FSCallback", this); //dodanie punktów do tablicy
                    Destroy(gameObject);
                }
                else
                {
                    state = eFSState.idle;
                }
            }
            //jeśli u znajduje się w przedziale <0,1), czas życia zmiennej jeszcze się nie skończył
            else
            {
                state = eFSState.active;
                txt.enabled = true;
            }
            Vector2 pos = Utils.Bezier(uC, bezierPts); //zewnętrzna klasa udostępniona przez przez autora książki 
            //"Projektowanie gier przy użyciu środowiska Unity i języka C#" J. G. Bonda. jako narzędzie do poruszania 
            //obiektami wykorzystujac krzywe Beziera
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontSizes != null && fontSizes.Count > 0)
            {
                //rozmiar czcionki zmienia się w zależności od aktualnego czasu
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
