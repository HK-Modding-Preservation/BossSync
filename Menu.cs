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
using MapChanger.UI;

namespace BossTrackerMod
{
    // More or less copied from RandoVanillaTracker/flibber's RandoPlus
    internal class Menu
    {
        internal MenuPage btPage;
        internal MenuLabel btPageTitle;
        internal SmallButton btMainButton;
        internal List<ToggleButton> btButtons;
        internal MenuElementFactory<GlobalSettings> btMEF;
        internal GridItemPanel btGIP;

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

        private static BaseButton ConstructMenu(MenuPage landingPage)
        {
            Instance = new(landingPage);
            return Instance.btMainButton;
        }

        private Menu(MenuPage landingPage)
        {
            btPage = new MenuPage(Localize("Boss Tracker"), landingPage);
            btPageTitle = new MenuLabel(btPage, "Select Boss Tracker", MenuLabel.Style.Title);
            btPageTitle.MoveTo(new Vector2(0, 400));
            btMEF = new(btPage, BossSyncMod.GS);
            btButtons = new();
            btGIP = new(btPage, new Vector2(0, 300), 1, 50f, 400f, true, btMEF.Elements.Concat(btButtons).ToArray());
            ConstructButtons();
            Localize(btMEF);


            foreach (IValueElement e in btMEF.Elements)
            {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            foreach (ToggleButton b in btButtons)
            {
                b.SelfChanged += obj => SetTopLevelButtonColor();
            }

            btMainButton = new SmallButton(landingPage, Localize("Boss Sync"));
            btMainButton.AddHideAndShowEvent(landingPage, btPage);
            SetTopLevelButtonColor();
        }

        private void ConstructButtons()
        {
            
            foreach(var element in btGIP.Items)
            {
                btButtons.Add((ToggleButton)element);
            }
        }



        private void SetTopLevelButtonColor()
        {
            if (btMainButton != null)
            {
                btMainButton.Text.color = btMEF.Elements.Any(e => e.Value is true) || btButtons.Any(b => b.Value is true)
                    ? Colors.TRUE_COLOR : Colors.FALSE_COLOR;
            }
        }
    }
}