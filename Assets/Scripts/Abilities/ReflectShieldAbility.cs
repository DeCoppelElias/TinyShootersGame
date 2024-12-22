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
    private System.Action onComplete;

    private GameObject reflectShieldObject;


    private void Start()
    {
        ReflectShield reflectShield = GetComponentInChildren<ReflectShield>();
        if (reflectShield == null) throw new System.Exception("Reflect Shield is missing.");

        reflectShieldObject = reflectShield.gameObject;
        reflectShieldObject.SetActive(false);
    }

    public void EnableReflectShield(System.Action action = null)
    {
        if (reflectShieldState != ReflectShieldState.Ready) return;

        if (action != null) onComplete = action;
        reflectShieldObject.SetActive(true);
        reflectShieldState = ReflectShieldState.Reflecting;
        reflectShieldStart = Time.time;
    }

    private void Update()
    {
        if (reflectShieldState == ReflectShieldState.Cooldown)
        {
            if (Time.time - lastReflectShieldUse > reflectShieldCooldown)
            {
                reflectShieldState = ReflectShieldState.Ready;

                if (onReady != null)
                {
                    onReady.Invoke();
                }
            }
        }
        if (reflectShieldState == ReflectShieldState.Reflecting)
        {
            if (Time.time - reflectShieldStart > reflectShieldDuration)
            {
                reflectShieldState = ReflectShieldState.Cooldown;
                reflectShieldObject.SetActive(false);
                lastReflectShieldUse = Time.time;

                if (onPerformed != null)
                {
                    onPerformed.Invoke();
                }
                onComplete?.Invoke();
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
