using System;
using UnityEngine;

public abstract class Unit : GameObjectBase
{
    public Player Owner { get; private set; }
    public float HP { get; internal set; }

    public bool isCanMoveInTurn = true;
    public bool isCanAttackInTurn = true;
    public Unit(Vector2Int pos, Player owner) : base(pos)
    {
        Owner = owner;
        HP = GetMaxHP();
    }

    public abstract float GetMaxHP();
    public abstract int GetMoveRange();
    public abstract int GetAttackRange();

    public void OnMove(bool isCanAttackNearest)
    { 
        isCanMoveInTurn = false; 

        if(!isCanAttackNearest)
            isCanAttackInTurn = false;
    }
    public void OnAttack(){ isCanAttackInTurn = false;}
}
