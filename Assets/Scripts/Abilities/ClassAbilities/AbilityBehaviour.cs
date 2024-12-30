using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbilityBehaviour : MonoBehaviour
{
    public ClassAbility ability;

    private bool initialised = false;
    private float lastUseTime;

    public UnityEvent onPerformed;
    public UnityEvent onReady;
    public UnityEvent onLinkAbility;

    private enum AbilityState { Ready, Cooldown}
    private AbilityState state = AbilityState.Ready;

    public void LinkAbility(ClassAbility classAbility)
    {
        ability = classAbility;
        initialised = false;
        lastUseTime = 0;

        onLinkAbility.Invoke();
    }

    private void Update()
    {
        if (state == AbilityState.Cooldown && Time.time - lastUseTime > ability.cooldown)
        {
            state = AbilityState.Ready;

            onReady.Invoke();
        }
    }

    public void UseAbility(Player player)
    {
        if (!initialised)
        {
            initialised = ability.Initialise(player);
        }

        if (initialised && state == AbilityState.Ready)
        {
            lastUseTime = Time.time;
            state = AbilityState.Cooldown;
            ability.PerformAbility(player);

            onPerformed.Invoke();
        }
    }

    public bool OnCooldown()
    {
        return this.state == AbilityState.Cooldown;
    }
}
