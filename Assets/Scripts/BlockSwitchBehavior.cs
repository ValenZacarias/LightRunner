using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSwitchBehavior : MonoBehaviour
{
    [SerializeField] bool isOn;
    [SerializeField] bool canSwitch = true;
    [SerializeField] float switchDelay = 1.0f;
    [SerializeField] SpriteRenderer spriteOn;
    [SerializeField] SpriteRenderer spriteOff;
    public event Action OnToggleTrigger;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] blockerSwitches = GameObject.FindGameObjectsWithTag("BlockSwitch");
        if (blockerSwitches.Length == 0) Debug.LogError("NO BLOCKER SWITCHES");
        else
        {
            for (int i = 0; i < blockerSwitches.Length; i++)
            {
                //Debug.Log("switch " + i);
                if(blockerSwitches[i] != this.gameObject)
                { 
                    blockerSwitches[i].GetComponent<BlockSwitchBehavior>().OnToggleTrigger += OtherSwitch_ToggleTrigger;
                }
            }
        }
    }

    private void OtherSwitch_ToggleTrigger()
    {
        Toggle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player") && canSwitch)
        {
            OnToggleTrigger?.Invoke();
            Toggle();
        }
    }

    private void Toggle()
    {
        //Debug.Log("Switch -> " + isOn);
        isOn = !isOn;
        UpdateVisuals(isOn);
        canSwitch = false;
        StartCoroutine("SwitchDelay");
    }

    private void UpdateVisuals(bool state)
    {
        if(state)
        {
            spriteOn.gameObject.SetActive(true);
            spriteOff.gameObject.SetActive(false);
        }
        else
        {
            spriteOn.gameObject.SetActive(false);
            spriteOff.gameObject.SetActive(true);
        }
    }

    public void Reset()
    {
        isOn = false;
        UpdateVisuals(isOn);
    }

    IEnumerator SwitchDelay()
    {
        yield return new WaitForSeconds(switchDelay);
        canSwitch = true;
    }

}
