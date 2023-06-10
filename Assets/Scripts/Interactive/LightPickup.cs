using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPickup : MonoBehaviour
{
    private SpriteRenderer sprite;
    public bool isOn = true;
    public bool useTimer = false;
    public event Action TurnedOff;
    public event Action TurnedOn;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();    
    }

    public void TryTurnOff()
    {
        if(isOn)
        {
            sprite.color = Color.gray;
            TurnedOff?.Invoke();

            if(useTimer) StartCoroutine("testTurnOnTimer");

            isOn = false;
        }
        
    }

    public void TurnOn()
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

    private void OnDrawGizmos()
    {

        if(!isOn)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
