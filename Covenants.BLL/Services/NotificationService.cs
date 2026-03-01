using System;
using System.Collections.Generic;
using Covenants.Common;
using Covenants.DAL.Interfaces;
using Covenants.Models;

namespace Covenants.BLL.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Notification> GetForUser(string userId) =>
            _repo.GetByUserId(userId);

        public int GetUnreadCount(string userId) =>
            _repo.GetUnreadCount(userId);

        public Result Create(int covenantId, string userId, string type, string message)
        {
            try
            {
                // Avoid duplicate unread notifications of the same type for the same covenant
                if (_repo.ExistsUnread(covenantId, userId, type))
                    return Result.Ok();

                _repo.Insert(new Notification
                {
                    CovenantId = covenantId,
                    UserId     = userId,
                    Type       = type,
                    Message    = message
                });
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        public void MarkRead(int id) => _repo.MarkRead(id);

        public void MarkAllRead(string userId) => _repo.MarkAllRead(userId);

        public void Dismiss(int id) => _repo.Dismiss(id);
    }
}
