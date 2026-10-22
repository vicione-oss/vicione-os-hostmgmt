# Supported Capabilities

The `SupportedCapabilities` of **HostManagement (HM)** describe which **settings** and/or **topics** the current system supports.

The feature is built from **compile-time metadata** and a **runtime configuration file**. Compile-time metadata defines the available capability schema. The runtime configuration enables or disables individual capabilities.

**ViciOne Suite**, **HM CLI** and **HM internal logic** should use `SupportedCapabilities` to determine which features are available.

The information is exposed via the `GetSupportedCapabilitiesResult`, which contains a `SupportedCapabilities` instance. It can be retrieved via the `GetSupportedCapabilities` pipe request.

> [!note]
> **HM internal logic** can access the information directly, without being dependent on the pipe request via the `SupportedCapabilitiesService`.

## INI Template

**HM** can create an INI template for the supported capabilities, by using the `--create-supported-capabilities-template` flag.\
The file must be named `SupportedCapabilities.conf` and located in the `DefaultSystemConfiguration` directory (`/etc/hostmanagement/` by default).

`sudo hostmanagement --create-supported-capabilities-template | sudo tee /etc/hostmanagement/SupportedCapabilities.conf > /dev/null`

The template **contains**:

* a generated header
* `[Settings.<path>]` sections for settings
* a `[Topics]` section for topics
* commented entries with compile-time default values

**Example:**

```ini
# This file describes the supported capabilities exposed by HostManagement.
# ...

[Settings]
#NTP=disabled
#Proxy=disabled
#Services=disabled

[Settings.DNS]
#Hostname=disabled
#StaticHosts=disabled

[Topics]
#InstallSignedDebianPackage=disabled
#UpdateSystem=disabled
```

All capabilities are **disabled by default**. To **enable a capability**, uncomment the entry and set it to `enabled`. The accepted values are `enabled` and `disabled` and are **case-insensitive**. Omitted or commented entries remain disabled.

## INI format

The supported INI format is intentionally small and specific to `SupportedCapabilities`.

* **Supported syntax:** section headers / `Key=Value` entries / blank lines / full-line comments (`#` or `;`)
* **Unsupported syntax:** inline comments / quoted or escaped values / multi-line values / entries outside a section

### Settings sections

**Settings** use section paths, that map to the generated settings hierarchy.

**Example:**

```ini
[Settings.DNS]
Hostname=enabled
```

The `Capability` property is **reserved** and must not be configured directly.

```ini
[Settings.DNS]
# This is invalid.
Capability=enabled
```

For parent settings, **HM** derives the parent capability from child settings. If any **child** is `Enabled`, the **parent** becomes also `Enabled`.

> [!note]
> Parent settings that contain child settings cannot be configured directly. Their child settings must be configured explicitly.

### Topics section

**Topics** are configured directly in the `[Topics]` section. **Unknown topics** are rejected during parsing.

**Example:**

```ini
[Topics]
RestartService=enabled
RestartSystem=disabled
```

## Application startup

During application startup, `SupportedCapabilitiesService`

1. **reads** the supported-capabilities **INI** configuration
1. **converts** the **INI** configuration **into** the **JSON** structure expected by the generated model
1. **initializes** a new `SupportedCapabilities` instance
1. **applies** the converted **configuration** by calling the `ApplyCapabilities` method
1. **caches** the configured instance in memory
1. **provides** the cached instance via the `GetSupportedCapabilities` pipe request

If **not initialized**, requests fail with an error result.\
If **retrieval fails**, an empty `SupportedCapabilities` instance is returned with **error status**.\
If **canceled**, an empty `SupportedCapabilities` instance is returned with **warning status**.

## Source generated model

The `SupportedCapabilities` model is generated at compile-time by `SupportedCapabilitiesGenerator`. It consists of:

* `SupportedCapabilities`
  * `SupportedSettings`
  * `SupportedTopics`

The structure is similar to:

