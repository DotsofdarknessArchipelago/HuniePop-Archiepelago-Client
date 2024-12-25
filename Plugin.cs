using System;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using HuniePopArchiepelagoClient.Archipelago;
using HuniePopArchiepelagoClient.Utils;
using UnityEngine;

namespace HuniePopArchiepelagoClient
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "Dots.Archipelago.huniepop";
        public const string PluginName = "Hunie Pop";
        public const string PluginVersion = "0.2.1";

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Archipelago v{PluginVersion}";
        public static ManualLogSource BepinLogger;
        //public static ArchipelagoClient ArchipelagoClient;
        public static CursedArchipelagoClient curse;

        public string playeruri = "ws://localhost:38281";
        public string playername = "Player1";
        public string playerpass = "";

        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            curse = new CursedArchipelagoClient();
            ArchipelagoConsole.Awake();

            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
            Patches.patch(curse);
            ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchipelagoConsole.toggle();
            }
        }

        private void OnGUI()
        {
            GUI.depth = -1;
            // show the mod is currently loaded in the corner
            ArchipelagoConsole.OnGUI();

            GUI.backgroundColor = Color.black;

            string statusMessage;
            // show the Archipelago Version and whether we're connected or not
            if (CursedArchipelagoClient.Authenticated)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 40), "");
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "Client V(" + PluginVersion + "), World V(" + curse.connected.SlotData["world_version"] +"): Status: Connected");

            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 130), "");

                statusMessage = " Status: Disconnected";
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), APDisplayInfo + statusMessage);
                GUI.Label(new Rect(Screen.width - 295, 40, 150, 20), "Host: ");
                GUI.Label(new Rect(Screen.width - 295, 60, 150, 20), "Player Name: ");
                GUI.Label(new Rect(Screen.width - 295, 80, 150, 20), "Password: ");

                playeruri = GUI.TextField(new Rect(Screen.width - 150, 40, 140, 20), playeruri, 100);
                playername = GUI.TextField(new Rect(Screen.width - 150, 60, 140, 20), playername, 100);
                playerpass = GUI.TextField(new Rect(Screen.width - 150, 80, 140, 20), playerpass, 100);

                // requires that the player at least puts *something* in the slot name
                if (GUI.Button(new Rect(Screen.width - 200, 105, 100, 20), "Connect") && !name.IsNullOrWhiteSpace())
                {
                    curse.setup(playeruri, playername, playerpass);
                    curse.connect();
                    Thread.Sleep(1000);
                    curse.sendConnectPacket();
                    //ArchipelagoClient.ServerData.Uri = playeruri.Trim();
                    //ArchipelagoClient.ServerData.SlotName = playername;
                    //ArchipelagoClient.ServerData.Password = playerpass;
                    //ArchipelagoClient.Connect();
                }

            }

            // this is a good place to create and add a bunch of debug buttons
        }
    }
}