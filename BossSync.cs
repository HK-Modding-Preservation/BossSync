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

namespace BossTrackerMod
{
    // Copied for the most part from MapSync mod
    public class BossSync : BaseSync
    {
        public BossSync() : base("BossTrackerMod-BossUnlock") { }
        protected override void OnEnterGame()
        {

            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataString.OnEnter += OnSetPlayerDataStringAction;

            var readyMetadata = ItemSyncMod.ItemSyncMod.ISSettings.readyMetadata;
            for (int playerid = 0; playerid < readyMetadata.Count; playerid++)
            {
                BossTrackerMod.Instance.BossSync.SyncPlayers.Add(playerid);
                BossTrackerMod.Instance.Log($"addBossSyncPlayers playerid[{playerid}]");
            }
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);

            BossTrackerMod.Instance.Log("Bool name: " + self.boolName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingKilledTrue = self.boolName.Value.ToLower().Contains("killed") && self.value.Value;
            bool settingDefeatedTrue = self.boolName.Value.ToLower().Contains("defeat") && self.value.Value;
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
                        JsonConvert.SerializeObject(self.boolName.Value),
                        toPlayerId);
                BossTrackerMod.Instance.Log($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");

                
            }
        }
        private void OnSetPlayerDataStringAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataString.orig_OnEnter orig, SetPlayerDataString self)
        {
            BossTrackerMod.Instance.Log("String variable name: " + self.stringName.Value + ":" + self.value.Value);
            // After every sync, sync the player's unlockedBossScenes
            //string[] unlockedBossScenes = PlayerData.instance.GetVariable<string[]>("unlockedBossScenes");
            //ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
            //       JsonConvert.SerializeObject(unlockedBossScenes),
            //       toPlayerId);
        }



        protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            string boolName = JsonConvert.DeserializeObject<string>(dataReceivedEvent.Content);
            BossTrackerMod.Instance.Log($"BossSync get Boss Kill[{boolName}] true\n     form[{dataReceivedEvent.From}]");

            PlayerData.instance.SetBool(boolName, true);
        }
    }


}