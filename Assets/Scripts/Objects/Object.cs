using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class Object : MonoBehaviour
{
    private int minimumMass = 30;
    private int maximumMass = 80;

    private int baseBulletKnockback = 30;
    private int baseContactKnockback = 120;
    private float velocityKnockbackMultiplier = 15;
    private float damageKnockbackMultiplier = 1.2f;

    protected Rigidbody2D rb;

    protected bool active = true;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = Random.Range(minimumMass, maximumMass);
    }

    public virtual void OnBulletHit(float damage, Vector3 direction)
    {
        if (!active) return;

        GiveKnockback(damage, direction);
    }

    /// <summary>
    /// Give knockback if dashing entity comes into contact
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DashAbility otherDashAbility = collision.transform.GetComponent<DashAbility>();
        Entity otherEntity = collision.transform.GetComponent<Entity>();
        Rigidbody2D otherRb = collision.transform.GetComponent<Rigidbody2D>();
        if (otherDashAbility is null || otherEntity is null || otherRb is null) return;

        if (!otherDashAbility.Dashing()) return;

        Vector2 direction = (this.transform.position - collision.transform.position).normalized;
        float damage = otherEntity.ContactDamage;
        float velocity = Vector2.Dot(otherRb.velocity, direction);

        Vector2 knockbackForce = direction.normalized * (
            baseContactKnockback +
            damageKnockbackMultiplier * damage +
            velocityKnockbackMultiplier * velocity
        );

        rb.AddForce(knockbackForce, ForceMode2D.Impulse);
    }

    private void GiveKnockback(float damage, Vector3 direction)
    {
        direction = direction.normalized;

        // Add knockback scaling with damage
        float t = Mathf.InverseLerp(10f, 40f, damage);
        float damageScale = Mathf.Lerp(1f, 2f, t);

        rb.AddForce(damageScale * baseBulletKnockback * direction, ForceMode2D.Impulse);
    }
}
