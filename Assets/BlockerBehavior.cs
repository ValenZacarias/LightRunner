using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockerBehavior : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteOn;
    [SerializeField] private SpriteRenderer spriteOff;
    [SerializeField] private float switchDelay = 1.0f;
    private bool canSwitch = true;
    private bool isOn = false;
    private BoxCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        col = gameObject.GetComponent<BoxCollider2D>();
        GameObject[] blockerSwitches = GameObject.FindGameObjectsWithTag("BlockSwitch");
        if (blockerSwitches.Length == 0) Debug.LogError("NO BLOCKER SWITCHES");
        else
        {
            for (int i = 0; i < blockerSwitches.Length; i++)
            {
                //Debug.Log("switch " + i);
                blockerSwitches[i].GetComponent<BlockSwitchBehavior>().OnToggleTrigger += BlockerSwitch_ToggleTrigger;
            }
        }
    }

    private void BlockerSwitch_ToggleTrigger()
    {
        ToggleBlocker();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Activate();
        }
    }

    private void ToggleBlocker()
    {
        if (!isOn)
        {
            Activate();
        }
        else
        {
            Deactivate();
        } 
    }

    private void Activate()
    {
        isOn = true;
        col.enabled = true;
        UpdateVisuals(isOn);
    }

    private void Deactivate()
    {
        isOn = false;
        col.enabled = false;
        UpdateVisuals(isOn);
    }

    private void UpdateVisuals(bool state)
    {
        if (state)
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
        Deactivate();
    }

    IEnumerator ToggleDelay()
    {
        yield return new WaitForSeconds(switchDelay);
        canSwitch = true;
    }
}
