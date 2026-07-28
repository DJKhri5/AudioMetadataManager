# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog and the project follows
Semantic Versioning where applicable.

## [Unreleased]

### Added

- Reusable file isolation testing infrastructure.
- Shared test context for original files, working copies, and backups.
- SHA-256 verification for isolated file operations.
- Reusable post-operation file safety verification.
- Controlled cleanup support for temporary test environments.
- Added `MetadataApplicationContext` as the shared execution state for
  metadata application pipelines.
- Added common contracts and base infrastructure for modular pipeline
  stages.
- Added `MetadataApplicationPipelineExecutor` for ordered stage
  execution and configurable flow control.
- Added structural tests for the application context, stage base, and
  pipeline executor.

### Changed

- Prepared the testing architecture to share file isolation logic
  between writer tests and application pipeline tests.
- Refactored `TagLibIsolatedWriteTestRunner` to use the shared
  `FileIsolationTestHarness`.
- Delegated working-copy creation, backup generation, SHA-256
  calculation, and isolation verification to the common testing
  infrastructure.
- Preserved the existing MP3 and FLAC isolated-write test behavior.
- Prepared the application pipeline to operate through independent,
  ordered stages sharing a single execution context.
- Centralized stage timing, exception handling, cancellation reporting,
  and auditable result registration.
- Decoupled pipeline execution flow from concrete validation, backup,
  writing, and verification implementations.

### Fixed

- No user-facing defects were addressed in this milestone.
- Removed duplicated file-isolation and hash-verification logic from
  the TagLibSharp isolated writer test runner.
- Prevented duplicate stage identities from being registered in the
  pipeline executor.
- Added optional rejection of duplicate execution orders.
- Prevented completed contexts from being modified or executed again.

### Internal

- Added `FileIsolationContext`.
- Added `FileIsolationVerificationResult`.
- Added `FileIsolationTestHarness`.
- Prepared the codebase for end-to-end metadata application testing.
- Added dependency injection support for `FileIsolationTestHarness`
  in `TagLibIsolatedWriteTestRunner`.
- Centralized isolated file safety checks for current and future writer
  integration tests.
- Revalidated real MP3 writing after the infrastructure refactor.
- Added `MetadataApplicationStageBase`.
- Added `MetadataApplicationPipelineOptions`.
- Added `MetadataApplicationPipelineExecutionResult`.
- Added structural coverage for ordering, blocking failures, duplicate
  identities, strict ordering, automatic completion, and context
  preservation.