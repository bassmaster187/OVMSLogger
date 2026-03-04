# .NET 10.0 Upgrade Plan for OVMS Solution

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
  - [OVMS.csproj](#ovmscsproj)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description

This plan details the upgrade of the OVMS solution from **.NET Framework 4.8** to **.NET 10.0 (Long Term Support)**. The solution consists of a single project (OVMS.csproj) with 3,415 lines of code and 14 NuGet package dependencies.

### Scope

**Projects Affected**: 1
- OVMS.csproj (ClassicDotNetApp, currently net48)

**Current State**:
- Target Framework: .NET Framework 4.8
- Project Style: Classic (non-SDK-style)
- NuGet Packages: 14 total
- Lines of Code: 3,415
- Code Files: 13 (3 with identified issues)

**Target State**:
- Target Framework: .NET 10.0 (net10.0)
- Project Style: SDK-style (required for .NET Core/modern .NET)
- NuGet Packages: Updated/removed as needed
- Legacy APIs: Migrated to modern equivalents

### Selected Strategy

**All-At-Once Strategy** - Single project upgraded in one atomic operation.

**Rationale**:
- **Single project** - No dependency coordination needed
- **Small codebase** - 3,415 LOC is manageable for atomic upgrade
- **Clear upgrade path** - SDK-style conversion + package updates + API fixes
- **All packages compatible or have target versions** - No blocking package issues

### Complexity Assessment

**Discovered Metrics**:
- Projects: 1
- Dependency depth: 0 (no project dependencies)
- Lines of code: 3,415
- Package updates required: 5 of 14
- Packages to remove (in framework): 3
- Security vulnerabilities: 1 (BouncyCastle)
- API issues: 22 (13 source incompatible, 9 behavioral changes)
- Project conversion: Required (classic → SDK-style)

**Complexity Classification**: **Simple**

This is a straightforward single-project upgrade with well-documented migration paths and no complex inter-project dependencies.

### Critical Issues

**Security Vulnerabilities**:
- ⚠️ **BouncyCastle 1.8.5** - Contains security vulnerabilities, must upgrade to 1.8.9

**Blocking Issues**:
- Project must be converted to SDK-style format (cannot target .NET 10.0 with classic project format)
- Legacy configuration system (app.config) must be migrated to modern configuration

**Breaking Changes**:
- 13 source incompatible APIs requiring code changes or compatibility packages
- 9 behavioral changes requiring validation testing
- Configuration system migration (app.config → appsettings.json or ConfigurationManager package)

### Recommended Approach

**All-at-Once atomic upgrade**:
1. Convert project to SDK-style format
2. Update target framework to net10.0
3. Update all package references simultaneously
4. Fix compilation errors from API incompatibilities
5. Build and validate solution

**Expected Iterations**: 7-8 iterations (fast batch approach)
- Phase 1: Discovery & Classification (3 iterations) ✅ Complete
- Phase 2: Foundation (3 iterations) - Next
- Phase 3: Dynamic Detail (1-2 iterations) - All project details in batch

---

## Migration Strategy

### Approach Selection

**Selected Strategy: All-At-Once Strategy**

All project components (framework target, packages, code) are upgraded simultaneously in a single atomic operation.

### Justification

**Why All-At-Once**:

1. **Single Project** - No dependency coordination required between multiple projects
2. **Small Codebase** - 3,415 LOC is manageable for comprehensive testing in one pass
3. **Clear Package Compatibility** - All packages have known upgrade paths or are being removed
4. **No Intermediate States Needed** - No benefit to partial upgrades in a single-project solution
5. **Faster Completion** - Eliminates overhead of multi-phase coordination
6. **Simpler Testing** - One comprehensive test pass vs. multiple incremental validations

**Why Not Incremental**:
- No project dependencies to sequence
- No value in multi-targeting (would add complexity without benefit)
- Testing surface is small enough for single validation pass

### All-At-Once Strategy Rationale

The All-At-Once strategy is ideal for this scenario because:

- **Atomic Operation**: All changes (SDK conversion, framework update, package updates, code fixes) happen together
- **Single Validation Point**: One build/test cycle instead of multiple checkpoints
- **No Multi-Targeting Complexity**: Direct migration from net48 to net10.0 without intermediate targets
- **Unified Breaking Changes**: All API incompatibilities addressed in same operation

### Dependency-Based Ordering

**Not Applicable** - Single project with no project dependencies.

However, within the atomic upgrade operation, the following **internal ordering** must be followed:

1. **Convert to SDK-style** - Must happen first (classic projects cannot target .NET Core/modern .NET)
2. **Update TargetFramework** - Change from net48 to net10.0
3. **Update/Remove PackageReferences** - Apply all package changes simultaneously
4. **Restore dependencies** - dotnet restore
5. **Fix compilation errors** - Address API incompatibilities revealed by build
6. **Build and verify** - Ensure 0 errors

This ordering is **sequential within the atomic operation**, not separate phases.

### Execution Approach

**Single Atomic Task**:

The entire upgrade is executed as one coordinated operation:
- SDK-style conversion (use dotnet upgrade-assistant or manual conversion)
- Target framework update (net48 → net10.0)
- Package updates (5 updates, 3 removals, 6 unchanged)
- Dependency restoration
- Code fixes for breaking changes
- Build verification

**No Intermediate Builds** - All changes applied before first build attempt

**Success Criteria**: Solution builds with 0 errors after all changes applied

### Parallel vs Sequential Execution

**Not Applicable** - Only one project exists.

Within the project, code fixes may be addressed in any order as they are all part of the same atomic operation.

### Risk Mitigation Through All-At-Once

While All-At-Once has higher initial risk than incremental approaches, this is mitigated by:

1. **Small scope** - Only 3,415 LOC to validate
2. **Good tooling** - SDK-style conversion is well-supported
3. **Clear breaking changes** - Assessment identified all API issues upfront
4. **Git branch** - Easy rollback if critical issues discovered
5. **Security benefit** - BouncyCastle vulnerability fixed immediately

### Phase Definition

**Phase 0: Prerequisites** (if needed)
- Verify .NET 10.0 SDK installed
- Backup current state (already on upgrade-to-NET10 branch)

**Phase 1: Atomic Upgrade**
All operations performed as single coordinated batch:
- Convert project to SDK-style
- Update target framework to net10.0
- Update all package references
- Remove packages now in framework
- Restore dependencies
- Build and fix all compilation errors
- Rebuild to verify 0 errors

**Deliverables**: Project builds successfully with 0 errors

**Phase 2: Validation**
- Execute any existing tests
- Address test failures
- Validate application functionality

**Deliverables**: All tests pass, application functional

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The OVMS solution consists of a **single project** with **no project dependencies**. This is the simplest possible dependency structure.

```
OVMS.csproj (net48 → net10.0)
└── No project dependencies
```

**Dependency Characteristics**:
- **Depth**: 0 (leaf node - no dependencies)
- **Breadth**: 0 (no sibling projects)
- **Circular dependencies**: None
- **External dependencies**: 14 NuGet packages

### Project Groupings by Migration Phase

Since this is a single-project solution, there is only one migration phase:

**Phase 1: Atomic Upgrade**
- OVMS.csproj (all changes applied simultaneously)

**Migration Order**: Not applicable (single project)

**Critical Path**: The entire project is the critical path

### Package Dependency Considerations

While there are no project-to-project dependencies, the following package dependency considerations exist:

**Core Framework Packages** (functionality now in .NET 10.0):
- System.Buffers 4.5.1 → Remove (in framework)
- System.Memory 4.5.4 → Remove (in framework)
- System.Threading.Tasks.Extensions 4.5.4 → Remove (in framework)

**Packages Requiring Updates**:
- BouncyCastle 1.8.5 → 1.8.9 (security vulnerability)
- Newtonsoft.Json 13.0.1 → 13.0.4
- System.Collections.Immutable 1.7.1 → 10.0.3
- System.Reflection.Metadata 1.8.1 → 10.0.3
- System.Runtime.CompilerServices.Unsafe 5.0.0 → 6.1.2

**Packages Remaining Unchanged**:
- Exceptionless 4.6.2 (compatible)
- Google.Protobuf 3.14.0 (compatible)
- K4os.Compression.LZ4 1.2.6 (compatible)
- K4os.Compression.LZ4.Streams 1.2.6 (compatible)
- K4os.Hash.xxHash 1.0.6 (compatible)
- MySql.Data 8.0.28 (compatible)

### Circular Dependencies

**None** - This is a single-project solution with no inter-project dependencies.

---

## Project-by-Project Plans

### OVMS.csproj

#### Current State

- **Target Framework**: .NET Framework 4.8 (net48)
- **Project Type**: ClassicDotNetApp (non-SDK-style)
- **Project Dependencies**: None
- **Dependent Projects**: None (no other projects in solution)
- **NuGet Packages**: 14 total
  - 6 compatible (no changes needed)
  - 5 require updates
  - 3 to be removed (functionality in framework)
- **Lines of Code**: 3,415
- **Code Files**: 13 total (3 with identified API issues)
- **Risk Level**: Medium (SDK conversion + API changes + security vulnerability)

#### Target State

- **Target Framework**: .NET 10.0 (net10.0)
- **Project Type**: SDK-style project
- **NuGet Packages**: 11 total (after removing 3, keeping 6, updating 5)
- **Configuration**: Migrated from app.config to modern configuration or ConfigurationManager package
- **Security**: BouncyCastle vulnerability resolved

#### Migration Steps

##### 1. Prerequisites

**Required Tools**:
- .NET 10.0 SDK installed and verified
- Visual Studio 2022 (version supporting .NET 10.0) or VS Code with C# extension

**Verification**:
```bash
dotnet --list-sdks
# Should show: 10.0.x or higher
```

**Dependencies**:
- None (no other projects to upgrade first)

##### 2. SDK-Style Conversion

**Current Project Format**: Classic .csproj (verbose, includes file listings, uses .NET Framework style)

**Target Project Format**: SDK-style .csproj (concise, implicit file inclusion, modern .NET style)

**Conversion Options**:

**Option A: Automated (Recommended)**
```bash
dotnet upgrade-assistant upgrade .\OVMS.csproj --non-interactive
```

**Option B: Manual Conversion**

1. Backup existing OVMS.csproj
2. Replace project file content with SDK-style template:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType> <!-- or Library, based on current project -->
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Package references will be added in next step -->
</Project>
```

3. Review any custom MSBuild targets, pre/post-build events from old .csproj
4. Manually re-add custom configurations if needed

**⚠️ Important**: Review the diff between old and new .csproj to ensure no custom build logic is lost.

##### 3. Target Framework Update

Update the `<TargetFramework>` element in the SDK-style .csproj:

```xml
<TargetFramework>net10.0</TargetFramework>
```

**Change**: `net48` → `net10.0`

##### 4. Package Reference Updates

Update all NuGet package references in the .csproj file:

**Packages to Update** (5 packages):

```xml
<!-- Security fix: BouncyCastle 1.8.5 → 1.8.9 -->
<PackageReference Include="BouncyCastle" Version="1.8.9" />

<!-- Recommended updates -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="System.Collections.Immutable" Version="10.0.3" />
<PackageReference Include="System.Reflection.Metadata" Version="10.0.3" />
<PackageReference Include="System.Runtime.CompilerServices.Unsafe" Version="6.1.2" />
```

**Packages to Remove** (3 packages - now in framework):

Remove these PackageReference lines entirely:
- `System.Buffers` (4.5.1)
- `System.Memory` (4.5.4)
- `System.Threading.Tasks.Extensions` (4.5.4)

**Packages Remaining Unchanged** (6 packages):

```xml
<PackageReference Include="Exceptionless" Version="4.6.2" />
<PackageReference Include="Google.Protobuf" Version="3.14.0" />
<PackageReference Include="K4os.Compression.LZ4" Version="1.2.6" />
<PackageReference Include="K4os.Compression.LZ4.Streams" Version="1.2.6" />
<PackageReference Include="K4os.Hash.xxHash" Version="1.0.6" />
<PackageReference Include="MySql.Data" Version="8.0.28" />
```

**Configuration Migration Package** (add if using app.config):

```xml
<PackageReference Include="System.Configuration.ConfigurationManager" Version="10.0.0" />
```

This package provides compatibility bridge for legacy configuration system.

**See [Package Update Reference](#package-update-reference) for complete table**

##### 5. Expected Breaking Changes

**Source Incompatible APIs** (13 instances requiring code changes):

1. **System.Net.ServicePointManager** (4 instances)
   - **Issue**: API not fully supported in .NET Core/modern .NET
   - **Migration**: Remove or replace with platform-specific TLS configuration
   - **Example**:
     ```csharp
     // Old (.NET Framework):
     ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

     // New (.NET 10.0):
     // TLS 1.2+ is enabled by default, remove this line
     // OR use SocketsHttpHandler.SslOptions if fine-grained control needed
     ```

2. **System.Configuration.ApplicationSettingsBase** (5 instances)
   - **Issue**: app.config access pattern changed
   - **Migration**: Use System.Configuration.ConfigurationManager package
   - **Example**:
     ```csharp
     // Old:
     string value = Properties.Settings.Default.MySetting;

     // New (with ConfigurationManager package):
     string value = ConfigurationManager.AppSettings["MySetting"];

     // OR migrate to appsettings.json with IConfiguration
     ```

3. **System.Net.WebClient** (1 instance)
   - **Issue**: WebClient is obsolete in modern .NET
   - **Migration**: Replace with HttpClient
   - **Example**:
     ```csharp
     // Old:
     using (var client = new WebClient())
     {
         string result = client.DownloadString(url);
     }

     // New:
     using (var client = new HttpClient())
     {
         string result = await client.GetStringAsync(url);
     }
     ```

4. **SecurityProtocolType.Ssl3** (2 instances)
   - **Issue**: SSL3 is obsolete and insecure
   - **Migration**: Remove references; modern .NET uses TLS 1.2+ by default

5. **Other APIs** (TimeSpan.FromSeconds, configuration constructors)
   - **Issue**: Minor signature or usage changes
   - **Migration**: Address compiler errors case-by-case using modern patterns

**Behavioral Changes** (9 instances requiring validation):

1. **System.Net.Http.HttpContent** (4 instances)
   - **Change**: Disposal and stream handling behavior
   - **Validation**: Ensure proper disposal patterns; test HTTP operations

2. **System.Uri** (5 instances including constructors)
   - **Change**: URI parsing or formatting differences
   - **Validation**: Test URI construction and parsing logic

**See [Breaking Changes Catalog](#breaking-changes-catalog) for comprehensive details**

##### 6. Code Modifications

**Files with Known Issues** (3 files from assessment):
- Identify specific files by searching for incompatible APIs
- Apply fixes from breaking changes catalog
- Common patterns to update:
  - ServicePointManager → remove or platform-specific handling
  - WebClient → HttpClient
  - app.config access → ConfigurationManager or IConfiguration
  - SSL3 references → remove

**Configuration Files**:
- **app.config**: Either keep (with ConfigurationManager package) or migrate to appsettings.json
- **Recommended**: Start with ConfigurationManager package for faster migration, refactor to appsettings.json later

**Areas Requiring Review**:
- HTTP client instantiation and usage
- TLS/SSL security protocol configuration
- Configuration value access patterns
- Cryptographic operations (BouncyCastle usage after version update)

##### 7. Dependency Restoration

After all project file changes:

```bash
dotnet restore OVMS.csproj
```

**Expected Outcome**: All packages restored successfully with no conflicts

##### 8. Build and Fix Compilation Errors

**Initial Build**:
```bash
dotnet build OVMS.csproj
```

**Expected**: Compilation errors from source incompatible APIs

**Fix Approach**:
1. Review each error message
2. Cross-reference with [Breaking Changes Catalog](#breaking-changes-catalog)
3. Apply appropriate fix (code change, compatibility package, or removal)
4. Rebuild incrementally to verify each fix

**Iterate** until build succeeds with 0 errors

##### 9. Testing Strategy

**Build Validation**:
- [ ] Project builds without errors
- [ ] Project builds without warnings (or only expected warnings)
- [ ] No package dependency conflicts

**Unit Tests** (if exist):
- [ ] Identify test projects (if any)
- [ ] Execute all unit tests: `dotnet test`
- [ ] All tests pass

**Integration Testing**:
- [ ] Database connectivity (MySql.Data operations)
- [ ] HTTP/HTTPS operations (validate TLS behavior)
- [ ] Exception logging (Exceptionless integration)
- [ ] Geocoding operations (validate Uri and HTTP usage)
- [ ] Configuration access (validate app.config or new config system)

**Functional Validation**:
- [ ] Application starts successfully
- [ ] Core workflows execute without errors
- [ ] No behavioral regressions from URI or HttpContent changes
- [ ] BouncyCastle cryptographic operations work correctly after upgrade

##### 10. Validation Checklist

**Technical Validation**:
- [ ] ✅ Project converted to SDK-style format
- [ ] ✅ Target framework updated to net10.0
- [ ] ✅ 5 packages updated to specified versions
- [ ] ✅ 3 packages removed (functionality in framework)
- [ ] ✅ BouncyCastle security vulnerability resolved (1.8.5 → 1.8.9)
- [ ] ✅ Solution builds with 0 errors
- [ ] ✅ No package dependency conflicts
- [ ] ✅ All breaking API changes addressed

**Quality Validation**:
- [ ] ✅ All existing tests pass (if tests exist)
- [ ] ✅ No new compiler warnings introduced
- [ ] ✅ Configuration system works (app.config or migrated)
- [ ] ✅ Database operations functional
- [ ] ✅ HTTP/HTTPS operations functional
- [ ] ✅ Exception logging functional

**Security Validation**:
- [ ] ✅ BouncyCastle 1.8.9 installed (vulnerability fixed)
- [ ] ✅ No vulnerable packages remain
- [ ] ✅ TLS 1.2+ enabled for network operations
- [ ] ✅ SSL3 references removed

---

## Package Update Reference

### Summary

- **Total Packages**: 14 (current) → 11 (after upgrade)
- **Updates Required**: 5
- **Packages to Remove**: 3 (functionality now in framework)
- **Unchanged**: 6

### Complete Package Matrix

| Package | Current Version | Target Version | Action | Projects | Reason |
|---------|----------------|----------------|--------|----------|---------|
| **BouncyCastle** | 1.8.5 | **1.8.9** | 🔴 **Update** | OVMS.csproj | **SECURITY: Contains vulnerabilities** |
| Newtonsoft.Json | 13.0.1 | **13.0.4** | 🔄 Update | OVMS.csproj | Recommended update for compatibility |
| System.Collections.Immutable | 1.7.1 | **10.0.3** | 🔄 Update | OVMS.csproj | Update to .NET 10.0 aligned version |
| System.Reflection.Metadata | 1.8.1 | **10.0.3** | 🔄 Update | OVMS.csproj | Update to .NET 10.0 aligned version |
| System.Runtime.CompilerServices.Unsafe | 5.0.0 | **6.1.2** | 🔄 Update | OVMS.csproj | Update to latest stable version |
| System.Buffers | 4.5.1 | **(remove)** | ❌ Remove | OVMS.csproj | Functionality included in .NET 10.0 framework |
| System.Memory | 4.5.4 | **(remove)** | ❌ Remove | OVMS.csproj | Functionality included in .NET 10.0 framework |
| System.Threading.Tasks.Extensions | 4.5.4 | **(remove)** | ❌ Remove | OVMS.csproj | Functionality included in .NET 10.0 framework |
| Exceptionless | 4.6.2 | 4.6.2 | ✅ Keep | OVMS.csproj | Compatible with .NET 10.0 |
| Google.Protobuf | 3.14.0 | 3.14.0 | ✅ Keep | OVMS.csproj | Compatible with .NET 10.0 |
| K4os.Compression.LZ4 | 1.2.6 | 1.2.6 | ✅ Keep | OVMS.csproj | Compatible with .NET 10.0 |
| K4os.Compression.LZ4.Streams | 1.2.6 | 1.2.6 | ✅ Keep | OVMS.csproj | Compatible with .NET 10.0 |
| K4os.Hash.xxHash | 1.0.6 | 1.0.6 | ✅ Keep | OVMS.csproj | Compatible with .NET 10.0 |
| MySql.Data | 8.0.28 | 8.0.28 | ✅ Keep | OVMS.csproj | Compatible with .NET 10.0 |

### Additional Package (Recommended)

| Package | Version | Action | Reason |
|---------|---------|--------|---------|
| **System.Configuration.ConfigurationManager** | 10.0.0 | ➕ Add | Compatibility bridge for app.config access patterns |

This package should be added if the application uses app.config for configuration and you want to minimize code changes during migration.

### Package Update Details

#### Security Updates

**BouncyCastle 1.8.5 → 1.8.9**
- **Priority**: 🔴 Critical
- **Reason**: Security vulnerabilities in 1.8.5
- **Impact**: Cryptographic operations
- **Testing**: Validate all cryptographic operations after upgrade
- **Migration Notes**: API compatible; no code changes expected

#### Recommended Updates

**Newtonsoft.Json 13.0.1 → 13.0.4**
- **Priority**: 🟡 Medium
- **Reason**: Bug fixes and .NET 10.0 compatibility improvements
- **Impact**: JSON serialization/deserialization
- **Testing**: Validate JSON operations
- **Migration Notes**: Backward compatible

**System.Collections.Immutable 1.7.1 → 10.0.3**
- **Priority**: 🟡 Medium
- **Reason**: Align with .NET 10.0 framework version
- **Impact**: Immutable collection usage
- **Testing**: Validate collection operations
- **Migration Notes**: Version jump is standard for .NET version alignment

**System.Reflection.Metadata 1.8.1 → 10.0.3**
- **Priority**: 🟡 Medium
- **Reason**: Align with .NET 10.0 framework version
- **Impact**: Reflection and metadata operations
- **Testing**: Validate reflection operations if used
- **Migration Notes**: Version jump is standard for .NET version alignment

**System.Runtime.CompilerServices.Unsafe 5.0.0 → 6.1.2**
- **Priority**: 🟢 Low
- **Reason**: Update to latest stable version
- **Impact**: Low-level unsafe operations (likely transitive dependency)
- **Testing**: Generally safe; validate if directly used
- **Migration Notes**: Typically a transitive dependency

#### Packages to Remove

These packages are no longer needed because their functionality is included in .NET 10.0:

**System.Buffers 4.5.1**
- **Reason**: `Span<T>`, `Memory<T>`, and buffer pooling are in framework
- **Action**: Remove `<PackageReference>` line from .csproj
- **Impact**: None (types resolve to framework implementations)

**System.Memory 4.5.4**
- **Reason**: Memory and Span types are in framework
- **Action**: Remove `<PackageReference>` line from .csproj
- **Impact**: None (types resolve to framework implementations)

**System.Threading.Tasks.Extensions 4.5.4**
- **Reason**: ValueTask and async extensions are in framework
- **Action**: Remove `<PackageReference>` line from .csproj
- **Impact**: None (types resolve to framework implementations)

### Package Compatibility Verification

After package updates, verify no conflicts exist:

```bash
dotnet restore OVMS.csproj
dotnet list OVMS.csproj package --vulnerable
dotnet list OVMS.csproj package --deprecated
```

**Expected Results**:
- No package conflicts
- No vulnerable packages (BouncyCastle vulnerability resolved)
- No deprecated packages warnings for critical dependencies

---

## Breaking Changes Catalog

### Overview

**Total API Issues**: 22
- **Source Incompatible**: 13 (require code changes)
- **Behavioral Changes**: 9 (require testing validation)

### Source Incompatible APIs (13 instances)

These APIs require code modifications to compile successfully on .NET 10.0.

#### 1. System.Net.ServicePointManager (4 instances)

**API**: `System.Net.ServicePointManager`  
**Issue**: Not fully supported in .NET Core/modern .NET; platform-specific TLS handling  
**Impact**: 🔴 High - Affects network security configuration  
**Category**: Source Incompatible

**Migration Paths**:

**Option A: Remove (Recommended)**
```csharp
// OLD (.NET Framework):
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
ServicePointManager.ServerCertificateValidationCallback = ...;

// NEW (.NET 10.0):
// TLS 1.2+ is enabled by default - remove the line entirely
// For certificate validation, use HttpClientHandler.ServerCertificateCustomValidationCallback
```

**Option B: Use HttpClientHandler**
```csharp
// OLD:
ServicePointManager.ServerCertificateValidationCallback = 
    (sender, cert, chain, errors) => true;

// NEW:
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = 
        (message, cert, chain, errors) => true
};
var client = new HttpClient(handler);
```

**Files to Review**: Search codebase for `ServicePointManager` usage

---

#### 2. System.Configuration.ApplicationSettingsBase (5 instances)

**API**: `System.Configuration.ApplicationSettingsBase` and `Item[String]` property  
**Issue**: app.config access pattern changed in .NET Core/modern .NET  
**Impact**: 🟡 Medium - Affects configuration reading  
**Category**: Source Incompatible

**Migration Paths**:

**Option A: Use ConfigurationManager Package (Quick Fix)**
```csharp
// Add package: System.Configuration.ConfigurationManager 10.0.0

