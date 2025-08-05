using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public static PowerupManager Instance { get; private set; }

    [SerializeField] private List<Powerup> commonPowerups = new List<Powerup>();
    [SerializeField] private List<Powerup> uncommonPowerups = new List<Powerup>();

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
    }

    public List<Powerup> ChooseRandomPowerups(int number)
    {
        if (commonPowerups.Count == 0 && uncommonPowerups.Count == 0) return new List<Powerup>();

        List<Powerup> randomPowerups = new List<Powerup>();

        for(int i = 0; i < number; i++)
        {
            float random = Random.Range(0f, 1f);
            if ((random < 0.7 || uncommonPowerups.Count == 0) && commonPowerups.Count > 0)
            {
                Powerup selectedPowerup = commonPowerups[Random.Range(0, commonPowerups.Count)];
                while (randomPowerups.Contains(selectedPowerup))
                {
                    selectedPowerup = commonPowerups[Random.Range(0, commonPowerups.Count)];
                }
                randomPowerups.Add(selectedPowerup);
            }
            else if (uncommonPowerups.Count > 0)
            {
                Powerup selectedPowerup = uncommonPowerups[Random.Range(0, uncommonPowerups.Count)];
                while (randomPowerups.Contains(selectedPowerup))
                {
                    selectedPowerup = uncommonPowerups[Random.Range(0, uncommonPowerups.Count)];
                }
                randomPowerups.Add(selectedPowerup);
            }
        }

        return randomPowerups;
    }
}
