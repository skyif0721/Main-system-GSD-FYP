using UnityEngine;

/// <summary>
/// Centralised defensive-buff state. Multiple gesture buffs can run at the
/// same time (Block + Qigong), each with their own damage multiplier. We
/// always pick the STRONGEST (lowest multiplier) and write it to
/// <see cref="VRGestureResponse"/>. This avoids the bug where one buff
/// ending wipes the other buff's state.
/// </summary>
public static class BlockState
{
    /// <summary>True while a Block gesture is active (full invulnerability).</summary>
    public static bool BlockActive  = false;

    /// <summary>True while a Qigong buff is active (50% damage reduction).</summary>
    public static bool QigongActive = false;

    /// <summary>True if any defensive buff is currently active.</summary>
    public static bool AnyActive => BlockActive || QigongActive;

    /// <summary>
    /// Recompute the active block flag + damage multiplier and push it to
    /// <see cref="VRGestureResponse"/>. Block (multiplier 0) wins over
    /// Qigong (multiplier 0.5) when both are simultaneously active.
    /// </summary>
    public static void Refresh()
    {
        if (BlockActive)
        {
            VRGestureResponse.PlayerIsBlocking      = true;
            VRGestureResponse.BlockDamageMultiplier = 0f;
        }
        else if (QigongActive)
        {
            VRGestureResponse.PlayerIsBlocking      = true;
            VRGestureResponse.BlockDamageMultiplier = 0.5f;
        }
        else
        {
            VRGestureResponse.PlayerIsBlocking      = false;
            VRGestureResponse.BlockDamageMultiplier = 1f;
        }
    }

    /// <summary>Clear all defensive state. Call on scene reload / death.</summary>
    public static void Reset()
    {
        BlockActive  = false;
        QigongActive = false;
        Refresh();
    }
}
