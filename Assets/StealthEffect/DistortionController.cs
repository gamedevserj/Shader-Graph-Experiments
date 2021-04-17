using UnityEngine;
using UnityEngine.UI;

public class DistortionController : MonoBehaviour
{
    public Renderer[] renderers;
    public string propertyName = "_DissolveAmount";
    public Slider slider;

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(ChangeMaterial);
    }
    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(ChangeMaterial);
    }

    void ChangeMaterial(float val)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.SetFloat(propertyName, val);
        }
    }
}
