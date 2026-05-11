using UnityEngine;

/// <summary>
/// Centralised defensive-buff state. Manages the Block gesture's
/// invulnerability and writes it to <see cref="VRGestureResponse"/>.
/// </summary>
public static class BlockState
{
    /// <summary>True while a Block gesture is active (full invulnerability).</summary>
    public static bool BlockActive  = false;

    /// <summary>True if any defensive buff is currently active.</summary>
    public static bool AnyActive => BlockActive;

    /// <summary>
    /// Recompute the active block flag + damage multiplier and push it to
    /// <see cref="VRGestureResponse"/>.
    /// </summary>
    public static void Refresh()
    {
        if (BlockActive)
        {
            VRGestureResponse.PlayerIsBlocking      = true;
            VRGestureResponse.BlockDamageMultiplier = 0f;
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
        Refresh();
    }
}
