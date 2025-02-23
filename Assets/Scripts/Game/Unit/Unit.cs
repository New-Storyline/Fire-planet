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

    public void OnMove(bool isCanAttackNearest)
    { 
        isCanMoveInTurn = false; 

        if(!isCanAttackNearest)
            isCanAttackInTurn = false;
    }

    public void OnAttack(){
        isCanMoveInTurn = false;
        isCanAttackInTurn = false;
    }
    public void OnKill(){Owner.RemoveUnit(this);}

    public abstract float GetMaxHP();
    public abstract int GetMoveRange();
    public abstract int GetAttackRange();
    public abstract float GetDefenceDamage();
    public abstract float GetAttackDamage();
    public abstract void ApplyDamage(float damage,Unit unit);
    /// <summary>
    /// Resurrection is needed if during an attack the health of both units was below 0, in this case the one with higher health is resurrected, and the other dies
    /// </summary>
    public void Resurrect() { HP = 0.1f; }

    internal abstract int GetGoldCost();
    public abstract float GetPopulationCost();
}
