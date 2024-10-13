using HBLibrary.Common.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Notifications;
public class NotificationService : INotificationService {
    public void ClearNotifications(AccountInfo account) {
        throw new NotImplementedException();
    }

    public void DeleteNotification(AccountInfo account, Guid notificationId) {
        throw new NotImplementedException();
    }

    public IEnumerable<Notification> EnumerateNotifications(AccountInfo account) {
        throw new NotImplementedException();
    }

    public void Notify(AccountInfo account, Notification notification) {
        throw new NotImplementedException();
    }
}
