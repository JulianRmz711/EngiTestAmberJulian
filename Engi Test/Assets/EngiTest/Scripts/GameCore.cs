using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCore : MonoBehaviour
{
    // Singleton 
    public static GameCore instance;
    public static GameCore Instance { get => instance; set => instance = value; }
    private void Awake()
    {
        instance = this;
    }

    // Varibles visibles en el editor, no modificables en el editor

    [SerializeField] List<Turret> turrets = new List<Turret>();
    public List<Turret> Turrets { get => turrets; set => turrets = value; }
    [SerializeField] List<Enemy> enemies;
    public List<Enemy> Enemies { get => enemies; set => enemies = value; }

    // Variables iniciadas en el editor
    [SerializeField] GameMessage gameMessage;
    [SerializeField] Life life;
    public Life m_Life { get => life; }
    [SerializeField] GameObject smallEnemyPrefab;
    [SerializeField] GameObject normalEnemyPrefab;
    [SerializeField] GameObject bigEnemyPrefab;
    [SerializeField] GameObject turretAutomatic;
    [SerializeField] GameObject turretMissile;
    [SerializeField] GameObject turretPulse;
    [SerializeField, Tooltip("Donde se castearan los enemigos")] Transform enemyCastPos;
    [SerializeField] int enemiesNumber;
    int enemiesKilledCounter;

    //coroutine settings
    bool isGameStarded;
    WaitForSeconds secsCache;
    private void Start()
    {
        CastEnemies(enemiesNumber);
        secsCache = new WaitForSeconds(.33f);
        m_Life.gameCore = Instance;
    }

    //casteo los enemigos
    void CastEnemies(int castValues)
    {
        for (int i = 0; i < castValues; i++)
        {
            int ran = Random.Range(0, 3);
            GameObject prefab = ran == 0 ? smallEnemyPrefab : ran == 1 ? normalEnemyPrefab : ran == 2 ? bigEnemyPrefab : smallEnemyPrefab;
            GameObject newEnemy = Instantiate(prefab, enemyCastPos.localPosition, Quaternion.identity) as GameObject;
            Enemy enemy = newEnemy.GetComponent<Enemy>();
            //enemy.enemyType = (EnemyType)ran; ya esta seteado en el prefab
            enemy.gameCoreInstance = Instance;
            enemy.SetDestination(m_Life.points);
            if (!Enemies.Contains(enemy))
            {
                Enemies.Add(enemy);
            }
        }
    }

    //Casteo las torretas 
    public void CastTurret(TurretType turretType, Vector3 pos)
    {
        if (turretType.Equals(TurretType.SelectTurret)) return;
        GameObject prefab = turretType.Equals(TurretType.Automatic) ? turretAutomatic :
        turretType.Equals(TurretType.Missile) ? turretMissile :
        turretType.Equals(TurretType.Pulse) ? turretPulse : turretAutomatic;
        Vector3 posRelY = new Vector3(pos.x, .5f, pos.z); // Evitar que haya torretas flotando
        GameObject newTurret = Instantiate(prefab, posRelY, Quaternion.identity) as GameObject;
        Turret turret = newTurret.GetComponent<Turret>();
        turret.gameCore = this;
        turret.Enemies = Enemies;
        turret.turretType = turretType;
        turret.StartTurret();

        if (!Turrets.Contains(turret))
        {
            Turrets.Add(turret);
        }
    }
    //Kick off del juego
    public void StartGame()
    {
        if (isGameStarded) return;
        else
        {
            isGameStarded = true;
            StopAllCoroutines();
            StartCoroutine(EnemySpammer());
        }
    }
    // Poolear la funcion update para performance
    private void Update()
    {
        for (int i = 0; i < Enemies.Count; i++)
        {
            Enemies[i].OptiUpdate();
        }
    }

    //Dañar el core
    public void DamageCore(int damage)
    {
        m_Life.DamageCore(damage);
    }

    IEnumerator EnemySpammer()
    {
        foreach (var item in Enemies)
        {
            yield return secsCache;
            item.StartWalking();
        }
    }
    public void OnEnemyKill(Enemy enemy)
    {
        Enemies.Remove(enemy);
        enemiesKilledCounter++;
        if (enemiesKilledCounter >= enemiesNumber)
        {
            gameMessage.SetMessage(true);
        }
    }
    public void SetWinLoseMessage(bool value) => gameMessage.SetMessage(value);
}


// Enums para el tipo de torretas
public enum TurretType
{
    SelectTurret, Automatic, Missile, Pulse
}