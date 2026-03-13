# Remote Tuning Package Documentation

## Purpose

This package provides a small remote tuning framework for Unity projects, covering:

- remote config fetch
- cache TTL and stale-serving rules
- feature flags
- deterministic A/B experiments
- persisted assignments
- safe fallback behavior

It is intended as a practical runtime package and portfolio-quality example, not a full live-ops platform.

---

## Public API

### RemoteTuningFactory

Creates a default-wired `RemoteTuningClient` using built-in runtime adapters.

#### Main entry point
- `RemoteTuningFactory.CreateDefault(RemoteTuningOptions options, ILogSink log = null)`

Returns:
- `RemoteTuningFactory.BuildResult`
  - `Client`
  - `TransportToggle`

---

### RemoteTuningOptions

Basic config for the runtime client.

#### Main fields
- `EndpointUrl`
- `TimeoutSeconds`
- `TtlSeconds`
- `StaleSeconds`

---

### RemoteTuningClient

Main runtime entry point.

#### Main members
- `Current`
- `UserId`
- `SetUserId(string userId)`
- `RefreshAsync()`
- `Flags`
- `Values`
- `Experiments`

#### Notes
- `Current` exposes source, reason, cache status, timestamps, config version, and raw JSON.
- `RefreshAsync()` updates the current snapshot using transport + cache + validation rules.

---

### FeatureFlags

Boolean feature flag access and experiment variant access.

#### Main methods
- `IsEnabled(string key, bool defaultValue = false)`
- `GetVariant(string experimentKey, string defaultVariant = "control")`

#### Notes
- `IsEnabled("iapVisible")` resolves to `features.iapVisible`
- Missing or invalid values fall back safely

---

### RemoteTuningValues

Typed access to config values.

#### Main methods
- `GetDifficultyScalar(float defaultValue = 1.0f)`
- `GetAdCooldownSeconds(int defaultValue = 60)`
- `GetRewardMultiplier(float defaultValue = 1.0f)`
- `GetBool(string dottedPath, bool defaultValue)`
- `GetInt(string dottedPath, int defaultValue)`
- `GetFloat(string dottedPath, float defaultValue)`
- `GetString(string dottedPath, string defaultValue = "")`

#### Notes
- Uses dotted paths like `tuning.adCooldownSeconds`
- Missing or invalid values return defaults

---

### Experiments

Current-user experiment API.

#### Main methods
- `GetVariant(string experimentKey, string defaultVariant = "control")`
- `IsInExperiment(string experimentKey)`
- `SetOverride(string experimentKey, string variantKey)`
- `ClearOverride(string experimentKey)`

#### Notes
- Assignment is deterministic
- Variants persist per `(userId, experimentKey)`
- Overrides are intended for demo/testing

---

### ToggleableTransport

Public wrapper around transport to simulate offline mode in demos.

#### Main members
- `IsOffline`
- `SetOffline(bool offline)`

---

## Extension Points

### Transport
Implement `IConfigTransport` to replace network behavior.

Use this if you want:
- custom HTTP stack
- mock/local transport
- integration with your own backend layer

---

### Persistence
Implement `IKeyValueStore` to replace PlayerPrefs-backed persistence.

Use this if you want:
- file-based persistence
- encrypted persistence
- custom save systems

---

### Clock
Implement `IClock` for deterministic time control or testing.

Useful for:
- unit tests
- simulated time
- custom environments

---

### Logging
Implement `ILogSink` for custom diagnostics.

Useful for:
- structured logs
- production telemetry
- editor tooling

---

### Hashing
Implement `IStableHasher` if a different deterministic bucketing strategy is needed.

Useful for:
- custom hashing standards
- compatibility with existing experiment systems

---

## Internal Concepts

### Cache semantics
- `Miss`
- `Fresh`
- `Stale`
- `Expired`

#### Meaning
- `Miss` → no usable cache
- `Fresh` → serve cache confidently
- `Stale` → serve cache, but revalidation should happen
- `Expired` → cache no longer served

---

### Validation
Remote config is accepted only after:
- JSON parse success
- required fields present
- type checks pass
- range checks pass

Invalid remote config must not overwrite last-known-good cache.

---

### Experiment assignment
Variant assignment is:
- deterministic
- rollout-gated
- weighted
- persisted

Assignment flow:
1. check override
2. parse experiment spec
3. check persisted assignment
4. evaluate rollout gate
5. pick weighted variant
6. persist result

---

## Intended Usage

This package is meant to be consumed as a runtime service for:
- tuning gameplay values
- controlling feature visibility
- running small experiments
- surviving offline/error conditions safely

It is not intended to replace:
- analytics platforms
- full live-ops dashboards
- enterprise experimentation suites

---

## Suggested Usage Pattern

Typical runtime usage:

1. create client through `RemoteTuningFactory`
2. set current `UserId`
3. call `RefreshAsync()` at startup or on demand
4. query:
   - `Flags`
   - `Values`
   - `Experiments`
5. react safely to defaults/cache/remote state

---

## Demo Usage

The sample UI Toolkit scene is intended to show the system quickly.

Demo controls include:
- user id apply/randomize
- refresh config
- offline simulation
- experiment override
- visible reward/cooldown/IAP changes

This is meant to make the package easy to evaluate in under a few minutes.