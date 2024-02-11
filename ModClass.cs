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
        new public string GetName() => "My First Mod";
        public override string GetVersion() => "v1";
        public override void Initialize()
        {
            ModHooks.HeroUpdateHook += OnHeroUpdate;
        }
        public void OnHeroUpdate()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Log("Key Pressed");
            }
        }
    }
}