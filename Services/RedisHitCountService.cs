using StackExchange.Redis;

namespace PaymentService.Services
{
    public class RedisHitCountService : IHitCountService
    {
        private IConnectionMultiplexer _conn;
        public RedisHitCountService(IConnectionMultiplexer conn)
        {
            _conn = conn;
        }

        public long GetAndIncrement()
        {
            IDatabase db = _conn.GetDatabase();
            return db.StringIncrement("payment-calculator");
        }

        public void Reset()
        {
            IDatabase db = _conn.GetDatabase();
            db.StringSet("payment-calculator", 0);
        }
    }
}
