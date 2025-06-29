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
    public float splitAmountDelta = 0;
    public float splitRangeDelta = 0;
    public float splitBulletSizeDelta = 0;
    public float splitBulletSpeedDelta = 0;
    public float splitDamagePercentageDelta = 0;

    [Header("Dash Upgrade")]
    public int dashCooldownDelta = 0;
    public float dashDurationDelta = 0;
    public float chargeDurationDelta = 0;
    public float dashSpeedDelta = 0;

    public string GenerateUIDescription()
    {
        StringBuilder sb = new StringBuilder();
        FieldInfo[] fields = typeof(Powerup).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

        foreach (FieldInfo field in fields)
        {
            if (field.Name == nameof(powerupName) || field.Name == nameof(rarity)) continue;

            object value = field.GetValue(this);

            if (value is float floatVal && Mathf.Abs(floatVal) > 0)
            {
                sb.AppendLine($"{SplitCamelCase(field.Name)}: {(floatVal > 0 ? "+" : "")}{floatVal}");
            }
            if (field.Name == nameof(recoverHealth) && value is true)
            {
                sb.AppendLine("Recover All Health!");
            }
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
}
