using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    // -----------------------------------------------------------------------
    // REPOSITORY INTERFACE — WHAT IS IT AND WHY?
    // -----------------------------------------------------------------------
    // An interface defines a CONTRACT: "any class that implements this interface
    // MUST provide these methods". It lists WHAT you can do, not HOW.
    //
    // The CovenantRepository class in the Repositories/ folder provides the HOW
    // (the actual SQL queries). The interface lives here in Interfaces/.
    //
    // Why bother with an interface?
    //   1. SEPARATION OF CONCERNS: The BLL (business logic) talks to the
    //      ICovenantRepository interface, not to the concrete class. This means
    //      business logic doesn't care HOW data is stored.
    //
    //   2. TESTABILITY: In unit tests you can create a "fake" repository that
    //      implements this interface and returns hard-coded data — no real
    //      database needed to test business rules.
    //
    //   3. FLEXIBILITY: If you ever switch from SQL Server to PostgreSQL, you
    //      only write a new repository class. The services don't change at all.
    //
    // DEPENDENCY INJECTION (manual style used here):
    //   CovenantService receives ICovenantRepository in its constructor.
    //   The page (List.aspx.cs) creates: new CovenantService(new CovenantRepository(), ...)
    //   The service never knows it's using SQL Server — it only sees the interface.
    // -----------------------------------------------------------------------

    public interface ICovenantRepository
    {
        /// <summary>Returns ALL covenants including soft-deleted ones, newest first.</summary>
        IEnumerable<Covenant> GetAll();

        /// <summary>Returns non-deleted, non-completed covenants ordered by ProcessingDate.</summary>
        IEnumerable<Covenant> GetActive();

        /// <summary>Returns non-deleted, completed covenants ordered by last update.</summary>
        IEnumerable<Covenant> GetCompleted();

        /// <summary>Returns a single covenant by its primary key, or null if not found.</summary>
        Covenant GetById(int id);

        /// <summary>Inserts a new row and returns the new auto-generated Id (SCOPE_IDENTITY).</summary>
        int Insert(Covenant covenant);

        /// <summary>Updates all editable fields of an existing covenant row.</summary>
        void Update(Covenant covenant);

        /// <summary>
        /// SOFT DELETE: sets IsDeleted=1, records who deleted and when.
        /// The row stays in the database and can be restored.
        /// </summary>
        void SoftDelete(int id, string deletedBy);

        /// <summary>
        /// RESTORE: clears IsDeleted, DeletedAt, DeletedBy — brings the record back.
        /// </summary>
        void Restore(int id, string restoredBy);

        /// <summary>
        /// Returns non-deleted, non-completed covenants whose ProcessingDate falls
        /// within the next <paramref name="daysThreshold"/> days from now (UTC).
        /// Used by the NotificationEngine.
        /// </summary>
        IEnumerable<Covenant> GetApproachingProcessingDate(int daysThreshold);
    }
}
