using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text LightsCounter;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void UpdateText_LightsCounter(string newText)
    {
        LightsCounter.text = newText;
    }
}
