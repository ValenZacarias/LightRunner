using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    public event Action PlayerTrigger;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            PlayerTrigger?.Invoke();
        }
    }

    public void Open()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void Close()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }


}
