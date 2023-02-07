using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Timer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text timerText;

    private float timer;

    private void Start()
    {
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        int mins = Mathf.FloorToInt(timer / 60f);
        float secs = timer % 60f;

        timerText.text = mins.ToString("00") + ":" + secs.ToString("00.00");
    }
}
