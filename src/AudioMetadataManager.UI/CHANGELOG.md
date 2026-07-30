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
- Added `IMetadataWriterVerificationEngine` as the reusable contract for
  post-write metadata verification.
- Added `MetadataVerificationStage` as the fourth concrete stage of the
  modular metadata application pipeline.
- Added structural coverage for successful verification, mismatches, and
  missing, skipped, cancelled, and failed write outcomes.
- Added `MetadataApplicationPipelineFactory` as the centralized default
  composition point for the metadata application pipeline.
- Added the default validation, backup, writing, and post-write verification
  stage composition.
- Added structural coverage for stage count, concrete types, identities,
  execution orders, safe default options, independent creation, and null
  option rejection.
- Added `MetadataApplicationPipelineIsolatedTestRunner` for isolated
  end-to-end execution of the default metadata application pipeline.
- Added `MetadataApplicationPipelineIsolatedTestResult` to consolidate
  pipeline execution, file isolation, artwork preservation, cleanup,
  timing, and error evidence.
- Added temporary diagnostic integration for executing the complete
  pipeline over selected audio files.

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
- Updated `MetadataWriterVerificationEngine` to implement the reusable
  verification contract.
- Preserved the pre-write embedded artwork count to verify artwork
  preservation after writing.
- Integrated verification results with `MetadataApplicationContext` and
  `MetadataApplicationPipelineResult`.
- Updated the verification stage to interpret the previous write result
  before invoking the verification engine.
- Centralized construction of the default metadata application pipeline
  outside `MainWindow` and individual consumers.
- Configured the default composition to reject duplicate execution orders.
- Preserved explicit context completion until a final result-building stage
  is incorporated.
- Reused `MetadataApplicationPipelineFactory.CreateDefault()` to execute
  validation, backup, writing, and post-write verification as one
  integrated workflow.
- Captured pipeline backup, write, verification, persisted genre, and
  artwork results before removing the isolated test environment.

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
- Prevented post-write verification from running when no previous write
  result is available.
- Prevented skipped, cancelled, or failed writes from being verified as
  successfully completed writes.
- Prevented duplicate execution of the concrete metadata verification stage.
- Propagated `PictureCountBefore` through
  `MetadataWriterEngine.MergeWriterResult(...)` so post-write verification
  receives the actual pre-write embedded artwork count.

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
- Added controlled verification engine injection for structural stage
  testing.
- Verified input mapping, cancellation-token forwarding, auditable
  information, and verification-result storage in the shared context.
- Temporarily integrated `MetadataVerificationStageTestRunner` into the
  `MainWindow` diagnostic workflow.
- Passed thirteen structural checks for the metadata verification stage.
- Revalidated isolated MP3 writing, including preservation of the original
  file, backup, and embedded artwork.
- Added `MetadataApplicationPipelineFactoryTestRunner`.
- Added `MetadataApplicationPipelineFactoryTestResult`.
- Verified the default registration of validation, backup, writing, and
  post-write verification stages.
- Verified stage identities and execution orders `100`, `200`, `300`, and
  `400`.
- Verified safe default pipeline options and independent instances across
  successive factory calls.
- Verified rejection of null pipeline options.
- Temporarily integrated the pipeline composition runner into the
  `MainWindow` diagnostic workflow.
- Passed eight structural checks for the default pipeline composition.
- Revalidated the structural verification-stage checks and isolated MP3
  diagnostic after integrating the composition test.
- Executed the complete four-stage metadata application pipeline over
  isolated MP3 and FLAC working copies.
- Verified successful backup creation, genre writing, post-write
  verification, and temporary directory cleanup.
- Verified that the original MP3 and FLAC files remained unchanged.
- Verified embedded artwork preservation with one picture in MP3 and two
  pictures in FLAC.
- Verified that the isolated working copies changed while the initial and
  pipeline backups preserved their pre-write state.
- Recorded test start, completion, elapsed time, exceptions, and auditable
  stage messages.