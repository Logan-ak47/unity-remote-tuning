# Case Study — Remote Tuning (Config, Flags, A/B) for Unity

## Summary

This project is a public Unity package and demo built to showcase platform-quality engineering for runtime configuration systems.

It covers:
- remote config fetch
- explicit cache semantics
- validation before commit
- safe feature flags
- deterministic A/B experiments
- persisted assignment stability
- a fast-to-evaluate UI Toolkit demo

The focus is reliability, testability, and clean architecture rather than visual polish.

---

## Problem

A lot of Unity projects start remote config as a simple JSON fetch, but the real complexity appears later:

- What happens when the user is offline?
- What happens when config is stale?
- What happens when remote JSON is invalid?
- How do flags fail safely?
- How do experiment variants stay stable across sessions?
- How do you make all of this easy to test?

I wanted to build a small but realistic package that handles those concerns explicitly.

---

## Project Goals

The package needed to demonstrate:

- remote config fetching with resilience
- cache TTL and stale-serving behavior
- validation with safe fallback
- feature flags with defaults
- deterministic A/B assignment
- persisted variants
- a visible demo scene that reviewers can understand quickly
- tests around the risky semantics

---

## Constraints

- Built in **Unity 6000.3 / Unity 6.3 LTS**
- No heavy visual polish required
- Easy reviewer path: open project, open scene, press Play
- Claims must stay truthful
- Architecture should be modular, testable, and package-friendly

---

## What I Built

### Runtime package
A Unity package with:
- runtime abstractions
- config fetch pipeline
- cache and validation logic
- feature flags
- experiments
- tests
- sample demo

### Demo scene
A UI Toolkit demo that shows:
- config source
- load reason
- cache state / TTL
- tuning values
- feature flag state
- current experiment variant
- offline simulation
- debug override for experiments

### Tests
Runtime tests for:
- stable hash consistency
- weighted assignment distribution
- cache TTL transitions
- persisted assignment behavior
- validation rejection

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
Structure intent

Abstractions holds seams for external dependencies

Core holds the main client and public models

Public exposes the consumer-facing API

Internal hides implementation details

Tests verifies behavior guarantees

Samples~/Demo makes evaluation fast

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
Key design principles

isolate dependencies behind interfaces

keep failure semantics explicit

validate before committing remote state

keep assignment deterministic

design internals for testability first

Key Technical Decisions
1) Dependency seams first

I started with interfaces for:

transport

storage

time

logging

hashing

This made the cache, validation, and experiment logic testable without relying on UnityWebRequest, PlayerPrefs, or real time.

2) Validation before cache commit

Remote JSON is never accepted blindly.

The flow is:

fetch remote

parse JSON

validate required keys/types/ranges

only then commit as last-known-good

This prevents invalid remote data from corrupting cache state.

3) Explicit cache semantics

Instead of “cached or not cached,” the package evaluates:

Miss

Fresh

Stale

Expired

That makes behavior easier to reason about and easier to surface in the demo.

4) Deterministic A/B assignment

Experiment assignment uses a stable hash of:

userId + experimentKey + salt + purpose

This allows:

stable rollout gating

stable weighted variant selection

persisted assignments that do not flip between sessions

5) Public API kept small

The consumer-facing API is intentionally simple:

Flags.IsEnabled(...)

Values.Get...(...)

Experiments.GetVariant(...)

RefreshAsync()

SetUserId(...)

This keeps the package easy to read and easy to review.

Failure Handling

The package has explicit fallback behavior for important failure scenarios.

Scenario	Result
No cache on startup	serve defaults
Fresh cache available	serve cache
Stale cache available	serve cache and revalidate
Cache expired	fall back to defaults
Remote 200 + valid config	accept remote and update cache
Remote 304	keep cache and refresh timestamps
Remote timeout/failure + usable cache	keep cache
Remote timeout/failure + no usable cache	use defaults
Remote invalid JSON/validation failure	reject remote and keep last-known-good/default

Important rule: invalid remote config must not overwrite good cached state.

Tradeoffs
JSON traversal vs strongly typed generated models

I used JSON object traversal for flexibility and speed of implementation.

Pros:

simple to evolve during the build

easy to inspect in the demo

good for a small package example

Cons:

less compile-time safety than a fully generated schema model

PlayerPrefs for persistence

I used PlayerPrefs for:

cache storage

assignment persistence

debug override persistence

Pros:

simple setup

no extra project configuration needed

fast reviewer path

Cons:

not ideal for every production persistence need

Compact experiment schema

The experiment structure is intentionally small and readable.

Pros:

easy to review

enough to demonstrate rollout + weights + persistence

Cons:

not a full enterprise experimentation model

Demo-first usability over editor tooling

I prioritized a runnable UI Toolkit demo over editor windows/custom inspectors.

Pros:

easier evaluation

faster understanding for reviewers

Cons:

fewer authoring/debug conveniences inside the editor

Demo Walkthrough

The demo scene is meant to be understandable in under a few minutes.

Controls

set user id

randomize user id

refresh config

simulate offline mode

force experiment override

Visible behaviors

config source / reason / TTL

difficulty scalar display

ad cooldown timer

IAP section visibility

reward label affected by experiment variant

This makes it easy to verify both happy path and failure behavior quickly.

Tests

The tests focus on behavior guarantees rather than line coverage.

Covered areas:

stable hash consistency

weighted assignment distribution

cache TTL transitions

assignment persistence

validation rejection

This was important because the project is mainly about runtime semantics and failure handling.

Results
What is working

remote config pipeline with validation gate

cache TTL and stale-serving behavior

feature flag access with safe defaults

deterministic rollout and variant assignment

persisted assignments

override support for demo/testing

demo scene with visible state and behavior

runtime tests around core semantics

What this project demonstrates well

runtime system design in Unity

public package structure

testable architecture

failure-aware engineering

practical live-ops style tooling concerns

What I Would Improve Next

If I continued this project, the next steps would be:

stronger typed schema support

better local/demo transport options

editor-side inspection/debug tools

richer experiment metadata

better visual polish in the demo





Closing Note

This project was built to demonstrate how I approach reliability-oriented Unity runtime systems:

clear seams

explicit behavior

safe fallback rules

deterministic outcomes

testability from the start

It is intentionally scoped, but the architecture is meant to feel like something a team could realistically extend.