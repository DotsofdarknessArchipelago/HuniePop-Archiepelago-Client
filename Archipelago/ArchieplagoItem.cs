using System.Collections.Generic;
using System.Linq;

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
            if (this.LocationId <= 0)
            {
                this.locationname = "SERVER";
            }
            else
            {
                this.locationname = Plugin.curse.data.data.games[game].idtolocation[item.location];
            }
        }

        public ArchipelagoItem()
        {
            this.Id = -1;
            this.playerslot = -1;
            this.LocationId = -1;
        }

        public bool equals(ArchipelagoItem i2)
        {
            if(i2 == null) return false;
            if(this.Id != i2.Id) return false;
            if(this.LocationId != i2.LocationId) return false;
            if(this.playerslot != i2.playerslot) return false;
            return true;
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
                    Plugin.BepinLogger.LogMessage($"-item({Plugin.curse.data.data.games["Hunie Pop"].idtoitem[netitem.item]} from loc:{netitem.location} total items={list.Count}) already in list skipping");
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem(netitem);
            list.Add(item);
            Plugin.BepinLogger.LogMessage($"+item({Plugin.curse.data.data.games["Hunie Pop"].idtoitem[netitem.item]} from loc:{netitem.location} total items={list.Count}) not in list adding");
        }

        public bool merge(List<ArchipelagoItem> oldlist)
        {
            if (oldlist.Count == 0) { return true; }
            if (list.Count == 0) { list = oldlist; return false; }

            for (int i = 0; i < oldlist.Count; i++)
            {
                bool f = true;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].equals(oldlist[i]))
                    {
                        list[j] = oldlist[i];
                        f = false;
                        break;
                    }
                }
                if (f)
                {
                    return true;
                }
            }

            return false;
        }

        public bool hasitem(long flag)
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
