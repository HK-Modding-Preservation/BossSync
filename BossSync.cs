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
        internal BossSync1000 bossSync1000;
        public BossSync() : base("BossSyncMod-BossUnlock") { bossSync1000 = new(); }
        private Hook journalHook;
        protected override void OnEnterGame()
        {
            //ModHooks.OnReceiveDeathEventHook += Events_OnBossKill;

            //journalHook = new(typeof(EnemyDeathEffects).GetMethod($"orig_RecordKillForJournal",
            //    BindingFlags.Instance | BindingFlags.NonPublic), OnJournalRecord);

            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;

            var readyMetadata = ItemSyncMod.ItemSyncMod.ISSettings.readyMetadata;
            for (int playerid = 0; playerid < readyMetadata.Count; playerid++)
            {
                //foreach (var item in readyMetadata[playerid])
                //{
                //    MapSyncMod.LogDebug($"playerid {playerid} mwplayerid {ItemSyncMod.ItemSyncMod.ISSettings.MWPlayerId}");
                //    MapSyncMod.LogDebug($"key[{item.Key}] value[{item.Value}]");
                //}
                //if (playerid == ItemSyncMod.ItemSyncMod.ISSettings.MWPlayerId) continue;
                //readyMetadata[playerid].TryGetValue(nameof(MapSync), out value);
                //if (readyMetadata[playerid].ContainsKey(nameof(MapSync)))
                //if (readyMetadata[playerid].TryGetValue(nameof(MapSync), out string value))
                //{
                //    switch (value)
                //    {
                //        case "1.0.0.0-debug":
                //        case "1.0.0.0":
                //            MapSyncMod.Instance.MapSync.mapSync1000.SyncPlayers.Add(playerid);
                //            MapSyncMod.LogDebug($"addMapSyncPlayers1000 playerid[{playerid}]");
                //            break;
                //        default:
                //            MapSyncMod.Instance.MapSync.SyncPlayers.Add(playerid);
                //            MapSyncMod.LogDebug($"addMapSyncPlayers playerid[{playerid}]");
                //            break;
                //    }
                //}
                BossTrackerMod.Instance.BossSync.SyncPlayers.Add(playerid);
                BossTrackerMod.Instance.Log($"addBossSyncPlayers playerid[{playerid}]");
            }
        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);

            BossTrackerMod.Instance.Log("Bool name: " + self.boolName.Value + ":" + self.value.Value);
            BossTrackerMod.Instance.Log(" " + self.boolName);

            // this hook handles most special cases (i.e. bosses) that unconditionally set the killedX PD bool to grant the journal.
            bool settingKilledTrue = self.boolName.Value.StartsWith("killed") && self.value.Value;

            if(!settingKilledTrue) 
            {
                return;
            }
            BossTrackerMod.Instance.Log(ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
            //if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
            foreach (var toPlayerId in SyncPlayers)
            {
                BossTrackerMod.Instance.Log($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");
                ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                        JsonConvert.SerializeObject(self.boolName.Value),
                        toPlayerId);
                BossTrackerMod.Instance.Log($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");
            }
        }

        protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            string boolName = JsonConvert.DeserializeObject<string>(dataReceivedEvent.Content);
            BossTrackerMod.Instance.Log($"BossSync get Boss Kill[{boolName}] true\n     form[{dataReceivedEvent.From}]");

            PlayerData.instance.SetBool(boolName, true);

            PlayerData.instance.SetBool("falseKnightDefeated", true);
            //PlayerData.instance.SetBool("killedFalseKnight", true);
            //PlayerData.instance.SetInt("killsFalseKnight", 0);
            //PlayerData.instance.SetBool("newDataFalseKnight", true);
        }

        private void Events_OnBossKill(EnemyDeathEffects enemyDeathEffects, bool eventAlreadyReceived, ref float? attackDirection, ref bool resetDeathEvent, ref bool spellBurn, ref bool isWatery)
        {
            if (eventAlreadyReceived) return;

            BossTrackerMod.Instance.Log("enemyDeathEffects name: " + enemyDeathEffects.name);
        }

        private void OnJournalRecord(Action<EnemyDeathEffects> orig, EnemyDeathEffects self)
        {
            BossTrackerMod.Instance.Log("enemyDeathEffects name: " + self.name);
            BossTrackerMod.Instance.Log("enemyDeathEffects ACTION: " + orig);
        }
    }


}