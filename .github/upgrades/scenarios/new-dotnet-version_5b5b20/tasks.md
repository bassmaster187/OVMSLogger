# OVMS .NET 10.0 Upgrade Tasks

## Overview

This document tracks the execution of the OVMS project upgrade from .NET Framework 4.8 to .NET 10.0. The project will be converted to SDK-style format and all components upgraded simultaneously in a single atomic operation, followed by testing and validation.

**Progress**: 1/4 tasks complete (25%) ![0%](https://progress-bar.xyz/25)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-03-04 22:29)*
**References**: Plan §Prerequisites

- [✓] (1) Verify .NET 10.0 SDK installed: `dotnet --list-sdks`
- [✓] (2) SDK version 10.0.x or higher available (**Verify**)

---

### [✗] TASK-002: Atomic framework and dependency upgrade
**References**: Plan §SDK-Style Conversion, Plan §Target Framework Update, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Convert OVMS.csproj to SDK-style format per Plan §SDK-Style Conversion
- [✓] (2) Update TargetFramework to net10.0 in OVMS.csproj
- [✓] (3) Update package references per Plan §Package Update Reference: BouncyCastle (1.8.5→1.8.9), Newtonsoft.Json (13.0.1→13.0.4), System.Collections.Immutable (1.7.1→10.0.3), System.Reflection.Metadata (1.8.1→10.0.3), System.Runtime.CompilerServices.Unsafe (5.0.0→6.1.2)
- [✓] (4) Remove packages now in framework: System.Buffers, System.Memory, System.Threading.Tasks.Extensions
- [✓] (5) Add System.Configuration.ConfigurationManager version 10.0.0
- [✓] (6) All package updates applied correctly (**Verify**)
- [✓] (7) Restore dependencies: `dotnet restore OVMS.csproj`
- [✓] (8) All dependencies restored successfully (**Verify**)
- [✗] (9) Build solution and fix all compilation errors per Plan §Breaking Changes Catalog (focus: ServicePointManager→HttpClientHandler, WebClient→HttpClient, ApplicationSettingsBase→ConfigurationManager, remove SSL3 references)
- [✗] (10) Solution builds with 0 errors (**Verify**)

---

### [ ] TASK-003: Run full test suite and validate upgrade
**References**: Plan §Testing & Validation Strategy

- [ ] (1) Identify and run all test projects (if tests exist)
- [ ] (2) Fix any test failures (reference Plan §Breaking Changes for behavioral changes in HttpContent and Uri)
- [ ] (3) Re-run tests after fixes
- [ ] (4) All tests pass with 0 failures (**Verify**)

---

### [ ] TASK-004: Final commit
**References**: Plan §Source Control Strategy

- [ ] (1) Commit all changes with message: "Upgrade OVMS to .NET 10.0 - Convert to SDK-style, update framework and packages, fix breaking changes, resolve BouncyCastle security vulnerability"

---



