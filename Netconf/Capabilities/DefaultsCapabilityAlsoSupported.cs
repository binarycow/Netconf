
namespace Netconf;

[Flags]
public enum DefaultsCapabilityAlsoSupported
{
    None = 0x00,
    ReportAll = 0x01,
    ReportAllTagged = 0x02,
    Trim = 0x04,
    Explicit = 0x08,
}