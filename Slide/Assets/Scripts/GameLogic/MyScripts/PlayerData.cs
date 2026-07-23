using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerData : MonoBehaviour
{
    // public static PlayerData instance;
    
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image image;
    
    public int score;
    public int rank;
    public string playerName;

    public void Awake()
    {
        Render();
    }

    public void Render()
    {
        rankText.text = rank.ToString();
        nameText.text = playerName;
        scoreText.text = score.ToString();
    }

    public void ChangeColor(Color color)
    {
        image.color = color;
    }

    public void TextEffectStarter(int currentScore)
    {
        StartCoroutine(TextEffect(currentScore));
    }

    IEnumerator TextEffect(int currentScore)
    {
        int scoreContainer = currentScore;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore -= 1000;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore += 423;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore -= 12;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore += 190;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore -= 2;
        scoreText.text = currentScore.ToString();
        currentScore += 190;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore += 190;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        currentScore += 190;
        scoreText.text = currentScore.ToString();
        yield return new WaitForSeconds(0.2f);
        scoreText.text = scoreContainer.ToString();


    }
}
