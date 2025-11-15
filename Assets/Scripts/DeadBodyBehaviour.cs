using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBodyBehaviour : MonoBehaviour
{
    private float startTime;
    [SerializeField] private float duration;
    private SpriteRenderer spriteRenderer;
    private bool start = false;
    private Rigidbody2D rb;

    // Update is called once per frame
    void Update()
    {
        if (!start) return;

        // Setting transparancy
        float p = Mathf.Clamp((Time.time - startTime) / duration, 0, 1);
        float a = 0.5f - (p / 2);
        Color currentColor = spriteRenderer.color;
        currentColor.a = a;
        spriteRenderer.color = currentColor;

        // Setting size
        // transform.localScale = new Vector3(1-p, 1-p, 1-p);

        if (p == 1)
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialise(float duration, Vector2 lastDamageDirection)
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        this.startTime = Time.time;
        this.duration = duration;
        this.start = true;

        this.rb = this.gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.drag = 5f;
        this.rb.AddForce(lastDamageDirection * 3, ForceMode2D.Impulse);
    }
}
