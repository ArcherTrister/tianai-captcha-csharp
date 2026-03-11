# Tianai Captcha - Resource Management Optimization Plan

## [ ] Task 1: Fix AddResourceAssembly method to handle multiple calls efficiently
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - Modify AddResourceAssembly to check if the assembly is already registered before adding it as a singleton
  - This prevents duplicate registrations when the method is called multiple times
- **Success Criteria**:
  - Calling AddResourceAssembly multiple times with the same assembly doesn't result in duplicate singleton registrations
  - The assembly is still properly registered with EmbeddedResourceProvider
- **Test Requirements**:
  - `programmatic` TR-1.1: Verify that calling AddResourceAssembly twice with the same assembly only adds one singleton registration
  - `programmatic` TR-1.2: Verify that the assembly is still properly registered with EmbeddedResourceProvider
- **Notes**: Use a HashSet or similar mechanism to track registered assemblies

## [ ] Task 2: Fix ScanDirectory method to handle multiple calls efficiently
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - Modify ScanDirectory to prevent adding multiple DirectoryScannerService instances
  - Instead, collect all directory paths and scan them with a single service
- **Success Criteria**:
  - Calling ScanDirectory multiple times doesn't result in multiple hosted services
  - All specified directories are still properly scanned
- **Test Requirements**:
  - `programmatic` TR-2.1: Verify that calling ScanDirectory multiple times only adds one hosted service
  - `programmatic` TR-2.2: Verify that all specified directories are properly scanned
- **Notes**: Use a collection to store directory paths and process them all in a single service

## [ ] Task 3: Rename ScanDirectory to AddDirectoryResource for naming consistency
- **Priority**: P1
- **Depends On**: Task 2
- **Description**:
  - Rename the ScanDirectory method to AddDirectoryResource to match the naming pattern of other resource-related methods
  - Update the DirectoryScannerService class and related documentation accordingly
- **Success Criteria**:
  - The method is renamed to AddDirectoryResource
  - All references to ScanDirectory are updated
  - The method continues to function as expected
- **Test Requirements**:
  - `programmatic` TR-3.1: Verify that the method is now called AddDirectoryResource
  - `programmatic` TR-3.2: Verify that the method still works correctly with the new name
  - `human-judgement` TR-3.3: Verify that the naming is consistent with other resource-related methods
- **Notes**: Update any documentation or comments that reference the old method name

## [ ] Task 4: Test the optimized implementation
- **Priority**: P2
- **Depends On**: Tasks 1, 2, 3
- **Description**:
  - Test the optimized methods to ensure they work correctly
  - Verify that multiple calls to AddResourceAssembly and AddDirectoryResource don't cause performance issues
- **Success Criteria**:
  - All tests pass
  - No performance degradation when calling the methods multiple times
- **Test Requirements**:
  - `programmatic` TR-4.1: Run existing tests to ensure no regressions
  - `programmatic` TR-4.2: Test multiple calls to AddResourceAssembly with the same assembly
  - `programmatic` TR-4.3: Test multiple calls to AddDirectoryResource with different directories
  - `human-judgement` TR-4.4: Verify that the code is clean and follows best practices
- **Notes**: Test edge cases like null values, empty directories, etc.
