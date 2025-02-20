using UnityEngine;

public class Rifleman : Unit
{
    public Rifleman(Vector2Int pos,Player owner) : base(pos, owner)
    {

    }

    public override int GetMoveRange() { return 1; }
    public override int GetAttackRange() { return 1; }
    public override float GetMaxHP(){ return 10;}
}
