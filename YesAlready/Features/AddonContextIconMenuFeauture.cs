using System;

using ClickLib.Clicks;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using YesAlready.BaseFeatures;

namespace YesAlready.Features
{
    /// <summary>
    /// AddonContextIconMenu feature.
    /// </summary>
    internal class AddonContextIconMenuFeauture : IBaseFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonContextIconMenuFeauture"/> class.
        /// </summary>
        public AddonContextIconMenuFeauture()
        {
            Service.Framework.Update += this.OnUpdate;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Service.Framework.Update -= this.OnUpdate;
        }

        private void OnUpdate(Framework framework)
        {
            if (Service.ClientState.LocalPlayer == null)
                return;

            if (!Service.Configuration.ContextIconMenuEnabled)
                return;

            try
            {
                unsafe
                {
                    var addon = (AddonContextIconMenu*)Service.GameGui.GetAddonByName("ContextIconMenu", 1);
                    if (addon != null && addon->AtkUnitBase.IsVisible)
                    {
                        if (addon->AtkComponentList240->ListLength == 1)
                        {
                            PluginLog.Debug($"Click context_icon_menu1");
                            ClickContextIconMenu.Using((IntPtr)addon).SelectItem1();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return;
        }
    }
}
