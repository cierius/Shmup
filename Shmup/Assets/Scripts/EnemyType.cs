using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EnemyType
{
    // States
    public enum AIState
    {
        Idle,
        Moving,
        MovingAndSearching,
        Attacking,
        Searching
    }
    private AIState aiState;

    public enum AttackState
    {
        NotAttacking,
        WindUp,
        Attacking,
        Cooldown
    }
    private AttackState attackState;

    // Basic stats
    public float moveSpeed;
    public int health;
    public int attackDamage;
    public bool invuln;

    // Attack & pathfinding vars
    public int minRangeSearch;
    public float attackRange;
    public float windUpTimer;
    public float AttackDurationTimer;
    public float attackDuration;


    public EnemyType(float _speed, int _health, int _attackDamage, float _attackRange, AIState _defaultState=AIState.Idle)
    {
        moveSpeed = _speed;
        health = _health;
        
        aiState = _defaultState;

        attackDamage = _attackDamage;
        attackRange = _attackRange;

        // Default variables assigned
        invuln = false;
        attackState = AttackState.NotAttacking;
        minRangeSearch = 2;
        windUpTimer = 0;
        AttackDurationTimer = 0;
        attackDuration = 0;
    }

    public void SetAIState(AIState state) => aiState = state;
    public AIState GetAIState() => aiState;

}
