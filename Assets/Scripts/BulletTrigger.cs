using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BulletTrigger : MonoBehaviour
{
    [SerializeField] private string ownerTag;
    private System.Action<Vector3> triggerAction;
    private void Start()
    {
        Entity parentEntity = GetComponentInParent<Entity>();
        if (parentEntity != null)
        {
            this.ownerTag = parentEntity.tag;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null && bullet.ownerTag != ownerTag)
        {
            Vector3 bulletDirection = collision.GetComponent<Rigidbody2D>().velocity.normalized;
            triggerAction(bulletDirection);
        }
    }

    public void AddTriggerAction(System.Action<Vector3> triggerAction)
    {
        this.triggerAction += triggerAction;
    }
}
