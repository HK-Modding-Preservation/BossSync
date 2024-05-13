using Modding;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Mono.Security.X509.X520;
using UObject = UnityEngine.Object;

namespace BossTrackerMod
{
    public class BossTrackerMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static BossTrackerMod Instance;
        public BossTrackerMod()
        {
            Instance = this;
        }
        new public string GetName() => "BossSync";
        public override string GetVersion() => "1.0.0.0";

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;
        public GlobalSettings OnSaveGlobal() => GS;

        internal Dictionary<string, Func<List<VanillaDef>>> Interops = new();

        public BossSync BossSync;

        public override void Initialize()
        {
           // if (ModHooks.GetMod("ItemSync") is not Mod) return;

            BossSync = new BossSync();

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            Menu.Hook();
        }

        private void LogAllUnlockedBossScenes()
        {
            Log("Boss Scenes Unlocked:");
            foreach(var bossScene in PlayerData.instance.unlockedBossScenes)
            {
                Log(bossScene);
            }
        }

        public void OnHeroUpdate()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                LogAllUnlockedBossScenes();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                LogFalseKnight();
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                KillFalseKnight();
            }
        }

        internal void LogFalseKnight()
        {
            //Log("falseKnightDefeated = " + PlayerData.instance.falseKnightDefeated);
            //Log("killedFalseKnight = " + PlayerData.instance.killedFalseKnight);
            //Log("killsFalseKnight = " + PlayerData.instance.killsFalseKnight);
            //Log("newDataFalseKnight = " + PlayerData.instance.newDataFalseKnight);
            //Log("newDataBlackKnight = " + PlayerData.instance.newDataBlackKnight);
            Log("unlockedBossScenes: ");
            foreach (string name in PlayerData.instance.unlockedBossScenes)
            {
                Log(name);
            }
        }

        internal void KillFalseKnight()
        {
            //PlayerData.instance.SetBool("falseKnightDefeated", true);
            //PlayerData.instance.SetBool("killedFalseKnight", true);
            //PlayerData.instance.SetInt("killsFalseKnight", 0);
            //PlayerData.instance.SetBool("newDataFalseKnight", true);
            PlayerData.instance.SetBool("killedBlackKnight", true);
            PlayerData.instance.unlockedBossScenes.Add("Watcher Knights Boss Scene");
        }

        internal void CustomLog(string log)
        {
            Log(log);
        }
    }
}