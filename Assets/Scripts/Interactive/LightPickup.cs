using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightPickup : MonoBehaviour
{
    private SpriteRenderer sprite;
    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();    
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("TRIGGER");
        if (col.CompareTag("Player"))
       {
            Debug.Log("LightPickup");
            sprite.color = Color.gray;
       }
    }
}
