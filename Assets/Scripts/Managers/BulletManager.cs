using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [SerializeField] private int bulletLimit = 1000;
    [SerializeField] private int bulletStart = 10;
    private int bulletCount = 0;
    [SerializeField] private GameObject bulletPrefab;

    private Queue<Bullet> availableBullets = new Queue<Bullet>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < bulletStart; i++)
        {
            SpawnBullet();
        }
    }

    private Bullet SpawnBullet()
    {
        if (bulletCount > bulletLimit) return null;
        bulletCount++;

        GameObject bulletGO = Instantiate(bulletPrefab, this.transform);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet == null) throw new System.Exception("Bullet prefab does not have bullet script.");
        
        availableBullets.Enqueue(bullet);
        bulletGO.SetActive(false);
        return bullet;
    }

    public Bullet GetBullet()
    {
        if (availableBullets.Count > 0)
        {
            Bullet bullet = availableBullets.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else
        {
            Bullet bullet = SpawnBullet();
            if (bullet == null) return null;

            bullet.gameObject.SetActive(true);
            return bullet;
        }
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        this.availableBullets.Enqueue(bullet);
        bullet.gameObject.SetActive(false);
    }
}
