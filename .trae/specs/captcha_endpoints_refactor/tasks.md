# Tianai Captcha Endpoints Refactor - Implementation Plan

## [x] Task 1: Add API Endpoint Path Configuration to TianaiCaptchaOptions
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - Add properties to TianaiCaptchaOptions for configuring API endpoints
  - Set default values to use Web SDK compatible endpoints (/gen and /check)
- **Acceptance Criteria Addressed**: AC-1, AC-2
- **Test Requirements**:
  - `programmatic` TR-1.1: Verify that the default endpoints are Web SDK compatible ✅
  - `programmatic` TR-1.2: Verify that custom endpoint paths can be set ✅
- **Notes**: Ensure backward compatibility by maintaining Web SDK compatible defaults

## [x] Task 2: Refactor AddTianaiCaptcha to Include Endpoint Mapping
- **Priority**: P0
- **Depends On**: Task 1
- **Description**:
  - Add MapEndpoints method to ITianaiCaptchaBuilder
  - Update MapCaptchaEndpoints to use options from the service container
  - Include both standard endpoints and Web SDK compatible endpoints by default
- **Acceptance Criteria Addressed**: AC-1, AC-2
- **Test Requirements**:
  - `programmatic` TR-2.1: Verify that Web SDK compatible endpoints are mapped by default ✅
  - `programmatic` TR-2.2: Verify that custom endpoint paths can be configured ✅
- **Notes**: Ensure that the endpoint mapping happens after the application builder is built

## [x] Task 3: Remove Redundant Endpoints from Sample Project
- **Priority**: P1
- **Depends On**: Task 2
- **Description**:
  - Remove the Web SDK compatible endpoints (/gen, /check) from the sample project
  - Remove the MapCaptchaEndpoints call since it's now integrated into AddTianaiCaptcha
  - Use the new MapEndpoints method from ITianaiCaptchaBuilder
- **Acceptance Criteria Addressed**: AC-3
- **Test Requirements**:
  - `programmatic` TR-3.1: Verify that /gen and /check endpoints are available (now provided by the integrated endpoint mapping) ✅
  - `programmatic` TR-3.2: Verify that the standard captcha endpoints still work ✅
- **Notes**: Keep the index.html redirection and static files middleware

## [x] Task 4: Test Directory Scanning Functionality in Sample Project
- **Priority**: P1
- **Depends On**: Task 3
- **Description**:
  - Add a ScanDirectory call to the AddTianaiCaptcha configuration in the sample project
  - Configure it to scan the wwwroot/CaptchaResources directory
  - Verify that the resources are loaded correctly
- **Acceptance Criteria Addressed**: AC-4
- **Test Requirements**:
  - `programmatic` TR-4.1: Verify that the directory scanning service is registered ✅
  - `programmatic` TR-4.2: Verify that resources from the directory are available for captcha generation ✅
- **Notes**: Ensure the directory path is correctly specified
