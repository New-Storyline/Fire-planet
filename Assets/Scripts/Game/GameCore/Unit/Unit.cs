using System;
using UnityEngine;

namespace GameCore { 
    public abstract class Unit : GameObjectBase
    {
        public Player Owner { get; private set; }
        public float HP { get; internal set; }

        public bool isCanMoveInTurn = true;
        public bool isCanAttackInTurn = true;
        internal Unit(Vector2Int pos, Player owner) : base(pos)
        {
            Owner = owner;
            HP = GetMaxHP();
        }

        internal void OnMove(bool isCanAttackNearest)
        { 
            isCanMoveInTurn = false; 

            if(!isCanAttackNearest)
                isCanAttackInTurn = false;
        }

        internal void OnAttack(){
            isCanMoveInTurn = false;
            isCanAttackInTurn = false;
        }
        internal void OnKill(){Owner.RemoveUnit(this);}

        internal abstract void ApplyDamage(float damage, Unit unit);
        /// <summary>
        /// Resurrection is needed if during an attack the health of both units was below 0, in this case the one with higher health is resurrected, and the other dies
        /// </summary>
        internal void Resurrect() { HP = 0.1f; }


        public abstract float GetMaxHP();
        public abstract int GetMoveRange();
        public abstract int GetAttackRange();
        public abstract float GetDefenceDamage();
        public abstract float GetAttackDamage();
        public abstract int GetGoldCost();
        public abstract float GetPopulationCost();
    }
}