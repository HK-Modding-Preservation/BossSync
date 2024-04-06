using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace BossSync
{
    public class BossSync : Mod
    {
        internal static BossSync Instance;
        new public string GetName() => "BossSync";
        public override string GetVersion() => "v1";

        public override void Initialize()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
            ModHooks.OnReceiveDeathEventHook += OnEnemyDeath;
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

            if(Input.GetKeyDown(KeyCode.P))
            {
                Log("falseKnightDefeated = " + PlayerData.instance.falseKnightDefeated);
                Log("killedFalseKnight = " + PlayerData.instance.killedFalseKnight);
                Log("killsFalseKnight = " + PlayerData.instance.killsFalseKnight);
                Log("newDataFalseKnight = " + PlayerData.instance.newDataFalseKnight);
            }

            if(Input.GetKeyDown(KeyCode.I))
            {
                PlayerData.instance.SetBool("falseKnightDefeated", true);
                PlayerData.instance.SetBool("killedFalseKnight", true);
                PlayerData.instance.SetInt("killsFalseKnight", 0);
                PlayerData.instance.SetBool("newDataFalseKnight", true);
            }
        }

    }
}