// OLD (.NET Framework):
string value = Properties.Settings.Default.MySetting;

// NEW (.NET 10.0 with ConfigurationManager):
using System.Configuration;
string value = ConfigurationManager.AppSettings["MySetting"];
```

**Option B: Migrate to appsettings.json (Modern Approach)**
```csharp
// Create appsettings.json:
{
  "MySetting": "value"
}

// Use IConfiguration:
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

string value = config["MySetting"];
```

**Recommendation**: Start with Option A (ConfigurationManager package) for faster migration, plan Option B for future refactoring.

**Files to Review**: Search codebase for `Properties.Settings`, `ApplicationSettingsBase`, or app.config access

---

#### 3. System.Net.WebClient (1 instance)

**API**: `System.Net.WebClient`  
**Issue**: Obsolete in .NET 5.0+; not recommended for modern .NET  
**Impact**: 🟡 Medium - Affects HTTP operations  
**Category**: Source Incompatible

**Migration Path**:

```csharp
// OLD (.NET Framework):
using (var client = new WebClient())
{
    string result = client.DownloadString(url);
    byte[] data = client.DownloadData(url);
    client.DownloadFile(url, filePath);
}

// NEW (.NET 10.0):
using (var client = new HttpClient())
{
    string result = await client.GetStringAsync(url);
    byte[] data = await client.GetByteArrayAsync(url);

    // For file download:
    using (var response = await client.GetAsync(url))
    using (var fileStream = File.Create(filePath))
    {
        await response.Content.CopyToAsync(fileStream);
    }
}
```

**Note**: Migration from synchronous to async patterns recommended. If synchronous is required:
```csharp
string result = client.GetStringAsync(url).GetAwaiter().GetResult();
```

**Files to Review**: Search codebase for `WebClient`

---

#### 4. SecurityProtocolType.Ssl3 (2 instances)

**API**: `System.Net.SecurityProtocolType.Ssl3`  
**Issue**: SSL 3.0 is obsolete and insecure; removed in modern .NET  
**Impact**: 🟢 Low - Should be removed anyway for security  
**Category**: Source Incompatible

**Migration Path**:

```csharp
// OLD (.NET Framework):
ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

