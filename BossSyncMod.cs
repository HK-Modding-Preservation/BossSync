using Modding;
using RandomizerMod.RandomizerData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BossTrackerMod
{
    public class BossSyncMod : Mod
    {
        internal static BossSyncMod Instance;
        new public string GetName() => "BossSync";
        public override string GetVersion() => "This mod is obsolete, install MapSyncMod and/or SpeedrunSync instead";
    }
}