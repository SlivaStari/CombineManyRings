﻿using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Harmony;
using Microsoft.Xna.Framework.Graphics;

namespace CombineManyRings
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static IMonitor ModMonitor { get; set; }
        internal new static IModHelper Helper { get; set; }

        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Helper = helper;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, EventArgs e)
        {
            var harmony = HarmonyInstance.Create("Stari.CombineManyRings");

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Objects.Ring), nameof(StardewValley.Objects.Ring.CanCombine)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CanCombine_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Objects.CombinedRing), nameof(StardewValley.Objects.CombinedRing.drawInMenu), new Type[] {typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)}),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DrawInMenu_Prefix))
            );
        }

        public static bool CanCombine_Prefix(StardewValley.Objects.Ring __instance, StardewValley.Objects.Ring ring, ref bool __result)
        {
            try
            {
                __result = true;
                if (ring is StardewValley.Objects.CombinedRing)
                {
                    foreach (StardewValley.Objects.Ring combinedRing in (ring as StardewValley.Objects.CombinedRing).combinedRings)
                    {
                        if (!__instance.CanCombine(combinedRing))
                        {
                            __result = false;
                            break;
                        }
                    }
                }
                else if (__instance is StardewValley.Objects.CombinedRing)
                {
                    foreach (StardewValley.Objects.Ring combinedRing in (__instance as StardewValley.Objects.CombinedRing).combinedRings)
                    {
                        if (!combinedRing.CanCombine(ring))
                        {
                            __result = false;
                            break;
                        }
                    }
                }
                else if (__instance.ParentSheetIndex == ring.ParentSheetIndex)
                {
                    __result = false;
                }
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                ModMonitor.Log($"Failed in {nameof(CanCombine_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
        public static bool DrawInMenu_Prefix(StardewValley.Objects.CombinedRing __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            try
            {
                if (__instance.combinedRings.Count >= 2)
                {
                    // Always use base rings as the sprites to draw. The first pair that are combined on the left hand side get used as the sprite.
                    if (__instance.combinedRings[0] is StardewValley.Objects.CombinedRing)
                    {
                        __instance.combinedRings[0].drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
                        return false; // don't run original logic
                    }
                    else if (__instance.combinedRings[1] is StardewValley.Objects.CombinedRing)
                    {
                        __instance.combinedRings[1].drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
                        return false; // don't run original logic
                    }
                }
                return true; // run original logic
            }
            catch (Exception ex)
            {
                ModMonitor.Log($"Failed in {nameof(DrawInMenu_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}