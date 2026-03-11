# Tianai.Captcha Resource Extension Fix Plan

## \[ ] Task 1: Add AddBackgroundResource extension method

* **Priority**: P0

* **Depends On**: None

* **Description**:

  * Add AddBackgroundResource method to match DefaultImageCaptchaResourceManager

  * Ensure it follows the same pattern as other extension methods

  * Map to the existing background resource handling

* **Success Criteria**:

  * AddBackgroundResource method is properly implemented

  * Method follows the same pattern as other extension methods

* **Test Requirements**:

  * `programmatic` TR-1.1: Method compiles successfully

  * `human-judgement` TR-1.2: Method follows the same pattern as other extension methods

## \[ ] Task 2: Add AddFontResource extension method

* **Priority**: P0

* **Depends On**: Task 1

* **Description**:

  * Add AddFontResource method to match DefaultImageCaptchaResourceManager

  * Ensure it follows the same pattern as other extension methods

  * Map to the existing font resource handling

* **Success Criteria**:

  * AddFontResource method is properly implemented

  * Method follows the same pattern as other extension methods

* **Test Requirements**:

  * `programmatic` TR-2.1: Method compiles successfully

  * `human-judgement` TR-2.2: Method follows the same pattern as other extension methods

## \[ ] Task 3: Keep existing AddResource and AddTemplate methods

* **Priority**: P0

* **Depends On**: Task 2

* **Description**:

  * Keep the existing AddResource method for backward compatibility

  * Keep the existing AddTemplate method as the single template extension method

  * Ensure all methods work together properly

* **Success Criteria**:

  * AddResource and AddTemplate methods remain functional

  * All resource methods work together properly

* **Test Requirements**:

  * `programmatic` TR-3.1: All methods compile successfully

  * `human-judgement` TR-3.2: Code is clean and follows conventions

## \[ ] Task 4: Remove commented-out template methods

* **Priority**: P0

* **Depends On**: Task 3

* **Description**:

  * Remove the commented-out AddSliderTemplate and AddRotateTemplate methods

  * Ensure the code is clean and follows the same pattern

* **Success Criteria**:

  * AddSliderTemplate and AddRotateTemplate methods are removed

  * Code is clean and follows conventions

* **Test Requirements**:

  * `programmatic` TR-4.1: Method compiles successfully

  * `human-judgement` TR-4.2: Code is clean and follows conventions

## \[ ] Task 5: Verify the fix

* **Priority**: P1

* **Depends On**: Task 4

* **Description**:

  * Run the build to ensure no compilation errors

  * Check that all resource methods are properly integrated with the existing code

* **Success Criteria**:

  * Project builds successfully

  * No compilation errors related to the restored methods

* **Test Requirements**:

  * `programmatic` TR-5.1: dotnet build completes successfully

  * `human-judgement` TR-5.2: All methods are properly integrated and follow code conventions

