namespace ConstructMS.Repositories
{
    public interface IUnitOfWork
    {
        IConstructionRequestRepository ConstructionRequests { get; }
    }
}