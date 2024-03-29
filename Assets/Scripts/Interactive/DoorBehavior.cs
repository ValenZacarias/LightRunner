using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    public event Action PlayerTrigger;
    private bool isOpen = false;
    private SpriteRenderer doorSprite;

    void Start()
    {
        doorSprite = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && !isOpen)
        {
            PlayerTrigger?.Invoke();
        }
    }

    public void EnableOpen()
    {
        doorSprite.color = Color.green;
        Debug.Log("DOOR ENABLE OPEN");
    }

    public void DisableOpen()
    {
        doorSprite.color = Color.gray;
        Debug.Log("DOOR DISABLE OPEN");
    }

    public void Open()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        isOpen = true;
    }
    public void Close()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        isOpen = false;
    }


}
