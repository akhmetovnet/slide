using UnityEngine;
using Zenject;

namespace GameLogic
{
    public class ThornPool
    {
        private TPool _pool;
        
        public ThornPool(TPool pool)
        {
            _pool = pool;
        }
        
        public ThornController GetThorn()
        {
            if (_pool.NumInactive == 0)
                return null;

            var thorn = _pool.Spawn();
            thorn.gameObject.name = $"Thorn {thorn.GetHashCode()}";
            return thorn;
        }

        public void ReturnThorn(ThornController thorn)
        {
//            Debug.Log($"Thorn {thorn.GetHashCode()}");
            thorn.Clear();
            _pool.Despawn(thorn);
        }
        
        public class TPool: MonoMemoryPool<ThornController> {}

        public int Count()
        {
            return _pool.NumTotal;
        }
    }
    
}
