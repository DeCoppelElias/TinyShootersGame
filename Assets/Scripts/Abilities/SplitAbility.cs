using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitAbility : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        this.rb = GetComponent<Rigidbody2D>();
    }

    public bool Split(Vector3 moveDirection)
    {
        int currentSplitAmount = GetComponent<SplitMeleeEnemy>().splitAmount;
        if (currentSplitAmount >= 3) return false;

        Vector3 throwDirection1 = Quaternion.Euler(0, 0, -90) * moveDirection;
        Vector3 throwDirection2 = Quaternion.Euler(0, 0, 90) * moveDirection;

        GenerateSmallerClone(currentSplitAmount, throwDirection1);
        GenerateSmallerClone(currentSplitAmount, throwDirection2);

        return true;
    }

    private void GenerateSmallerClone(int currentSplitAmount, Vector3 throwDirection)
    {
        GameObject clone = Instantiate(this.gameObject, transform.position, Quaternion.identity, transform.parent);
        clone.transform.localScale = transform.localScale / 1.5f;
        clone.GetComponent<Entity>().MaxHealth = GetComponent<Entity>().MaxHealth / 2;
        clone.GetComponent<Entity>().Health = GetComponent<Entity>().MaxHealth / 2;
        clone.GetComponent<Entity>().ContactDamage = GetComponent<Entity>().ContactDamage / 1.2f;
        clone.GetComponent<Entity>().onDeathScore = GetComponent<Entity>().onDeathScore / 2;
        clone.GetComponent<Entity>().lastValidPosition = GetComponent<Entity>().lastValidPosition;
        clone.GetComponent<SplitMeleeEnemy>().splitAmount = currentSplitAmount + 1;
        clone.GetComponent<Rigidbody2D>().mass = rb.mass / 1.5f;
        clone.GetComponent<Rigidbody2D>().AddForce(throwDirection * rb.mass * 500);
    }

    private IEnumerator PerformAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);

        action();
    }
}
