namespace BlazorSignalProxyServices.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SignalProxyServerSettings
{
    public string EndpointRelative { get; set; } = "/blazorsignalproxy";
    public bool UseResponseCompression { get; set; } = true;
}
