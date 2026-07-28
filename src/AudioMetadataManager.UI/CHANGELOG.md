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
- Added `IMetadataApplyRequestValidator` as the reusable validation
  contract for metadata application requests.
- Added `MetadataValidationStage` as the first concrete stage of the
  modular metadata application pipeline.
- Added structural coverage for valid, warning, and invalid validation
  outcomes.
- Added `IMetadataBackupEngine` as the reusable contract for creating and
  verifying metadata backups.
- Added `MetadataBackupStage` as the second concrete stage of the modular
  metadata application pipeline.
- Added structural coverage for successful, failed, and cancelled backup
  outcomes.
- Added `IMetadataWriterEngine` as the reusable contract for executing
  metadata write requests.
- Added `MetadataWritingStage` as the third concrete stage of the modular
  metadata application pipeline.
- Added structural coverage for successful, no-writable-change, cancelled,
  failed, and missing-backup writing scenarios.

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
- Updated `MetadataApplyRequestValidator` to implement the reusable
  validation contract.
- Integrated validation results with the shared
  `MetadataApplicationContext`.
- Updated `MetadataBackupEngine` to implement the reusable backup engine
  contract.
- Integrated verified backup results with the shared
  `MetadataApplicationContext`.
- Extended the common stage infrastructure to represent controlled
  cancellation results.
- Updated `MetadataWriterEngine` to implement the reusable writer engine
  contract.
- Integrated metadata writing results with the shared
  `MetadataApplicationContext`.
- Required a verified backup before the metadata writing stage can invoke its
  writer engine.

### Fixed

- No user-facing defects were addressed in this milestone.
- Removed duplicated file-isolation and hash-verification logic from
  the TagLibSharp isolated writer test runner.
- Prevented duplicate stage identities from being registered in the
  pipeline executor.
- Added optional rejection of duplicate execution orders.
- Prevented completed contexts from being modified or executed again.
- Prevented duplicate execution of the concrete metadata validation
  stage through the common stage infrastructure.
- Prevented duplicate execution of the concrete metadata backup stage
  through the common stage infrastructure.
- Prevented metadata writing from starting when no verified backup is
  available.
- Prevented duplicate execution of the concrete metadata writing stage
  through the common stage infrastructure.

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
- Added controlled in-memory validator injection for structural stage
  testing.
- Verified auditable validation results, context storage, and status
  mapping without requiring real audio files.
- Added controlled in-memory backup engine injection for structural stage
  testing.
- Verified backup request mapping, cancellation-token forwarding,
  auditable results, and context storage.
- Verified cleanup of the isolated temporary file used by the successful
  backup test.
- Added controlled writer engine injection for structural stage testing.
- Verified write-request mapping, cancellation-token forwarding, auditable
  results, and context storage.
- Verified special handling of `NoWritableChanges` as a completed stage with
  warnings.
- Verified cleanup of the isolated temporary backup used by the writing-stage
  test.