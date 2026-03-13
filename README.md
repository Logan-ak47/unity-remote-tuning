# Remote Tuning (Config, Flags, A/B) for Unity

A public Unity package and demo project that showcases production-minded engineering for:

- remote config fetching
- TTL caching with stale-serving behavior
- safe feature flags
- deterministic A/B assignment
- persisted experiment variants
- UI Toolkit demo for quick evaluation

Built with **Unity 6000.3 / Unity 6.3 LTS**.

---

## Overview

This project demonstrates a small but realistic remote tuning framework for Unity games.

It is designed to show platform-quality concerns rather than just “fetch JSON and hope it works”:

- config is fetched through a transport seam
- cache behavior is explicit and testable
- invalid config is rejected safely
- feature flags have defaults and failure semantics
- A/B experiments are deterministic and persisted
- demo behavior is visible immediately in a UI Toolkit scene

This project is less about visual polish and more about how I design reliable runtime systems for Unity: failure handling, determinism, seams for testing, and clear public APIs.

---

## Why this exists

In live Unity games, remote config often grows from a simple JSON fetch into a system that affects:

- gameplay tuning
- ad pacing
- IAP visibility
- rollout safety
- A/B experimentation
- offline behavior

This package focuses on those reliability concerns in a way that is easy to review, run, and extend.

---

## Features

### Remote Config
- GET-based config fetch
- timeout support
- optional ETag / If-None-Match flow
- validation gate before accepting remote data
- config version field for debugging

### Cache
- persisted last-known-good config
- TTL + stale window evaluation
- stale-serving behavior with revalidation intent
- expired cache falls back safely

### Feature Flags
- `IsEnabled(key, defaultValue)`
- typed accessors for tuning values
- safe default return on missing or invalid values
- reasoned logging

### A/B Testing
- deterministic stable hashing
- rollout gate (0–100%)
- weighted variants
- persisted assignment per `(userId, experimentKey)`
- debug override for demo/testing

### Demo
- UI Toolkit sample scene
- refresh config
- change/randomize user id
- simulate offline mode
- force experiment override
- visible changes in reward/IAP/cooldown behavior

---

## Demo

Reviewer flow:

1. Open the Unity project
2. Open the demo scene
3. Press Play
4. Observe:
   - config source
   - load reason
   - cache status / TTL
   - feature flag state
   - experiment variant
5. Try:
   - Refresh
   - Offline toggle
   - Randomize UserId
   - Force variant override

---

## Quickstart

### Open the project
Open the repository in Unity **6000.3 / Unity 6.3 LTS**.

### Open the demo scene
If you are using the imported sample, open:

`Assets/Samples/com.ashutosh.remotetuning/0.1.0/Remote Tuning Demo/Scenes/RemoteTuningDemo.unity`

If you copied the demo locally into `Assets/RemoteTuningDemo`, open your equivalent scene path there.

### Run
Press Play.

### Expected behavior
The scene displays:
- current config source (`Default`, `Cache`, `Remote`)
- load reason
- cache state (`Miss`, `Fresh`, `Stale`, `Expired`)
- tuning values
- IAP visibility
- experiment variant for current user

---

## Package Structure

```text
Packages/remotetuning/
  Runtime/
    Abstractions/
    Core/
    Internal/
    Public/
  Editor/
  Tests/
    Runtime/
    Editor/
  Documentation~/
  Samples~/
    Demo/
Key areas

Abstractions/ → seams (IConfigTransport, IKeyValueStore, IClock, ILogSink, IStableHasher)

Core/ → main public models and client

Public/ → user-facing API surface

Internal/ → implementation details (cache, validation, transport, experiments)

Tests/ → runtime/editor tests

Samples~/Demo/ → UI Toolkit demo scene

Architecture
High-level flow
RemoteTuningClient
 ├─ RemoteConfigService
 │   ├─ IConfigTransport
 │   ├─ ConfigCacheRepository
 │   ├─ IConfigValidator
 │   └─ IClock
 │
 ├─ ConfigAccessor
 │   ├─ FeatureFlags
 │   ├─ RemoteTuningValues
 │   └─ ExperimentService
 │       ├─ ExperimentSpecParser
 │       ├─ ExperimentAssigner
 │       ├─ AssignmentStore
 │       └─ ExperimentOverrideStore
 │
 └─ Demo / gameplay consumers
Design goals

isolate external dependencies

keep risky semantics testable

separate public API from implementation details

prefer safe fallback over silent failure

Main seams

IConfigTransport → HTTP/network

IKeyValueStore → persistence

IClock → time-based rules

ILogSink → diagnostics

IStableHasher → deterministic bucketing

Failure Matrix
Scenario	Served Result	Cache Write?	Reason
No cache on startup	Defaults	No	Default_NoCache
Fresh cache available	Cache	No	Cache_Fresh
Stale cache available	Cache	No	Cache_Stale_Served
Cache expired	Defaults	No	Default_ExpiredCache
Remote 200 + valid JSON	Remote	Yes	Remote_Success
Remote 304 Not Modified	Cache	Cache timestamp refreshed	Remote_NotModified
Remote timeout / network fail + usable cache	Cache	No	Remote_Failed_UsingCache
Remote timeout / network fail + no usable cache	Defaults	No	Remote_Failed_UsingDefault
Remote invalid JSON / validation fail + usable cache	Cache	No	Remote_Invalid_UsingCache
Remote invalid JSON / validation fail + no usable cache	Defaults	No	Remote_Invalid_UsingDefault
Important rule

Remote config is only committed if validation passes.
Invalid remote data must not overwrite last-known-good cache.

Tests

Runtime tests cover:

stable hash consistency

weighted assignment distribution

cache TTL transitions

persisted assignment behavior

validation rejection

These tests focus on behavior guarantees, not just implementation details.

Known Tradeoffs

Uses JSON object traversal rather than generated strongly typed config classes

Demo prioritizes clarity and reviewer speed over visual polish

Current demo uses PlayerPrefs for persistence to keep setup simple

Experiment schema is intentionally compact rather than fully enterprise-style

Demo networking setup depends on the endpoint you provide

Roadmap

Planned improvements:

stronger experiment schema support

richer typed config models

editor tools / debug window

optional local file transport for easier demo setup

improved docs/screenshots/GIF

Status

Current milestone: public package + demo + tests + docs