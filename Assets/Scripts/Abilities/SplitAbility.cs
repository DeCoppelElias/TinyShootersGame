using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitAbility : MonoBehaviour
{
    public void Split(Vector3 moveDirection)
    {
        float currentSplitAmount = GetComponent<SplitMeleeEnemy>().splitAmount;
        if (currentSplitAmount < 3)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector3 throwDirection1 = Quaternion.Euler(0, 0, -90) * moveDirection;
            Vector3 throwDirection2 = Quaternion.Euler(0, 0, 90) * moveDirection;

            GameObject clone1 = Instantiate(this.gameObject, transform.position + throwDirection1 * (transform.localScale / 2).x, Quaternion.identity,transform.parent);
            clone1.transform.localScale = transform.localScale / 2;
            clone1.GetComponent<Entity>().maxHealth = GetComponent<Entity>().maxHealth / 2;
            clone1.GetComponent<Entity>().health = GetComponent<Entity>().maxHealth / 2;
            clone1.GetComponent<Entity>().contactDamage = GetComponent<Entity>().contactDamage / 2;
            clone1.GetComponent<Entity>().onDeathScore = GetComponent<Entity>().onDeathScore / 2;
            clone1.GetComponent<Entity>().lastValidPosition = GetComponent<Entity>().lastValidPosition;
            clone1.GetComponent<SplitMeleeEnemy>().splitAmount = currentSplitAmount + 1;
            clone1.GetComponent<Rigidbody2D>().mass = rb.mass / 2f;
            clone1.GetComponent<Rigidbody2D>().AddForce(throwDirection1 * rb.mass * 100);

            GameObject clone2 = Instantiate(this.gameObject, transform.position + throwDirection2 * (transform.localScale / 2).x, Quaternion.identity, transform.parent);
            clone2.transform.localScale = transform.localScale / 2;
            clone2.GetComponent<Entity>().maxHealth = GetComponent<Entity>().maxHealth / 2;
            clone2.GetComponent<Entity>().health = GetComponent<Entity>().maxHealth / 2;
            clone2.GetComponent<Entity>().contactDamage = GetComponent<Entity>().contactDamage / 2;
            clone2.GetComponent<Entity>().onDeathScore = GetComponent<Entity>().onDeathScore / 2;
            clone2.GetComponent<Entity>().lastValidPosition = GetComponent<Entity>().lastValidPosition;
            clone2.GetComponent<SplitMeleeEnemy>().splitAmount = currentSplitAmount + 1;
            clone2.GetComponent<Rigidbody2D>().mass = rb.mass / 2f;
            clone2.GetComponent<Rigidbody2D>().AddForce(throwDirection2 * rb.mass * 100);
        }
        Destroy(this.gameObject);
    }
}
