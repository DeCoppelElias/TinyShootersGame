using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PVPUIManager : MonoBehaviour
{
    private Canvas canvas1;
    private Canvas canvas2;

    private GameObject abilityUI1;
    private GameObject dashAbilityUI1;
    private bool dashAbilityEnabled1 = true;
    private GameObject reflectAbilityUI1;
    private bool reflectAbilityEnabled1 = true;
    private GameObject classAbilityUI1;
    private bool classAbilityEnabled1 = true;
    private bool classAbilityInitialised1 = false;

    private GameObject abilityUI2;
    private GameObject dashAbilityUI2;
    private bool dashAbilityEnabled2 = true;
    private GameObject reflectAbilityUI2;
    private bool reflectAbilityEnabled2 = true;
    private GameObject classAbilityUI2;
    private bool classAbilityEnabled2 = true;
    private bool classAbilityInitialised2 = false;

    public (GameObject ui, bool enabled) GetDashAbilityUI(Player player)
    {
        if (player.tag == "Player") return (dashAbilityUI1, dashAbilityEnabled1);
        else return (dashAbilityUI2, dashAbilityEnabled2);
    }
    public void SetDashAbilityEnabled(Player player, bool enabled)
    {
        if (player.tag == "Player") dashAbilityEnabled1 = enabled;
        else dashAbilityEnabled2 = enabled;
    }
    public (GameObject ui, bool enabled) GetReflectAbilityUI(Player player)
    {
        if (player.tag == "Player") return (reflectAbilityUI1, reflectAbilityEnabled1);
        else return (reflectAbilityUI2, reflectAbilityEnabled2);
    }
    public void SetReflectAbilityEnabled(Player player, bool enabled)
    {
        if (player.tag == "Player") reflectAbilityEnabled1 = enabled;
        else reflectAbilityEnabled2 = enabled;
    }
    public (GameObject ui, bool enabled, bool initialised) GetClassAbilityUI(Player player)
    {
        if (player.tag == "Player") return (classAbilityUI1, classAbilityEnabled1, classAbilityInitialised1);
        else return (classAbilityUI2, classAbilityEnabled2, classAbilityInitialised2);
    }
    public void SetClassAbilityEnabled(Player player, bool enabled)
    {
        if (player.tag == "Player") classAbilityEnabled1 = enabled;
        else classAbilityEnabled2 = enabled;
    }
    public void SetClassAbilityInitialised(Player player, bool initialised)
    {
        if (player.tag == "Player") classAbilityInitialised1 = initialised;
        else classAbilityInitialised2 = initialised;
    }

    // Start is called before the first frame update
    void Awake()
    {
        canvas1 = GameObject.Find("Canvas 1").GetComponent<Canvas>();
        canvas2 = GameObject.Find("Canvas 2").GetComponent<Canvas>();

        abilityUI1 = canvas1.transform.Find("AbilityUI").gameObject;
        dashAbilityUI1 = abilityUI1.transform.Find("DashAbility").gameObject;
        reflectAbilityUI1 = abilityUI1.transform.Find("ReflectAbility").gameObject;
        classAbilityUI1 = abilityUI1.transform.Find("ClassAbility").gameObject;
        if (!classAbilityInitialised1) classAbilityUI1.SetActive(false);

        abilityUI2 = canvas2.transform.Find("AbilityUI").gameObject;
        dashAbilityUI2 = abilityUI2.transform.Find("DashAbility").gameObject;
        reflectAbilityUI2 = abilityUI2.transform.Find("ReflectAbility").gameObject;
        classAbilityUI2 = abilityUI2.transform.Find("ClassAbility").gameObject;
        if (!classAbilityInitialised2) classAbilityUI2.SetActive(false);
    }

    public void SetClassAbilityUI(Player player)
    {
        (GameObject ui, bool enabled, bool initialised) = GetClassAbilityUI(player);
        ui.SetActive(true);
        SetClassAbilityInitialised(player, true);
    }

    public void DisableClassAbility(Player player)
    {
        (GameObject ui, bool enabled, bool initialised) = GetClassAbilityUI(player);
        if (!ui.activeSelf) return;
        if (!enabled) return;
        if (!initialised) return;
        SetClassAbilityEnabled(player, false);

        int cooldown = player.GetComponent<AbilityBehaviour>().ability.cooldown;

        Image image = ui.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = ui.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableClassAbility(Player player)
    {
        (GameObject ui, bool enabled, bool initialised) = GetClassAbilityUI(player);
        if (!initialised) return;
        SetClassAbilityEnabled(player, true);

        Image image = ui.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = ui.GetComponentInChildren<Text>();
        text.text = "";
    }

    public void DisableDashAbility(Player player)
    {
        (GameObject ui, bool enabled) = GetDashAbilityUI(player);
        if (!enabled) return;
        SetDashAbilityEnabled(player, false);

        DashAbility dashAbility = player.GetComponent<DashAbility>();
        int cooldown = dashAbility.GetDashCooldown();

        Image image = ui.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = ui.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableDashAbility(Player player)
    {
        (GameObject ui, bool enabled) = GetDashAbilityUI(player);
        SetDashAbilityEnabled(player, true);

        Image image = ui.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = ui.GetComponentInChildren<Text>();
        text.text = "";
    }

    public void DisableReflectAbility(Player player)
    {
        (GameObject ui, bool enabled) = GetReflectAbilityUI(player);
        if (!enabled) return;
        SetReflectAbilityEnabled(player, false);

        ReflectShieldAbility reflectAbility = player.GetComponent<ReflectShieldAbility>();
        int cooldown = reflectAbility.GetReflectShieldCooldown();

        Image image = ui.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.1f);

        Text text = ui.GetComponentInChildren<Text>();
        text.text = cooldown.ToString();

        StartCoroutine(ReduceCountEverySecond(text));
    }

    public void EnableReflectAbility(Player player)
    {
        (GameObject ui, bool enabled) = GetReflectAbilityUI(player);
        SetReflectAbilityEnabled(player, true);

        Image image = ui.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.7f);

        Text text = ui.GetComponentInChildren<Text>();
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
