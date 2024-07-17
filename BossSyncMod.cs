using Modding;
using RandomizerMod.RandomizerData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BossTrackerMod
{
    public class BossSyncMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static BossSyncMod Instance;
        new public string GetName() => "BossSync";
        public override string GetVersion() => "This mod is obsolete, install MapSyncMod and/or SpeedrunSync instead";

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;
        public GlobalSettings OnSaveGlobal() => GS;

        internal Dictionary<string, Func<List<VanillaDef>>> Interops = new();
        //public BossSync BossSync;
        //public CompletionSync CompletionSync;
        //public BreakableSync BreakableSync;
        public SpeedrunSync SpeedrunSync;
        public BossSyncMod()
        {
            Instance = this;
        }
        public override void Initialize()
        {
            if (ModHooks.GetMod("ItemSyncMod") is not Mod) return;

            Interop.FindInteropMods();
            //BossSync = new BossSync();
            //BreakableSync = new BreakableSync();
            //CompletionSync = new CompletionSync();
            SpeedrunSync = new SpeedrunSync();
            Menu.Hook();

            //On.HeroController.Update += Update;
            //On.GameManager.CountGameCompletion += Test;
            //On.GameManager.SaveGame_int_Action1 += Save;
        }

        //private void Save(On.GameManager.orig_SaveGame_int_Action1 orig, GameManager self, int saveSlot, Action<bool> callback)
        //{
        //    Log("Game Saved, Save Slot: " + saveSlot);
        //    orig(self, 3, callback);
        //    Log("Count Charms" + PlayerData.instance.charmsOwned);
        //    PlayerData.instance.CountCharms();
        //    PlayerData.instance.completionPercentage = PlayerData.instance.charmsOwned;
        //    Action<SaveStats> saveStats = saveStatInstance =>
        //    {
        //        saveStatInstance.completionPercentage
        //    };
        //    GameManager.instance.GetSaveStatsForSlot(3, saveStats);
            


        //}

        //private void Test(On.GameManager.orig_CountGameCompletion orig, GameManager self)
        //{
        //    orig(self);
        //    Log("Count game completion" + self);
        //}

        //private void Test(On.PlayerData.orig_CountGameCompletion orig, PlayerData self)
        //{
        //    orig(self);
        //    Log("Count game completion" + self);
        //}

        //private void Update(On.HeroController.orig_Update orig, HeroController self)
        //{
        //    orig(self);


        //    if(Input.GetKeyDown(KeyCode.O))
        //    {
        //        Log("Completion: " + PlayerData.instance.completionPercent + " " + PlayerData.instance.completionPercentage);
        //    }

        //    if(Input.GetKeyDown(KeyCode.I))
        //    {
        //        Log("Count Charms" + PlayerData.instance.charmsOwned);
        //        PlayerData.instance.CountCharms();
        //        PlayerData.instance.completionPercentage = PlayerData.instance.charmsOwned;
        //        //PlayerData.instance.CountGameCompletion();
        //    }

        //}


    }
}