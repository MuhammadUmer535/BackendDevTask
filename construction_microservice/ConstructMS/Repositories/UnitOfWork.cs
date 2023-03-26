namespace ConstructMS.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IConstructionRequestRepository constructionRequestRepository)
        {
            ConstructionRequests = constructionRequestRepository;
        }

        public IConstructionRequestRepository ConstructionRequests { get; }

    }
}