```csharp
public sealed class SupportedCapabilities
{
    public SupportedSettings Settings { get; init; } = new();
    public SupportedTopics Topics { get; init; } = new();

    public void ApplyCapabilities(JsonElement configuration)
    {
        // Applies runtime configuration to Settings and Topics.
    }
}
```

`ApplyCapabilities` expects a JSON object and applies the `Settings` and `Topics` sections to the generated model. Unknown top-level sections are rejected.

The `SupportedCapabilities` model is split across multiple source generators.

### SupportedSettingsGenerator

The `SupportedSettingsGenerator` builds the `SupportedSettings` model, using **properties** defined in `SystemConfiguration`, **annotated** with `SupportedSettingAttribute`.

**Example:**

```csharp
[SupportedSetting]
public string? Hostname { get; init; }
```

A setting can also define an **explicit display name**:

```csharp
[SupportedSetting("DNS")]
public NetworkDNSSettings NetworkDNSSettings { get; init; }
```

If no explicit name is provided, the property name is used.

The generator creates:

* a `SupportedSettings` root type
* one generated `Support` record per supported setting occurrence (e.g.: `NetworkDNSSettingsSupport`)
* nested setting structures for hierarchical configuration objects
* intermediate settings for supported settings inside collection element types, if applicable (e.g.: `NetworkInterfaceDetailSupport` or `ServiceDetailSupport`)

The generated settings model contains a `Capability` value for each configurable setting. Settings that contain child settings derive their effective capability from their children.

**Capabilities** are represented by the `Capability` enum. Valid values are `Enabled` and `Disabled`.

### SupportedTopicsGenerator

The `SupportedTopicsGenerator` builds the `SupportedTopics` model, using **fields** defined in `Topics`, **annotated** with `SupportedTopicAttribute`.

**Example:**

```csharp
[SupportedTopic]
public const string GetSystemConfiguration = "GetSystemConfiguration";
```

A topic can also define an **explicit display name**:

```csharp
[SupportedTopic("GetNetworkStatus")]
public const string GetNetworkStatusInformation = "GetNetworkStatusInformation";
```

If no explicit name is provided, the field name is used.

The generated `SupportedTopics` type contains one capability entry per supported topic.

### SupportedCapabilitiesGenerator

The `SupportedCapabilitiesGenerator` creates the root `SupportedCapabilities` type.

It is registered as a **post-initialization** source generator and always emits the root type. It references the generated `SupportedSettings` and `SupportedTopics`.

It also contains an `ApplyCapabilities` method, which delegates runtime configuration to the generated `Settings` and `Topics` models.

### JSON source generation

`GetSupportedCapabilitiesResult` gets its **JSON metadata** from the `SourceGenerationContext` of `HostManagement.Shared.Communication`, like every other communication contract.

This requires the capability schema to be generated in `HostManagement.Shared`, one assembly upstream: a source generator cannot see the output of another source generator in the same compilation, so `SupportedCapabilities` has to reach the JSON source generator as a referenced type.

### Containing NuGet packages

| Type                             | Containing NuGet / Namespace        |
| -------------------------------- | ----------------------------------- |
| `GetSupportedCapabilitiesResult` | HostManagement.Shared.Communication |
| `SupportedCapabilities`          | HostManagement.Shared               |
| `SupportedTopics`                | HostManagement.Shared               |
| `SupportedSettings`              | HostManagement.Shared               |

## Using SupportedCapabilities

1. Add `SupportedSettingAttribute` or `SupportedTopicAttribute` to relevant production code.
1. **Build the project** so the source generators update the capability schema.
1. **Generate** a **template** with `--create-supported-capabilities-template` flag.
1. Store as `SupportedCapabilities.conf` in the `DefaultSystemConfiguration` directory.
1. **Uncomment and enable** entries for the target system. _Disabled entries can be removed._
1. **Verify effective capabilities** by using the `PipeCommunicationSample`.
