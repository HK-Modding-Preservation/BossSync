

//using IL;
//using MultiWorldLib;
//using Newtonsoft.Json;
//using On;
//using System;
//using System.Collections.Generic;

//namespace BossTrackerMod
//{
//    public class CompletionSync : BaseSync
//    {
//        public CompletionSync() : base("BossTrackerMod-CompletionSync") { }

//        protected override void OnEnterGame()
//        {
//            if (!BossSyncMod.GS.SyncCompletionPercent)
//            {
//                //BossSyncMod.Instance.Log("Bosses Sync not enabled");
//                return;
//            }

//            GameManager.instance.CountGameCompletion();
//            On.HutongGames.PlayMaker.Actions.SetPlayerDataFloat.OnEnter += OnPlayerDataFloat;
//        }

//        private void OnPlayerDataFloat(On.HutongGames.PlayMaker.Actions.SetPlayerDataFloat.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SetPlayerDataFloat self)
//        {
//            orig(self);

//            if (self.floatName.Value.ToLower().Contains("completion"))
//            {
//                foreach (var toPlayerId in SyncPlayers)
//                {
//                    // Sync player data for any defeated/killed bools 
//                    // uses "." as a delimeter to separate the sceneName from the id 
//                    ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
//                            JsonConvert.SerializeObject(self.floatName.Value + "," + self.value.Value),
//                            toPlayerId);
//                    BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");
//                }
//            }
//        }


//        protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
//        {


//            string dataName = JsonConvert.DeserializeObject<string>(dataReceivedEvent.Content);

//            float dataValue = float.Parse(dataName.Substring(dataName.IndexOf(",") + 1));
//            dataName = dataName.Substring(0, dataName.IndexOf(","));

//            BossSyncMod.Instance.Log($"BossSync get Data[{dataName}, {dataValue}] true\n     from[{dataReceivedEvent.From}]");

//            PlayerData.instance.SetFloat(dataName, dataValue);
//        }

//    }
//}