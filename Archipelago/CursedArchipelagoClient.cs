using System;
using System.Runtime.InteropServices;
using Archipelago.MultiClient.Net.Models;
using static UnityEngine.Application;
using WebSocketSharp;
using Archipelago.MultiClient.Net.Packets;
using Newtonsoft.Json;
using System.Security.Principal;
using Archipelago.MultiClient.Net;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Converters;
using Archipelago.MultiClient.Net.Enums;
using UnityEngine;
using System.Threading;
using System.Linq;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using tk2dRuntime.TileMap;
using Archipelago.MultiClient.Net.MessageLog.Parts;
using System.Runtime.Remoting.Lifetime;
using HuniePopArchiepelagoClient.Utils;
using static System.Collections.Specialized.BitVector32;
using System.IO;
using System.Text;
using System.Net.Configuration;
using System.Net.Sockets;

namespace HuniePopArchiepelagoClient.Archipelago
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void callback(string msg);

    public class CursedArchipelagoClient
    {

        public static callback myCallBack = new callback(tmp);

        static readonly ArchipelagoPacketConverter Converter = new ArchipelagoPacketConverter();
        public RoomInfoPacket roominfo;
        public ConnectedPacket connected;
        public string seed = "";
        public string error = null;

        public const string APVersion = "0.5.0";
        private const string Game = "Hunie Pop";

        string url;
        string username;
        string password;

        public bool Authenticated = false;
        public bool working = false;
        public bool fullconnect = false;

        public static ArchipelageItemList alist = new ArchipelageItemList();

        public IntPtr ws;

        public void setup(string h, string u, string p)
        {
            url = h.Trim();
            username = u.Trim();
            password = p;

            if (!url.StartsWith("ws://") && !url.StartsWith("wss://"))
            {
                helper.MessageBox(IntPtr.Zero, "URL supplied does not contain \"ws://\" or \"wss://\" at the start\ni.e. wss://archipelago.gg:12345\nNOTE: if connecting to archipelago.gg use \"wss://\"", "URL ERROR", 0);
                return;
            }

            ws = helper.getWS();
            helper.seturlWS(ws, url);
            helper.setcallWS(ws, myCallBack);

        }

        public void connect()
        {
            if (ws == null || url == null || username == null) { return; }
            if (helper.readyWS(ws) == 3) { return; }
            if (!url.StartsWith("ws://") && !url.StartsWith("wss://")) { return; }
            helper.startWS(ws);
        }

        public static void tmp(string g)
        {

        }

        public void sendConnectPacket()
        {
            if (helper.readyWS(ws)!=3) { return; }
            ConnectPacket packet = new ConnectPacket();
            packet.Game = Game;
            packet.Name = username;
            packet.Password = password;
            packet.Uuid = Guid.NewGuid().ToString();
            packet.Version = new NetworkVersion(new Version(APVersion));
            packet.Tags = ["AP"];
            packet.ItemsHandling = ItemsHandlingFlags.AllItems;
            packet.RequestSlotData = true;
            helper.sendWS(ws, JsonConvert.SerializeObject(packet));
        }

        public void sendJson(string json)
        {
            helper.sendWS(ws, json);
        }

        public void sendLoc(int loc)
        {
            helper.sendWS(ws, "{\"cmd\":\"LocationChecks\",\"locations\":[" + loc + "]}");
        }

        public void sendCompletion()
        {
            helper.sendWS(ws, "{\"cmd\":\"StatusUpdate\",\"status\":" + 30 + "}");
        }

        public void sendSay(string msg)
        {
            if (msg.StartsWith("$")) { code(msg); return; }
            helper.sendWS(ws, "{\"cmd\":\"Say\",\"text\":\"" + msg + "\"}");
        }

        public void code(string msg)
        {

            ArchipelagoConsole.LogMessage("CODE ENTERED");
            if (msg == "$archdata")
            {
                ArchipelagoConsole.LogMessage("-------DEBUG DATA-------");
                ArchipelagoConsole.LogMessage("List size: " + alist.list.Count.ToString());
                ArchipelagoConsole.LogMessage("------------------------");
                for (int i = 0; i < alist.list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("#" + i.ToString());
                    ArchipelagoConsole.LogMessage("ID:" + alist.list[i].Id.ToString());
                    ArchipelagoConsole.LogMessage("PROCESSED:" + alist.list[i].processed.ToString());
                    ArchipelagoConsole.LogMessage("PUTINSHOP:" + alist.list[i].putinshop.ToString());
                    ArchipelagoConsole.LogMessage("------------------------");
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$resync")
            {
                CursedArchipelagoClient.alist = new ArchipelageItemList();
                sendJson("{\"cmd\":" + "\"Sync\"}");
            }
            if (msg == "$giftids")
            {
                List<ItemDefinition> list;
                list = GameManager.Data.Items.GetAllOfType(ItemType.GIFT, false);
                ArchipelagoConsole.LogMessage("-------GIFT DATA-------");
                for (int i = 0; i < list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + list[i].id + " | ITEM NAME:" + list[i].name);
                }
                List<ItemDefinition> list2;
                list2 = GameManager.Data.Items.GetAllOfType(ItemType.UNIQUE_GIFT, false);
                ArchipelagoConsole.LogMessage("-------UNIQUE GIFT DATA-------");
                for (int i = 0; i < list2.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + list2[i].id + " | ITEM NAME:" + list2[i].name);
                }
                List<ItemDefinition> list3;
                list3 = GameManager.Data.Items.GetAllOfType(ItemType.PANTIES, false);
                ArchipelagoConsole.LogMessage("-------PANTIES DATA-------");
                for (int i = 0; i < list3.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + list3[i].id + " | ITEM NAME:" + list3[i].name);
                }
            }
        }

        public void setupdata()
        {

            alist.seed = roominfo.SeedName;


            if (File.Exists(Application.persistentDataPath + "/archdata"))
            {
                using (StreamReader file = File.OpenText(Application.persistentDataPath + "/archdata"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                    if (alist.seed == savedlist.seed)
                    {
                        ArchipelagoConsole.LogMessage("archdata file found restoring session");
                        if (alist.merge(savedlist.list))
                        {
                            ArchipelagoConsole.LogMessage("ERROR LOADING SAVED ITEM LIST RESETING ITEM LIST");
                            alist = new ArchipelageItemList();
                            alist.seed = roominfo.SeedName;
                            sendJson("{\"cmd\":"+"\"Sync\"}");
                        }
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("archdata file found but dosent match server seed creating new session");
                        ArchipelagoConsole.LogMessage(roominfo.SeedName + ": does not equal :" + savedlist.seed);
                        alist.seed = roominfo.SeedName;
                    }
                }
            }
            else
            {
                ArchipelagoConsole.LogMessage("archdata file not found creating new session");
            }
        }

        public static void connerror(ConnectionRefusedPacket msg)
        {
            ArchipelagoConsole.LogMessage("CONNECTION TO ARCHIPELAGO SERVER REFUSED WITH ERRORS:");
            for (int i = 0; i < msg.Errors.Length; i++)
            {
                switch (msg.Errors[i])
                {
                    case ConnectionRefusedError.InvalidSlot:
                        ArchipelagoConsole.LogMessage("ERROR: NAME DID NOT MATCH ANY NAMES ON SERVER");
                        break;
                    case ConnectionRefusedError.InvalidGame:
                        ArchipelagoConsole.LogMessage("ERROR: NAME IS CORRECT BUT GAME ASSOIATED WITH IT IS WRONG");
                        break;
                    case ConnectionRefusedError.SlotAlreadyTaken:
                        ArchipelagoConsole.LogMessage("ERROR: CONNECTION TO THIS SLOT IS ALREADY ESTABLISHED");
                        break;
                    case ConnectionRefusedError.IncompatibleVersion:
                        ArchipelagoConsole.LogMessage("ERROR: VERSION MISSMATCH");
                        break;
                    case ConnectionRefusedError.InvalidPassword:
                        ArchipelagoConsole.LogMessage("ERROR: PASSWORD IS WRONG");
                        break;
                    case ConnectionRefusedError.InvalidItemsHandling:
                        ArchipelagoConsole.LogMessage("ERROR: ITEM HANDLING FLAGS ARE WRONG");
                        break;
                    default:
                        break;
                }
            }


            foreach (ConnectionRefusedError error in msg.Errors)
            {
            }
        }

        public static void msgCallback(string msg)
        {
            //ArchipelagoConsole.LogMessage($"{msg}");
            if (!msg.StartsWith("{") && !msg.StartsWith("["))
            {
                ArchipelagoConsole.LogMessage(msg);
                Plugin.curse.error = msg;
                return;
            }
            ArchipelagoPacketBase packet = null;

            packet = JsonConvert.DeserializeObject<ArchipelagoPacketBase>(msg, Converter);

            if (packet != null)
                    //DEBUG PACKET CODE
                    Plugin.BepinLogger.LogMessage(JsonConvert.SerializeObject(packet));
                    if (packet is RoomInfoPacket)
                    {
                        Plugin.curse.roominfo = (RoomInfoPacket)packet;
                    }
                    else if (packet is ConnectionRefusedPacket)
                    {
                        connerror((ConnectionRefusedPacket)packet);
                        Plugin.curse.Authenticated = false;
                        Plugin.curse.working = false;
                        Plugin.curse.fullconnect = false;
                        Plugin.tringtoconnect = false;
                        Plugin.connectstage = 0;
                    }
                    else if (packet is ConnectedPacket)
                    {
                        Plugin.curse.connected = (ConnectedPacket)packet;
                        Plugin.curse.Authenticated = true;
                        Plugin.curse.setupdata();
                    }
                    else if (packet is ReceivedItemsPacket)
                    {
                        ReceivedItemsPacket p = (ReceivedItemsPacket)packet;
                        foreach (NetworkItem item in p.Items)
                        {
                            alist.add(item);
                        }
                        if (!Plugin.curse.fullconnect) 
                        {
                            Plugin.BepinLogger.LogMessage("recieved an item so everything look like its working");
                            Plugin.curse.fullconnect = true; 
                        }
                    }
                    else if (packet is LocationInfoPacket)
                    {
                    }
                    else if (packet is RoomUpdatePacket)
                    {
                    }
                    else if (packet is PrintJsonPacket)
                    {
                        printJSON((PrintJsonPacket)packet);
                    }
                    else if (packet is BouncedPacket)
                    {
                    }
                    else if (packet is InvalidPacketPacket)
                    {
                    }
                    else if (packet is RetrievedPacket)
                    {
                    }
                    else if (packet is SetReplyPacket)
                    {
                    }
                
        }

        public static void printJSON(PrintJsonPacket packet)
        {
            Plugin.BepinLogger.LogMessage(JsonConvert.SerializeObject(packet));
            string msg = "";

            Plugin.BepinLogger.LogMessage(packet.Data.Length);

            for (int i = 0; i < packet.Data.Count(); i++)
            {
                var part = packet.Data[i];

                if (part.Type == JsonMessagePartType.PlayerId)
                {
                    bool f = true;
                    if (Plugin.curse.roominfo.Players == null)
                    {
                        msg += "PLAYER{" + part.Text + "}";
                        continue;
                    }
                    for(int j=0; j<Plugin.curse.roominfo.Players.Length; j++)
                    {
                        if (Plugin.curse.roominfo.Players[j].Slot.ToString() == part.Text)
                        {
                            msg += Plugin.curse.roominfo.Players[j].Name;
                            f= false;
                        }
                    }
                    if (f)
                    {
                        msg += "PLAYER{" + part.Text + "}";
                    }

                }
                else if (part.Type == JsonMessagePartType.ItemId)
                {
                    string s = Util.idtoitem(Convert.ToInt32(part.Text));
                    if (s == "")
                    {
                        msg += "Item{" + part.Text + "}";
                    }
                    else
                    {
                        msg += s;
                    }
                }
                else if (part.Type == JsonMessagePartType.LocationId)
                {
                    msg += "LOCATION{" + part.Text + "}";
                }
                else
                {
                    msg += part.Text;
                }
            }
            if (msg == "") { msg = "ERROR DECODING PRINT JSON"; }
            ArchipelagoConsole.LogMessage(msg);
            /*
            if (packet is ItemPrintJsonPacket)
            {
                ItemPrintJsonPacket p = (ItemPrintJsonPacket)packet;
                
            }
            else if (packet is ItemCheatPrintJsonPacket)
            {
            }
            else if (packet is HintPrintJsonPacket)
            {
            }
            else if (packet is JoinPrintJsonPacket)
            {
            }
            else if (packet is LeavePrintJsonPacket)
            {
            }
            else if (packet is ChatPrintJsonPacket)
            {
            }
            else if (packet is ServerChatPrintJsonPacket)
            {
            }
            else if (packet is TutorialPrintJsonPacket)
            {
            }
            else if (packet is TagsChangedPrintJsonPacket)
            {
            }
            else if (packet is CommandResultPrintJsonPacket)
            {
            }
            else if (packet is AdminCommandResultPrintJsonPacket)
            {
            }
            else if (packet is GoalPrintJsonPacket)
            {
            }
            else if (packet is ReleasePrintJsonPacket)
            {
            }
            else if (packet is CollectPrintJsonPacket)
            {
            }
            else if (packet is CountdownPrintJsonPacket)
            {
            }*/
        }

    }


    public class helper
    {

        [DllImport("user32.dll")]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);


        [DllImport("DotsWebSocket.dll")]
        public static extern IntPtr getWS();

        [DllImport("DotsWebSocket.dll")]
        public static extern void startWS(IntPtr ws);

        [DllImport("DotsWebSocket.dll")]
        public static extern void seturlWS(IntPtr ws, string url);

        [DllImport("DotsWebSocket.dll")]
        public static extern void setcallWS(IntPtr ws, callback call);

        [DllImport("DotsWebSocket.dll")]
        public static extern void sendWS(IntPtr ws, string msg);

        [DllImport("DotsWebSocket.dll")]
        public static extern int readyWS(IntPtr ws);

        [DllImport("DotsWebSocket.dll")]
        public static extern bool hasmsg(IntPtr ws);

        [DllImport("DotsWebSocket.dll")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string getmsg(IntPtr ws);


    }
}
