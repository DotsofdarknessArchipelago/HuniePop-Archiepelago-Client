using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using HuniePopArchiepelagoClient.ArchipelagoPackets;
using HuniePopArchiepelagoClient.Utils;
using System.Security.Principal;
using System.Net.NetworkInformation;

namespace HuniePopArchiepelagoClient.Archipelago
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void callback(string msg);

    public class CursedArchipelagoClient
    {

        public static callback myCallBack = new callback(tmp);

        public RoomInfoPacket room;
        public DataPackagePacket data;
        public ConnectedPacket connected;
        public NetworkVersion worldver;

        public string seed = "";
        public string error = null;

        public const string APVersion = "0.5.0";
        private const string Game = "Hunie Pop";

        string url;
        string username;
        string password;

        public bool recievedroominfo = false;
        public bool sendroomdatapackage = false;
        public bool recievedroomdatapackage = false;
        public bool processedroomdatapackage = false;
        public bool startprocessedroomdatapackage = false;
        public bool processeddatapackage = false;
        public bool sentconnectedpacket = false;
        public bool recievedconnectedpacket = false;
        public bool fullconnection = false;

        public static ArchipelageItemList alist = new ArchipelageItemList();

        public IntPtr ws;

        public void setup(string h, string u, string p)
        {
            url = h.Trim();
            username = u.Trim();
            password = p;

            if (!url.StartsWith("ws://") && !url.StartsWith("wss://"))
            {
                ArchipelagoConsole.LogImportant("URL supplied does not contain \"ws://\" or \"wss://\" at the start\ni.e. wss://archipelago.gg:12345\nNOTE: if connecting to archipelago.gg use \"wss://\"");
                return;
            }

            ws = helper.getWS();
            helper.seturlWS(ws, url);
            helper.setcallWS(ws);

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
            if (helper.readyWS(ws) != 3) { return; }
            string pack = "{\"cmd\":\"Connect\",\"game\":\"" + Game + "\",\"name\":\"" + username + "\",\"password\":\"" + password + "\",\"uuid\":\"" + Guid.NewGuid().ToString() + "\",\"version\":{\"major\":0,\"minor\":5,\"build\":1,\"class\":\"Version\"},\"tags\":[\"AP\"],\"items_handling\":7,\"slot_data\":true}";
            helper.sendWS(ws, pack);
        }

        public void sendGetPackage()
        {
            if (helper.readyWS(ws) != 3) { return; }
            string games = "";
            for (int i = 0; i < room.games.Count(); i++)
            {
                if (i == 0) { games = "\""+room.games[i]+"\""; continue; }
                games += ",\"" + room.games[i] + "\"";
            }
            helper.sendWS(ws, "{\"cmd\":\"GetDataPackage\", \"games\":[" + games + "]}");
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

            ArchipelagoConsole.LogMessage($"CODE ENTERED: {msg}");
            if (msg == "$archdata")
            {
                ArchipelagoConsole.LogMessage("-------DEBUG DATA-------");
                ArchipelagoConsole.LogMessage("List size: " + alist.list.Count.ToString());
                ArchipelagoConsole.LogMessage("------------------------");
                for (int i = 0; i < alist.list.Count; i++)
                {
                    ArchipelagoConsole.LogMessage("#" + i.ToString());
                    ArchipelagoConsole.LogMessage("ID:" + alist.list[i].Id.ToString());
                    ArchipelagoConsole.LogMessage("NAME:" + alist.list[i].itemname);
                    ArchipelagoConsole.LogMessage("LOCID:" + alist.list[i].LocationId.ToString());
                    ArchipelagoConsole.LogMessage("LOCNAME:" + alist.list[i].locationname);
                    ArchipelagoConsole.LogMessage("PROCESSED:" + alist.list[i].processed.ToString());
                    ArchipelagoConsole.LogMessage("PUTINSHOP:" + alist.list[i].putinshop.ToString());
                    ArchipelagoConsole.LogMessage("------------------------");
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$resetitems")
            {
                CursedArchipelagoClient.alist = new ArchipelageItemList();
                sendJson("{\"cmd\":\"Sync\"}");
            }
            if (msg == "$resync")
            {
                sendJson("{\"cmd\":\"Sync\"}");
            }
            if (msg == "$deldata")
            {
                File.Delete(Application.persistentDataPath + "/archdata");
            }

            if (msg == "$girls")
            {
                List<GirlDefinition> l = GameManager.Data.Girls.GetAll();
                ArchipelagoConsole.LogMessage("-------GIRL DATA-------");
                foreach (GirlDefinition g in l)
                {
                    ArchipelagoConsole.LogMessage("GIRL ID:" + g.id + " | GIRL NAME:" + g.name);
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
            if (msg == "$items")
            {
                List<ItemDefinition> list = GameManager.Data.Items.GetAllOfType(ItemType.FOOD, false);
                List<ItemDefinition> list2 = GameManager.Data.Items.GetAllOfType(ItemType.DRINK, false);
                List<ItemDefinition> list3 = GameManager.Data.Items.GetAllOfType(ItemType.GIFT, false);
                List<ItemDefinition> list4 = GameManager.Data.Items.GetAllOfType(ItemType.UNIQUE_GIFT, false);
                List<ItemDefinition> list5 = GameManager.Data.Items.GetAllOfType(ItemType.DATE_GIFT, false);
                List<ItemDefinition> list6 = GameManager.Data.Items.GetAllOfType(ItemType.ACCESSORY, false);
                List<ItemDefinition> list7 = GameManager.Data.Items.GetAllOfType(ItemType.PANTIES, false);
                List<ItemDefinition> list8 = GameManager.Data.Items.GetAllOfType(ItemType.PRESENT, false);
                List<ItemDefinition> list9 = GameManager.Data.Items.GetAllOfType(ItemType.MISC, false);
                ArchipelagoConsole.LogMessage("-------FOOD DATA-------");
                foreach(ItemDefinition i in list)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------DRINK DATA-------");
                foreach (ItemDefinition i in list2)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------GIFT DATA-------");
                foreach (ItemDefinition i in list3)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------UNIQUE_GIFT DATA-------");
                foreach (ItemDefinition i in list4)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------DATE_GIFT DATA-------");
                foreach (ItemDefinition i in list5)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------ACCESSORY DATA-------");
                foreach (ItemDefinition i in list6)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------PANTIES DATA-------");
                foreach (ItemDefinition i in list7)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------PRESENT DATA-------");
                foreach (ItemDefinition i in list8)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-------MISC DATA-------");
                foreach (ItemDefinition i in list9)
                {
                    ArchipelagoConsole.LogMessage("ITEM ID:" + i.id + " | ITEM NAME:" + i.name);
                }
                ArchipelagoConsole.LogMessage("-----END DEBUG DATA-----");
            }
        }

        public void setupdata()
        {
            alist.seed = room.seed_name;

            if (File.Exists(Application.persistentDataPath + "/archdata"))
            {
                using (StreamReader file = File.OpenText(Application.persistentDataPath + "/archdata"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ArchipelageItemList savedlist = (ArchipelageItemList)serializer.Deserialize(file, typeof(ArchipelageItemList));
                    if (savedlist.listversion != 2)
                    {
                        ArchipelagoConsole.LogMessage($"ARCHDATA version missmatch({savedlist.listversion}!=2) using new archdata");
                    }
                    else if (savedlist.seed != alist.seed)
                    {
                        ArchipelagoConsole.LogMessage($"ARCHDATA seed missmatch(archdata:{savedlist.seed}!=server:{alist.seed}) using new archdata");
                    }
                    else if (alist.seed == savedlist.seed && savedlist.listversion == 2)
                    {
                        ArchipelagoConsole.LogMessage("archdata file found restoring session");
                        if (alist.merge(savedlist.list))
                        {
                            ArchipelagoConsole.LogMessage("ERROR LOADING SAVED ARCHDATA RESETING ARCHDATA");
                            alist = new ArchipelageItemList();
                            alist.seed = room.seed_name;
                            sendJson("{\"cmd\":\"Sync\"}");
                        }
                    }
                    else
                    {
                        ArchipelagoConsole.LogMessage("ARCHDATA found but an error occoured just going to use new archdata");
                    }
                }
            }
            else
            {
                ArchipelagoConsole.LogMessage("archdata file not found creating new session");
            }
            ArchipelagoConsole.CommandText = "!help";
        }

        public static void msgCallback(string msg)
        {
            if (!msg.StartsWith("{") && !msg.StartsWith("["))
            {
                ArchipelagoConsole.LogImportant(msg);
                Plugin.curse.error = msg;
                return;
            }

            string cmd = "";
            JObject msgjson = JObject.Parse(msg);
            if (msgjson.ContainsKey("cmd"))
            {
                cmd = (string)msgjson["cmd"];
            }

            //ArchipelagoConsole.LogMessage("MESSAGE GOTTEN\n" + msg);
            if (cmd == "RoomInfo")
            {
                //ArchipelagoConsole.LogMessage("RoomInfo PACKET GOTTEN");
                Plugin.curse.room = JsonConvert.DeserializeObject<RoomInfoPacket>(msg);
                Plugin.curse.recievedroominfo = true;
            }
            else if (cmd == "ConnectionRefused")
            {
                ArchipelagoConsole.LogMessage("ConnectionRefused PACKET GOTTEN");
                ArchipelagoConsole.LogImportant(msg);

            }
            else if (cmd == "Connected")
            {
                //ArchipelagoConsole.LogMessage("Connected PACKET GOTTEN");
                Plugin.curse.connected = JsonConvert.DeserializeObject<ConnectedPacket>(msg);
                NetworkVersion wv = JsonConvert.DeserializeObject<NetworkVersion>(msgjson["slot_data"]["world_version"].ToString());
                Plugin.curse.worldver = wv;
                if (wv.major > Plugin.compatworldmajor)
                {
                    ArchipelagoConsole.LogImportant($"APVERSION ERROR:Major version({wv.major}) greater than compatible Major version({Plugin.compatworldmajor}) HIGH chance of errors occurring");
                }
                if (wv.minor > Plugin.compatworldminor)
                {
                    ArchipelagoConsole.LogImportant($"APVERSION ERROR:Minor version({wv.minor}) greater than compatible Minor version({Plugin.compatworldminor}) HIGH chance of errors occurring");
                }
                if (wv.build < Plugin.compatworldbuild)
                {
                    ArchipelagoConsole.LogImportant($"APVERSION ERROR:Build version({wv.build}) lower than compatible build version({Plugin.compatworldbuild}) chance of errors occurring");
                }
                Plugin.curse.setupdata();
                Plugin.curse.recievedconnectedpacket = true;

            }
            else if (cmd == "ReceivedItems")
            {
                //ArchipelagoConsole.LogMessage("ReceivedItems PACKET GOTTEN");
                Plugin.BepinLogger.LogMessage("itempacketmsg:\n" + msg);
                if (!Plugin.curse.fullconnection)
                {
                    Plugin.curse.fullconnection = true;
                }
                ReceivedItemsPacket pack = JsonConvert.DeserializeObject<ReceivedItemsPacket>(msg);
                foreach (NetworkItem item in pack.items)
                {
                    Plugin.BepinLogger.LogMessage("adding item");
                    alist.add(item);
                }

            }
            else if (cmd == "LocationInfo")
            {
                ArchipelagoConsole.LogMessage("LocationInfo PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "RoomUpdate")
            {
                //ArchipelagoConsole.LogMessage("RoomUpdate PACKET GOTTEN");
                //ArchipelagoConsole.LogMessage(msg);
                JsonConvert.DeserializeObject<RoomUpdatePacket>(msg).update();

            }
            else if (cmd == "PrintJSON")
            {
                //ArchipelagoConsole.LogMessage("PrintJSON PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(JsonConvert.DeserializeObject<PrintJSONPacket>(msg).print());

            }
            else if (cmd == "DataPackage")
            {
                //ArchipelagoConsole.LogMessage("DataPackage PACKET GOTTEN");
                Plugin.curse.data = JsonConvert.DeserializeObject<DataPackagePacket>(msg);
                //ArchipelagoConsole.LogMessage(Plugin.curse.data.data.games.ToString());
                Plugin.curse.recievedroomdatapackage = true;

            }
            else if (cmd == "Bounced")
            {
                ArchipelagoConsole.LogMessage("Bounced PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "InvalidPacket")
            {
                ArchipelagoConsole.LogMessage("InvalidPacket PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "Retrieved")
            {
                ArchipelagoConsole.LogMessage("Retrieved PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else if (cmd == "SetReply")
            {
                ArchipelagoConsole.LogMessage("SetReply PACKET GOTTEN");
                ArchipelagoConsole.LogMessage(msg);

            }
            else
            {
                ArchipelagoConsole.LogMessage("---MESSAGE ERROR PRINTING MESSAGE---");
                ArchipelagoConsole.LogMessage($"{msg}");
            }

        }
    }


    public class helper
    {

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern IntPtr getWS();

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void startWS(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void seturlWS(IntPtr ws, string url);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void setcallWS(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern void sendWS(IntPtr ws, string msg);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern int readyWS(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern int dotsV();

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern bool hasmsg(IntPtr ws);

        [DllImport("/BepInEx/plugins/Hunie Pop Archipelago Client/DotsWebSocket.dll")]
        public static extern IntPtr getmsg(IntPtr ws);


    }
}
