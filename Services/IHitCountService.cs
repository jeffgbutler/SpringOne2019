namespace PaymentService.Services
{
    public interface IHitCountService
    {
        long GetAndIncrement();
        void Reset();
    }
}
