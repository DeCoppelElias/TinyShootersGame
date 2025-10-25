using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [SerializeField] private List<Powerup> commonPowerups = new List<Powerup>();
    [SerializeField] private List<Powerup> uncommonPowerups = new List<Powerup>();
    [SerializeField] private List<Powerup> rarePowerups = new List<Powerup>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadPowerups();
    }

    private void LoadPowerups()
    {
        UnityEngine.Object[] commonPowerupsObjects = Resources.LoadAll("Powerups/Common", typeof(Powerup));
        foreach (UnityEngine.Object powerup in commonPowerupsObjects)
        {
            commonPowerups.Add((Powerup)powerup);
        }

        UnityEngine.Object[] uncommonPowerupsObjects = Resources.LoadAll("Powerups/Uncommon", typeof(Powerup));
        foreach (UnityEngine.Object powerup in uncommonPowerupsObjects)
        {
            uncommonPowerups.Add((Powerup)powerup);
        }

        UnityEngine.Object[] rarePowerupsObjects = Resources.LoadAll("Powerups/Rare", typeof(Powerup));
        foreach (UnityEngine.Object powerup in rarePowerupsObjects)
        {
            rarePowerups.Add((Powerup)powerup);
        }
    }

    public List<Powerup> ChooseRandomPowerups(int number)
    {
        // Create temporary copies so original lists aren't modified
        var availableCommons = new List<Powerup>(commonPowerups);
        var availableUncommons = new List<Powerup>(uncommonPowerups);
        var availableRares = new List<Powerup>(rarePowerups);

        int allPowerups = availableCommons.Count + availableUncommons.Count + availableRares.Count;
        if (allPowerups < number)
            return new List<Powerup>();

        var chosen = new List<Powerup>();
        var rng = new System.Random();

        for (int i = 0; i < number; i++)
        {
            List<Powerup> sourceList = null;
            double roll = rng.NextDouble();

            // Select rarity based on probability, but only if list is not empty
            if (roll < 0.6 && availableCommons.Count > 0)
                sourceList = availableCommons;
            else if (roll < 0.9 && availableUncommons.Count > 0)
                sourceList = availableUncommons;
            else if (availableRares.Count > 0)
                sourceList = availableRares;
            else
            {
                // Fallback in case the selected list is empty
                if (availableCommons.Count > 0) sourceList = availableCommons;
                else if (availableUncommons.Count > 0) sourceList = availableUncommons;
                else sourceList = availableRares;
            }

            // Pick a random powerup from the chosen list
            int index = UnityEngine.Random.Range(0, sourceList.Count);
            var selected = sourceList[index];

            chosen.Add(selected);
            sourceList.RemoveAt(index); // Remove only from the temporary list
        }

        return chosen;
    }

}
