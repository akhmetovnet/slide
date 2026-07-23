using Zenject;

namespace GameLogic
{
    public class BonusPool
    {
        private BPool _pool;
        
        public BonusPool(BPool pool)
        {
            _pool = pool;
        }
        
        public BonusController GetBonus()
        {
            if (_pool.NumInactive == 0)
                return null;

            var bonus = _pool.Spawn();
            bonus.gameObject.name = $"Bonus {bonus.GetHashCode()}";
            return bonus;
        }

        public void ReturnBonus(BonusController bonus)
        {
//            Debug.Log($"Thorn {bonus.GetHashCode()}");
            bonus.Clear();
            _pool.Despawn(bonus);
        }
        
        public class BPool: MonoMemoryPool<BonusController> {}

        public int Count()
        {
            return _pool.NumTotal;
        }
    }
}
