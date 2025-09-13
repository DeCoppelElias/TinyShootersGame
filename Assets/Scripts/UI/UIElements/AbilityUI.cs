using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : PlayerUIElement
{
    private GameObject dashAbilityUI;
    private bool dashAbilityEnabled = true;
    private GameObject reflectAbilityUI;
    private bool reflectAbilityEnabled = true;
    private GameObject classAbilityUI;
    private bool classAbilityEnabled = true;
    private bool classAbilityInitialised = false;

    protected override void Start()
    {
        base.Start();

        dashAbilityUI = this.transform.Find("DashAbility").gameObject;
        reflectAbilityUI = this.transform.Find("ReflectAbility").gameObject;
        classAbilityUI = this.transform.Find("ClassAbility").gameObject;
        if (!classAbilityInitialised) classAbilityUI.SetActive(false);

        AddOnInitializedListener(LinkToPlayer);
    }

    private void LinkToPlayer()
    {
        DashAbility dashAbility = player.GetComponent<DashAbility>();
        if (dashAbility != null)
        {
            dashAbility.onReady.AddListener(EnableDashAbility);
            dashAbility.onPerformed.AddListener(DisableDashAbility);
        }

        ReflectShieldAbility reflectShieldAbility = player.GetComponent<ReflectShieldAbility>();
        if (reflectShieldAbility != null)
        {
            reflectShieldAbility.onReady.AddListener(EnableReflectAbility);
            reflectShieldAbility.onPerformed.AddListener(DisableReflectAbility);
        }

        AbilityBehaviour abilityBehaviour = player.GetComponent<AbilityBehaviour>();
        if (abilityBehaviour != null)
        {
            abilityBehaviour.onReady.AddListener(() => EnableClassAbility(abilityBehaviour));
            abilityBehaviour.onPerformed.AddListener(() => DisableClassAbility(abilityBehaviour));
            abilityBehaviour.onLinkAbility.AddListener(() => SetClassAbilityUI(abilityBehaviour));
        }
    }

    protected override void DisableActions()
    {
        this.gameObject.SetActive(false);
    }

    protected override void EnableActions()
    {
        this.gameObject.SetActive(true);
    }

    public override bool Enabled()
    {
        return this.gameObject.activeSelf;
    }

    protected override void InstantDisableActions()
    {
        DisableActions();
    }

    public void DisableDashAbility()
    {
        if (!dashAbilityEnabled) return;
        dashAbilityEnabled = false;

        DashAbility dashAbility = player.GetComponent<DashAbility>();
        int cooldown = Mathf.RoundToInt(dashAbility.GetDashCooldown());

        Image image = dashAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = dashAbilityUI.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableDashAbility()
    {
        dashAbilityEnabled = true;

        Image image = dashAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = dashAbilityUI.GetComponentInChildren<Text>();
        text.text = "";
    }

    public void DisableReflectAbility()
    {
        if (!reflectAbilityEnabled) return;
        reflectAbilityEnabled = false;

        ReflectShieldAbility reflectAbility = player.GetComponent<ReflectShieldAbility>();
        int cooldown = reflectAbility.GetReflectShieldCooldown();

        Image image = reflectAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = reflectAbilityUI.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableReflectAbility()
    {
        reflectAbilityEnabled = true;

        Image image = reflectAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = reflectAbilityUI.GetComponentInChildren<Text>();
        text.text = "";
    }

    public void SetClassAbilityUI(AbilityBehaviour abilityBehaviour)
    {
        classAbilityUI.SetActive(true);
        classAbilityInitialised = true;
    }

    public void DisableClassAbility(AbilityBehaviour abilityBehaviour)
    {
        if (!classAbilityUI.activeSelf) return;
        if (!classAbilityEnabled) return;
        classAbilityEnabled = false;

        int cooldown = abilityBehaviour.ability.cooldown;

        Image image = classAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = classAbilityUI.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableClassAbility(AbilityBehaviour abilityBehaviour)
    {
        if (!classAbilityUI.activeSelf) return;
        classAbilityEnabled = true;

        Image image = classAbilityUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = classAbilityUI.GetComponentInChildren<Text>();
        text.text = "";
    }

    private IEnumerator ReduceCountEverySecond(Text text)
    {
        yield return new WaitForSeconds(1);
        if (text.text != "")
        {
            int cooldown = int.Parse(text.text);
            if (cooldown > 0)
            {
                text.text = (cooldown - 1).ToString();
                StartCoroutine(ReduceCountEverySecond(text));
            }
        }
    }
}
