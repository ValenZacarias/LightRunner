using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightPickup : MonoBehaviour
{
    private SpriteRenderer sprite;
    private ParticleSystem ps;

    public GameObject LitVisual;
    public GameObject UnlitVisual;

    public bool isOn = true;
    public bool useTimer = false;
    public event Action TurnedOff;
    public event Action TurnedOn;
    //[SerializeField] private int roomNumber;

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        //ps = GetComponentInChildren<ParticleSystem>();
        sprite.material.SetFloat("_TimeOffset", UnityEngine.Random.Range(0.0f, 10.0f));
        //ps.Play();
    }

    public void TryTurnOff()
    {
        if(isOn)
        {
            sprite.color = Color.gray;
            TurnedOff?.Invoke();

            if(useTimer) StartCoroutine("testTurnOnTimer");

            LitVisual.SetActive(false);
            UnlitVisual.SetActive(true);

            isOn = false;
        }
        
    }

    public void TurnOn()
    {
        //Debug.Log("LIGHT ON");
        sprite.color = Color.yellow;

        TurnedOn?.Invoke();

        LitVisual.SetActive(true);
        UnlitVisual.SetActive(false);

        isOn = true;
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
        //GUIStyle style = new GUIStyle();
        //style.contentOffset = Vector2.right * 15.0f;
        //style.normal.textColor = Color.magenta;
        //style.fontSize = 15;
        //Handles.Label(transform.position, roomNumber.ToString(), style);
        if (!isOn)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
