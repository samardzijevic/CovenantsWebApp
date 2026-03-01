using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    public interface ICovenantFollowUpRepository
    {
        IEnumerable<CovenantFollowUp> GetByCovenantId(int covenantId);
        CovenantFollowUp GetById(int id);
        int Insert(CovenantFollowUp followUp);
        void Update(CovenantFollowUp followUp);
        void UpdateStatus(int id, string status, string updatedBy);
        void Start(int id, string startedBy);
        void Complete(int id, string completedBy, string completionNotes, string status);
        void Cancel(int id, string updatedBy);
    }
}
