using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Keyboard input that works with both the old and the new Unity Input System.</summary>
public static class GameInput
{
    /// <summary>x = left/right (A/D, arrows), y = backward/forward (S/W, arrows).</summary>
    public static Vector2 Move()
    {
        float x = 0f, z = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
#elif ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.wKey.isPressed || k.upArrowKey.isPressed) z += 1f;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) z -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1f;
        }
#endif
        return new Vector2(x, z);
    }
}
