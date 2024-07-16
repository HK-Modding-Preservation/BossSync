using System;
using MultiWorldLib;
using Newtonsoft.Json;
using ItemChanger.UIDefs;
using ItemChanger;
using HutongGames.PlayMaker.Actions;
using System.Collections.Generic;

namespace BossTrackerMod
{
    public class BreakableSync : BaseSync
    {
        public BreakableSync() : base("BossTrackerMod-BreakableSync") { }
        internal List<string> prevDisplayedNames = new List<string>();

        protected override void OnEnterGame()
        {
            if (!BossSyncMod.GS.BreakableWallsAndFloors)
            {
                //BossSyncMod.Instance.Log("Bosses Sync not enabled");
                return;
            }

            On.SceneData.SaveMyState_PersistentBoolData += OnPersistentBoolAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;

            prevDisplayedNames = new List<string>();
        }

        protected override void OnQuitToMenu()
        {
            On.SceneData.SaveMyState_PersistentBoolData -= OnPersistentBoolAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter -= OnSetPlayerDataBoolAction;
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);

            BossSyncMod.Instance.Log("BREAKABLEWALL" + self.boolName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingWallTrue = self.boolName.Value.ToLower().Contains("wall") && self.value.Value;
            bool settingArchiveTrue = self.boolName.Value.ToLower().Contains("onewayarchive") && self.value.Value;
            BossSyncMod.Instance.Log("BREAKABLEWALL" + settingWallTrue);
            BossSyncMod.Instance.Log("BREAKABLEWALL" + settingArchiveTrue);
            if (!settingWallTrue && !settingArchiveTrue)
            {
                return;
            }
            if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            foreach (var toPlayerId in BossSyncMod.Instance.BossSync.SyncPlayers)
            {
                // Sync player data for any defeated/killed bools
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject("b." + self.boolName.Value),
                        toPlayerId);
                BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");


            }
        }


        private void OnPersistentBoolAction(On.SceneData.orig_SaveMyState_PersistentBoolData orig, SceneData self, PersistentBoolData persistentBoolData)
        {
            orig(self, persistentBoolData);

           

            bool breakableWallTrue = (persistentBoolData.id.ToLower().Contains("wall") || persistentBoolData.id.ToLower().Contains("floor")) && persistentBoolData.activated;
            BossSyncMod.Instance.Log("BREAKABLEWALL" + breakableWallTrue);
            // Searches through all id names to find a match
            if (!breakableWallTrue)
            {
                return;
            }
            BossSyncMod.Instance.Log("BREAKABLEWALLISCONNECTED?" + ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
            if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            BossSyncMod.Instance.Log("BREAKABLEWALL Sync Players" + BossSyncMod.Instance.BossSync.SyncPlayers);
            foreach (var toPlayerId in BossSyncMod.Instance.BossSync.SyncPlayers)
            {
                BossSyncMod.Instance.Log("BREAKABLEWALL PLAYER");
                // Sync player data for any defeated/killed bools 
                // uses "." as a delimeter to separate the sceneName from the id 
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject(persistentBoolData.id + "." + persistentBoolData.sceneName + "." + persistentBoolData.semiPersistent),
                        toPlayerId);
                BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");

            }

        }


        protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            string dataName = JsonConvert.DeserializeObject<string>(dataReceivedEvent.Content);
            BossSyncMod.Instance.Log($"BREAKABLEWALL get Data[{dataName}] true\n     from[{dataReceivedEvent.From}]");

            if (dataName.StartsWith("b."))
            {
                // Get rid of "b." identifier
                dataName = dataName.Substring(2);
                PlayerData.instance.SetBool(dataName, true);
                DisplayItem(dataName);
            }
            else
            {
                AddPersistentBoolItem(dataName);
            }
        }

        private void AddPersistentBoolItem(string dataName)
        {



            string id = dataName.Substring(0, dataName.IndexOf("."));
            dataName = dataName.Substring(dataName.IndexOf(".") + 1);
            string sceneName = dataName.Substring(0, dataName.IndexOf("."));
            dataName = dataName.Substring(dataName.IndexOf(".") + 1);
            bool semiPersistent;

            if (dataName.StartsWith("t"))
            {
                semiPersistent = true;
            }
            else
            {
                semiPersistent = false;
            }

            bool found = false;
            foreach (var item in GameManager.instance.sceneData.persistentBoolItems)
            {
                if (item.id == id)
                {
                    // if the item exists and is not already true, set activated value
                    if (item.activated) return;
                    item.activated = true;
                    found = true;
                    break;
                }
            }
            // otherwise create the new item
            if (!found)
            {
                PersistentBoolData battleControlData = new PersistentBoolData();
                battleControlData.id = id;
                battleControlData.sceneName = sceneName;
                battleControlData.activated = true;
                battleControlData.semiPersistent = semiPersistent;

                GameManager.instance.sceneData.persistentBoolItems.Add(battleControlData);
            }


            DisplayItem(id);
            
        }


        private void DisplayItem(string name)
        {
            foreach (string prevName in prevDisplayedNames)
            {
                if (prevName == name)
                {
                    return;
                }
            }

            prevDisplayedNames.Add(name);

            MsgUIDef msgUIDef = new MsgUIDef();
            msgUIDef.sprite = new ItemChangerSprite("ShopIcons.Marker_B");
            msgUIDef.name = new BoxedString(name);
            msgUIDef.SendMessage(MessageType.Corner, null);

            RecentItemsDisplay.ItemDisplayMethods.ShowItemInternal(msgUIDef, name);
        }
    }
}
