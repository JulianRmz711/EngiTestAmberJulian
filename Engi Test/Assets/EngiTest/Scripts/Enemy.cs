using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    public NavMeshAgent Agent { get => agent; }
    List<Transform> Points = new List<Transform>();
    public Transform destination;
    // seteado en prefab via editor
    public float Cooldown;
    public int Damage;
    public int EnemyLife;
    public int actualPosition;
    [SerializeField] bool isInCore;
    //On trigger stay es llamado en cada fixedUpdate de las fisicas debemos controlar eso
    bool isAffectedByPulse, isAttacking;
    public GameCore gameCoreInstance;
    public float distance;
    public void SetDestination(List<Transform> points)
    {
        Points = points;
        SetDestination(0, false);
        Agent.speed = 0;
    }
    void SetDestination(int cast, bool isCount)
    {
        if (isCount)
        {
            actualPosition++;
            if (actualPosition >= Points.Count) isInCore = true;
        }
        if (actualPosition < Points.Count)
        {
            destination = Points[actualPosition];
            Agent.destination = destination.localPosition + Random.insideUnitSphere * .25f;
        }
    }
    public void StartWalking()
    {
        Agent.speed = 1;
    }
    public void OptiUpdate()
    {
        distance = Vector3.Distance(Agent.transform.localPosition, destination.localPosition);
        if (distance < 1.5f)
        {
            if (isInCore && !isAttacking)
            {
                isAttacking = true;
                InvokeRepeating("Attack", Cooldown, Cooldown);
            }
            else if (!isInCore)
                SetDestination(actualPosition, true);
        }
    }
    // Con animaciones y eventos de aniamcion solo seria necesario llamar el estado de animacion y refrescar el ataque con sus eventos
    void Attack()
    {
        gameCoreInstance.DamageCore(Damage);
    }

    //Hardcoded params para rapidez, generalmente checaria el tipo de trigger de una lista disponible y obtendria el damage de un Scriptable objet o playeprefb
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Automatic"))
        {
            OnDamageToEnemy(2);
        }
        if (other.CompareTag("Missile"))
        {
            OnDamageToEnemy(10);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Pulse"))
        {
            OnPulse();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pulse"))
        {
            PulseOff();
        }
    }

    public void OnDamageToEnemy(int damage)
    {
        EnemyLife -= damage;
        if (EnemyLife <= 0)
        {
            KillEnemy();
        }
    }
    public void OnPulse()
    {
        if (isAffectedByPulse) return;
        else if (!isAffectedByPulse)
        {
            Agent.speed = .5f;
            Invoke("PulseOff", 2.3f);
        }
    }
    void PulseOff()
    {
        Agent.speed = 1f;
        isAffectedByPulse = false;
    }
    void KillEnemy()
    {
        gameCoreInstance.OnEnemyKill(this);
        gameObject.SetActive(false);
    }
}