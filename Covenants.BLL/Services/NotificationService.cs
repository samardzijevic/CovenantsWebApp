using System;
using System.Collections.Generic;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    // -----------------------------------------------------------------------
    // NOTIFICATION SERVICE — IN-APP NOTIFICATIONS
    // -----------------------------------------------------------------------
    // Notifications appear in the bell icon (🔔) in the navbar and on the
    // Notifications/List.aspx page. They are created by the NotificationEngine
    // background thread when a covenant's ProcessingDate is approaching.
    //
    // Key design decision: notifications are IN-APP ONLY (no email, no SMS).
    // This keeps the system simple — the web page polls for unread count.
    //
    // Duplicate prevention:
    //   The NotificationEngine fires every 60 minutes. Without a duplicate
    //   check, a covenant with 7 days left would generate 7*24 = 168 identical
    //   notifications. The ExistsUnread() check ensures only ONE unread
    //   notification of each type exists per covenant per user.
    //
    // UserId = null means "broadcast to all users" — useful when there is no
    // authentication or when a notification is relevant to the whole team.
    // -----------------------------------------------------------------------

    public class NotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        // -------------------------------------------------------------------
        // READ
        // -------------------------------------------------------------------

        /// <summary>Returns all notifications for a specific user (including read ones).</summary>
        public IEnumerable<Notification> GetForUser(string userId) =>
            _repo.GetByUserId(userId);

        /// <summary>
        /// Returns the count of unread notifications for the bell icon badge.
        /// Called on every page load from Site.Master's Page_Load.
        /// </summary>
        public int GetUnreadCount(string userId) =>
            _repo.GetUnreadCount(userId);

        // -------------------------------------------------------------------
        // CREATE — called by NotificationEngine
        // -------------------------------------------------------------------

        /// <summary>
        /// Creates a new notification, but only if no unread notification of the
        /// same type already exists for this covenant+user combination.
        /// This prevents duplicate alerts for the same approaching deadline.
        /// </summary>
        /// <param name="covenantId">Which covenant this notification is about.</param>
        /// <param name="userId">Which user to notify (null = all users).</param>
        /// <param name="type">Category from Constants.NotificationTypes.</param>
        /// <param name="message">Human-readable text shown in the notification list.</param>
        public Result Create(int covenantId, string userId, string type, string message)
        {
            try
            {
                // IDEMPOTENCY GUARD: if an unread notification of this type already
                // exists for this covenant+user, return Ok() without inserting a duplicate.
                // This means calling Create() many times is safe — it's "idempotent".
                if (_repo.ExistsUnread(covenantId, userId, type))
                    return Result.Ok();

                _repo.Insert(new Notification
                {
                    CovenantId = covenantId,
                    UserId     = userId,     // null = broadcast
                    Type       = type,
                    Message    = message
                    // IsRead defaults to 0 (false) — set by the DB column DEFAULT
                    // CreatedAt defaults to GETUTCDATE() — also set by the DB
                });
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        // -------------------------------------------------------------------
        // MARK READ / DISMISS — called from Notifications/List.aspx
        // -------------------------------------------------------------------

        /// <summary>Marks a single notification as read (sets IsRead=1, ReadAt=now).</summary>
        public void MarkRead(int id) => _repo.MarkRead(id);

        /// <summary>Marks ALL of a user's notifications as read at once ("Mark All Read" button).</summary>
        public void MarkAllRead(string userId) => _repo.MarkAllRead(userId);

        /// <summary>
        /// Dismisses (soft-deletes) a notification by setting DismissedAt.
        /// Dismissed notifications are hidden from the list but stay in the DB for audit.
        /// </summary>
        public void Dismiss(int id) => _repo.Dismiss(id);
    }
}
