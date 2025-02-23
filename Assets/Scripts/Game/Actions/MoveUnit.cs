using UnityEngine;

public class MoveUnit : Action
{
    private Vector2Int unitPos,unitNewPos;
    private bool isDeactivate;
    public MoveUnit(Vector2Int unitPos, Vector2Int unitNewPos, bool isDeactivate,Render render) : base(render)
    {
        this.unitPos = unitPos;
        this.unitNewPos = unitNewPos;
        this.isDeactivate = isDeactivate;
    }

    public override void Run(Render.Map map)
    {
        map.unitsMap.Get(unitPos).transform.position = render.LocalPosToGlobal(unitNewPos) + new Vector3(0, render.GC.GetSelectSpriteYoffset(unitNewPos),0);
        map.unitsMap.Move(unitPos, unitNewPos);

        if (isDeactivate)
            map.unitsMap.Get(unitNewPos).transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
}
