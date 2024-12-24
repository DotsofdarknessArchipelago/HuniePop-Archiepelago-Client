using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Models;

namespace HuniePopArchiepelagoClient.Archipelago
{
    public class ArchipelagoItem
    {
        public long Id;
        public long LocationId;
        public int playerslot;
        public bool processed = false;
        public bool putinshop = false;

        public ArchipelagoItem(NetworkItem item)
        {
            this.Id = item.Item;
            this.LocationId = item.Location;
            this.playerslot = item.Player;
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
        public int listversion = 1;

        public void add(NetworkItem netitem)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == netitem.Item && list[i].playerslot == netitem.Player && list[i].LocationId == netitem.Location)
                {
                    return;
                }
            }
            ArchipelagoItem item = new ArchipelagoItem(netitem);
            list.Add(item);
        }

        public bool merge(List<ArchipelagoItem> oldlist)
        {
            for (int i = 0; i < oldlist.Count; i++)
            {
                if (list[i].Id != oldlist[i].Id && list[i].playerslot != oldlist[i].playerslot && list[i].LocationId != oldlist[i].LocationId)
                {
                    return true;
                }
                if (i >= list.Count)
                {
                    ArchipelagoItem tmp = new ArchipelagoItem();
                    tmp.Id = oldlist[i].Id;
                    tmp.playerslot = oldlist[i].playerslot;
                    tmp.LocationId = oldlist[i].LocationId;
                    tmp.processed = oldlist[i].processed;
                    tmp.putinshop = oldlist[i].putinshop;
                    list.Add(tmp);
                }
                list[i].Id = oldlist[i].Id;
                list[i].playerslot = oldlist[i].playerslot;
                list[i].LocationId = oldlist[i].LocationId;
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
