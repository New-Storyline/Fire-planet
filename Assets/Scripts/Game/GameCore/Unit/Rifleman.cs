using UnityEngine;

namespace GameCore { 
    public class Rifleman : Unit
    {
        public Rifleman(Vector2Int pos,Player owner) : base(pos, owner)
        {

        }

        public override int GetMoveRange() { return 1; }
        public override int GetAttackRange() { return 1; }
        public override float GetMaxHP(){ return 10;}

        public override float GetDefenceDamage()
        {
            return Mathf.Lerp(2,3,HP / GetMaxHP());
        }

        public override float GetAttackDamage()
        {
            return Mathf.Lerp(1, 2, HP / GetMaxHP());
        }

        public override int GetGoldCost()
        {
            return 5;
        }

        public override float GetPopulationCost()
        {
            return 10;
        }

        internal override void ApplyDamage(float damage, Unit damageSource)
        {
            HP -= damage;
        }
    }
}