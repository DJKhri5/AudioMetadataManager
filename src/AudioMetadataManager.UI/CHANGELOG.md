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
- Added `IMetadataApplyResultBuilder` as the reusable contract for building
  consolidated metadata application results.
- Added `MetadataApplyResultBuilder` to consolidate field results, operation
  status, timing, paths, identifiers, and deduplicated auditable messages.
- Added `MetadataFinalizationStage` as the fifth and final stage of the
  modular metadata application pipeline.
- Added structural coverage for consolidated result construction and final
  status mapping.
- Added `IMetadataApplicationCoordinator` as the productive entry-point
  contract for approved metadata application requests.
- Added `MetadataApplicationCoordinator` to execute the complete default
  metadata application pipeline through a single reusable operation.
- Added controlled coordinator coverage for null requests, null factories,
  pre-cancelled execution, null executors, and factory exceptions.
- Added automatic synchronization between approved simulation changes and
  the productive apply button state.
- Added explicit user confirmation before starting a future productive
  metadata application.
- Added `MetadataApplyRequestIsolationFactory` to redirect approved requests
  to temporary working copies while preserving request data.
- Added controlled coverage for isolated request preparation.
- Added a resizable activity log area to the main window.
- Added `MetadataApplicationIsolatedExecutionResult` to consolidate
  pipeline, isolation, verification, cleanup, and error evidence.
- Added `MetadataApplicationIsolatedExecutor` to coordinate complete
  metadata application over temporary working copies.
- Added controlled end-to-end coverage for the coordinated isolated
  executor.
- Added isolated pipeline execution from the approved-changes action in
  `MainWindow`.
- Added user-facing completion and failure reporting for isolated
  application operations.
- Added configurable lifecycle options for isolated metadata application
  executions.
- Added controlled preservation of successfully verified working copies.
- Added `MetadataApplicationPromotionResult` for consolidated promotion,
  backup, verification, rollback, and safety evidence.
- Added `IMetadataApplicationPromotionService` and
  `MetadataApplicationPromotionService`.
- Added reusable `FileSha256Service` for binary file-integrity checks.
- Added productive backup creation before controlled destination
  replacement.
- Added same-directory staging files for controlled promotion and rollback.
- Added configurable promotion options for normal execution and simulated
  post-replacement verification failures.
- Added automatic rollback from a verified productive backup.
- Added controlled temporary-file coverage for successful promotion and
  automatic rollback.
- Added `MetadataPromotionDecision` to represent the second-confirmation state.
- Added `MetadataProductiveApplicationResult` to consolidate isolated preparation, promotion decisions, cleanup, and final safety evidence.
- Added `IMetadataProductiveApplicationCoordinator` and `MetadataProductiveApplicationCoordinator`.
- Added two-phase productive application coordination through `PrepareAsync` and `CompleteAsync`.
- Added single-use protection for preserved productive preparations.
- Added controlled cleanup restricted to valid reserved completions.
- Added controlled coverage for safe `Declined` completion.
- Added controlled coverage for successful `Approved` promotion over temporary destinations.
- Added real two-phase productive application integration to `MainWindow`.
- Added explicit user-facing second confirmation before modifying an original audio file.
- Added UI handling for `Approved` and `Declined` productive promotion decisions.
- Added productive completion reporting for successful promotion, safe rejection, and incomplete finalization.
- Added auditable UI logging for productive backup, promotion, cleanup, and final safety state.

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
- Extended the default metadata application pipeline with the finalization
  stage at execution order `500`.
- Updated the default pipeline composition tests to validate five stages,
  their concrete types, identities, and execution orders.
- Updated the isolated end-to-end pipeline test to register and execute all
  five stages.
- Consolidated validation, backup, writing, verification, and finalization
  evidence into `MetadataApplyResult`.
- Centralized creation of `MetadataApplicationContext` and execution of the
  default five-stage pipeline in `MetadataApplicationCoordinator`.
- Translated pipeline cancellation and stage failures into auditable
  `MetadataApplicationStopReason` values.
- Integrated the coordinator test runner into the temporary technical
  diagnostic workflow.
- Centralized active simulation plan replacement and property-change
  subscriptions in `MainWindow`.
- Updated the apply action to react automatically when approved proposals
  are selected or cleared.
- Compactened the main window header and redistributed vertical space
  between the file table, details, and activity log.
- Integrated isolated request factory diagnostics into the temporary
  technical diagnostic workflow.
- Converted `ApplyChangesButton_Click` to an asynchronous isolated
  application workflow.
- Replaced the provisional apply confirmation behavior with execution of
  the complete five-stage pipeline over a temporary working copy.
- Updated the apply confirmation message to explain temporary-copy
  execution, original-file protection, and automatic cleanup.
- Routed approved simulation plans through `MetadataApplyRequestFactory`
  and `MetadataApplicationIsolatedExecutor`.
- Temporarily disabled the apply button while an isolated execution is in
  progress.
- Extended the temporary technical diagnostic workflow with coordinated
  isolated-executor validation.
- Extended `MetadataApplicationIsolatedExecutionResult` to distinguish
  automatic cleanup from intentional environment preservation.
- Updated `MetadataApplicationIsolatedExecutor` to support configurable
  cleanup and successful working-copy preservation.
- Preserved the existing safe-cleanup behavior for all current callers.
- Refactored `FileIsolationTestHarness` to use the shared
  `FileSha256Service`.
- Extended the promotion service contract with configurable execution
  options.
- Updated the temporary technical diagnostic workflow to validate
  preserved executions, controlled promotion, and automatic rollback.
