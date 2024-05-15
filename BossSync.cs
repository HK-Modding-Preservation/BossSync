using MultiWorldLib;
using Newtonsoft.Json;
using HutongGames.PlayMaker.Actions;
using ItemChanger.UIDefs;
using ItemChanger;
using System.Collections.Generic;


namespace BossTrackerMod
{
    
    public class BossSync : BaseSync
    {
        public BossSync() : base("BossTrackerMod-BossUnlock") { }
        internal List<string> displayedNames = new List<string>();
        protected override void OnEnterGame()
        {
            //Get button value
            if (Menu.Instance.btButtons[0].Value == false)
            {
                BossSyncMod.Instance.Log("Bosses Sync not enabled");
                return;
            }

            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += OnSetPlayerDataIntAction;
            On.SceneData.SaveMyState_PersistentBoolData += OnPersistentBoolAction;

            var readyMetadata = ItemSyncMod.ItemSyncMod.ISSettings.readyMetadata;
            for (int playerid = 0; playerid < readyMetadata.Count; playerid++)
            {
                BossSyncMod.Instance.BossSync.SyncPlayers.Add(playerid);
                BossSyncMod.Instance.Log($"addBossSyncPlayers playerid[{playerid}]");
            }
        }

        protected override void OnQuitToMenu()
        {
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= OnSetPlayerDataBoolAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter -= OnSetPlayerDataIntAction;
            On.SceneData.SaveMyState_PersistentBoolData -= OnPersistentBoolAction;
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);

            //BossSyncMod.Instance.Log("Bool name: " + self.boolName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingKilledTrue = self.boolName.Value.ToLower().Contains("kill") && self.value.Value;
            bool settingDefeatedTrue = self.boolName.Value.ToLower().Contains("defeat") && self.value.Value;
            bool zoteRescued = self.boolName.Value.ToLower().Contains("zoterescued") && self.value.Value;
            if (!settingKilledTrue && !settingDefeatedTrue && !zoteRescued) 
            {
                return;
            }
            BossSyncMod.Instance.Log(ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
            if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            foreach (var toPlayerId in SyncPlayers)
            {
                // Sync player data for any defeated/killed bools
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject("b." + self.boolName.Value),
                        toPlayerId);
                BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");

                
            }
        }

        private void OnSetPlayerDataIntAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, SetPlayerDataInt self)
        {
            orig(self);
            //BossSyncMod.Instance.Log("Int variable name: " + self.intName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingKilledTrue = self.intName.Value.ToLower().Contains("defeat") && self.value.Value == 2;
            if (!settingKilledTrue)
            {
                return;
            }
            BossSyncMod.Instance.Log(ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
            if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            foreach (var toPlayerId in SyncPlayers)
            {
                // Sync player data for any defeated/killed bools
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject("i." + self.intName.Value),
                        toPlayerId);
                BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");


            }
        }

        private void OnPersistentBoolAction(On.SceneData.orig_SaveMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData)
        {
            orig(self, persistentBoolData);
            //BossSyncMod.Instance.Log("Persistent Bool" + persistentBoolData.id + ":" + persistentBoolData.activated);
            string[] idNames = { "Mawlek Body", "Battle Scene", "Battle Scene v2", "Shiny Item-Pale_Ore-Colosseum",
                "Camera Locks Boss", "Shiny Item Stand", "Zombie Beam Miner Rematch" };

            // Searches through all id names to find a match
            foreach (var id in idNames)
            {
                if(id == persistentBoolData.id)
                {
                    if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
                    foreach (var toPlayerId in SyncPlayers)
                    {
                        // Sync player data for any defeated/killed bools 
                        // uses "." as a delimeter to separate the sceneName from the id 
                        ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                                JsonConvert.SerializeObject(id+"."+persistentBoolData.sceneName),
                                toPlayerId);
                        BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");

                    }
                    break;
                }

            }

        }



        protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            string dataName = JsonConvert.DeserializeObject<string>(dataReceivedEvent.Content);
            BossSyncMod.Instance.Log($"BossSync get Data[{dataName}] true\n     from[{dataReceivedEvent.From}]");


            if (dataName.StartsWith("b."))
            {
                // Get rid of "b." identifier
                dataName = dataName.Substring(2);
                DataRecievedBool(dataName);
            }
            else if (dataName.StartsWith("i."))
            {
                // Get rid of "i." identifier
                dataName = dataName.Substring(2);
                DataRecievedInt(dataName);
            }
            else
            {
                AddPersistentBoolItem(dataName, true, false);
                switch (dataName)
                {
                    // Brooding Mawlek Cases
                    case "Mawlek Body.Crossroads_09":
                        PlayerData.instance.unlockedBossScenes.Add("Brooding Mawlek Boss Scene");
                        DisplayItem("Mawlek");
                        break;
                    case "Battle Scene.Crossroads_09":
                        break;

                    // Soul Warrior
                    case "Battle Scene v2.Ruins1_23":
                        DisplayItem("Soul Warrior");
                        break;

                    // Oblobble
                    case "Shiny Item-Pale_Ore-Colosseum.Room_Colosseum_Silver":
                        PlayerData.instance.unlockedBossScenes.Add("Oblobbles Boss Scene");
                        PlayerData.instance.SetBool("killedOblobble", true);
                        DisplayItem("Oblobbles");
                        break;

                    // Broken Vessel
                    case "Camera Locks Boss.Abyss_19":
                        PlayerData.instance.unlockedBossScenes.Add("Broken Vessel Boss Scene");
                        PlayerData.instance.SetBool("killedInfectedKnight", true);
                        DisplayItem("Broken Vessel");
                        break;

                    // Hive knight
                    // NOTE: the trigger for syncing is linked to picking up hiveblood not actually killing hive knight
                    case "Shiny Item Stand.Hive_05":
                        PlayerData.instance.unlockedBossScenes.Add("Hive Knight Boss Scene");
                        PlayerData.instance.SetBool("killedHiveKnight", true);
                        DisplayItem("Hive Knight");
                        break;

                    // Nosk
                    case "Battle Scene.Deepnest_32":
                        PlayerData.instance.unlockedBossScenes.Add("Nosk Boss Scene");
                        PlayerData.instance.SetBool("killedMimicSpider", true);
                        DisplayItem("Nosk");
                        break;

                    // Enraged Guardian
                    case "Battle Scene.Mines_32":
                        break;
                    case "Zombie Beam Miner Rematch.Mines_32":
                        PlayerData.instance.unlockedBossScenes.Add("Crystal Guardian 2 Boss Scene");
                        DisplayItem("Enraged Guardian");
                        break;

                    // Traitor Lord
                    case "Battle Scene.Fungus3_23":
                        PlayerData.instance.unlockedBossScenes.Add("Traitor Lord Boss Scene");
                        PlayerData.instance.SetBool("killedTraitorLord", true);
                        DisplayItem("Traitor Lord");
                        break;

                    default:
                        break;
                }
            }


           
        }

        private void DataRecievedInt(string intName)
        {
            
            
            switch (intName)
            {
                // Special case for all dream warriors
                case "xeroDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Xero Boss Scene");
                    DisplayItem("Xero");
                    break;
                case "aladarSlugDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Gorb Boss Scene");
                    DisplayItem("Gorb");
                    break;
                case "elderHuDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Elder Hu Boss Scene");
                    DisplayItem("Elder Hu");
                    break;
                case "mumCaterpillarDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Marmu Boss Scene");
                    DisplayItem("Marmu");
                    break;
                case "noEyesDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("No Eyes Boss Scene");
                    DisplayItem("No Eyes");
                    break;
                case "markothDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Markoth Boss Scene");
                    DisplayItem("Markoth");
                    break;
                default:
                    PlayerData.instance.SetInt(intName, 2);
                    DisplayItem(intName);
                    break;
            }
            
        }


        private void DataRecievedBool(string boolName) 
        {
            
            switch (boolName)
            {
                // Special handling for watcher knights
                case "killedBlackKnight":
                    PlayerData.instance.unlockedBossScenes.Add("Watcher Knights Boss Scene");
                    AddPersistentBoolItem("Battle Control.Ruins2_03", true, false);
                    DisplayItem("Watcher Knights");
                    break;

                // These bosses need to have their bossScenes manually unlocked
                case "megaMossChargerDefeated":
                    PlayerData.instance.unlockedBossScenes.Add("Mega Moss Charger Boss Scene");
                    DisplayItem("Mega Moss Charger");
                    break;
                case "giantFlyDefeated":
                    PlayerData.instance.unlockedBossScenes.Add("Gruz Boss Scene");
                    DisplayItem("Gruz Mother");
                    break;
                case "killedMawlek":
                    PlayerData.instance.unlockedBossScenes.Add("Brooding Mawlek Boss Scene");
                    DisplayItem("Mawlek");
                    break;
                case "killedInfectedKnight":
                    PlayerData.instance.unlockedBossScenes.Add("Broken Vessel Boss Scene");
                    DisplayItem("Broken Vessel");
                    break;
                case "killedMegaBeamMiner":
                    PlayerData.instance.unlockedBossScenes.Add("Crystal Guardian Boss Scene");
                    DisplayItem("Crystal Guardian");
                    break;
                default:
                    DisplayItem(boolName);
                    break;
            }
            PlayerData.instance.SetBool(boolName, true);
            
        }
        
        // updates the persistent bool item (adds a new item if the item doesn't already exist)
        private void AddPersistentBoolItem(string data, bool activated, bool semiPersistent)
        {
            // Separate data from scene name
            string sceneName = data.Substring(data.IndexOf('.') + 1);
            string id = data.Substring(0, data.IndexOf('.'));



            bool found = false;
            foreach (var item in GameManager.instance.sceneData.persistentBoolItems)
            {
                if (item.id == id)
                {
                    // if the bool item exists, set activated value
                    item.activated = activated;
                    found = true;
                    break;
                }
            }
            // otherwise create the new item
            if(!found)
            {
                PersistentBoolData battleControlData = new PersistentBoolData();
                battleControlData.id = id;
                battleControlData.sceneName = sceneName;
                battleControlData.activated = activated;
                battleControlData.semiPersistent = semiPersistent;

                GameManager.instance.sceneData.persistentBoolItems.Add(battleControlData);
            }
        }

        private void DisplayItem(string name)
        {
            if(name.Contains("Battle") || name.Contains("Shiny"))
            {
                return;
            }

            // if the name contains defeated or killed, format the string
            if (name.Contains("killed"))
            {
                name = name.Replace("killed", "");

                for(int index = name.Length - 1; index >= 0; index--)
                {
                    if (char.IsUpper(name[index]))
                    {
                        name = name.Insert(index, " ");
                    }
                }
            }
            else if (name.Contains("Defeated"))
            {
                name = name.Replace("Defeated", "");

                // Places spaces before all capitalized chars (excluding the first one)
                for (int index = name.Length - 1; index >= 0; index--)
                {
                    if (char.IsUpper(name[index]))
                    {
                        name = name.Insert(index, " ");
                    }
                }
            }
            else if(name.Contains("defeated"))
            {
                name = name.Replace("defeated", "");

                // Places spaces before all capitalized chars (excluding the first one)
                for (int index = name.Length - 1; index >= 0; index--)
                {
                    if (char.IsUpper(name[index]))
                    {
                        name = name.Insert(index, " ");
                    }
                }
            }

            // remove space at beginning of string if its there
            if (name[0] == ' ')
            {
                name = name.Substring(1);
            }

            // Uppercases the first char
            name = name.Replace(name.Substring(0, 1), name.Substring(0, 1).ToUpper());

            // if the name has been previously displayed (boss has already been killed), do not display
            foreach (string prevName in displayedNames)
            {
                if(prevName == name)
                {
                    return;
                }
            }

            displayedNames.Add(name);

            name += " Defeated";

            

            MsgUIDef msgUIDef = new MsgUIDef();
            msgUIDef.sprite = new ItemChangerSprite("ShopIcons.Marker_B");
            msgUIDef.name = new BoxedString(name);
            msgUIDef.SendMessage(MessageType.Corner, null);
            TryDisplayOnRecentItems(msgUIDef, name);
        }
        private void TryDisplayOnRecentItems(MsgUIDef msgUIDef, string name)
        {
            if (Interop.HasRecentItemsDisplay())
            {
                RecentItemsDisplay.ItemDisplayMethods.ShowItemInternal(msgUIDef, name);
            }
        }
    }




}