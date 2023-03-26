namespace PaymentMS.src.Repositories
{
    public interface IUnitOfWork
    {
        IPaymentRepository Payments { get; }
    }
}