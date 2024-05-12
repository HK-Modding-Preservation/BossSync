using ItemSyncMod;
using ItemSyncMod.SyncFeatures.TransitionsFoundSync;
using MultiWorldLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossTrackerMod
{
    public class BossSync1000 : BaseSync
    {
        public BossSync1000() : base("ItemSync-BossUnlock") { }
        //internal void Events_OnBenchUnlock(Benchwarp.BenchKey benchKey)
        //{
        //    if (ItemSyncMod.ItemSyncMod.Connection?.IsConnected() != true) return;
        //    foreach (var toPlayerId in SyncPlayers)
        //    {
        //        ItemSyncMod.ItemSyncMod.Connection.SendData(MESSAGE_LABEL,
        //                JsonConvert.SerializeObject(benchKey),
        //                toPlayerId);
        //        MapSyncMod.LogDebug($"1000send to id[{toPlayerId}] name[{ItemSyncMod.ItemSyncMod.ISSettings.GetNicknames()[toPlayerId]}]");
        //    }
        //}

        //protected override void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        //{
        //    MapSyncMod.Instance.BenchSync.OnDataReceived1000(dataReceivedEvent);
        //}
    }
}