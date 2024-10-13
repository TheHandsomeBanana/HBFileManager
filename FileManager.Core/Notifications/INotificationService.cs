using HBLibrary.Common.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Notifications;
public interface INotificationService {
    public void Notify(AccountInfo account, Notification notification);
    public IEnumerable<Notification> EnumerateNotifications(AccountInfo account);
    public void DeleteNotification(AccountInfo account, Guid notificationId);
    public void ClearNotifications(AccountInfo account);
}
