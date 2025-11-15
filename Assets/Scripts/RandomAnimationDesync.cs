using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimationDesync : MonoBehaviour
{
    void Start()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0, 0, Random.value);
            animator.speed = Random.Range(0.9f, 1.1f);
        }
    }
}
