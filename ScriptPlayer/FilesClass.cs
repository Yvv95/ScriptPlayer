using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptPlayer
{

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class FilesNames
    {
        public List<string> Files;
    }

    [SettingsSerializeAs(SettingsSerializeAs.String)]
    public class fileItem
    {
        public string file { get; set; }
    }

}
