using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Transform PlayerStart;
    private GameObject[] PickLightsGO;
    private GameObject Player;
    private List<LightPickup> PickLights = new List<LightPickup>();

    private int totalPickLights;
    private int unlitPickLights = 0;

    private void Awake()
    {
        
    }

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        PickLightsGO = GameObject.FindGameObjectsWithTag("PickLight");

        for (int i = 0; i < PickLightsGO.Length; i++)
        {
            PickLightsGO[i].GetComponent<LightPickup>().TurnedOff += OnPickLightTurnedOff;
            PickLightsGO[i].GetComponent<LightPickup>().TurnedOn += OnPickLightTurnedOn;
            PickLights.Add(PickLightsGO[i].GetComponent<LightPickup>());
        }

        totalPickLights = PickLightsGO.Length;

        UpdateHUD();

        Player.GetComponent<PlayerController>().OnDamageAction += PlayerController_OnDamageAction;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
    }


    private void PlayerController_OnDamageAction(object sender, System.EventArgs e)
    {
        RestartLevel();
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
        if(unlitPickLights == totalPickLights)
        {
            FinishLevel();
        }
        UpdateHUD();
    }
    private void OnPickLightTurnedOn()
    {
        --unlitPickLights;
        Debug.Log("[LevelManager] LIGHT TURNED ON");
        UpdateHUD();
    }

    private void FinishLevel()
    {

        HUD.Win();
    }

    private void RestartLevel()
    {
        unlitPickLights = PickLights.Count;
        UpdateHUD();
        Player.transform.position = PlayerStart.position;
        foreach (LightPickup light in PickLights)
        {
            light.TurnOn();
        }
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemie in enemies)
        {
            Debug.Log("RESTART GHOST");
            enemie.GetComponent<GhostBehavior>().Reset();
        }
        HUD.Restart();
    }


    #region UI
    public HUDController HUD;
    private void UpdateHUD()
    {
        HUD.UpdateText_LightsCounter(unlitPickLights + " / " + totalPickLights);
    }

    #endregion

}
