using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{

    private BoxCollider2D collider;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();   
    }

    
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PickLight"))
        {
            Debug.Log("Lucecita!");
        }
        else
        {
            Debug.Log("Other");
        }
    }
}
