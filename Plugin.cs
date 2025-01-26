using System;
using System.IO;
using System.Security;
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
        public const string PluginVersion = "0.3.0";

        public const string ModDisplayInfo = $"{PluginName} v{PluginVersion}";
        private const string APDisplayInfo = $"Archipelago v{PluginVersion}";
        public static ManualLogSource BepinLogger;
        //public static ArchipelagoClient ArchipelagoClient;
        public static CursedArchipelagoClient curse;

        public string playeruri = "ws://localhost:38281";
        public string playername = "Player1";
        public string playerpass = "";

        public static bool tringtoconnect = false;
        public static int connectstage = 0;
        bool dll;

        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            curse = new CursedArchipelagoClient();
            ArchipelagoConsole.Awake();

            Patches.patch(curse);

            try
            {
                ArchipelagoConsole.LogMessage("DotsWebsocket.dll version:" + helper.dotsV().ToString());
                ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
                dll = true;
            }
            catch (Exception e)
            {
                dll = false;
                ArchipelagoConsole.LogMessage("FATAL ERROR: DotsWebSocket.dll not found");
                if (File.Exists("DotsWebSocket.dll"))
                {
                    ArchipelagoConsole.Hidden = true;
                    ArchipelagoConsole.toggle();
                    ArchipelagoConsole.LogMessage("DotsWebSocket.dll exists but errored on client.\nmake sure you have \"Microsoft Visual C++ Redistributable x86\" version greater than 14.42.34433.0 installed.\nPermalink to latest: https://aka.ms/vs/17/release/vc_redist.x86.exe");
                }
            }

        }
        void Update()
        {
            if (dll)
            {
                if (tringtoconnect && helper.hasmsg(curse.ws))
                {
                    CursedArchipelagoClient.msgCallback(helper.getmsg(curse.ws));
                }
            }
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
            if (curse.fullconnect)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 40), "");
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "Client V(" + PluginVersion + "), World V(" + curse.connected.SlotData["world_version"] +"): Status: Connected");

            }
            else if (tringtoconnect)
            {
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 140), "");
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "-trying to connect to server");
                if (helper.readyWS(curse.ws)==3)
                {
                    GUI.Label(new Rect(Screen.width - 295, 40, 300, 20), "-initial server connection established");
                    GUI.Label(new Rect(Screen.width - 295, 60, 300, 20), "-sending archipelago connect packet");
                    if (connectstage == 0) { connectstage++; curse.sendConnectPacket(); }
                }
                if (curse.Authenticated)
                {
                    GUI.Label(new Rect(Screen.width - 295, 80, 300, 20), "-connection to archipelago server established");
                    GUI.Label(new Rect(Screen.width - 295, 100, 300, 20), "-waiting on geting a packet to know if connection");
                    GUI.Label(new Rect(Screen.width - 295, 120, 300, 20), "is fully working");
                }
                if (helper.readyWS(curse.ws) == 2)
                {
                    if (GUI.Button(new Rect(Screen.width - 200, 120, 100, 20), "RESET")) { tringtoconnect = false; }
                }
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
                    if (dll)
                    {
                        tringtoconnect = true;
                        curse.setup(playeruri, playername, playerpass);
                        curse.connect();
                    }
                    //Thread.Sleep(1000);
                    //curse.sendConnectPacket();
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