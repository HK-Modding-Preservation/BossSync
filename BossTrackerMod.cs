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
        new public string GetName() => "BossSync";
        public override string GetVersion() => "1.0.0.1";

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;
        public GlobalSettings OnSaveGlobal() => GS;

        internal Dictionary<string, Func<List<VanillaDef>>> Interops = new();
        public BossSync BossSync;
        public BossTrackerMod()
        {
            Instance = this;
        }
        public override void Initialize()
        {
           // if (ModHooks.GetMod("ItemSync") is not Mod) return;

            BossSync = new BossSync();

            Menu.Hook();
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
            PlayerData.instance.xeroDefeated = 2;
            //PlayerData.instance.SetBool("newDataFalseKnight", true);
            //PlayerData.instance.SetBool("killedBlackKnight", true);
            //PlayerData.instance.unlockedBossScenes.Add("Xero Boss Scene");
        }

        internal void CustomLog(string log)
        {
            Log(log);
        }
    }
}