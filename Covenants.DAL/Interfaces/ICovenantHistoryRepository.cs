using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    public interface ICovenantHistoryRepository
    {
        IEnumerable<CovenantHistory> GetByCovenantId(int covenantId);
        void Insert(CovenantHistory history);
    }
}
