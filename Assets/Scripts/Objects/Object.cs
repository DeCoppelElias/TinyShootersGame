using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Object : MonoBehaviour
{
    private int knockbackForce = 30;
    private float knockbackCooldown = 0.3f;
    private float lastKnockback;

    protected Rigidbody2D rb;

    protected bool active = true;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual void OnBulletHit(float damage, Vector3 direction)
    {
        if (!active) return;

        GiveKnockback(damage, direction);
    }

    private void GiveKnockback(float damage, Vector3 direction)
    {
        direction = direction.normalized;

        // Add knockback scaling with damage
        float t = Mathf.InverseLerp(10f, 40f, damage);
        float damageScale = Mathf.Lerp(1f, 2f, t);

        rb.AddForce(damageScale * knockbackForce * direction, ForceMode2D.Impulse);
        lastKnockback = Time.time;
    }
}
