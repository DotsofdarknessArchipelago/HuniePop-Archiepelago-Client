using System;
using System.Collections.Generic;
using System.Linq;

namespace HuniePopArchiepelagoClient.ArchipelagoPackets
{
    public class RoomInfoPacket
    {
        public string Cmd;
        public NetworkVersion version;
        public NetworkVersion generatorversion;
        public List<string> tags;
        public bool password;
        public Dictionary<string, int> permissions;
        public int hint_cost;
        public int location_check_points;
        public List<string> games;
        public Dictionary<string, string> datapackage_checksums;
        public string seed_name;
        public float time;
    }
    public class ConnectionRefusedPacket
    {
        public string Cmd;
        public List<string> errors;
    }
    public class ConnectedPacket
    {
        public string Cmd;
        public int team;
        public int slot;
        public List<NetworkPlayer> players;
        public List<int> missing_locations;
        public List<int> checked_locaions;
        public Dictionary<string, object> slot_data;
        public Dictionary<int, NetworkSlot> slot_info;
        public int hint_points;
    }
    public class ReceivedItemsPacket
    {
        public string Cmd;
        public int index;
        public List<NetworkItem> items;
    }
    public class LocationInfoPacket
    {
        public string Cmd;
        public List<NetworkItem> locations;
    }
    public class RoomUpdatePacket
    {
        public string Cmd;

        public NetworkVersion version;
        public NetworkVersion generatorversion;
        public List<string> tags;
        public bool password;
        public Dictionary<string, int> permissions;
        public int hint_cost;
        public int location_check_points;
        public List<string> games;
        public Dictionary<string, string> datapackage_checksums;
        public string seed_name;
        public float time;

        public int team;
        public int slot;
        public List<NetworkPlayer> players;
        public List<int> missing_locations;
        public List<int> checked_locaions;
        public Dictionary<string, object> slotdata;
        public Dictionary<int, NetworkSlot> slot_info;
        public int hint_points;

        public void update()
        {
            if (this.checked_locaions != null)
            {
                foreach (int i in this.checked_locaions)
                {
                    if (!Plugin.curse.connected.checked_locaions.Contains(i))
                    {
                        Plugin.curse.connected.checked_locaions.Add(i);
                    }
                }
            }
            if(this.hint_points != null)
            {
                Plugin.curse.connected.hint_points = this.hint_points;
            }
        }
    }
    public class PrintJSONPacket
    {
        public string Cmd;
        public List<JsonMessagePart> data;
        public string type;
        public int recieving;
        public NetworkItem item;
        public bool found;
        public int team;
        public int slot;
        public string message;
        public List<string> tags;
        public int countdown;

        public string print()
        {
            string output = "";

            foreach (JsonMessagePart part in data)
            {
                output += part.print();
            }
            return output;
        }
    }
    public class DataPackagePacket
    {
        public string Cmd;
        public datapackageobject data;
    }
    public class datapackageobject
    {
        public Dictionary<string, GameData> games;

        public void setup()
        {
            foreach (KeyValuePair<string, GameData> i in games)
            {
                i.Value.setup();
            }
        }
    }

    public class BouncedPacket
    {
        public string Cmd;
    }
    public class InvalidPacketPacket
    {
        public string Cmd;
    }
    public class RetrievedPacket
    {
        public string Cmd;
    }
    public class SetReplyPacket
    {
        public string Cmd;
    }

    public class NetworkVersion
    {
        public int major = 0;
        public int minor = 0;
        public int build = 0;
        public string Class;
    }
    public class NetworkPlayer
    {
        public int team;
        public int slot;
        public string alias;
        public string name;
    }
    public class NetworkItem
    {
        public long item;
        public long location;
        public int player;
        public int flags;
    }
    public class NetworkSlot
    {
        public string name;
        public string game;
        public int type;
        public List<int> group_members;
    }
    public class JsonMessagePart
    {
        public string type;
        public string text;
        public string color;
        public int flags;
        public int player;
        public int hintstatus;

        public string print()
        {
            if (type == "player_id")
            {
                for (int i = 0; i < Plugin.curse.connected.players.Count(); i++)
                {
                    if (Plugin.curse.connected.players[i].slot.ToString() == text)
                    {
                        return Plugin.curse.connected.players[i].name;
                    }
                }
            }
            else if (type == "player_name")
            {
                //???
                return text;
            }
            else if (type == "item_id")
            {
                string playername = "";
                for (int i = 0; i < Plugin.curse.connected.players.Count(); i++)
                {
                    if (Plugin.curse.connected.players[i].slot == player)
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
                return Plugin.curse.data.data.games[game].idtoitem[Convert.ToInt32(text)];
            }
            else if (type == "item_name")
            {
                //???
                return text;
            }
            else if (type == "location_id")
            {
                string playername = "";
                for (int i = 0; i < Plugin.curse.connected.players.Count(); i++)
                {
                    if (Plugin.curse.connected.players[i].slot == player)
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
                return Plugin.curse.data.data.games[game].idtolocation[Convert.ToInt32(text)];
            }
            else if (type == "location_name")
            {
                //???
                return text;
            }
            else if (type == "entrance_name")
            {
                //???
                return text;
            }
            else if (type == "hint_status")
            {
                //???
                return text;
            }
            else if (type == "color")
            {
                //???
                return text;
            }
            return text;
        }
    }
    public class GameData
    {
        public Dictionary<string, long> item_name_to_id;
        public Dictionary<string, long> location_name_to_id;
        public Dictionary<long, string> idtoitem;
        public Dictionary<long, string> idtolocation;
        public string checksum;

        public void setup()
        {
            Dictionary<long, string> tmpi = new Dictionary<long, string>();
            Dictionary<long, string> tmpl = new Dictionary<long, string>();
            foreach (KeyValuePair<string, long> i in item_name_to_id)
            {
                tmpi.Add(i.Value, i.Key);
            }
            foreach (KeyValuePair<string, long> l in location_name_to_id)
            {
                tmpl.Add(l.Value, l.Key);
            }
            idtoitem = tmpi;
            idtolocation = tmpl;
        }
    }
}

