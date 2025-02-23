using UnityEngine;

namespace GameCore { 
    public class GameObjectBase
    {
        public GameObjectBase(Vector2Int pos)
        {
            position = pos;
        }

        private Vector2Int position;
        public Vector2Int GetPosition()
        {
            return position;
        }

        internal void SetPosition(Vector2Int position)
        {
            this.position = position;
        }

        internal void SetPosition(int x, int y)
        {
            position = new Vector2Int(x, y);
        }

        internal void MovePosition(Vector2Int offset)
        {
            position += offset;
        }

    }
}