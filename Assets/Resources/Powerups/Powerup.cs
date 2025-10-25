using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPowerup", menuName = "Player/Powerup")]
public class Powerup : ScriptableObject
{
    [Header("Information")]
    public string powerupName;
    public enum Rarity { Common, Uncommon, Rare};
    public Rarity rarity;

    [Header("General Upgrade")]
    public float healthDelta = 0;
    public bool recoverHealth = false;

    public float normalMoveSpeedDelta = 0;
    public float shootingMoveSpeedDelta = 0;

    public float invulnerableDurationDelta = 0;

    public float contactDamageDelta = 0;
    public float contactHitCooldownDelta = 0;

    [Header("Ranged Combat Upgrade")]
    public float damageDelta = 0;
    public float attackCooldownDelta = 0;
    public float rangeDelta = 0;
    public int pierceDelta = 0;
    public float totalSplitDelta = 0;
    public float totalFanDelta = 0;
    public float bulletSizeDelta = 0;
    public float bulletSpeedDelta = 0;
    public bool splitOnHit = false;
    public int splitAmountDelta = 0;
    public float splitRangeDelta = 0;
    public float splitBulletSizeDelta = 0;
    public float splitBulletSpeedDelta = 0;
    public float splitDamagePercentageDelta = 0;

    [Header("Dash Upgrade")]
    public float dashCooldownDelta = 0;
    public float dashDurationDelta = 0;
    public float chargeDurationDelta = 0;
    public float dashSpeedDelta = 0;
    public float contactDamageIncreaseDelta = 0;

    public string GenerateUIDescription()
    {
        StringBuilder sb = new StringBuilder();
        FieldInfo[] fields = typeof(Powerup).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (FieldInfo field in fields)
        {
            if (field.Name == nameof(powerupName) || field.Name == nameof(rarity)) continue;

            string prettyString = GetPrettyStringForField(field);
            if (prettyString.Length > 0) sb.AppendLine(prettyString);
        }

        return sb.ToString().TrimEnd();
    }

    private string SplitCamelCase(string input)
    {
        // Remove "Delta" if present at the end
        if (input.EndsWith("Delta"))
            input = input.Substring(0, input.Length - 5);

        // Insert a space before every capital letter (except the first one)
        string spaced = System.Text.RegularExpressions.Regex.Replace(input, "(\\B[A-Z])", " $1");

        // Capitalize the first letter
        return char.ToUpper(spaced[0]) + spaced.Substring(1);
    }

    private string GetPrettyStringForField(FieldInfo field)
    {
        string name = field.Name;
        object value = field.GetValue(this);

        string EffectText(string positive, string negative, float delta)
        {
            if (Mathf.Approximately(delta, 0)) return "";
            return delta > 0 ? positive : negative;
        }

        string EffectTextInt(string positive, string negative, int delta)
        {
            if (delta == 0) return "";
            return delta > 0 ? positive : negative;
        }

        switch (name)
        {
            // --- General Upgrade ---
            case nameof(healthDelta):
                return EffectText("Increased Max Health", "Reduced Max Health", (float)value);

            case nameof(recoverHealth):
                return value is true ? "Recover All Health!" : "";

            case nameof(normalMoveSpeedDelta):
                return EffectText("Faster Movement", "Slower Movement", (float)value);

            case nameof(shootingMoveSpeedDelta):
                return EffectText("Faster While Shooting", "Slower While Shooting", (float)value);

            case nameof(invulnerableDurationDelta):
                return EffectText("Longer Invulnerability", "Shorter Invulnerability", (float)value);

            case nameof(contactDamageDelta):
                return EffectText("Increased Contact Damage", "Reduced Contact Damage", (float)value);

            case nameof(contactHitCooldownDelta):
                return EffectText("Shorter Contact Cooldown", "Longer Contact Cooldown", (float)value);

            // --- Ranged Combat Upgrade ---
            case nameof(damageDelta):
                return EffectText("Increased Damage", "Reduced Damage", (float)value);

            case nameof(attackCooldownDelta):
                return EffectText("Longer Attack Cooldown", "Shorter Attack Cooldown", (float)value);

            case nameof(rangeDelta):
                return EffectText("Increased Range", "Reduced Range", (float)value);

            case nameof(pierceDelta):
                return EffectTextInt("More Pierce", "Less Pierce", (int)value);

            case nameof(totalSplitDelta):
                return EffectText("More Split Shots", "Fewer Split Shots", (float)value);

            case nameof(totalFanDelta):
                return EffectText("More Fan Shots", "Fewer Fan Shots", (float)value);

            case nameof(bulletSizeDelta):
                return EffectText("Larger Bullets", "Smaller Bullets", (float)value);

            case nameof(bulletSpeedDelta):
                return EffectText("Faster Bullets", "Slower Bullets", (float)value);

            case nameof(splitOnHit):
                return value is true ? "Bullets Split on Hit!" : "";

            case nameof(splitAmountDelta):
                return EffectText("More Split Bullets", "Fewer Split Bullets", (int)value);

            case nameof(splitRangeDelta):
                return EffectText("Wider Split Range", "Shorter Split Range", (float)value);

            case nameof(splitBulletSizeDelta):
                return EffectText("Larger Split Bullets", "Smaller Split Bullets", (float)value);

            case nameof(splitBulletSpeedDelta):
                return EffectText("Faster Split Bullets", "Slower Split Bullets", (float)value);

            case nameof(splitDamagePercentageDelta):
                return EffectText("Higher Split Damage", "Lower Split Damage", (float)value);

            // --- Dash Upgrade ---
            case nameof(dashCooldownDelta):
                return EffectText("Longer Dash Cooldown", "Shorter Dash Cooldown", (float)value);

            case nameof(dashDurationDelta):
                return EffectText("Longer Dash Duration", "Shorter Dash Duration", (float)value);

            case nameof(chargeDurationDelta):
                return EffectText("Longer Charge Time", "Shorter Charge Time", (float)value);

            case nameof(dashSpeedDelta):
                return EffectText("Faster Dash", "Slower Dash", (float)value);

            case nameof(contactDamageIncreaseDelta):
                return EffectText("Increased Dash Damage", "Reduced Dash Damage", (float)value);

            default:
                if (value is float floatVal && Mathf.Abs(floatVal) > 0)
                    return $"{SplitCamelCase(field.Name)} {(floatVal > 0 ? "+" : "")}{floatVal}";
                if (value is int intVal && intVal != 0)
                    return $"{SplitCamelCase(field.Name)} {(intVal > 0 ? "+" : "")}{intVal}";
                return "";
        }
    }
}
