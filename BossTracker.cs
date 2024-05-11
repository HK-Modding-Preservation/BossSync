using Modding;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace BossTracker
{
    public class BossTracker : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static BossTracker Instance;
        new public string GetName() => "BossSync";
        public override string GetVersion() => "1.0.1";

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings gs) => GS = gs;
        public GlobalSettings OnSaveGlobal() => GS;

        internal Dictionary<string, Func<List<VanillaDef>>> Interops = new();

        public override void Initialize()
        {
            if (ModHooks.GetMod("Randomizer 4") is not Mod) return;

            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.OnReceiveDeathEventHook += OnEnemyDeath;
            Menu.Hook();
        }

        private void OnEnemyDeath(EnemyDeathEffects enemyDeathEffects, bool eventAlreadyReceived, ref float? attackDirection, ref bool resetDeathEvent, ref bool spellBurn, ref bool isWatery)
        {
            if (eventAlreadyReceived) return;

            Log("enemyDeathEffects name: " + enemyDeathEffects.name);
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
                Log("falseKnightDefeated = " + PlayerData.instance.falseKnightDefeated);
                Log("killedFalseKnight = " + PlayerData.instance.killedFalseKnight);
                Log("killsFalseKnight = " + PlayerData.instance.killsFalseKnight);
                Log("newDataFalseKnight = " + PlayerData.instance.newDataFalseKnight);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                PlayerData.instance.SetBool("falseKnightDefeated", true);
                PlayerData.instance.SetBool("killedFalseKnight", true);
                PlayerData.instance.SetInt("killsFalseKnight", 0);
                PlayerData.instance.SetBool("newDataFalseKnight", true);
            }
        }
    }
}