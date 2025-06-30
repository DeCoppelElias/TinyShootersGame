using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class Flicker : MonoBehaviour
{
    private Light2D _lightSource;

    [SerializeField] private float MaxReduction = 0.2f;
    [SerializeField] private float MaxIncrease = 0.2f;
    [SerializeField] private float RateDamping = 0.1f;
    [SerializeField] private float Strength = 300;
    [SerializeField] private bool StopFlickering;

    private float _baseIntensity;
    private bool _flickering;

    public void Start()
    {
        _lightSource = GetComponent<Light2D>();
        _baseIntensity = _lightSource.intensity;
        StartCoroutine(DoFlicker());
    }

    void Update()
    {
        if (!StopFlickering && !_flickering)
        {
            StartCoroutine(DoFlicker());
        }
    }

    private IEnumerator DoFlicker()
    {
        _flickering = true;
        while (!StopFlickering)
        {
            _lightSource.intensity = Mathf.Lerp(_lightSource.intensity, Random.Range(_baseIntensity - MaxReduction, _baseIntensity + MaxIncrease), Strength * Time.deltaTime);
            yield return new WaitForSeconds(RateDamping);
        }
        _flickering = false;
    }
}
