using System;
using System.IO;
using System.Runtime.InteropServices;
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
        public const string PluginVersion = "0.6.1";
        public static int compatworldmajor = 0;
        public static int compatworldminor = 6;
        public static int compatworldbuild = 1;


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

        private static Texture2D SolidBoxTex;

        private void Awake()
        {
            // Plugin startup logic
            BepinLogger = Logger;
            curse = new CursedArchipelagoClient();
            ArchipelagoConsole.Awake();

            Patches.patch(curse);

            try
            {
                ArchipelagoConsole.LogMessage($"{ModDisplayInfo} loaded!");
                ArchipelagoConsole.LogMessage("DotsWebsocket.dll version:" + helper.dotsV().ToString());
                if (helper.dotsV() == 2) { dll = true; }
                else { ArchipelagoConsole.LogMessage("DotsWebsocket Not Correct Version"); }

            }
            catch (Exception e)
            {
                dll = false;
                ArchipelagoConsole.LogImportant("FATAL ERROR: DotsWebSocket.dll not found");
                if (File.Exists("DotsWebSocket.dll"))
                {
                    ArchipelagoConsole.LogImportant("DotsWebSocket.dll exists but errored on client.\nmake sure you have \"Microsoft Visual C++ Redistributable x86\" version greater than 14.42.34433.0 installed.\nPermalink to latest: https://aka.ms/vs/17/release/vc_redist.x86.exe");
                }
            }

        }
        void Update()
        {
            if (dll)
            {
                if (tringtoconnect && helper.hasmsg(curse.ws))
                {
                    CursedArchipelagoClient.msgCallback(Marshal.PtrToStringAnsi(helper.getmsg(curse.ws)));
                }
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                ArchipelagoConsole.toggle();
            }
            if (Input.GetKeyDown(KeyCode.Return) && !ArchipelagoConsole.Hidden && !ArchipelagoConsole.CommandText.IsNullOrWhiteSpace())
            {
                Plugin.curse.sendSay(ArchipelagoConsole.CommandText);
                ArchipelagoConsole.CommandText = "";
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
            if (curse.fullconnection)
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = false;
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 40), "");
                if (curse.worldver == null)
                {
                    GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "Client V(" + PluginVersion + "), World V(ERROR VERSION NOT SUPPORTED): Status: Connected");
                }
                else
                {
                    GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "Client V(" + PluginVersion + "), World V(" + curse.worldver.major + "." + curse.worldver.minor + "." + curse.worldver.build + "): Status: Connected");
                }

            }
            else if (tringtoconnect)
            {
                DrawSolidBox(new Rect(Screen.width - 300, 10, 300, 200));
                GUI.Box(new Rect(Screen.width - 300, 10, 300, 200), "");
                GUI.Label(new Rect(Screen.width - 295, 20, 300, 20), "-trying to connect to server");
                if (helper.readyWS(curse.ws) == 3)
                {
                    GUI.Label(new Rect(Screen.width - 295, 40, 300, 20), "-initial server connection established");
                }
                if (curse.recievedroominfo)
                {
                    GUI.Label(new Rect(Screen.width - 295, 60, 300, 20), "-sending archipelago GetDataPackages packet");
                    if (!curse.sendroomdatapackage)
                    {
                        curse.sendGetPackage();
                        curse.sendroomdatapackage = true;
                    }
                }
                if (curse.recievedroomdatapackage)
                {
                    GUI.Label(new Rect(Screen.width - 295, 80, 300, 20), "-recieved archipelago GetDataPackages packet");
                    if (!curse.startprocessedroomdatapackage && !curse.processeddatapackage)
                    {
                        curse.data.data.setup();
                        curse.processeddatapackage = true;
                    }
                }
                if (curse.processeddatapackage)
                {
                    GUI.Label(new Rect(Screen.width - 295, 100, 300, 20), "-processed archipelago GetDataPackages");
                    if (!curse.startprocessedroomdatapackage)
                    {
                        GUI.Label(new Rect(Screen.width - 295, 120, 300, 20), "-sending archipelago Connect Packet");
                        if (!curse.sentconnectedpacket)
                        {
                            curse.sendConnectPacket();
                            curse.sentconnectedpacket = true;
                        }
                    }
                }
                if (curse.recievedconnectedpacket)
                {
                    GUI.Label(new Rect(Screen.width - 295, 140, 300, 20), "-connection to archipelago server established");
                    GUI.Label(new Rect(Screen.width - 295, 160, 300, 20), "-waiting on geting a packet to know if connection");
                    GUI.Label(new Rect(Screen.width - 295, 180, 300, 20), "is fully working");
                }
                if (helper.readyWS(curse.ws) == 2)
                {
                    if (GUI.Button(new Rect(Screen.width - 200, 180, 100, 20), "RESET")) { tringtoconnect = false; }
                }
            }
            else
            {
                // if your game doesn't usually show the cursor this line may be necessary
                // Cursor.visible = true;
                DrawSolidBox(new Rect(Screen.width - 300, 10, 300, 130));
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


        public static void DrawSolidBox(Rect boxRect)
        {
            if (SolidBoxTex == null)
            {
                var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                windowBackground.SetPixel(0, 0, new Color(0, 0, 0));
                windowBackground.Apply();
                SolidBoxTex = windowBackground;
            }

            // It's necessary to make a new GUIStyle here or the texture doesn't show up
            GUI.Box(boxRect, "", new GUIStyle { normal = new GUIStyleState { background = SolidBoxTex } });
        }
    }
}