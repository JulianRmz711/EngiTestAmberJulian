using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour
{
    public GameCore gameCore;
    public List<Transform> points = new List<Transform>();
    [Tooltip("Visible para debug")] public int health;
    public int m_Health;
    public int Health
    {
        get => health;
        set => health = value;
    }
    private void Start()
    {
        Health = m_Health;
    }
    public void DamageCore(int damage)
    {
        int actualHealth = Health;
        actualHealth -= damage;
        if (actualHealth <= 0)
        {
            gameCore.SetWinLoseMessage(false);
        }
        Health = actualHealth;
    }

}
