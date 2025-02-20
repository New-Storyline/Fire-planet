using UnityEngine;

public class Map<T>
{
    private T[,] map;

    public Map(Vector2Int size)
    {
        map = new T[size.x, size.y];
    }

    public void Set(int x, int y, T value)
    {
        map[x, y] = value;
    }

    public void Set(Vector2Int pos, T value)
    {
        map[pos.x, pos.y] = value;
    }

    public T Get(Vector2Int pos)
    {
        return map[pos.x, pos.y];
    }

    public T Get(int x,int y)
    {
        return map[x, y];
    }

    public void Move(Vector2Int from, Vector2Int to)
    {
        if(map[to.x, to.y] != null)
            throw new System.Exception($"Cannot move to a non-empty tile! From: {from.ToString()}. To: {to.ToString()}. Class name: {map[from.x, from.y].GetType().Name}");

        T temp = map[from.x, from.y];
        map[from.x, from.y] = default;
        map[to.x, to.y] = temp;
    }

    public Vector2Int GetSize()
    {
        return new Vector2Int(map.GetLength(0), map.GetLength(1));
    }

    public int GetSize(int n)
    {
        return map.GetLength(n);
    }
}