- Extended the technical diagnostic with productive coordinator preparation, rejection, approval, promotion, verification, and cleanup evidence.
- Restricted isolated-environment cleanup so invalid promotion decisions do not consume pending preparations.
- Replaced direct isolated execution in `ApplyChangesButton_Click` with `MetadataProductiveApplicationCoordinator.PrepareAsync`.
- Updated the apply workflow to preserve verified working copies until the user completes the second confirmation.
- Connected the approved-changes action to `MetadataProductiveApplicationCoordinator.CompleteAsync`.
- Updated the apply button to promote verified copies only after an explicit `Approved` decision.
- Updated the declined path to remove the preserved environment without modifying the original file.
- Updated user-facing messages to distinguish productive preparation, successful application, safe cancellation, and incomplete finalization.

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
- Prevented a pre-cancelled productive execution from invoking the pipeline
  executor factory.
- Prevented null executor instances and factory exceptions from escaping
  without a finalized auditable result.
- Prevented the productive apply action from being enabled without an active
  plan containing approved changes.
- Prevented stale simulation plans from remaining active after selecting or
  rescanning a library.
- Prevented an application confirmation from proceeding when the plan or
  approved changes are no longer available.
- Prevented the activity log from becoming effectively unusable at the
  default window size.
- Removed a duplicated active-plan reset from `ScanButton_Click`.
- Prevented a simulation plan from remaining active after selecting a
  different library folder.
- Prevented repeated apply-button activation while an isolated pipeline
  execution is in progress.
- Prevented the approved-changes action from delivering the original file
  path directly to the metadata writer.
- Prevented preserved successful executions from being reported as failed
  only because automatic cleanup was intentionally deferred.
- Prevented a destination replacement from starting before its productive
  backup was created and verified.
- Prevented unverified staging files from replacing a destination.
- Prevented a failed post-replacement verification from leaving the
  destination without an automatic restoration attempt.
- Prevented promotion and rollback helper files from remaining after a
  completed operation whenever cleanup succeeds.

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
- Added controlled structural testing for `MetadataApplyResultBuilder`.
- Verified preservation of request and plan identifiers, file information,
  backup paths, field values, write states, and verification states.
- Verified final status calculation, coherent timing, message consolidation,
  and duplicate-message removal.
- Updated the pipeline composition to validation, backup, writing,
  post-write verification, and finalization.
- Verified stage identities and execution orders `100`, `200`, `300`, `400`,
  and `500`.
- Executed the complete five-stage pipeline over an isolated FLAC working
  copy.
- Verified successful result construction, backup, writing, post-write
  verification, and temporary directory cleanup.
- Verified preservation of the original FLAC file and its two embedded
  pictures.
- Added `MetadataApplicationCoordinatorTestRunner`.
- Added `MetadataApplicationCoordinatorTestResult`.
- Verified rejection of null requests and null executor factories.
- Verified controlled handling of pre-cancelled executions, null executors,
  and factory exceptions.
- Verified that all controlled coordinator results receive a completion time.
- Revalidated the complete five-stage pipeline over an isolated FLAC working
  copy after integrating the coordinator diagnostics.
- Revalidated preservation of the original FLAC file and its two embedded
  pictures.
- Added `MetadataApplyRequestIsolationFactoryTestResult`.
- Added `MetadataApplyRequestIsolationFactoryTestRunner`.
- Verified rejection of null requests and empty working-copy paths.
- Verified preservation of request identifiers, creation time, approved
  changes, backup requirements, and post-write verification requirements.
- Verified replacement of only the destination path and file name.
- Added temporary diagnostic reporting for isolated request preparation.
- Revalidated the complete five-stage pipeline over an isolated FLAC file.
- Revalidated preservation of the original FLAC file and its two embedded
  pictures.
- Added a `GridSplitter` between the details panel and activity log.
- Added `MetadataApplicationIsolatedExecutorTestResult`.
- Added `MetadataApplicationIsolatedExecutorTestRunner`.
- Verified preparation of the isolated execution environment.
- Verified complete coordinator and five-stage pipeline execution over a
  temporary FLAC working copy.
- Verified persistence of the requested genre value.
- Verified that the original FLAC file remained unchanged.
- Verified that the temporary working copy changed.
- Verified preservation of the initial working-copy backup.
- Verified successful removal of the isolated execution directory.
- Revalidated embedded-artwork preservation with two FLAC pictures.
- Integrated temporary diagnostic reporting for the coordinated isolated
  executor.
- Validated approved isolated execution from the main-window apply action.
- Verified five successful stages and one applied field from the
  user-initiated isolated workflow.
- Added `MetadataApplicationIsolatedExecutionOptions`.
- Added `MetadataApplicationPreservedExecutionTestResult`.
- Added `MetadataApplicationPreservedExecutionTestRunner`.
- Added `MetadataApplicationPromotionOptions`.
- Added `MetadataApplicationPromotionTestResult`.
- Added `MetadataApplicationPromotionTestRunner`.
- Added `MetadataApplicationRollbackTestResult`.
- Added `MetadataApplicationRollbackTestRunner`.
- Verified deferred automatic cleanup after a successful isolated
  execution.
- Verified that a preserved working copy and its initial backup remained
  available before explicit cleanup.
- Verified successful manual cleanup of a preserved isolated environment.
- Verified productive backup creation and SHA-256 validation.
- Verified successful replacement of a temporary destination with a
  verified working copy.
- Verified that the promoted destination matched the verified copy.
- Verified that the reference original remained unchanged.
- Verified simulated post-replacement verification failure handling.
- Verified automatic rollback from the productive backup.
- Verified restoration of the destination to its original SHA-256 hash.
- Verified that promotion and rollback tests removed their temporary
  environments and temporary backups.
- Revalidated the complete five-stage pipeline and FLAC artwork
  preservation after the promotion infrastructure changes.