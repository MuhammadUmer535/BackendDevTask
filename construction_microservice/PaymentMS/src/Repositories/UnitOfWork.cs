namespace PaymentMS.src.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IPaymentRepository paymentRepository)
        {
            Payments = paymentRepository;
        }

        public IPaymentRepository Payments { get; }

    }
}