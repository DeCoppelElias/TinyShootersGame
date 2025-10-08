using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReflectShieldAbility : MonoBehaviour
{
    public GameObject playerReflectShield;
    public GameObject enemyReflectShield;

    [Header("Reflect Shield Settings")]
    [SerializeField] private ReflectShieldState reflectShieldState;
    private enum ReflectShieldState { Ready, Reflecting, Cooldown }

    [SerializeField] private float reflectShieldDuration = 0.2f;
    [SerializeField] private int reflectShieldCooldown = 3;
    private float reflectShieldStart = 0;
    private float lastReflectShieldUse;
    [SerializeField] private Sprite reflectBulletSprite;

    public UnityEvent onPerformed;
    public UnityEvent onReady;

    private GameObject reflectShieldObject;


    private void Start()
    {
        ReflectShield reflectShield = GetComponentInChildren<ReflectShield>();
        if (reflectShield == null) throw new System.Exception("Reflect Shield is missing.");

        reflectShieldObject = reflectShield.gameObject;
        reflectShieldObject.SetActive(false);
    }

    public void EnableReflectShield()
    {
        if (reflectShieldState != ReflectShieldState.Ready) return;

        reflectShieldObject.SetActive(true);
        reflectShieldState = ReflectShieldState.Reflecting;
        reflectShieldStart = Time.time;
    }

    public void SetReady()
    {
        reflectShieldState = ReflectShieldState.Ready;
        reflectShieldObject.SetActive(false);

        if (onReady != null)
        {
            onReady?.Invoke();
        }
    }

    private void Update()
    {
        if (reflectShieldState == ReflectShieldState.Cooldown)
        {
            if (Time.time - lastReflectShieldUse > reflectShieldCooldown)
            {
                SetReady();
            }
        }
        if (reflectShieldState == ReflectShieldState.Reflecting)
        {
            if (Time.time - reflectShieldStart > reflectShieldDuration)
            {
                reflectShieldState = ReflectShieldState.Cooldown;
                reflectShieldObject.SetActive(false);
                lastReflectShieldUse = Time.time;

                onPerformed?.Invoke();
            }
        }
    }

    public int GetReflectShieldCooldown()
    {
        return this.reflectShieldCooldown;
    }

    public bool IsReflecting()
    {
        return this.reflectShieldState == ReflectShieldState.Reflecting;
    }
}
