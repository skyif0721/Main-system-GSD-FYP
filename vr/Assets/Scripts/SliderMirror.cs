using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Copies the master slider's value/maxValue every frame to the slider on
/// this GameObject. Used to keep mirrored HUD bars (e.g. the VR cam HUD) in
/// sync with the main PlayerStats sliders.
/// </summary>
[RequireComponent(typeof(Slider))]
public class SliderMirror : MonoBehaviour
{
    public Slider master;
    private Slider _self;

    void Awake() { _self = GetComponent<Slider>(); }

    void LateUpdate()
    {
        if (master == null || _self == null) return;
        if (_self.maxValue != master.maxValue) _self.maxValue = master.maxValue;
        if (_self.value    != master.value)    _self.value    = master.value;
    }
}
