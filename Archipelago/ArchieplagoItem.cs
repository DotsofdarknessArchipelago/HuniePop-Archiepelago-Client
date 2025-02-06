using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HuniePopArchiepelagoClient.ArchipelagoPackets;
using HuniePopArchiepelagoClient.Utils;

namespace HuniePopArchiepelagoClient.Archipelago
{
    public class ArchipelagoItem
    {
        public long Id;
        public long LocationId;
        public int playerslot;
        public string playername;
        public string itemname;
        public string locationname;
        public bool processed = false;
        public bool putinshop = false;

        public ArchipelagoItem(NetworkItem item)
        {
            this.Id = item.item;
            this.LocationId = item.location;
            this.playerslot = item.player;
            this.itemname = Plugin.curse.data.data.games["Hunie Pop"].idtoitem[item.item];
            if (item.player <= 0)
            {
                this.playername = "SERVER";
                this.locationname = "SERVER";
                return;
            }
            string playername = "";
            for (int i = 0; i < Plugin.curse.connected.players.Count(); i++)
            {
                if (Plugin.curse.connected.players[i].slot == item.player)
                {
                    playername = Plugin.curse.connected.players[i].name;
                    break;
                }
            }
            string game = "";
            foreach (KeyValuePair<int, NetworkSlot> v in Plugin.curse.connected.slot_info)
            {
                if (v.Value.name == playername)
                {
                    game = v.Value.game;
                    break;
                }
            }
            this.playername = playername;
            this.locationname = Plugin.curse.data.data.games[game].idtolocation[item.location];
        }

        public ArchipelagoItem()
        {
            this.Id = -1;
            this.playerslot = -1;
            this.LocationId = -1;
        }

    }

    public class ArchipelageItemList
    {
        public List<ArchipelagoItem> list = new List<ArchipelagoItem>();
        public string seed = "";
        public bool needtoreset = false;
        public int listversion = 2;

        public void add(NetworkItem netitem)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == netitem.item && list[i].playerslot == netitem.player && list[i].LocationId == netitem.location)
                {
                    Plugin.BepinLogger.LogMessage("item already in list skipping");
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem(netitem);
            list.Add(item);
            Plugin.BepinLogger.LogMessage($"item not in list adding:{Plugin.curse.data.data.games["Hunie Pop"].idtoitem[netitem.item]} from loc:{netitem.location} total items={list.Count}");
            //ArchipelagoConsole.LogMessage($"ADDED ITEM TO LIST:{item.itemname} total items={list.Count}");
        }

        public bool merge(List<ArchipelagoItem> oldlist)
        {
            for (int i = 0; i < oldlist.Count; i++)
            {
                if (list.Count == 0)
                {
                    list = oldlist;
                    ArchipelagoConsole.LogMessage(list.Count.ToString());
                    break;
                }
                if (i > list.Count)
                {
                    ArchipelagoItem tmp = new ArchipelagoItem();
                    tmp.Id = oldlist[i].Id;
                    tmp.LocationId = oldlist[i].LocationId;
                    tmp.playerslot = oldlist[i].playerslot;
                    tmp.playername = oldlist[i].playername;
                    tmp.itemname = oldlist[i].itemname;
                    tmp.locationname = oldlist[i].locationname;
                    tmp.processed = oldlist[i].processed;
                    tmp.putinshop = oldlist[i].putinshop;
                    list.Add(tmp);
                    continue;
                }
                if (list[i].Id != oldlist[i].Id && list[i].playerslot != oldlist[i].playerslot && list[i].LocationId != oldlist[i].LocationId)
                {
                    return true;
                }
                list[i].Id = oldlist[i].Id;
                list[i].LocationId = oldlist[i].LocationId;
                list[i].playerslot = oldlist[i].playerslot;
                list[i].playername = oldlist[i].playername;
                list[i].itemname = oldlist[i].itemname;
                list[i].locationname = oldlist[i].locationname;
                list[i].processed = oldlist[i].processed;
                list[i].putinshop = oldlist[i].putinshop;
            }
            return false;
        }

        public bool hasitem(int flag)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == flag)
                {
                    return true;
                }
            }
            return false;
        }

        public string listprint()
        {
            string output = "";
            output += "-------------\n";
            for (int i = 0; i < list.Count; i++)
            {
                output += $"I:{i}";
                output += $"ID:{list[i].Id} PLAYER:{list[i].playerslot} LOC:{list[i].LocationId}\n";
                output += $"PROCESSED:{list[i].processed} PUTINSHOP:{list[i].putinshop}\n";
            }
            return output;
        }

        public bool needprocessing()
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].processed) { return true; }
            }
            return false;
        }
    }
}
