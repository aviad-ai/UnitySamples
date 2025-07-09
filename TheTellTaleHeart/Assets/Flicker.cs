using System;
using UnityEngine;

public class Flicker : MonoBehaviour
{
    private Light light;
    [SerializeField] private float variance = 0.02f;
    [SerializeField] private float min = 0.02f;
    [SerializeField] private float max = 2.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        light = this.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (light != null)
        {
            ApplyFlickerNoise();
        }
    }

    private void ApplyFlickerNoise()
    {
        light.intensity = Math.Clamp(light.intensity + UnityEngine.Random.Range(-variance, variance), min, max);
    }
}