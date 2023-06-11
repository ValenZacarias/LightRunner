using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private GameObject WinPanel;
    [SerializeField] private TMP_Text LightsCounter;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Win()
    {
        WinPanel.SetActive(true);
    }

    public void Restart()
    {
        WinPanel.SetActive(false);
    }

    public void UpdateText_LightsCounter(string newText)
    {
        LightsCounter.text = newText;
    }
}
