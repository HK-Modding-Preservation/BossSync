using MultiWorldLib;
using Newtonsoft.Json;
using HutongGames.PlayMaker.Actions;
using ItemChanger.UIDefs;
using ItemChanger;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using static Mono.Security.X509.X520;
using RandomizerMod.RandomizerData;

namespace BossTrackerMod
{
    public class SpeedrunSync : BaseSync
    {
        public SpeedrunSync() : base("BossTrackerMod-SpeedrunSync") { }
        private List<string> recievedMessages = new List<string>();
        private bool hasRecentItems = false;

        protected override void OnEnterGame()
        {
            //Get button value
            //if (!BossSyncMod.GS.Bosses)
            //{
            //    //BossSyncMod.Instance.Log("Bosses Sync not enabled");
            //    return;
            //}

            hasRecentItems = ModHooks.GetMod("ItemSyncMod") is Mod;

            On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.OnEnter += OnSetPlayerDataBoolAction;
            On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.OnEnter += OnSetPlayerDataIntAction;

            var readyMetadata = ItemSyncMod.ItemSyncMod.ISSettings.readyMetadata;
            for (int playerid = 0; playerid < readyMetadata.Count; playerid++)
            {
                BossSyncMod.Instance.SpeedrunSync.SyncPlayers.Add(playerid);
                BossSyncMod.Instance.Log($"addSpeedrunSyncPlayers playerid[{playerid}]");
            }
        }

        private void OnSetPlayerDataIntAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataInt.orig_OnEnter orig, SetPlayerDataInt self)
        {
            orig(self);
            BossSyncMod.Instance.Log("Int variable name: " + self.intName.Value + ":" + self.value.Value);
            bool settingNail = self.intName.Value.ToLower().Contains("naildamage");
            bool settingGrimmchild = self.intName.Value.ToLower().Contains("grimmchildlevel");

            if((settingNail && BossSyncMod.GS.SyncNailDamage) || (settingGrimmchild && BossSyncMod.GS.SyncGrimmchildUpgrades))
            {
                BossSyncMod.Instance.Log(ItemSyncMod.ItemSyncMod.Connection?.IsConnected());
                if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
                foreach (var toPlayerId in SyncPlayers)
                {
                    // Sync player data for any defeated/killed bools
                    ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
                            JsonConvert.SerializeObject("i." + self.intName.Value + "." + self.value.Value),
                            toPlayerId);
                    BossSyncMod.Instance.LogDebug($"send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");


                }
            }

        }

        private void OnSetPlayerDataBoolAction(On.HutongGames.PlayMaker.Actions.SetPlayerDataBool.orig_OnEnter orig, SetPlayerDataBool self)
        {
            orig(self);

            BossSyncMod.Instance.Log("Bool name: " + self.boolName.Value + ":" + self.value.Value);

            // this hook handles most bosses
            bool settingBrumm = self.boolName.Value.ToLower().Contains("gotbrummsflame") && self.value.Value && BossSyncMod.GS.SyncGrimmchildUpgrades;
            bool settingBanishment = self.boolName.Value.ToLower().Contains("destroyednightmarelantern") && self.value.Value && BossSyncMod.GS.SyncGrimmchildUpgrades;
            if (!settingBrumm && !settingBanishment)
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

        }

        private void DataRecievedBool(string boolName)
        {
            if (recievedMessages.Contains(boolName)) return;

            switch(boolName)
            {
                // When banishment happens
                case "destroyedNightmareLantern":
                    recievedMessages.Add(boolName);
                    PlayerData.instance.SetBool(boolName, true);
                    PlayerData.instance.SetBool("nymmInTown", true);
                    PlayerData.instance.SetBool("divineInTown", false);
                    PlayerData.instance.SetBool("troupeInTown", false);
                    PlayerData.instance.SetBool("gotCharm_40", false);
                    PlayerData.instance.SetBool("equippedCharm_40", false);
                    break;
                default:
                    recievedMessages.Add(boolName);
                    PlayerData.instance.SetBool(boolName, true);
                    break;
            }

           
            
        }


        private void DataRecievedInt(string intName)
        {
            int intValue = int.Parse(intName.Substring(intName.IndexOf('.') + 1));
            intName = intName.Substring(0, intName.IndexOf('.'));

            if (recievedMessages.Contains(intName)) return;
            

            switch (intName)
            {
                case "nailDamage":
                    recievedMessages.Add(intName);
                    PlayerData.instance.SetInt(intName, intValue);
                    PlayerData.instance.SetBool("nailHoned", true);
                    if (intValue == 9)
                    {
                        PlayerData.instance.SetInt("nailSmithUpgrades", 1);
                        DisplayItem("Nail Upgrade 1");
                        if (!PlayerData.instance.GetBool("equippedCharm_25"))
                            PlayerData.instance.SetInt("beamDamage", 4);
                        else
                            PlayerData.instance.SetInt("beamDamage", 7);
                    }
                    else if (intValue == 13)
                    {
                        PlayerData.instance.SetInt("nailSmithUpgrades", 2);
                        DisplayItem("Nail Upgrade 2");
                        if (!PlayerData.instance.GetBool("equippedCharm_25"))
                            PlayerData.instance.SetInt("beamDamage", 6);
                        else
                            PlayerData.instance.SetInt("beamDamage", 10);
                    }
                    else if (intValue == 17)
                    {
                        PlayerData.instance.SetInt("nailSmithUpgrades", 3);
                        DisplayItem("Nail Upgrade 3");
                        if (!PlayerData.instance.GetBool("equippedCharm_25"))
                            PlayerData.instance.SetInt("beamDamage", 8);
                        else
                            PlayerData.instance.SetInt("beamDamage", 13);
                    }
                    else if (intValue == 21)
                    {
                        PlayerData.instance.SetInt("nailSmithUpgrades", 4);
                        DisplayItem("Nail Upgrade 4");
                        if (!PlayerData.instance.GetBool("equippedCharm_25"))
                            PlayerData.instance.SetInt("beamDamage", 10);
                        else
                            PlayerData.instance.SetInt("beamDamage", 16);
                    }
                    break;
                case "grimmChildLevel":
                    {
                        PlayerData.instance.SetInt(intName, intValue);
                        DisplayItem("Grimmchild Level " + intValue);
                        break;
                    }
                default:
                    recievedMessages.Add(intName);
                    PlayerData.instance.SetInt(intName, intValue);
                    DisplayItem(intName);
                    break;
            }

        }

        private void DisplayItem(string name)
        {

            MsgUIDef msgUIDef = new MsgUIDef();
            if(name.Contains("Nail"))
            {
                msgUIDef.sprite = new ItemChangerSprite("ShopIcons.Leftslash");
            }
            else
            {
                msgUIDef.sprite = new ItemChangerSprite("Charms.40");
            }
            
            msgUIDef.name = new BoxedString(name);
            msgUIDef.SendMessage(MessageType.Corner, null);

            if (hasRecentItems)
            {
                RecentItemsDisplay.ItemDisplayMethods.ShowItemInternal(msgUIDef, name);
                //RecentItemsDisplay.Export.ShowItemChangerSprite($"{name}\n from {dataReceivedEvent.From}", "ShopIcons.BenchPin");
                //RecentItemsDisplay.Export.ShowItemChangerSprite(name, "ShopIcons.Marker_B");
            }
        }
    }
}

