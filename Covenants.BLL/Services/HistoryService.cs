using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    // -----------------------------------------------------------------------
    // HISTORY SERVICE — SINGLE RESPONSIBILITY PRINCIPLE
    // -----------------------------------------------------------------------
    // The Single Responsibility Principle says: a class should do ONE thing.
    // Writing audit history is used by CovenantService, FollowUpService,
    // ScheduleService, and the scheduler engines — so it was extracted into
    // its own service rather than duplicated in each one.
    //
    // Every important change in the system calls HistoryService.Write().
    // This gives us a full audit trail: who changed what, when, and to what value.
    //
    // The method uses optional parameters (fieldName = null, etc.) so callers
    // only provide the information that applies to their situation:
    //
    //   histSvc.Write(id, "Created", userId, notes: "Covenant created.");
    //   histSvc.Write(id, "Updated", userId, "Title", oldTitle, newTitle);
    //   histSvc.Write(id, "Deleted", userId, notes: "Soft-deleted by admin.");
    // -----------------------------------------------------------------------

    public class HistoryService
    {
        // The underscore prefix on _repo is a naming convention for private fields.
        // 'readonly' means it can only be set in the constructor — good practice
        // to make the dependency clear and prevent accidental reassignment.
        private readonly ICovenantHistoryRepository _repo;

        // DEPENDENCY INJECTION (constructor injection):
        // Instead of creating "new CovenantHistoryRepository()" inside this class,
        // we receive it from outside. This makes the service testable and decoupled.
        public HistoryService(ICovenantHistoryRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Writes one row to the CovenantHistory table.
        /// </summary>
        /// <param name="covenantId">Which covenant this history row belongs to.</param>
        /// <param name="action">What happened (use Constants.HistoryActions).</param>
        /// <param name="changedBy">Username of the person (or "SYSTEM") who triggered the change.</param>
        /// <param name="fieldName">Optional: which field changed (e.g. "Title").</param>
        /// <param name="oldValue">Optional: what the value was before the change.</param>
        /// <param name="newValue">Optional: what the value is after the change.</param>
        /// <param name="notes">Optional: free-text description of what happened.</param>
        public void Write(int covenantId, string action, string changedBy,
                          string fieldName = null, string oldValue = null,
                          string newValue  = null, string notes    = null)
        {
            // We build the model object here and hand it to the repository.
            // The repository's only job is to INSERT it — no business logic there.
            _repo.Insert(new CovenantHistory
            {
                CovenantId = covenantId,
                Action     = action,
                FieldName  = fieldName,
                OldValue   = oldValue,
                NewValue   = newValue,
                ChangedBy  = changedBy,
                Notes      = notes
                // ChangedAt is set by the database (DEFAULT GETUTCDATE()) — no need to set it here.
            });
        }
    }
}
