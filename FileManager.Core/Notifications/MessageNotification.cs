using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Notifications;
public sealed class MessageNotification : Notification<string> {
    public override object GetContent() {
        return Content;
    }
}
