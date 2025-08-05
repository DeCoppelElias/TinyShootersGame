using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Instance { get; private set; }

    [SerializeField] private int bulletLimit = 1000;
    [SerializeField] private int bulletStart = 10;
    [SerializeField] private int bulletCount = 0;
    [SerializeField] private GameObject bulletPrefab;

    private Queue<Bullet> availableBullets = new Queue<Bullet>();

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < bulletStart; i++)
        {
            TrySpawnBullet();
        }
    }

    private void TrySpawnBullet()
    {
        if (bulletCount >= bulletLimit) return;
        bulletCount++;

        GameObject bulletGO = Instantiate(bulletPrefab, this.transform);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet == null) throw new System.Exception("Bullet prefab does not have bullet script.");
        
        availableBullets.Enqueue(bullet);
        bulletGO.SetActive(false);
    }

    public Bullet TryGetBullet()
    {
        if (availableBullets.Count == 0)
        {
            TrySpawnBullet();
        }

        if (availableBullets.Count > 0)
        {
            Bullet bullet = availableBullets.Dequeue();
            bullet.gameObject.SetActive(true);
            return bullet;
        }
        else return null;
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (bullet == null) return;

        bullet.transform.position = Vector3.zero;
        bullet.transform.localScale = Vector3.one;
        bullet.GetComponent<Rigidbody2D>().velocity = Vector3.zero;

        this.availableBullets.Enqueue(bullet);
        bullet.gameObject.SetActive(false);
    }
}
