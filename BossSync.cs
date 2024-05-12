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

namespace BossTrackerMod
{
    // Copied for the most part from MapSync mod
    public class BossSync : BaseSync
    {
        internal BossSync1000 bossSync1000;
        public BossSync() : base("BossSyncMod-BossUnlock") { bossSync1000 = new(); }
        protected override void OnEnterGame()
        {
            ModHooks.OnReceiveDeathEventHook += Events_OnBossKill;
            //if (Interop.HasRecentItemsDisplay())
            //    RecentItemsDisplay.ItemDisplayMethods.ShowItemInternal(new ItemChanger.UIDefs.MsgUIDef() { sprite = new ItemChangerSprite("ShopIcons.Marker_B") },
            //        $"{"Bench Sync".L()} {(MapSyncMod.GS.BenchSync ? "Enabled".L() : "Disabled".L())}");
        }
        private void Events_OnBossKill(EnemyDeathEffects enemyDeathEffects, bool eventAlreadyReceived, ref float? attackDirection, ref bool resetDeathEvent, ref bool spellBurn, ref bool isWatery)
        {
            if (eventAlreadyReceived) return;

            BossTrackerMod.Instance.Log("enemyDeathEffects name: " + enemyDeathEffects.name);
        }
    }


}