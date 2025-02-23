using UnityEngine;

public class AttackUnit : Action
{
    private readonly Vector2Int attackerPos;
    private readonly Vector2Int defenderPos;
    private readonly Unit attacker;
    private readonly Unit defender;

    public AttackUnit(Vector2Int attackerPos,Vector2Int defenderPos,Unit attacker, Unit defender,Render render) : base(render) {
        this.attackerPos = attackerPos;
        this.defenderPos = defenderPos;
        this.attacker = attacker;
        this.defender = defender;
    }

    public override void Run(Render.Map map)
    {
        GameObject attackerObj = map.unitsMap.Get(attackerPos);
        GameObject defenderObj = map.unitsMap.Get(defenderPos);

        UnitObject attackerUnitObj = attackerObj.GetComponent<UnitObject>();
        UnitObject defenderUnitObj = defenderObj.GetComponent<UnitObject>();

        if (attacker.HP <= 0)
        {
            Object.Destroy(attackerObj);
            defenderUnitObj.SetHP(defender.HP, defender.GetMaxHP());
        }
        else if (defender.HP <= 0)
        {
            Object.Destroy(defenderObj);
            attackerUnitObj.SetHP(attacker.HP, attacker.GetMaxHP());
            attackerObj.transform.position = render.LocalPosToGlobal(defenderPos) + new Vector3(0, render.GC.GetSelectSpriteYoffset(defenderPos), 0);
        }
        else {

            defenderUnitObj.SetHP(defender.HP, defender.GetMaxHP());
            attackerUnitObj.SetHP(attacker.HP, attacker.GetMaxHP());
        }
    }
}