// NEW (.NET 10.0):
// Remove SSL3 entirely (insecure)
// TLS 1.2+ is enabled by default, so likely no configuration needed
// If explicit configuration required:
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
// OR (better) just remove the line - defaults are secure
```

**Recommendation**: Remove SSL3 references entirely. Modern .NET defaults to TLS 1.2+.

**Files to Review**: Search codebase for `Ssl3` or `SecurityProtocolType`

---

#### 5. System.TimeSpan.FromSeconds (1 instance)

**API**: `System.TimeSpan.FromSeconds(Double)`  
**Issue**: Potential precision or overflow behavior changes  
**Impact**: 🟢 Low - Generally compatible  
**Category**: Source Incompatible (minor)

**Migration Path**:

Usually no changes needed. If compiler errors occur:
```csharp
// Ensure proper type casting
TimeSpan ts = TimeSpan.FromSeconds((double)value);
```

**Files to Review**: Check compiler errors if they reference TimeSpan.FromSeconds

---

#### 6. System.Configuration.ApplicationSettingsBase Constructor (1 instance)

**API**: `System.Configuration.ApplicationSettingsBase.#ctor`  
**Issue**: Constructor behavior changed in modern .NET  
**Impact**: 🟡 Medium  
**Category**: Source Incompatible

**Migration Path**:

