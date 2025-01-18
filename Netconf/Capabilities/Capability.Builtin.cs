namespace Netconf;

// ReSharper disable InconsistentNaming
public partial class Capability
{
    
    public static Capability WritableRunning { get; } = Get(
        "urn:ietf:params:netconf:capability:writable-running:1.0",
        ":writable-running	",
        ["RFC6241"]
    );

    public static Capability Candidate { get; } = Get(
        "urn:ietf:params:netconf:capability:candidate:1.0",
        ":candidate",
        ["RFC6241"]
    );

    public static Capability ConfirmedCommit { get; } = Get(
        "urn:ietf:params:netconf:capability:confirmed-commit:1.0",
        ":confirmed-commit",
        ["RFC4741"]
    );

    public static Capability ConfirmedCommit__1_1 { get; } = Get(
        "urn:ietf:params:netconf:capability:confirmed-commit:1.1",
        ":confirmed-commit:1.1",
        ["RFC6241"]
    );

    public static Capability RollbackOnError { get; } = Get(
        "urn:ietf:params:netconf:capability:rollback-on-error:1.0",
        ":rollback-on-error",
        ["RFC6241"]
    );

    public static Capability Validate { get; } = Get(
        "urn:ietf:params:netconf:capability:validate:1.0",
        ":validate",
        ["RFC4741"]
    );

    public static Capability Validate__1_1 { get; } = Get(
        "urn:ietf:params:netconf:capability:validate:1.1",
        ":validate:1.1",
        ["RFC6241"]
    );

    public static Capability Startup { get; } = Get(
        "urn:ietf:params:netconf:capability:startup:1.0",
        ":startup",
        ["RFC6241"]
    );

    public static Capability Url { get; } = Get(
        "urn:ietf:params:netconf:capability:url:1.0",
        ":url",
        ["RFC6241"]
    );

    public static Capability XPath { get; } = Get(
        "urn:ietf:params:netconf:capability:xpath:1.0",
        ":xpath",
        ["RFC6241"]
    );

    public static Capability Notification { get; } = Get(
        "urn:ietf:params:netconf:capability:notification:1.0",
        ":notification",
        ["RFC5277"]
    );

    public static Capability Interleave { get; } = Get(
        "urn:ietf:params:netconf:capability:interleave:1.0",
        ":interleave",
        ["RFC5277"]
    );

    public static Capability PartialLock { get; } = Get(
        "urn:ietf:params:netconf:capability:partial-lock:1.0",
        ":partial-lock",
        ["RFC5717"]
    );

    public static Capability WithDefaults { get; } = Get(
        "urn:ietf:params:netconf:capability:with-defaults:1.0",
        ":with-defaults",
        ["RFC6243"]
    );

    public static Capability Base__1_0 { get; } = Get(
        "urn:ietf:params:netconf:base:1.0",
        ":base:1.0",
        ["RFC4741", "RFC6241"]
    );

    public static Capability Base => Base__1_0;

    public static Capability Base__1_1 { get; } = Get(
        "urn:ietf:params:netconf:base:1.1",
        ":base:1.1",
        ["RFC6241"]
    );

    public static Capability Time__1_0 { get; } = Get(
        "urn:ietf:params:netconf:capability:time:1.0",
        ":time:1.0",
        ["RFC7758"]
    );

    public static Capability Time => Time__1_0;

    public static Capability YangLibrary { get; } = Get(
        "urn:ietf:params:netconf:capability:yang-library:1.0",
        ":yang-library",
        ["RFC7950"]
    );

    public static Capability YangLibrary__1_1 { get; } = Get(
        "urn:ietf:params:netconf:capability:yang-library:1.1",
        ":yang-library:1.1",
        ["RFC8526"]
    );

    public static Capability WithOperationalDefaults { get; } = Get(
        "urn:ietf:params:netconf:capability:with-operational-defaults:1.0",
        ":with-operational-defaults",
        ["RFC8526"]
    );
}