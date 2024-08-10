﻿using HarmonyLib;
using UniLinq;
using UnityEngine;

namespace ImmersiveCrosshair.Harmony
{
    public class ImmersiveCrosshair
    {
        private static ILogger _logger = new Logger();
        public const float MinimumInteractableDistance = 2.4f;

        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static void ApplyPatch(IEntityPlayerLocal entityPlayerLocal)
        {
            if (entityPlayerLocal == null) return;

            var playerUI = entityPlayerLocal.playerUI;
            var hud = playerUI.GetComponentInChildren<IGuiWdwInGameHUD>();

            if (hud == null) return;

            var holdingItem = entityPlayerLocal.inventory.holdingItemItemValue;
            var actions = holdingItem?.ItemClass?.Actions;

            if (!entityPlayerLocal.bFirstPersonView)
            {
                hud.showCrosshair = true;
                return;
            }

            var holdsInteractable = actions?.Any(action => action.IsTool) ?? false;

            if (!holdsInteractable)
            {
                hud.showCrosshair = false;
                return;
            }

            var hitInfo = entityPlayerLocal.HitInfo;

            if (hitInfo == null) return;

            var hasInteractable =
                hitInfo.bHitValid && Mathf.Sqrt(hitInfo.hit.distanceSq) <= MinimumInteractableDistance;

            hud.showCrosshair = hasInteractable;
        }


        public class Init : IModApi
        {
            public void InitMod(Mod modInstance)
            {
                var type = GetType();
                var message = type.ToString();
                _logger.Info("Loading Patch: " + message);
                var harmony = new HarmonyLib.Harmony(message);
                harmony.PatchAll();
            }
        }

        [HarmonyPatch(typeof(EntityPlayerLocal), "Update")]
        public static class Update
        {
            public static void Postfix(EntityPlayerLocal __instance)
            {
                var entityPlayerLocalWrapper = new EntityPlayerLocalWrapper(__instance);
                ApplyPatch(entityPlayerLocalWrapper);
            }
        }
    }
}