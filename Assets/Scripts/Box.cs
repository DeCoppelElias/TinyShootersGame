using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private int knockbackForce = 30;
    private float knockbackCooldown = 0.3f;
    private float lastKnockback;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GiveKnockback(float damage, Vector3 direction)
    {
        direction = direction.normalized;

        // Add knockback scaling with damage
        float t = Mathf.InverseLerp(10f, 40f, damage);
        float damageScale = Mathf.Lerp(1f, 2f, t);

        rb.AddForce(damageScale * knockbackForce * direction, ForceMode2D.Impulse);
        Debug.Log("Triggered with: " + damageScale * knockbackForce * direction);
        lastKnockback = Time.time;
    }
}
