using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPickup : MonoBehaviour
{
    private SpriteRenderer sprite;
    private bool isOn = true;
    public event Action TurnedOff;
    public event Action TurnedOn;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();    
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
       {
            TryTurnOff();
       }
    }

    private void TryTurnOff()
    {
        if(isOn)
        {
            //Debug.Log("LIGHT OFF");
            sprite.color = Color.gray;
            TurnedOff?.Invoke();
            StartCoroutine("testTurnOnTimer");

            isOn = false;
        }
        
    }

    private void TurnOn()
    {
        //Debug.Log("LIGHT ON");
        sprite.color = Color.yellow;
        isOn = true;

        TurnedOn?.Invoke();
    }

    public bool GetState()
    {
        return isOn;
    }

    IEnumerator testTurnOnTimer()
    {
        //Debug.Log("Starting timer");
        yield return new WaitForSeconds(6.2f);
        TurnOn();
    }
}
