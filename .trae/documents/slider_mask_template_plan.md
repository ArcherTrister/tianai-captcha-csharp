# Tianai Captcha - Slider Mask Template Implementation Plan

## [x] Task 1: Add Mask Template Support to SliderImageCaptchaGenerator
- **Priority**: P0
- **Depends On**: None
- **Description**: 
  - Add `TemplateMaskImageName` constant to SliderImageCaptchaGenerator
  - Load mask template in DoGenerateCaptchaImage method
  - Apply mask template to the cut image
- **Success Criteria**: 
  - SliderImageCaptchaGenerator now supports mask templates
  - Mask template is applied to the cut image before overlaying active template
- **Test Requirements**:
  - `programmatic` TR-1.1: SliderImageCaptchaGenerator can load and apply mask templates
  - `programmatic` TR-1.2: Generated slider captchas with mask templates look correct
  - `human-judgement` TR-1.3: Code follows the same pattern as RotateImageCaptchaGenerator
- **Notes**: Follow the same pattern as RotateImageCaptchaGenerator for mask template handling

## [x] Task 2: Test the Implementation
- **Priority**: P1
- **Depends On**: Task 1
- **Description**: 
  - Run the captcha generator to test mask template functionality
  - Verify that mask templates are properly applied
  - Ensure no regressions in existing functionality
- **Success Criteria**:
  - Slider captcha generation works correctly with mask templates
  - No errors occur during generation
  - Existing functionality remains intact
- **Test Requirements**:
  - `programmatic` TR-2.1: Generate slider captcha with mask template
  - `programmatic` TR-2.2: Generate slider captcha without mask template (backward compatibility)
  - `human-judgement` TR-2.3: Generated captchas look visually correct
- **Notes**: Test with both mask and non-mask template scenarios

## [x] Task 3: Update Documentation
- **Priority**: P2
- **Depends On**: Task 2
- **Description**: 
  - Update any relevant documentation to reflect the new mask template support
  - Ensure documentation matches the implementation
- **Success Criteria**:
  - Documentation is up-to-date with the new functionality
  - No conflicting information in documentation
- **Test Requirements**:
  - `human-judgement` TR-3.1: Documentation accurately describes mask template support
  - `human-judgement` TR-3.2: Documentation is consistent with implementation
- **Notes**: Check if there are any README or usage documents that need updating