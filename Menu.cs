using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using ItemSyncMod;
using ItemSyncMod.Connection;
using ItemSyncMod.Menu;
using MultiWorldLib;
using RandomizerMod.Menu;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static RandomizerMod.Localization;
using static UnityEngine.GridBrushBase;
using MultiWorldLib.ExportedAPI;

namespace BossTracker
{
    // More or less copied from RandoVanillaTracker/flibber's RandoPlus
    internal class Menu
    {
        internal MenuPage btPage;
        internal MenuLabel btPageTitle;
        internal MenuElementFactory<GlobalSettings> btMEF;
        internal GridItemPanel btGIP;

        internal List<ToggleButton> btInteropButtons;
        internal SmallButton JumpToBTButton;

        internal static Menu Instance { get; private set; }

        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void Hook()
        {
            ExportedExtensionsMenuAPI.AddExtensionsMenu(ConstructMenu);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        //private static bool HandleButton(MenuPage landingPage, out SmallButton button)
        //{
        //    button = Instance.JumpToBTButton;
        //    return true;
        //}

        private static BaseButton ConstructMenu(MenuPage landingPage)
        {
            Instance = new (landingPage);
            return Instance.JumpToBTButton;
        }

        private Menu(MenuPage landingPage)
        {
            btPage = new MenuPage(Localize("Boss Tracker"), landingPage);
            btPageTitle = new MenuLabel(btPage, "Select Boss Tracker", MenuLabel.Style.Title);
            btPageTitle.MoveTo(new Vector2(0, 400));
            btMEF = new (btPage, BossTracker.GS);

            //rvtMEF.ElementLookup["Charms"].SelfChanged += CostFixes.Other_SelfChanged;
            //rvtMEF.ElementLookup["Relics"].SelfChanged += CostFixes.Other_SelfChanged;
            //rvtMEF.ElementLookup["PaleOre"].SelfChanged += CostFixes.Other_SelfChanged;
            //rvtMEF.ElementLookup["RancidEggs"].SelfChanged += CostFixes.Other_SelfChanged;
            //rvtMEF.ElementLookup["MaskShards"].SelfChanged += CostFixes.Other_SelfChanged;

            ConstructInteropButtons();
            btGIP = new(btPage, new Vector2(0, 300), 4, 50f, 400f, true, btMEF.Elements.Concat(btInteropButtons).ToArray());
            Localize(btMEF);

            foreach (IValueElement e in btMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            foreach (ToggleButton b in btInteropButtons)
            {
                b.SelfChanged += obj => SetTopLevelButtonColor();
            }

            JumpToBTButton = new(landingPage, Localize("Boss Tracker"));
            JumpToBTButton.AddHideAndShowEvent(landingPage, btPage);
            SetTopLevelButtonColor();
        }

        private void ConstructInteropButtons()
        {
            btInteropButtons = new();

            ToggleButton button = new(btPage, "Track False Knight");
            btInteropButtons.Add(button);

            //foreach (string pool in RVT.Instance.Interops.Keys)
            //{
            //    ToggleButton button = new(rvtPage, pool);
            //    button.SetValue(RVT.GS.trackInteropPool[pool]);
            //    button.SelfChanged += b => RVT.GS.trackInteropPool[pool] = (bool)b.Value;

            //    rvtInteropButtons.Add(button);
            //}
        }

        private void SetTopLevelButtonColor()
        {
            if (JumpToBTButton != null)
            {
                JumpToBTButton.Text.color = btMEF.Elements.Any(e => e.Value is true) || btInteropButtons.Any(b => b.Value is true)
                    ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }
    }
}