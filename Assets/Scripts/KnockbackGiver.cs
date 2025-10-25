using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackGiver : MonoBehaviour
{
    public List<Rigidbody2D> rbs = new List<Rigidbody2D>();
    public Vector3 startPosition;
    public Vector2 force = new Vector2();

    [ContextMenu("Apply Knockback Test")]
    public void AddKnockback()
    {
        Vector3 currentPosition = startPosition;
        foreach (Rigidbody2D rb in rbs)
        {
            rb.transform.position = currentPosition;
            currentPosition = new Vector3(currentPosition.x, currentPosition.y - 2, currentPosition.z);
        }

        foreach (Rigidbody2D rb in rbs) rb.AddForce(force, ForceMode2D.Impulse);
    }
}
