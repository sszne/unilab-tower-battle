using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreScript : MonoBehaviour
{
    public static int Score; //得点の変数
    public Text ScoreText; //得点の文字の変数

    void Start()
    {
        Score = 0; //得点を0にする
    }

    void Update()
    {
        ScoreText.text = "スコア:" + Score.ToString(); //ScoreTextの文字をScore:Scoreの値にする
    }
}
