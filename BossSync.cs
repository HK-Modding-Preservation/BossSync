using BossTrackerMod;
using ItemSyncMod;
using ItemSyncMod.SyncFeatures.TransitionsFoundSync;
using MapChanger;
using Modding;
using MultiWorldLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod.RuntimeDetour;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MenuChanger.MenuElements;
using ItemChanger;
using ItemChanger.Modules;


namespace BossTrackerMod
{
    
    public class BossSync : BaseSync
    {
        public BossSync() : base("BossTrackerMod-BossUnlock") { }
        protected override void OnEnterGame()
        {
            //Get button value
            if (Menu.Instance.btButtons[0].Value == false)
            {
                BossTrackerMod.Instance.Log("Bosses Not Synced");
                return;
            }

            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
            //On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter += OnSetPlayerDataStringAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += OnSetPlayerDataIntAction;
            On.SceneData.SaveMyState_PersistentBoolData += OnPersistentBoolAction;
            //On.SceneData.FindPersistentBoolItemInList += FindBoolItem;
            //On.SceneData.FindMyState_PersistentBoolData += FindBoolItemState;
            var readyMetadata = ItemSyncMod.ItemSyncMod.ISSettings.readyMetadata;
            for (int playerid = 0; playerid < readyMetadata.Count; playerid++)
            {
                BossTrackerMod.Instance.BossSync.SyncPlayers.Add(playerid);
                BossTrackerMod.Instance.Log($"addBossSyncPlayers playerid[{playerid}]");
            }
        }

        protected override void OnQuitToMenu()
        {
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= OnSetPlayerDataBoolAction;
            //On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter -= OnSetPlayerDataStringAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter -= OnSetPlayerDataIntAction;
            On.SceneData.SaveMyState_PersistentBoolData -= OnPersistentBoolAction;
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);

            BossTrackerMod.Instance.Log("Bool name: " + self.boolName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingKilledTrue = self.boolName.Value.ToLower().Contains("kill") && self.value.Value;
            bool settingDefeatedTrue = self.boolName.Value.ToLower().Contains("defeat") && self.value.Value;
            bool zoteRescued = self.boolName.Value.ToLower().Contains("zoterescued") && self.value.Value;
            if (!settingKilledTrue && !settingDefeatedTrue) 
            {
                return;
            }
            BossTrackerMod.Instance.Log(ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
            if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            foreach (var toPlayerId in SyncPlayers)
            {
                // Sync player data for any defeated/killed bools
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject("b." + self.boolName.Value),
                        toPlayerId);
                BossTrackerMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");

                
            }
        }
        private void OnSetPlayerDataStringAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataString.orig_OnEnter orig, SetPlayerDataString self)
        {
            orig(self);
            //BossTrackerMod.Instance.Log("String variable name: " + self.stringName.Value + ":" + self.value.Value);
            // After every sync, sync the player's unlockedBossScenes
            //string[] unlockedBossScenes = PlayerData.instance.GetVariable<string[]>("unlockedBossScenes");
            //ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
            //       JsonConvert.SerializeObject(unlockedBossScenes),
            //       toPlayerId);
        }

        private void OnSetPlayerDataIntAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, SetPlayerDataInt self)
        {
            orig(self);
            BossTrackerMod.Instance.Log("Int variable name: " + self.intName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingKilledTrue = self.intName.Value.ToLower().Contains("defeat") && self.value.Value == 2;
            if (!settingKilledTrue)
            {
                return;
            }
            BossTrackerMod.Instance.Log(ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
            if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            foreach (var toPlayerId in SyncPlayers)
            {
                // Sync player data for any defeated/killed bools
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject("i." + self.intName.Value),
                        toPlayerId);
                BossTrackerMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");


            }
        }

        private void OnPersistentBoolAction(On.SceneData.orig_SaveMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData)
        {
            orig(self, persistentBoolData);
            BossTrackerMod.Instance.Log("Persistent Bool" + persistentBoolData.id + ":" + persistentBoolData.activated);
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
                        BossTrackerMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");

                    }
                    break;
                }

            }

        }



        protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            //if (Interop.HasRecentItemsDisplay())
            //{
            //    RecentItemsDisplay.Export.ShowItemChangerSprite($"{getBenchNmae(benchKey)}", "ShopIcons.BenchPin");
                
            //}

                
            string dataName = JsonConvert.DeserializeObject<string>(dataReceivedEvent.Content);
            BossTrackerMod.Instance.Log($"BossSync get Data[{dataName}] true\n     from[{dataReceivedEvent.From}]");


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
                        break;
                    case "Battle Scene.Crossroads_09":
                        break;

                    // Soul Warrior
                    case "Battle Scene v2.Ruins1_23":
                        break;

                    // Oblobble
                    case "Shiny Item-Pale_Ore-Colosseum.Room_Colosseum_Silver":
                        PlayerData.instance.unlockedBossScenes.Add("Oblobbles Boss Scene");
                        PlayerData.instance.SetBool("killedOblobble", true);
                        break;

                    // Broken Vessel
                    case "Camera Locks Boss.Abyss_19":
                        PlayerData.instance.unlockedBossScenes.Add("Broken Vessel Boss Scene");
                        PlayerData.instance.SetBool("killedInfectedKnight", true);
                        break;

                    // Hive knight
                    // NOTE: the trigger for syncing is linked to picking up hiveblood not actually killing hive knight
                    case "Shiny Item Stand.Hive_05":
                        PlayerData.instance.unlockedBossScenes.Add("Hive Knight Boss Scene");
                        PlayerData.instance.SetBool("killedHiveKnight", true);
                        break;

                    // Nosk
                    case "Battle Scene.Deepnest_32":
                        PlayerData.instance.unlockedBossScenes.Add("Nosk Boss Scene");
                        PlayerData.instance.SetBool("killedMimicSpider", true);
                        break;

                    // Enraged Guardian
                    case "Battle Scene.Mines_32":
                        break;
                    case "Zombie Beam Miner Rematch.Mines_32":
                        PlayerData.instance.unlockedBossScenes.Add("Crystal Guardian 2 Boss Scene");
                        break;

                    // Traitor Lord
                    case "Battle Scene.Fungus3_23":
                        PlayerData.instance.unlockedBossScenes.Add("Traitor Lord Boss Scene");
                        PlayerData.instance.SetBool("killedTraitorLord", true);
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
                    break;
                case "aladarSlugDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Gorb Boss Scene");
                    break;
                case "elderHuDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Elder Hu Boss Scene");
                    break;
                case "mumCaterpillarDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Marmu Boss Scene");
                    break;
                case "noEyesDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("No Eyes Boss Scene");
                    break;
                case "markothDefeated":
                    PlayerData.instance.SetInt(intName, 2);
                    PlayerData.instance.unlockedBossScenes.Add("Markoth Boss Scene");
                    break;
                default:
                    PlayerData.instance.SetInt(intName, 2);
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
                    break;

                // These bosses need to have their bossScenes manually unlocked
                case "megaMossChargerDefeated":
                    PlayerData.instance.unlockedBossScenes.Add("Mega Moss Charger Boss Scene");
                    break;
                case "giantFlyDefeated":
                    PlayerData.instance.unlockedBossScenes.Add("Gruz Boss Scene");
                    break;
                case "killedMawlek":
                    PlayerData.instance.unlockedBossScenes.Add("Brooding Mawlek Boss Scene");
                    break;
                case "killedInfectedKnight":
                    PlayerData.instance.unlockedBossScenes.Add("Broken Vessel Boss Scene");
                    break;
                case "killedMegaBeamMiner":
                    PlayerData.instance.unlockedBossScenes.Add("Crystal Guardian Boss Scene");
                    break;
                default:
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
    }




}