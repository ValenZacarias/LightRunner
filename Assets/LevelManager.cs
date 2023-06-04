using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject[] PickLightsGO;
    private List<LightPickup> PickLights = new List<LightPickup>();

    private int totalPickLights;
    private int unlitPickLights = 0;

    private void Start()
    {
        PickLightsGO = GameObject.FindGameObjectsWithTag("PickLight");

        for(int i = 0; i < PickLightsGO.Length; i++)
        {
            PickLightsGO[i].GetComponent<LightPickup>().TurnedOff += OnPickLightTurnedOff;
            PickLightsGO[i].GetComponent<LightPickup>().TurnedOn += OnPickLightTurnedOn;
            PickLights.Add(PickLightsGO[i].GetComponent<LightPickup>());
        }

        totalPickLights = PickLightsGO.Length;

        UpdateHUD();
    }

    private void Update()
    {
        
    }

    public int GetLitPickLights()
    {
        int LitCount = 0;
        foreach(LightPickup light in PickLights)
        {
            if (light.GetState()) ++LitCount;
        }

        return LitCount;
    }

    private void OnPickLightTurnedOff()
    {
        ++unlitPickLights;
        Debug.Log("[LevelManager] LIGHT TURNED OFF");
        UpdateHUD();
    }
    private void OnPickLightTurnedOn()
    {
        --unlitPickLights;
        Debug.Log("[LevelManager] LIGHT TURNED ON");
        UpdateHUD();
    }

    #region UI
    public HUDController HUD;
    private void UpdateHUD()
    {
        HUD.UpdateText_LightsCounter(unlitPickLights + " / " + totalPickLights);
    }

    #endregion

}
