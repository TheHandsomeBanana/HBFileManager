using HBLibrary.Interface.Security.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Notifications;
public interface INotificationService {
    public void Notify(IAccountInfo account, Notification notification);
    public IEnumerable<Notification> EnumerateNotifications(IAccountInfo account);
    public void DeleteNotification(IAccountInfo account, Guid notificationId);
    public void ClearNotifications(IAccountInfo account);
}
