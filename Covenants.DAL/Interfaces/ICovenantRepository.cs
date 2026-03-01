using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    public interface ICovenantRepository
    {
        IEnumerable<Covenant> GetAll();
        IEnumerable<Covenant> GetActive();
        IEnumerable<Covenant> GetCompleted();
        Covenant GetById(int id);
        int Insert(Covenant covenant);
        void Update(Covenant covenant);
        void SoftDelete(int id, string deletedBy);
        void Restore(int id, string restoredBy);
        IEnumerable<Covenant> GetApproachingProcessingDate(int daysThreshold);
    }
}
