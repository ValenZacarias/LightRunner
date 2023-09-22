using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    //[SerializeField] private List<LightPickup> PickLights = new List<LightPickup>();
    [Header("LIGHTS")]
    [SerializeField] private LightPickup[] RoomPickLights;
    [SerializeField] private BlockSwitchBehavior[] RoomBlockSwitches;
    [SerializeField] private BlockerBehavior[] RoomBlockers;
    public Transform PlayerStart;
    private int totalPickLights;
    private int unlitPickLights = 0;
    public event Action OnLightsStateChange;
    public event Action OnRoomFinished;

    [Header("DOOR")]
    public DoorBehavior Door;


    public IEnemyBehavior[] Enemies;
    // Start is called before the first frame update
    void Start()
    {
        // DOOR
        if (Door == null) Debug.LogError("[RoomManager] NO DOOR SETTED UP");
        else
        {
            Door.PlayerTrigger += Door_PlayerTrigger;
        }

        // PICK LIGHTS
        RoomPickLights = this.GetComponentsInChildren<LightPickup>();
        if (RoomPickLights.Length == 0) Debug.LogError("[RoomManager] NO LIGHTS SETTED UP");
        else 
        {
            for (int i = 0; i < RoomPickLights.Length; i++)
            {
                RoomPickLights[i].TurnedOff += OnPickLightTurnedOff;
                RoomPickLights[i].GetComponent<LightPickup>().TurnedOn += OnPickLightTurnedOn;
                RoomPickLights[i].gameObject.name = "LightPickup_" + this.name + "_" + i;
            }
            totalPickLights = RoomPickLights.Length;
        }

        //BLOCKERS
        RoomBlockers = this.GetComponentsInChildren<BlockerBehavior>();
        RoomBlockSwitches = this.GetComponentsInChildren<BlockSwitchBehavior>();

        // ENEMIES
        Enemies = this.GetComponentsInChildren<IEnemyBehavior>();

    }

    private void Door_PlayerTrigger()
    {
        Debug.Log("[RoomManager] DoorTrigger");
        if (totalPickLights == unlitPickLights)
        {
            FinishRoom();
            Door.Open();
        }

    }

    private void OnPickLightTurnedOff()
    {
        ++unlitPickLights;
        OnLightsStateChange?.Invoke();
        //if(totalPickLights == unlitPickLights) FinishRoom();
       
        if (totalPickLights == unlitPickLights)
        {
            Door.EnableOpen();
        }
    }
    private void OnPickLightTurnedOn()
    {
        if(unlitPickLights != 0) --unlitPickLights;
        OnLightsStateChange?.Invoke();
        Door.DisableOpen();
    }

    public int GetTotalLights() { return totalPickLights; }
    public int GetUnlitLights() { return unlitPickLights; }

    public void ResetBlockers()
    {
        for (int i = 0; i < RoomBlockers.Length; i++)
        {
            RoomBlockers[i].Reset();
        }
        for (int i = 0; i < RoomBlockSwitches.Length; i++)
        {
            RoomBlockSwitches[i].Reset();
        }
    }

    public void ResetRoom()
    {
        Door.Close();
        foreach (LightPickup light in RoomPickLights)
        {
            light.TurnOn();
        }

        foreach (BlockerBehavior blocker in RoomBlockers)
        {
            blocker.Reset();
        }

        foreach (BlockSwitchBehavior blockerSwitch in RoomBlockSwitches)
        {
            blockerSwitch.Reset();
        }
        //var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < Enemies.Length; i++)
        {
            Debug.Log("RESTART GHOST");
            Enemies[i].Reset();
        }
    }

    private void FinishRoom()
    {
        OnRoomFinished?.Invoke();
        for (int i = 0; i < Enemies.Length; i++)
        {
            Debug.Log("STOP GHOST");
            Enemies[i].Stop();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(PlayerStart.position, 0.5f);
    }
}
