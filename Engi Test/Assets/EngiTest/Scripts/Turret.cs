using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public GameCore gameCore;
    public List<Enemy> Enemies = new List<Enemy>();
    public List<Enemy> AttacableEnemies = new List<Enemy>();
    public TurretType turretType;
    bool EnemySelected, ableToAttack;
    Enemy target;
    WaitForSeconds cooldown, attackRate, rateOfFire;
    bool isAutomaticType() => turretType.Equals(TurretType.Automatic);
    bool isMissileType() => turretType.Equals(TurretType.Missile);
    bool isPulseType() => turretType.Equals(TurretType.Pulse);

    //Weapon Mechanics
    [SerializeField] Transform head;
    [SerializeField] GameObject bullet;


    // Automatic Turret Mechanics
    [Space(3)]
    [Header("Automatic turret mechanics")]
    int CannonPooler, BulletCounter, bulletsInt = 3;
    List<Rigidbody> SpawnBullet = new List<Rigidbody>();
    [SerializeField] List<Transform> TurretHeadCannon = new List<Transform>();
    float speed = 15;
    Vector3 Direction;
    //Missile Turret Mechanics
    [Space(3)]
    [Header("Missile turret mechanics")]
    public bool missileHasExploded;
    public Transform MissileLauncher;
    Missile missile;
    //Pulse Turret Mechanics
    [Space(3)]
    [SerializeField] SphereCollider pulseCollider;
    WaitForSeconds pulseDuration;
    float pulseRadius;
    public void StartTurret()
    {
        speed = 15;
        cooldown = new WaitForSeconds(1);
        rateOfFire = new WaitForSeconds(.2f);
        if (isAutomaticType())
        {
            attackRate = new WaitForSeconds(2);
            for (int i = 0; i < 3; i++)
            {
                GameObject gObject = Instantiate(bullet, transform.position, Quaternion.identity);
                Rigidbody rObject = gObject.AddComponent<Rigidbody>();
                gObject.tag = "Automatic";
                if (!SpawnBullet.Contains(rObject))
                {
                    SpawnBullet.Add(rObject);
                }
                gObject.SetActive(false);
            }
        }
        else if (isMissileType())
        {
            GameObject gObject = Instantiate(bullet, transform.position, Quaternion.identity);
            SphereCollider sphereCollider = gObject.GetComponent<SphereCollider>();
            gObject.tag = "Missile";
            missile = gObject.AddComponent<Missile>();
            sphereCollider.radius = 0;
            missile.sphereCollider = sphereCollider;
            attackRate = new WaitForSeconds(4);
        }
        else if (isPulseType())
        {
            gameObject.tag = "Pulse";
            pulseRadius = 2.01f;
            pulseDuration = new WaitForSeconds(7);
            attackRate = new WaitForSeconds(3);
        }
        if (Enemies == null)
        {
            Enemies = gameCore.Enemies;
        }
        StopAllCoroutines();
        StartCoroutine(TurretCycle());
    }
    IEnumerator TurretCycle()
    {
        while (true)
        {
            yield return cooldown;
            FindAnEnemiesToAttack();
            if (ableToAttack)
            {
                yield return new WaitUntil(() => EnemySelected);

                //// Automatic turret logic
                if (isAutomaticType())
                {
                    float actual = 0;
                    float duration = 1;
                    while (actual < duration)
                    {
                        actual += .02f;
                        Direction = (target.transform.position - transform.position);
                        if (target.isActiveAndEnabled)
                            head.LookAt(target.transform);
                        else ableToAttack = false;
                        yield return rateOfFire;
                        Fire(Direction);
                    }
                    ableToAttack = false;
                }
                //// Missile turret logic
                else if (isMissileType())
                {
                    float actual = 0;
                    float duration = 1;
                    while (actual < duration)
                    {
                        actual += .1f;
                        Direction = (target.transform.position - transform.position);
                        Vector3 targetPostition = new Vector3(target.transform.position.x, head.position.y, target.transform.position.z);
                        head.LookAt(targetPostition);
                        yield return rateOfFire;
                    }
                    FireMissile();
                    yield return new WaitUntil(() => missileHasExploded);
                    missile.gameObject.SetActive(false);
                    ableToAttack = false;
                }
                //// Pulse turret logic
                else if (isPulseType())
                {
                    pulseCollider.radius = pulseRadius;
                    yield return pulseDuration;
                    ableToAttack = false;
                    pulseCollider.radius = 0;
                }

                yield return attackRate;
            }

            yield return cooldown;
        }
    }
    void FindAnEnemiesToAttack()
    {

        //En calculo de distancias, operaciones de vectores que necesiten saber la info de un transform X siempre he optado por localPosition
        //En este caso como los enemigos y torretas estan en el root pues la pos local sera igual a la pos raiz
        EnemySelected = false;
        AttacableEnemies.Clear();
        foreach (var item in Enemies)
        {
            if (isAutomaticType())
            {
                if (Vector3.Distance(transform.position, item.transform.position) < 4f)
                {
                    AttacableEnemies.Add(item);
                }
            }
            else if (isMissileType())
            {
                if (item.EnemyLife > 0)
                    AttacableEnemies.Add(item);
            }
            else if (isPulseType())
            {
                if (Vector3.Distance(transform.position, item.transform.position) < 2)
                {
                    AttacableEnemies.Add(item);
                }
            }
        }
        Attack();
    }
    void Attack()
    {
        ableToAttack = (AttacableEnemies.Count > 0);
        if (ableToAttack)
        {
            if (isAutomaticType() || isMissileType())
            {
                foreach (var item in AttacableEnemies)
                {
                    if (item.EnemyLife > 0) // Enemigo con mas vida
                    {
                        EnemySelected = true;
                        target = item;
                        break;
                    }
                }
            }
            if (isPulseType())
            {

                foreach (var item in AttacableEnemies)
                {
                    if (Vector3.Distance(transform.position, item.transform.position) < 2)  //enemigo mas cercano
                    {
                        EnemySelected = true;
                        target = item;
                        break;
                    }
                }
            }

        }
    }
    void FireMissile()
    {
        missileHasExploded = false;
        missile.gameObject.SetActive(true);
        if (AttacableEnemies.Count > 0)
            missile.StartMissileGuidence(AttacableEnemies[0], this);
        else
            missileHasExploded = true;
    }
    void Fire(Vector3 direction)
    {
        CannonPooler++;
        BulletCounter++;
        if (BulletCounter >= bulletsInt)
            BulletCounter = 0;
        if (CannonPooler >= 2)
        {
            CannonPooler = 0;
        }
        try
        {
            SpawnBullet[BulletCounter].transform.position = TurretHeadCannon[CannonPooler].transform.position;
            SpawnBullet[BulletCounter].gameObject.SetActive(true);
            SpawnBullet[BulletCounter].velocity = TurretHeadCannon[CannonPooler].transform.forward * speed;
        }
        catch (System.Exception)
        {
            //Raras veces counter pasa el tamao del array 
            return;
        }

    }

}
