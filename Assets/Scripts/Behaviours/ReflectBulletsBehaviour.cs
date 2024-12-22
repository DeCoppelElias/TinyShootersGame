using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReflectShieldAbility))]
public class ReflectBulletsBehaviour : MonoBehaviour
{
    private ReflectShieldAbility reflectShieldAbility;

    [Header("Trigger Settings")]
    [SerializeField] private bool customBulletTrigger = false;
    [SerializeField] private BulletTrigger bulletTrigger;

    // Start is called before the first frame update
    void Start()
    {
        reflectShieldAbility = GetComponent<ReflectShieldAbility>();

        if (!customBulletTrigger) this.bulletTrigger = GetComponentInChildren<BulletTrigger>();

        if (bulletTrigger != null) bulletTrigger.AddTriggerAction(EnableReflectShield);
    }

    private void EnableReflectShield(Vector3 bulletDirection)
    {
        reflectShieldAbility.EnableReflectShield();
    }
}
