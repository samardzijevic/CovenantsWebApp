using System.Collections.Generic;
using Covenants.Models;

namespace Covenants.DAL.Interfaces
{
    public interface INotificationRepository
    {
        IEnumerable<Notification> GetByUserId(string userId);
        int GetUnreadCount(string userId);
        bool ExistsUnread(int covenantId, string userId, string type);
        int Insert(Notification notification);
        void MarkRead(int id);
        void MarkAllRead(string userId);
        void Dismiss(int id);
    }
}
