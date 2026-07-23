using UnityEngine;
using Zenject;

namespace GameLogic
{
    public class LinePool
    {
        private LPool _pool;
        
        public LinePool(LPool pool)
        {
            _pool = pool;
        }
        
        public LineController GetLine()
        {
            if (_pool.NumInactive == 0)
                return null;

            var line = _pool.Spawn();
            line.Collider.isTrigger = false;
            line.gameObject.name = $"Line {line.GetHashCode()}";
            return line;
        }

        public void ReturnLine(LineController line)
        {
//            Debug.Log($"Line {line.GetHashCode()}");
            line.Collider.isTrigger = false;
            _pool.Despawn(line);
        }
        
        public class LPool: MonoMemoryPool<LineController> {}

        public int Count()
        {
            return _pool.NumTotal;
        }
    }
    
}
