using Modding;
using RandomizerMod.RandomizerData;
using System;
using System.Collections.Generic;

namespace BossTrackerMod
{
    public class BossSyncMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static BossSyncMod Instance;
        new public string GetName() => "BossSync";
        public override string GetVersion() => "1.0.1.0";

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;
        public GlobalSettings OnSaveGlobal() => GS;

        internal Dictionary<string, Func<List<VanillaDef>>> Interops = new();
        public BossSync BossSync;
        public BossSyncMod()
        {
            Instance = this;
        }
        public override void Initialize()
        {
            if (ModHooks.GetMod("ItemSyncMod") is not Mod) return;

            Interop.FindInteropMods();
            BossSync = new BossSync();
            Menu.Hook();
        }

    }
}