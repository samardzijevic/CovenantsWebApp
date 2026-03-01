using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    public class HistoryService
    {
        private readonly ICovenantHistoryRepository _repo;

        public HistoryService(ICovenantHistoryRepository repo)
        {
            _repo = repo;
        }

        public void Write(int covenantId, string action, string changedBy,
                          string fieldName = null, string oldValue = null,
                          string newValue = null, string notes = null)
        {
            _repo.Insert(new CovenantHistory
            {
                CovenantId = covenantId,
                Action     = action,
                FieldName  = fieldName,
                OldValue   = oldValue,
                NewValue   = newValue,
                ChangedBy  = changedBy,
                Notes      = notes
            });
        }
    }
}
