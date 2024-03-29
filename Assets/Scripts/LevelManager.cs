using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using IngameDebugConsole;


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
        
    }

    private void Start()
    {
        //DebugLogConsole.AddCommand("rl", "Restart Level", RestartLevel);
        //DebugLogConsole.AddCommand<int>("rr", "Restart Room", RestartRoom);

        // EVENT LISTENERS SETUP
        if (Rooms.Length == 0) Debug.LogError("[LevelManager] ROOM LIST IS EMPTY");
        if (cam == null) Debug.LogError("[LevelManager] NO CAM SETTED UP");
        for (int i = 0; i < Rooms.Length; i++)
        {
            Rooms[i].OnLightsStateChange += RoomManager_OnLightsStateChange;
            Rooms[i].OnRoomFinished += RoomManager_OnRoomFinished;
        }

        if (HUD == null) Debug.LogError("[LevelManager] NO HUD SETTED UP");

        Player = GameObject.FindGameObjectWithTag("Player");
        Player.GetComponent<PlayerController>().OnDamageAction += PlayerController_OnDamageAction;

        // LEVEL SETUP
        InitRoom(0);

    }

    private void Update()
    {
        camWorldHeight = 2 * cam.orthographicSize;
        camWorldWidht = camWorldHeight * Camera.main.aspect;

        if (Input.GetKeyDown(KeyCode.R)) RestartRoom(currentRoom); 

    }


    private void PlayerController_OnDamageAction(object sender, System.EventArgs e)
    {
        Debug.Log("[LeveLManager] DeadTRIGGER");
        RestartRoom(currentRoom);
    }

    private void RoomManager_OnLightsStateChange()
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

    private void RoomManager_OnRoomFinished()
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
        cam.transform.position = new Vector3(currentRoom * camWorldWidht, cam.transform.position.y, cam.transform.position.z);
        UpdateHUD();
        HUD.Restart();
        Rooms[currentRoom].ResetBlockers();
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

        Player.GetComponent<PlayerController>().ResetDash();
    }

    private void RestartRoom(int roomNumber)
    {
        if(roomNumber >= Rooms.Length)
        {
            Debug.LogWarning("Room " + roomNumber + "do not exist");
            return;
        }
        Rooms[roomNumber].ResetRoom();
        Player.transform.position = Rooms[roomNumber].PlayerStart.position;
        InitRoom(roomNumber);
        currentRoom = roomNumber;
        cam.transform.position = new Vector3(currentRoom * camWorldWidht, cam.transform.position.y, cam.transform.position.z);

        UpdateHUD();
        HUD.Restart();
        Player.GetComponent<PlayerController>().ResetDash();
    }


    #region UI
    public HUDController HUD;
    private void UpdateHUD()
    {
        HUD.UpdateText_LightsCounter(currentRoomUnlitLights + " / " + currentRoomTotalLights);
    }

    #endregion

}
