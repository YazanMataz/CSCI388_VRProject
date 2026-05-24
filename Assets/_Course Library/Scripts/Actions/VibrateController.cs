using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

/// <summary>
/// Vibrate the XR Controller
/// </summary>
public class VibrateController : MonoBehaviour
{
    public float strongVibrate = 0.75f;
    public float weakVibrate = 0.25f;

    private HapticImpulsePlayer hapticPlayer;

    private void Awake()
    {
        hapticPlayer = GetComponent<HapticImpulsePlayer>();
        if (hapticPlayer == null)
            hapticPlayer = GetComponentInParent<HapticImpulsePlayer>();
    }

    public void Vibrate(float amplitude, float duration)
    {
        if (hapticPlayer == null)
            return;

        hapticPlayer.SendHapticImpulse(amplitude, duration);
    }

    public void VibrateWeak(float duration)
    {
        Vibrate(weakVibrate, duration);
    }

    public void VibrateStrong(float duration)
    {
        Vibrate(strongVibrate, duration);
    }
}
