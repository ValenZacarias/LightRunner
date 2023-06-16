using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LevelManager : MonoBehaviour
{
    public Camera cam;
    private GameObject[] PickLightsGO;
    private GameObject Player;
    [SerializeField] private List<LightPickup> PickLights = new List<LightPickup>();

    private int currentRoomTotalLights;
    private int currentRoomUnlitLights = 0;

    [Header("DEBUG")] [SerializeField] private float camWorldWidht;
    [SerializeField] private float camWorldHeight;

    [Header("ROOMS")]
    [SerializeField] private RoomManager[] Rooms;
    [SerializeField] private int currentRoom = 0;
    public GameObject[] levelRooms;

    private void Awake()
    {
        if (Rooms.Length == 0) Debug.LogError("[LevelManager] ROOM LIST IS EMPTY");
        if (cam == null) Debug.LogError("[LevelManager] NO CAM SETTED UP");
        for (int i = 0; i < Rooms.Length; i++) 
        { 
            Rooms[i].LightsStateChange += RoomManager_LightsStateChange;
            Rooms[i].RoomFinished += RoomManager_RoomFinished;
        }

        if (HUD == null) Debug.LogError("[LevelManager] NO HUD SETTED UP");
    }

    private void Start()
    {
        // EVENT LISTENERS SETUP
        Player = GameObject.FindGameObjectWithTag("Player");
        Player.GetComponent<PlayerController>().OnDamageAction += PlayerController_OnDamageAction;

        // LEVEL SETUP
        InitRoom(0);

    }

    private void Update()
    {
        camWorldHeight = 2 * cam.orthographicSize;
        camWorldWidht = camWorldHeight * Camera.main.aspect;

        if (Input.GetKeyDown(KeyCode.R)) RestartLevel();
        if (Input.GetKeyDown(KeyCode.T)) RestartRoom();
    }


    private void PlayerController_OnDamageAction(object sender, System.EventArgs e)
    {
        Debug.Log("[LeveLManager] DeadTRIGGER");
        RestartRoom();
    }

    private void RoomManager_LightsStateChange()
    {
        if (currentRoom == Rooms.Length && currentRoomTotalLights == currentRoomUnlitLights)
        {
            FinishLevel();
            return;
        }

        currentRoomTotalLights = Rooms[currentRoom].GetTotalLights();
        currentRoomUnlitLights = Rooms[currentRoom].GetUnlitLights();
        
        UpdateHUD();

        //WIN
    }

    private void RoomManager_RoomFinished()
    {
        currentRoom += 1;
        if (currentRoom == Rooms.Length) 
        { 
            FinishLevel(); 
        }
        else
        {
            InitRoom(currentRoom);
        }
    }

    private void InitRoom(int roomNumber)
    {
        currentRoomTotalLights = Rooms[currentRoom].GetTotalLights();
        currentRoomUnlitLights = Rooms[currentRoom].GetUnlitLights();
        cam.transform.position = new Vector3(currentRoom * Mathf.FloorToInt(camWorldWidht), cam.transform.position.y, cam.transform.position.z);
        UpdateHUD();
    }

    private void FinishLevel()
    {
        HUD.Win();
    }

    private void RestartLevel()
    {
        for (int i = 0; i < Rooms.Length; i++) { Rooms[i].ResetRoom(); }

        currentRoom = 0;
        Rooms[currentRoom].ResetRoom();
        Player.transform.position = Rooms[currentRoom].PlayerStart.position;
        InitRoom(0);

        UpdateHUD();
        HUD.Restart();
    }

    private void RestartRoom()
    {
        Rooms[currentRoom].ResetRoom();
        Player.transform.position = Rooms[currentRoom].PlayerStart.position;
        InitRoom(currentRoom);

        UpdateHUD();
        HUD.Restart();
    }


    #region UI
    public HUDController HUD;
    private void UpdateHUD()
    {
        HUD.UpdateText_LightsCounter(currentRoomUnlitLights + " / " + currentRoomTotalLights);
    }

    #endregion

}
