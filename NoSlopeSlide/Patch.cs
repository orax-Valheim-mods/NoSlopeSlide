using HarmonyLib;
using UnityEngine;

namespace NoSlopeSlide;

internal class Patch
{
    // Skip Valheim's native sliding and wall-running physics for local players.
    [HarmonyPatch(typeof(Character), "ApplySlide")]
    private class Character_ApplySlide_Patch
    {
        private static bool Prefix(Character __instance)
        {
            if (!NoSlopeSlide.Enabled.Value)
                return true;

            return __instance == null || !__instance.IsPlayer();
        }
    }

    // Counteract slope gravity force when player is stationary.
    [HarmonyPatch(typeof(Character), "UpdateMotion")]
    private class Character_UpdateMotion_Patch
    {
        private static void Prefix(Character __instance)
        {
            if (!NoSlopeSlide.Enabled.Value)
                return;
            if (__instance == null || __instance != Player.m_localPlayer)
                return;

            // Apply compensation only when grounded and idle.
            if (
                __instance.GetMoveDir() != Vector3.zero
                || !__instance.IsOnGround()
                || __instance.IsSwimming()
                || __instance.IsAttached()
                || __instance.IsRiding()
                || __instance.IsDead()
                || __instance.GetStandingOnShip() != null
            )
            {
                return;
            }

            Rigidbody body = __instance.GetComponent<Rigidbody>();
            Vector3 groundNormal = __instance.m_lastGroundNormal;
            if (body == null || groundNormal == null)
                return;

            // Prevent anti-gravity launch when ground normal vector is invalid.
            if (groundNormal.sqrMagnitude < 0.5f)
                return;

            groundNormal.Normalize();

            // Isolate the gravity component acting along the slope.
            Vector3 slopeGravity = Vector3.ProjectOnPlane(Physics.gravity, groundNormal);

            // Apply opposing force to cancel downhill acceleration.
            body.AddForce(-slopeGravity, ForceMode.Acceleration);
        }
    }
}
