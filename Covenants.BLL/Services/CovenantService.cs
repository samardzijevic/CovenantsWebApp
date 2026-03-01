using System;
using System.Collections.Generic;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    public class CovenantService
    {
        private readonly ICovenantRepository _repo;
        private readonly HistoryService _history;

        public CovenantService(ICovenantRepository repo, HistoryService history)
        {
            _repo    = repo;
            _history = history;
        }

        public IEnumerable<Covenant> GetAll()    => _repo.GetAll();
        public IEnumerable<Covenant> GetActive()    => _repo.GetActive();
        public IEnumerable<Covenant> GetCompleted() => _repo.GetCompleted();
        public Covenant GetById(int id)             => _repo.GetById(id);

        public Result<int> Create(Covenant covenant, string createdBy)
        {
            try
            {
                covenant.CreatedBy = createdBy;
                int id = _repo.Insert(covenant);
                _history.Write(id, Constants.HistoryActions.Created, createdBy,
                    notes: $"Covenant '{covenant.Title}' created.");
                return Result<int>.Ok(id);
            }
            catch (Exception ex)
            {
                return Result<int>.Fail(ex.Message);
            }
        }

        public Result Update(Covenant updated, string updatedBy)
        {
            try
            {
                var existing = _repo.GetById(updated.Id);
                if (existing == null) return Result.Fail("Covenant not found.");
                if (existing.IsDeleted) return Result.Fail("Cannot edit a deleted covenant.");

                updated.UpdatedBy = updatedBy;
                _repo.Update(updated);

                // Record field-level changes
                WriteFieldHistory(existing, updated, updatedBy);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result SoftDelete(int id, string deletedBy)
        {
            try
            {
                var existing = _repo.GetById(id);
                if (existing == null) return Result.Fail("Covenant not found.");
                if (existing.IsDeleted) return Result.Fail("Covenant is already deleted.");

                _repo.SoftDelete(id, deletedBy);
                _history.Write(id, Constants.HistoryActions.Deleted, deletedBy,
                    notes: $"Covenant soft-deleted by {deletedBy}.");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public Result Restore(int id, string restoredBy)
        {
            try
            {
                var existing = _repo.GetById(id);
                if (existing == null) return Result.Fail("Covenant not found.");
                if (!existing.IsDeleted) return Result.Fail("Covenant is not deleted.");

                _repo.Restore(id, restoredBy);
                _history.Write(id, Constants.HistoryActions.Restored, restoredBy,
                    notes: $"Covenant restored by {restoredBy}.");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        private void WriteFieldHistory(Covenant old, Covenant updated, string by)
        {
            Check(old.Id, "Title",          old.Title,                  updated.Title,          by);
            Check(old.Id, "Description",    old.Description,            updated.Description,    by);
            Check(old.Id, "ProcessingDate", old.ProcessingDate.ToString("O"), updated.ProcessingDate.ToString("O"), by);
            Check(old.Id, "Value",          old.Value?.ToString(),       updated.Value?.ToString(),   by);
            Check(old.Id, "Currency",       old.Currency,               updated.Currency,        by);
            Check(old.Id, "Status",         old.Status,                 updated.Status,          by);
            Check(old.Id, "CovenantTypeId", old.CovenantTypeId.ToString(), updated.CovenantTypeId.ToString(), by);
        }

        private void Check(int id, string field, string oldVal, string newVal, string by)
        {
            if (oldVal != newVal)
                _history.Write(id, Constants.HistoryActions.Updated, by, field, oldVal, newVal);
        }
    }
}