Use ConfigurationManager package (same as #2 above) or migrate to IConfiguration.

---

### Behavioral Changes (9 instances)

These APIs compile successfully but have changed runtime behavior. Thorough testing required.

#### 7. System.Net.Http.HttpContent (4 instances)

**API**: `System.Net.Http.HttpContent`  
**Issue**: Disposal behavior, stream handling, or buffering differences  
**Impact**: 🟡 Medium  
**Category**: Behavioral Change

**Migration Considerations**:

```csharp
// Ensure proper disposal patterns
using (var response = await client.GetAsync(url))
using (var content = response.Content)
{
    string result = await content.ReadAsStringAsync();
}

// Or use simplified pattern:
string result = await client.GetStringAsync(url);
```

**Testing Focus**:
- Verify HTTP response content is read correctly
- Ensure no memory leaks from improper disposal
- Test multipart content handling if used
- Validate stream positions and disposal

**Files to Review**: Search codebase for `HttpContent`, `GetAsync`, `PostAsync`

---

#### 8. System.Uri (5 instances: 3 type usage + 2 constructors)

**API**: `System.Uri` and `Uri(String)` constructor  
**Issue**: URI parsing, escaping, or formatting behavior changes  
**Impact**: 🟡 Medium  
**Category**: Behavioral Change

**Migration Considerations**:

```csharp
// Verify URI parsing is consistent
var uri = new Uri("http://example.com/path?query=value");

// Pay attention to:
// - URI escaping behavior
// - Relative vs absolute URI handling
// - Query string parsing
// - URI component extraction
```

**Testing Focus**:
- Validate URI construction with special characters
- Test relative and absolute URI combinations
- Verify query string parsing
- Check URI comparison operations

**Files to Review**: Search codebase for `new Uri(`, `Uri.`, or URI string manipulation

---

### Summary of Required Actions

| API/Type | Instances | Action Required | Priority |
|----------|-----------|-----------------|----------|
| ServicePointManager | 4 | Remove or migrate to HttpClientHandler | 🔴 High |
| ApplicationSettingsBase | 5 | Add ConfigurationManager package | 🟡 Medium |
| WebClient | 1 | Replace with HttpClient | 🟡 Medium |
| SecurityProtocolType.Ssl3 | 2 | Remove (insecure) | 🔴 High |
| TimeSpan.FromSeconds | 1 | Verify/fix if compiler error | 🟢 Low |
| HttpContent | 4 | Test disposal and reading patterns | 🟡 Medium |
| Uri | 5 | Test parsing and formatting | 🟡 Medium |

### Migration Checklist

**Before Building**:
- [ ] Install System.Configuration.ConfigurationManager 10.0.0 package
- [ ] Search and identify all ServicePointManager usages
- [ ] Search and identify WebClient usages
- [ ] Search and identify SSL3 references

**During Build Fixes**:
- [ ] Replace ServicePointManager with appropriate modern patterns
- [ ] Replace WebClient with HttpClient
- [ ] Remove SSL3 references
- [ ] Update ApplicationSettingsBase to ConfigurationManager
- [ ] Fix any TimeSpan compilation errors

**After Build Succeeds**:
- [ ] Test all HTTP/HTTPS operations
- [ ] Test configuration value reading
- [ ] Test URI construction and parsing
- [ ] Test TLS/SSL connections
- [ ] Validate no behavioral regressions

---

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation |
|---------|-----------|-------------|------------|
| OVMS.csproj | 🟡 Medium | SDK-style conversion may lose custom build configurations | Review .csproj diff carefully; manual adjustments if needed |
| OVMS.csproj | 🟡 Medium | Legacy configuration system (app.config) migration | Use System.Configuration.ConfigurationManager package as bridge |
| OVMS.csproj | 🟡 Medium | 13 source incompatible APIs requiring code changes | Reference breaking changes catalog; test each fix |
| OVMS.csproj | 🔴 High | BouncyCastle security vulnerability (1.8.5 → 1.8.9) | Upgrade immediately; validate cryptographic operations |
| OVMS.csproj | 🟡 Medium | ServicePointManager API marked incompatible | Review TLS/SSL configuration; may need platform-specific handling |

### Security Vulnerabilities

| Package | Current Version | CVE/Issue | Remediation |
|---------|----------------|-----------|-------------|
| BouncyCastle | 1.8.5 | Security vulnerabilities (details from assessment) | Upgrade to 1.8.9 immediately |

**Priority**: This security issue must be resolved as part of the upgrade.

### Contingency Plans

**If SDK-style conversion fails**:
- Alternative 1: Use `dotnet upgrade-assistant` tool for automated conversion
- Alternative 2: Manual conversion following Microsoft SDK-style migration guide
- Alternative 3: Create new SDK-style project and migrate files incrementally

**If breaking API changes are extensive**:
- Alternative 1: Use compatibility packages (e.g., `System.Configuration.ConfigurationManager`)
- Alternative 2: Refactor to modern .NET patterns incrementally
- Rollback: Revert to main branch if blockers exceed 8 hours effort

**If MySql.Data package incompatibilities arise**:
- Alternative 1: Update to latest MySql.Data version compatible with .NET 10.0
- Alternative 2: Migrate to MySqlConnector (modern, fully async MySQL driver)

**If Exceptionless compatibility issues**:
- Alternative 1: Update Exceptionless to latest version
- Alternative 2: Temporarily disable Exceptionless integration
- Alternative 3: Replace with built-in logging/Application Insights

### Breaking Change Risk Assessment

**Source Incompatible APIs (13 instances)**:
- **System.Net.ServicePointManager** (4 instances) - High impact
  - Risk: TLS/SSL configuration may behave differently
  - Mitigation: Review all ServicePointManager usage; test HTTPS connections

- **System.Configuration.ApplicationSettingsBase** (5 instances) - Medium impact
  - Risk: app.config settings access pattern changed
  - Mitigation: Use System.Configuration.ConfigurationManager NuGet package

- **System.Net.WebClient** (1 instance) - Low impact
  - Risk: Obsolete in modern .NET
  - Mitigation: Replace with HttpClient

- **Other APIs** (3 instances) - Low impact
  - Risk: Minor signature or behavior changes
  - Mitigation: Address compilation errors case-by-case

**Behavioral Changes (9 instances)**:
- **HttpContent** (4 instances) - Medium impact
  - Risk: Disposal behavior or stream handling differences
  - Mitigation: Review HTTP client usage; add explicit disposal

- **Uri** (3 instances + 2 constructors) - Low impact
  - Risk: Parsing or formatting behavior changes
  - Mitigation: Validate URI handling in tests

### Risk Mitigation Summary

**Overall Risk Level**: 🟡 **Medium**

**Primary Risk Factors**:
1. SDK-style conversion (structural change)
2. Configuration system migration (architectural change)
3. Security vulnerability in BouncyCastle (must fix)
4. ServicePointManager API changes (TLS/SSL behavior)

**Mitigation Strategy**:
1. Use automated tools (upgrade-assistant) for SDK conversion
2. Use ConfigurationManager compatibility package for interim solution
3. Upgrade BouncyCastle immediately (well-tested upgrade path)
4. Comprehensive testing of network operations after migration
5. Git branch provides easy rollback if needed

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

The testing strategy follows a progressive validation approach, starting with compilation and progressing through functional testing.

---

### Phase 0: Pre-Migration Baseline

**Before Starting Migration**:

- [ ] Document current application behavior (if not already documented)
- [ ] Identify critical workflows for post-migration validation
- [ ] Take note of any existing test failures (to distinguish from migration issues)
- [ ] Ensure source control backup (already on `upgrade-to-NET10` branch)

---

### Phase 1: Compilation Validation

**After SDK-style conversion and package updates**:

**Build Verification**:
```bash
dotnet build OVMS.csproj --configuration Debug
dotnet build OVMS.csproj --configuration Release
```

**Success Criteria**:
- [ ] ✅ Project builds without errors (0 errors)
- [ ] ✅ No dependency conflicts during restore
- [ ] ✅ Builds complete in both Debug and Release configurations

**Warning Acceptance**:
- Some warnings are acceptable during initial build
- Document any new warnings for later review
- Critical warnings (CS0618 obsolete APIs, etc.) should be addressed

---

### Phase 2: Package & Dependency Validation

**After successful compilation**:

**Package Health Check**:
```bash
dotnet list OVMS.csproj package
dotnet list OVMS.csproj package --vulnerable
dotnet list OVMS.csproj package --deprecated
dotnet list OVMS.csproj package --outdated
```

**Success Criteria**:
- [ ] ✅ No vulnerable packages reported (BouncyCastle 1.8.9 installed)
- [ ] ✅ No critical deprecated packages
- [ ] ✅ All package versions match plan specifications
- [ ] ✅ No dependency conflicts or version mismatches

---

### Phase 3: Unit Testing (if tests exist)

**If unit tests exist in solution**:

**Test Discovery**:
```bash
dotnet test --list-tests
```

**Test Execution**:
```bash
dotnet test OVMS.csproj --configuration Debug
dotnet test OVMS.csproj --configuration Release
```

**Success Criteria**:
- [ ] ✅ All pre-existing tests pass
- [ ] ✅ No new test failures introduced by migration
- [ ] ✅ Test execution completes without crashes

**If tests fail**:
- Distinguish between pre-existing failures and migration-induced failures
- Prioritize fixing migration-induced failures
- Document any behavioral changes discovered through tests

---

### Phase 4: Integration & Functional Testing

**Critical Functional Areas** (based on assessment):

#### 4.1 Database Connectivity

**Test**: MySQL database operations
```csharp
// Validate MySql.Data 8.0.28 works correctly
- Connection establishment
- Query execution
- Data retrieval
- Transaction handling
```

**Success Criteria**:
- [ ] ✅ Database connections succeed
- [ ] ✅ CRUD operations work correctly
- [ ] ✅ No connection pool or async behavior issues

---

#### 4.2 HTTP/HTTPS Operations

**Test**: Network operations (WebHelper class likely)
```csharp
// Validate migration from WebClient to HttpClient
- HTTP GET requests
- HTTP POST requests
- HTTPS with TLS 1.2+
- SSL certificate validation
```

**Success Criteria**:
- [ ] ✅ HTTP requests complete successfully
- [ ] ✅ HTTPS/TLS operations work (no ServicePointManager issues)
- [ ] ✅ Certificate validation behaves correctly
- [ ] ✅ No timeout or disposal issues

**Special Attention**:
- Test with actual HTTPS endpoints
- Verify TLS 1.2+ is being used (SSL3 removed)
- Validate any certificate validation callbacks work correctly

---

#### 4.3 Exception Logging (Exceptionless)

**Test**: Exceptionless integration
```csharp
// Validate Exceptionless 4.6.2 works on .NET 10.0
- Exception capture
- Event submission
- User identity tracking
- Custom object attachments
```

**Success Criteria**:
- [ ] ✅ Exceptions are captured correctly
- [ ] ✅ Events submit to Exceptionless service
- [ ] ✅ No exceptions in exception handling code
- [ ] ✅ Custom data (Cartype, OVMSVersion) attached correctly

---

#### 4.4 Configuration System

**Test**: app.config or ConfigurationManager access
```csharp
// Validate configuration reading works
- Application settings retrieval
- Connection strings retrieval
- Custom configuration sections (if any)
```

**Success Criteria**:
- [ ] ✅ Configuration values read correctly
- [ ] ✅ Default values handled properly
- [ ] ✅ No exceptions from configuration access

**If using ConfigurationManager package**:
- Verify app.config is copied to output directory
- Test that ConfigurationManager.AppSettings works

---

#### 4.5 Geocoding Operations (GeocodeCache)

**Test**: URI and XML operations
```csharp
// Validate Uri behavioral changes don't break logic
- URI construction for geocoding APIs
- XML serialization/deserialization (GeocodeCache.xml)
- Cache read/write operations
```

**Success Criteria**:
- [ ] ✅ URIs constructed correctly (no parsing issues)
- [ ] ✅ XML cache operations work (ReadXml/WriteXml)
- [ ] ✅ No exceptions from URI or XML changes

---

#### 4.6 Cryptographic Operations (BouncyCastle)

**Test**: BouncyCastle usage after upgrade to 1.8.9
```csharp
// Validate cryptographic operations still work
- Encryption/decryption operations
- Key generation/handling
- Any protocol buffer encryption
```

**Success Criteria**:
- [ ] ✅ Cryptographic operations produce expected results
- [ ] ✅ No exceptions from BouncyCastle API
- [ ] ✅ Data encrypted with old version can be decrypted with new version

---

### Phase 5: End-to-End Validation

**Full Application Workflow Testing**:

Based on Car.cs state machine, test complete workflows:

**Workflow 1: Car State Transitions**
- [ ] Start → Online transition
- [ ] Online → Drive transition
- [ ] Drive → Start transition (end driving)
- [ ] Online → Charge transition
- [ ] Charge → Start transition (end charging)

**Workflow 2: Data Persistence**
- [ ] Trip data saved to database correctly
- [ ] Charging state data saved correctly
- [ ] Current JSON state generated correctly
- [ ] Geocode cache persists and reloads

**Workflow 3: Network Operations**
- [ ] Authentication succeeds
- [ ] Driving status detection works
- [ ] Charging status detection works
- [ ] Protocol information retrieval works

**Success Criteria**:
- [ ] ✅ All state transitions work correctly
- [ ] ✅ Data persists and retrieves correctly
- [ ] ✅ No exceptions during normal operations
- [ ] ✅ Application remains stable over extended run

---

### Phase 6: Performance & Stability

**Performance Validation**:
- [ ] Application startup time comparable to pre-migration
- [ ] Database query performance unchanged
- [ ] HTTP request latency comparable
- [ ] Memory usage reasonable (no leaks from HttpClient or disposal issues)

**Stability Testing**:
- [ ] Application runs without crashes for extended period
- [ ] No memory leaks over time
- [ ] Exception handling works correctly
- [ ] State machine remains stable

---

### Regression Testing Checklist

**Behavioral Changes to Validate**:

- [ ] ✅ HttpContent disposal doesn't cause issues
- [ ] ✅ Uri parsing produces same results as before
- [ ] ✅ Configuration values match expected values
- [ ] ✅ TLS/SSL connections work without ServicePointManager
- [ ] ✅ Database operations have same behavior
- [ ] ✅ Exception logging captures same information

**Known Risk Areas**:
- ServicePointManager removal → test all HTTPS operations
- WebClient → HttpClient migration → test all HTTP operations
- app.config access → test all configuration reads
- Uri behavioral changes → test geocoding and URL construction

---

### Testing Tools & Commands

**Build & Test Commands**:
```bash
# Clean build
dotnet clean OVMS.csproj
dotnet build OVMS.csproj

# Run tests (if exist)
dotnet test

# Check for vulnerabilities
dotnet list package --vulnerable

# Run application
dotnet run --project OVMS.csproj
```

**Debugging**:
- Use Visual Studio debugger to step through critical code paths
- Enable detailed logging for HTTP operations
- Monitor Exceptionless dashboard for runtime exceptions

---

### Test Results Documentation

**Create test report documenting**:
- All test executions and results
- Any failures and their resolutions
- Performance comparison (before/after)
- Behavioral differences observed
- Outstanding issues or tech debt

**Format**:
```markdown
## Migration Test Report - .NET 10.0 Upgrade

### Build Results
- Debug build: [PASS/FAIL]
- Release build: [PASS/FAIL]
- Warnings: [count]

### Unit Tests
- Tests run: [count]
- Tests passed: [count]
- Tests failed: [count]
- Failures: [list]

### Integration Tests
- Database: [PASS/FAIL]
- HTTP/HTTPS: [PASS/FAIL]
- Configuration: [PASS/FAIL]
- Exceptionless: [PASS/FAIL]
- Geocoding: [PASS/FAIL]
- BouncyCastle: [PASS/FAIL]

### E2E Validation
- State transitions: [PASS/FAIL]
- Data persistence: [PASS/FAIL]
- Network operations: [PASS/FAIL]

### Issues Found
- [List any issues]

### Performance
- Startup time: [before] → [after]
- Memory usage: [before] → [after]
- Notable differences: [describe]
```

---

### Test Completion Criteria

**Migration testing is complete when**:
- [ ] ✅ All builds succeed (0 errors)
- [ ] ✅ No vulnerable packages remain
- [ ] ✅ All unit tests pass (if tests exist)
- [ ] ✅ All critical integration tests pass
- [ ] ✅ End-to-end workflows validated
- [ ] ✅ No behavioral regressions identified
- [ ] ✅ Performance is acceptable
- [ ] ✅ Application stable over test period
- [ ] ✅ Test report documented

---

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity Rating | Dependencies | Risk | Key Challenges |
|---------|------------------|--------------|------|----------------|
| OVMS.csproj | 🟡 Medium | 0 projects, 14 packages | Medium | SDK conversion, config migration, API incompatibilities |

**Complexity Breakdown**:
- **Low complexity factors**: Single project, small codebase (3,415 LOC), no project dependencies
- **Medium complexity factors**: SDK-style conversion required, 13 source incompatible APIs, legacy configuration system
- **High complexity factors**: None

### Phase Complexity Assessment

**Phase 0: Prerequisites**
- **Complexity**: 🟢 Low
- **Key Activities**: Verify .NET 10.0 SDK installation
- **Dependencies**: None

**Phase 1: Atomic Upgrade**
- **Complexity**: 🟡 Medium
- **Key Activities**: SDK conversion, framework update, package updates, code fixes
- **Dependencies**: .NET 10.0 SDK
- **Challenges**: 
  - SDK-style conversion (structural)
  - 13 source incompatible API fixes
  - Configuration system migration
  - Security vulnerability remediation

**Phase 2: Validation**
- **Complexity**: 🟢 Low to 🟡 Medium
- **Key Activities**: Test execution, functional validation
- **Dependencies**: Phase 1 completion (builds with 0 errors)
- **Challenges**: 
  - Behavioral changes may surface in runtime testing
  - Network/TLS operations need validation

### Resource Requirements

**Skills Required**:
- .NET migration experience (SDK-style conversion)
- Understanding of .NET Framework → .NET Core/modern .NET differences
- API compatibility troubleshooting
- Configuration system migration knowledge
- MySQL/database connectivity validation

**Parallel Work Capacity**: 
- Not applicable (single project, all work is sequential within atomic upgrade)

**Estimated Relative Effort**:
- **SDK-style conversion**: 🟡 Medium (automated tools available, but may need manual adjustments)
- **Package updates**: 🟢 Low (straightforward version updates)
- **API fixes**: 🟡 Medium (13 source incompatibilities, well-documented migration paths)
- **Configuration migration**: 🟡 Medium (app.config → modern config or compatibility package)
- **Testing & validation**: 🟢 Low to 🟡 Medium (depends on existing test coverage)

**Note**: No real-time estimates provided. Complexity ratings indicate relative effort and risk, not duration.

---

## Source Control Strategy

### Branching Strategy

**Current Setup**:
- **Main Branch**: `main` (source branch, untouched)
- **Upgrade Branch**: `upgrade-to-NET10` (all migration work happens here)
- **Pre-Migration State**: Committed before branch creation

### All-At-Once Source Control Approach

Since this is an All-At-Once migration with a single project, the recommended approach is:

**Single Atomic Commit** (Preferred):
- All changes (SDK conversion, package updates, code fixes) in one commit
- Easier to review as complete unit
- Simpler to revert if needed
- Clear "before/after" state

**Alternative: Logical Commits** (if changes are extensive):
1. Commit: SDK-style conversion
2. Commit: Package updates
3. Commit: Breaking API fixes
4. Commit: Final cleanup

**Recommendation**: Attempt single atomic commit first. Only split if the diff is too large to review effectively (unlikely for this 3,415 LOC project).

### Commit Strategy

#### Atomic Commit Approach (Recommended)

**Single Commit**:
```bash
# After all changes complete and builds succeed:
git add -A
git commit -m "Upgrade OVMS to .NET 10.0

- Convert project to SDK-style format
- Update target framework from net48 to net10.0
- Update packages: BouncyCastle (1.8.5→1.8.9), Newtonsoft.Json (13.0.1→13.0.4), System.Collections.Immutable (1.7.1→10.0.3), System.Reflection.Metadata (1.8.1→10.0.3), System.Runtime.CompilerServices.Unsafe (5.0.0→6.1.2)
- Remove packages now in framework: System.Buffers, System.Memory, System.Threading.Tasks.Extensions
- Add System.Configuration.ConfigurationManager (10.0.0) for app.config compatibility
- Fix breaking API changes: ServicePointManager→HttpClientHandler, WebClient→HttpClient, remove SSL3 references
- Migrate configuration access to ConfigurationManager package
- Resolve BouncyCastle security vulnerability

Builds successfully with 0 errors.
All tests passing (if applicable).

Refs: .NET 10.0 upgrade plan"
```

#### Logical Commits Approach (Alternative)

If splitting into multiple commits:

**Commit 1: SDK-style conversion**
```bash
git add OVMS.csproj
git commit -m "Convert OVMS.csproj to SDK-style format

- Replace classic .csproj with SDK-style template
- Preserve custom build configurations
- No functional changes yet

Refs: .NET 10.0 upgrade - step 1/4"
```

**Commit 2: Framework and package updates**
```bash
git add OVMS.csproj
git commit -m "Update target framework and packages for .NET 10.0

- Update TargetFramework: net48 → net10.0
- Update 5 packages, remove 3 (now in framework)
- Add ConfigurationManager compatibility package
- Fix BouncyCastle security vulnerability (1.8.5 → 1.8.9)

Does not build yet - breaking changes to be fixed in next commit.

Refs: .NET 10.0 upgrade - step 2/4"
```

**Commit 3: Breaking API fixes**
```bash
git add *.cs
git commit -m "Fix breaking API changes for .NET 10.0

- Replace ServicePointManager with HttpClientHandler patterns
- Migrate WebClient to HttpClient
- Remove SSL3 references
- Migrate configuration access to ConfigurationManager

Builds successfully with 0 errors.

Refs: .NET 10.0 upgrade - step 3/4"
```

**Commit 4: Final cleanup**
```bash
git add -A
git commit -m "Final cleanup after .NET 10.0 migration

- Remove obsolete comments
- Update documentation
- Clean up unused usings

All tests passing.

Refs: .NET 10.0 upgrade - step 4/4"
```

### Commit Message Format

**Use conventional commit style**:

```
<type>: <subject>

<body>

<footer>
```

**Types**:
- `feat`: SDK conversion, framework upgrade
- `fix`: Breaking API fixes
- `chore`: Package updates, cleanup
- `docs`: Documentation updates
- `test`: Test additions or fixes

### Review and Merge Process

#### Pull Request Requirements

**PR Title**: "Upgrade OVMS to .NET 10.0"

**PR Description Template**:
```markdown
## Migration Summary

Upgrades OVMS project from .NET Framework 4.8 to .NET 10.0 (LTS).

### Changes Made

- ✅ Converted project to SDK-style format
- ✅ Updated target framework: net48 → net10.0
- ✅ Updated 5 packages, removed 3 (now in framework)
- ✅ Fixed BouncyCastle security vulnerability (1.8.5 → 1.8.9)
- ✅ Fixed 13 source incompatible APIs
- ✅ Migrated configuration system to ConfigurationManager package

### Testing Performed

- ✅ Builds with 0 errors (Debug & Release)
- ✅ No vulnerable packages
- ✅ [Unit tests pass / No unit tests exist]
- ✅ Database connectivity validated
- ✅ HTTP/HTTPS operations validated
- ✅ Configuration access validated
- ✅ Exceptionless integration validated
- ✅ State machine transitions validated

### Breaking Changes

- ServicePointManager removed → using HttpClientHandler
- WebClient replaced with HttpClient
- SSL3 references removed (security improvement)
- app.config access via ConfigurationManager package

### Migration Plan

See: `.github/upgrades/scenarios/new-dotnet-version_5b5b20/plan.md`

### Checklist

- [ ] Code review completed
- [ ] All builds succeed
- [ ] All tests pass
- [ ] No vulnerable packages
- [ ] Documentation updated
- [ ] Ready to merge
```

#### Review Checklist

**Reviewer should verify**:

**Code Quality**:
- [ ] SDK-style .csproj is correct and complete
- [ ] No unnecessary files in project
- [ ] Code follows project conventions
- [ ] Breaking changes handled appropriately

**Migration Completeness**:
- [ ] All packages updated per plan
- [ ] All breaking APIs addressed
- [ ] No ServicePointManager, WebClient, or SSL3 references remain
- [ ] Configuration system works correctly

**Testing**:
- [ ] Build logs show 0 errors
- [ ] No vulnerable packages (`dotnet list package --vulnerable`)
- [ ] Test results provided (if tests exist)
- [ ] Critical workflows validated

**Security**:
- [ ] BouncyCastle 1.8.9 installed (vulnerability resolved)
- [ ] SSL3 removed
- [ ] TLS 1.2+ in use

**Documentation**:
- [ ] PR description complete
- [ ] Migration rationale clear
- [ ] Known issues documented (if any)

### Merge Criteria

**Ready to merge when**:
- ✅ All PR checklist items complete
- ✅ Code review approved
- ✅ All builds succeed (CI/CD if applicable)
- ✅ All tests pass
- ✅ No blocking issues identified
- ✅ Documentation updated

**Merge Command**:
```bash
# From main branch:
git checkout main
git merge --no-ff upgrade-to-NET10 -m "Merge .NET 10.0 upgrade

Completes migration from .NET Framework 4.8 to .NET 10.0 LTS.
See upgrade-to-NET10 branch for detailed changes."

git push origin main
```

**Merge Options**:
- **Squash merge**: If using logical commits and want clean main history
- **Regular merge**: If using single atomic commit (recommended)
- **No fast-forward**: Preserves branch history for traceability

### Post-Merge Actions

**After successful merge**:

1. **Tag the release**:
```bash
git tag -a v1.0.0-net10.0 -m "OVMS migrated to .NET 10.0"
git push origin v1.0.0-net10.0
```

2. **Clean up branch** (optional):
```bash
git branch -d upgrade-to-NET10
git push origin --delete upgrade-to-NET10
```

3. **Update documentation**:
- README: Update .NET version requirements
- Build instructions: Update SDK requirements
- Deployment guide: Update runtime requirements

4. **Notify team**:
- Developers need .NET 10.0 SDK
- CI/CD may need updates
- Deployment environments need .NET 10.0 runtime

### Rollback Plan

**If critical issues discovered post-merge**:

**Option 1: Revert merge commit**
```bash
git revert -m 1 <merge-commit-sha>
git push origin main
```

**Option 2: Hard reset** (if not yet pushed or coordinated with team)
```bash
git reset --hard <commit-before-merge>
git push --force origin main  # USE WITH CAUTION
```

**Option 3: Fix forward**
- Create hotfix branch from main
- Fix critical issue
- Merge hotfix back to main

**Rollback Decision Criteria**:
- Application doesn't start
- Critical functionality broken
- Data corruption or loss
- Security vulnerability introduced
- Performance degradation >50%

Otherwise: **Fix forward** with hotfix branch (preferred)

---

## Success Criteria

### Technical Criteria

The migration is technically complete when all of the following are met:

#### Build Success
- [ ] ✅ **Project builds without errors** (0 errors in both Debug and Release)
- [ ] ✅ **Project converted to SDK-style format** (verified .csproj structure)
- [ ] ✅ **Target framework updated** to net10.0 (verified in .csproj)
- [ ] ✅ **All package updates applied** per plan specification:
  - BouncyCastle 1.8.9
  - Newtonsoft.Json 13.0.4
  - System.Collections.Immutable 10.0.3
  - System.Reflection.Metadata 10.0.3
  - System.Runtime.CompilerServices.Unsafe 6.1.2
- [ ] ✅ **Framework packages removed**: System.Buffers, System.Memory, System.Threading.Tasks.Extensions
- [ ] ✅ **ConfigurationManager package added** (if using app.config)

#### Security & Quality
- [ ] ✅ **No vulnerable packages** (`dotnet list package --vulnerable` returns clean)
- [ ] ✅ **BouncyCastle security vulnerability resolved** (1.8.5 → 1.8.9 confirmed)
- [ ] ✅ **No deprecated packages** (or documented exceptions)
- [ ] ✅ **No dependency conflicts** (dotnet restore succeeds cleanly)
- [ ] ✅ **SSL3 references removed** (security improvement)

#### Breaking Changes Addressed
- [ ] ✅ **All source incompatible APIs fixed** (13 instances):
  - ServicePointManager (4) → migrated or removed
  - ApplicationSettingsBase (5) → ConfigurationManager or IConfiguration
  - WebClient (1) → HttpClient
  - SecurityProtocolType.Ssl3 (2) → removed
  - Other APIs (1) → fixed
- [ ] ✅ **Behavioral changes validated** (9 instances):
  - HttpContent (4) → tested
  - Uri (5) → tested

#### Testing & Validation
- [ ] ✅ **Unit tests pass** (if unit tests exist) - 100% pass rate
- [ ] ✅ **Integration tests pass**:
  - Database connectivity works
  - HTTP/HTTPS operations functional
  - Configuration access functional
  - Exceptionless integration works
  - Geocoding operations work
  - BouncyCastle operations work
- [ ] ✅ **End-to-end workflows validated**:
  - Car state transitions work
  - Data persistence works
  - Network operations work
- [ ] ✅ **No runtime exceptions** in normal operation

---

### Quality Criteria

The migration meets quality standards when:

#### Code Quality
- [ ] ✅ **Code follows project conventions** (style, patterns, architecture)
- [ ] ✅ **No code smells introduced** by migration (e.g., .GetAwaiter().GetResult() overuse)
- [ ] ✅ **Proper disposal patterns** used (HttpClient, streams, etc.)
- [ ] ✅ **Appropriate async/await usage** (especially for HttpClient)
- [ ] ✅ **Configuration access is clean** (not mixed patterns)

#### Test Coverage
- [ ] ✅ **Test coverage maintained** (no reduction from pre-migration)
- [ ] ✅ **New tests added** for behavioral changes (if needed)
- [ ] ✅ **All critical paths tested** (state machine, network, database)

#### Documentation
- [ ] ✅ **Migration documented** (this plan, PR description, commits)
- [ ] ✅ **README updated** (.NET 10.0 requirements documented)
- [ ] ✅ **Build instructions updated** (SDK version requirements)
- [ ] ✅ **Known issues documented** (if any remain)
- [ ] ✅ **Breaking changes communicated** to team

---

### Process Criteria

The migration follows proper process when:

#### All-At-Once Strategy Followed
- [ ] ✅ **All changes applied atomically** (single coordinated operation)
- [ ] ✅ **No intermediate multi-targeting** (direct net48 → net10.0)
- [ ] ✅ **Single validation checkpoint** (build + test once at end)
- [ ] ✅ **Strategy principles applied** as documented in plan

#### Source Control
- [ ] ✅ **Changes on upgrade branch** (`upgrade-to-NET10`)
- [ ] ✅ **Clear commit history** (atomic commit or logical commits)
- [ ] ✅ **Commit messages descriptive** (explain what and why)
- [ ] ✅ **PR created with complete description**
- [ ] ✅ **Code review completed** (if applicable)
- [ ] ✅ **Ready to merge to main**

#### Risk Management
- [ ] ✅ **High-risk items addressed**:
  - SDK conversion succeeded
  - Configuration migration completed
  - API incompatibilities fixed
  - BouncyCastle vulnerability resolved
- [ ] ✅ **Contingency plans available** (rollback possible via Git)
- [ ] ✅ **No blocking issues** identified

---

### Functional Criteria

The application maintains functionality when:

#### Core Functionality
- [ ] ✅ **Application starts successfully**
- [ ] ✅ **Authentication works** (WebHelper.Auth())
- [ ] ✅ **Car state machine functions**:
  - Start state works
  - Online state works
  - Drive state detection works
  - Charge state detection works
  - State transitions work correctly
- [ ] ✅ **Database operations work**:
  - Trip logging
  - Charging state logging
  - Data retrieval
- [ ] ✅ **Network operations work**:
  - HTTP requests succeed
  - HTTPS/TLS works
  - API calls succeed
- [ ] ✅ **Configuration access works** (app.config or migrated system)
- [ ] ✅ **Exception handling works** (Exceptionless integration)
- [ ] ✅ **Geocoding operations work** (cache read/write)

#### Non-Functional Requirements
- [ ] ✅ **Performance acceptable** (comparable to pre-migration)
- [ ] ✅ **Memory usage reasonable** (no leaks)
- [ ] ✅ **Application stable** over extended runtime
- [ ] ✅ **No unexpected behavior** from API changes

---

### Completion Checklist

**The .NET 10.0 upgrade is COMPLETE when**:

#### ✅ All Technical Criteria Met
- Project builds successfully
- All packages updated/removed correctly
- Security vulnerability resolved
- All breaking changes addressed
- Tests passing

#### ✅ All Quality Criteria Met
- Code quality maintained
- Test coverage maintained
- Documentation updated

#### ✅ All Process Criteria Met
- All-At-Once strategy followed
- Source control properly managed
- Risk items addressed

#### ✅ All Functional Criteria Met
- Application fully functional
- No regressions identified
- Performance acceptable

#### ✅ Ready for Production
- [ ] Code reviewed and approved
- [ ] Merged to main branch
- [ ] Tagged with version
- [ ] Team notified of changes
- [ ] Deployment plan ready

---

### Sign-Off

**Migration Sign-Off** (to be completed at end):

```
Migration Completed By: __________________
Date: __________________

Technical Validation: ✅ PASS / ❌ FAIL
Quality Validation: ✅ PASS / ❌ FAIL
Process Validation: ✅ PASS / ❌ FAIL
Functional Validation: ✅ PASS / ❌ FAIL

Overall Status: ✅ APPROVED / ⚠️ APPROVED WITH ISSUES / ❌ NOT APPROVED

Known Issues: 
[List any known issues or tech debt]

Notes:
[Any additional notes]

Reviewer: __________________
Date: __________________
```

---

### Post-Migration Success Metrics

**Track these metrics post-deployment**:

**Week 1**:
- [ ] Application stability (uptime %)
- [ ] Error rate (vs. baseline)
- [ ] Performance metrics (startup time, response times)
- [ ] Resource usage (memory, CPU)

**Week 2-4**:
- [ ] No critical bugs reported
- [ ] User acceptance confirmed
- [ ] Performance stable
- [ ] No security incidents

**Success Indicators**:
- ✅ Uptime ≥ pre-migration levels
- ✅ Error rate ≤ pre-migration levels
- ✅ Performance within 10% of baseline
- ✅ No critical bugs
- ✅ No security vulnerabilities
- ✅ Team productivity maintained

**If success metrics not met**: Execute rollback or fix-forward plan from Source Control Strategy section.
