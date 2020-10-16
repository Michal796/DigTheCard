using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// klasa odpowiada za wyświetlanie wyniku gracza 
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    [Header("definiowanie ręczne w panelu inspekcyjnym")]
    public GameObject prefabFloatingScore;

    [Header("definiowanie dynamiczne")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;

    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            _scoreString = _score.ToString("N0");
            GetComponent<Text>().text = _scoreString;
        }
    }
    public string scoreString
    {
        get
        {
            return (_scoreString);
        }
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }
    void Awake()
    {
        if (S == null)
        {
            S = this;

        }
        else
        {
            Debug.LogError("Błąd");
        }
        canvasTrans = transform.parent;
    }
    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;
        fs.Init(pts);
        return (fs);
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
