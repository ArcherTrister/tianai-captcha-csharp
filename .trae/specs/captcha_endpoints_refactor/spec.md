# Tianai Captcha Endpoints Refactor - Product Requirement Document

## Overview
- **Summary**: Refactor the Tianai Captcha library to integrate endpoint mapping into the AddTianaiCaptcha extension method with configurable API paths, remove redundant endpoints from the sample project, and test the directory scanning functionality for captcha resources.
- **Purpose**: Simplify user configuration by integrating endpoint setup into the main extension method and provide more flexible API path configuration.
- **Target Users**: Developers using Tianai Captcha in their ASP.NET Core applications.

## Goals
- Integrate MapCaptchaEndpoints into AddTianaiCaptcha extension method
- Make API endpoint paths configurable via options with default values using Web SDK compatible endpoints
- Completely remove Web SDK compatible endpoints from the sample project
- Test directory scanning functionality for captcha resources in the sample project

## Non-Goals (Out of Scope)
- Changing the core captcha generation and validation logic
- Adding new captcha types or features
- Modifying existing resource management functionality

## Background & Context
- Currently, MapCaptchaEndpoints is a separate extension method that needs to be called on the app builder
- The sample project contains both standard endpoints and Web SDK compatible endpoints, causing redundancy
- The library has directory scanning functionality that needs to be tested with the sample resources

## Functional Requirements
- **FR-1**: Add API endpoint path configuration to TianaiCaptchaOptions with default values
- **FR-2**: Integrate MapCaptchaEndpoints functionality into AddTianaiCaptcha extension method
- **FR-3**: Remove redundant captcha API endpoints from the sample project
- **FR-4**: Use directory scanning to add captcha resources from the sample project's wwwroot directory

## Non-Functional Requirements
- **NFR-1**: Maintain backward compatibility with existing code
- **NFR-2**: Ensure the refactored code follows existing coding conventions
- **NFR-3**: The directory scanning functionality should work correctly with the sample resources

## Constraints
- **Technical**: ASP.NET Core 6.0+
- **Dependencies**: Existing Tianai Captcha library dependencies

## Assumptions
- The directory scanning functionality is already implemented
- The sample project's wwwroot/CaptchaResources directory contains valid captcha resources

## Acceptance Criteria

### AC-1: API Endpoint Path Configuration
- **Given**: A developer uses AddTianaiCaptcha without specifying endpoint paths
- **When**: The application starts
- **Then**: Captcha endpoints should be mapped with default Web SDK compatible paths
- **Verification**: `programmatic`

### AC-2: Custom API Endpoint Paths
- **Given**: A developer configures custom endpoint paths in TianaiCaptchaOptions
- **When**: The application starts
- **Then**: Captcha endpoints should be mapped with the custom paths
- **Verification**: `programmatic`

### AC-3: Redundant Endpoints Removed
- **Given**: The sample project is run
- **When**: Accessing the old Web SDK compatible endpoints (/gen, /check)
- **Then**: These endpoints should no longer be available
- **Verification**: `programmatic`

### AC-4: Directory Scanning Test
- **Given**: The sample project is configured to scan the wwwroot/CaptchaResources directory
- **When**: The application starts
- **Then**: The captcha resources should be loaded from the directory
- **Verification**: `programmatic`

## Open Questions
- [x] What should be the default API endpoint path? - Use Web SDK compatible endpoints
- [x] Should the Web SDK compatible endpoints be completely removed or just moved to the main endpoint configuration? - Completely remove them
