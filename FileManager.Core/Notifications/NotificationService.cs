using HBLibrary.Interface.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Notifications;
public class NotificationService : INotificationService {
    public void ClearNotifications(IAccountInfo account) {
        throw new NotImplementedException();
    }

    public void DeleteNotification(IAccountInfo account, Guid notificationId) {
        throw new NotImplementedException();
    }

    public IEnumerable<Notification> EnumerateNotifications(IAccountInfo account) {
        throw new NotImplementedException();
    }

    public void Notify(IAccountInfo account, Notification notification) {
        throw new NotImplementedException();
    }
}
