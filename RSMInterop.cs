using System.Collections.Generic;
using MenuChanger.MenuElements;
using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using BossTracker;


// More or less copied from RandoVanillaTracker

namespace BossTracker
{
    internal static class RSMInterop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new RVTSettingsProxy());
        }
    }

    internal class RVTSettingsProxy : RandoSettingsProxy<GlobalSettings, string>
    {
        public override string ModKey => BossTracker.Instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new EqualityVersioningPolicy<string>(BossTracker.Instance.GetVersion());

        public override void ReceiveSettings(GlobalSettings settings)
        {
            if (settings is not null)
            {
                HashSet<string> invalidPools = new();

                // Validate interop settings
                foreach (KeyValuePair<string, bool> kvp in settings.trackInteropPool)
                {
                    ToggleButton button = Menu.Instance.btInteropButtons.Find(button => button.Name == kvp.Key);
                    if (button is null && kvp.Value)
                    {
                        invalidPools.Add(kvp.Key);
                    }
                }

                if (invalidPools.Count > 0)
                {
                    throw new ValidationException($"Connection mods are missing for the following pool settings in BossTracker: {string.Join(", ", invalidPools)}");
                }

                // Set vanilla settings
                Menu.Instance.btMEF.SetMenuValues(settings);

                // Set interop settings
                foreach (ToggleButton b in Menu.Instance.btInteropButtons)
                {
                    if (settings.trackInteropPool.TryGetValue(b.Name, out bool value))
                    {
                        // This should be true if the settings were properly provided, but just in case...
                        b.SetValue(value);
                    }
                    else
                    {
                        b.SetValue(false);
                    }
                }
            }
            else
            {
                // Turn off everything except for pools from interop mods that aren't currently registered/installed
                // (and therefore are left alone to preserve their setting)
                foreach (IValueElement e in Menu.Instance.btMEF.Elements)
                {
                    e.SetValue(false);
                }

                foreach (ToggleButton b in Menu.Instance.btInteropButtons)
                {
                    b.SetValue(false);
                }
            }
        }

        public override bool TryProvideSettings(out GlobalSettings settings)
        {
            settings = GlobalSettings.MinimalClone(BossTracker.GS);
            return settings.AnyEnabled();
        }
    }
}