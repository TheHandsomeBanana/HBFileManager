using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core.Notifications;
public abstract class Notification {
    public Guid Id { get; set; } = Guid.NewGuid();
    public abstract object GetContent();
}

public abstract class Notification<T> : Notification {
    public required T Content { get; set; }
}
