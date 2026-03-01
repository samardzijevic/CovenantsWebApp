using System;
using System.Collections.Generic;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    // -----------------------------------------------------------------------
    // COVENANT SERVICE — THE BUSINESS LOGIC LAYER (BLL)
    // -----------------------------------------------------------------------
    // This is where the RULES of the application live. Examples:
    //   - You cannot edit a deleted covenant.
    //   - You cannot delete a covenant that is already deleted.
    //   - Every change must be recorded in the history table.
    //
    // The service talks to the DAL (repositories) and the HistoryService.
    // It NEVER talks to the UI — the UI talks to the service.
    //
    // LAYERED ARCHITECTURE:
    //   UI (aspx.cs pages)
    //       ↓ calls
    //   BLL (Services) ← you are here
    //       ↓ calls
    //   DAL (Repositories)
    //       ↓ calls
    //   SQL Server
    //
    // Each service method returns a Result<T> or Result so the caller knows
    // whether the operation succeeded without throwing exceptions.
    // -----------------------------------------------------------------------

    public class CovenantService
    {
        private readonly ICovenantRepository _repo;
        private readonly HistoryService _history;

        public CovenantService(ICovenantRepository repo, HistoryService history)
        {
            _repo    = repo;
            _history = history;
        }

        // ---------------------------------------------------------------
        // READ — simple pass-through to the repository
        // No business rules needed for reads.
        // ---------------------------------------------------------------

        public IEnumerable<Covenant> GetAll()       => _repo.GetAll();
        public IEnumerable<Covenant> GetActive()    => _repo.GetActive();
        public IEnumerable<Covenant> GetCompleted() => _repo.GetCompleted();
        public Covenant GetById(int id)             => _repo.GetById(id);

        // ---------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------

        public Result<int> Create(Covenant covenant, string createdBy)
        {
            try
            {
                covenant.CreatedBy = createdBy; // stamp who created it
                int id = _repo.Insert(covenant);

                // Write a "Created" entry to the audit trail.
                _history.Write(id, Constants.HistoryActions.Created, createdBy,
                    notes: string.Format("Covenant '{0}' created.", covenant.Title));

                // Return success with the new Id so the caller can redirect to the detail page.
                return Result<int>.Ok(id);
            }
            catch (Exception ex)
            {
                // Catch database errors (connection failure, constraint violation, etc.)
                // and wrap them in a Result.Fail so the UI can show the message.
                return Result<int>.Fail(ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------

        public Result Update(Covenant updated, string updatedBy)
        {
            try
            {
                // Business rule: load the existing record first so we can:
                //   1. Check it's not deleted (business rule).
                //   2. Compare old vs new values for field-level history.
                var existing = _repo.GetById(updated.Id);
                if (existing == null)       return Result.Fail("Covenant not found.");
                if (existing.IsDeleted)     return Result.Fail("Cannot edit a deleted covenant.");

                updated.UpdatedBy = updatedBy;
                _repo.Update(updated);

                // Record exactly which fields changed — this populates the History tab.
                WriteFieldHistory(existing, updated, updatedBy);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // SOFT DELETE
        // ---------------------------------------------------------------

        public Result SoftDelete(int id, string deletedBy)
        {
            try
            {
                var existing = _repo.GetById(id);
                if (existing == null)    return Result.Fail("Covenant not found.");
                if (existing.IsDeleted)  return Result.Fail("Covenant is already deleted.");

                _repo.SoftDelete(id, deletedBy);
                _history.Write(id, Constants.HistoryActions.Deleted, deletedBy,
                    notes: string.Format("Covenant soft-deleted by {0}.", deletedBy));

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // RESTORE
        // ---------------------------------------------------------------

        public Result Restore(int id, string restoredBy)
        {
            try
            {
                var existing = _repo.GetById(id);
                if (existing == null)    return Result.Fail("Covenant not found.");
                if (!existing.IsDeleted) return Result.Fail("Covenant is not deleted.");

                _repo.Restore(id, restoredBy);
                _history.Write(id, Constants.HistoryActions.Restored, restoredBy,
                    notes: string.Format("Covenant restored by {0}.", restoredBy));

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // FIELD-LEVEL HISTORY (private helper)
        // ---------------------------------------------------------------
        // Called from Update() to compare each field individually.
        // Only fields that actually changed get a history row — no noise.

        private void WriteFieldHistory(Covenant old, Covenant updated, string by)
        {
            Check(old.Id, "Title",          old.Title,                       updated.Title,          by);
            Check(old.Id, "Description",    old.Description,                 updated.Description,    by);
            Check(old.Id, "ProcessingDate", old.ProcessingDate.ToString("O"), updated.ProcessingDate.ToString("O"), by);
            Check(old.Id, "Value",          old.Value?.ToString(),            updated.Value?.ToString(),            by);
            Check(old.Id, "Currency",       old.Currency,                    updated.Currency,       by);
            Check(old.Id, "Status",         old.Status,                      updated.Status,         by);
            Check(old.Id, "CovenantTypeId", old.CovenantTypeId.ToString(),   updated.CovenantTypeId.ToString(),    by);
        }

        // Writes a history row only if the old and new values differ.
        private void Check(int id, string field, string oldVal, string newVal, string by)
        {
            if (oldVal != newVal)
                _history.Write(id, Constants.HistoryActions.Updated, by, field, oldVal, newVal);
        }
    }
}
