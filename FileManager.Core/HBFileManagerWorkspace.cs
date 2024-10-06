using HBLibrary.Common.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Core;
public class HBFileManagerWorkspace : ApplicationWorkspace {
    public string Name { get; }
    public HBFileManagerWorkspace(string name) {
        this.Name = name;
    }


}
