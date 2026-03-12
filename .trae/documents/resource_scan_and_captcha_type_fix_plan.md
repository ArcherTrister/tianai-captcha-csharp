# Tianai Captcha - 资源扫描和验证码类型修复计划

## 问题分析

### 问题 1: 扫描文件夹添加资源没有效果
- **原因**: FileResourceProvider 在扫描目录时，资源的 Data 属性存储的是相对路径，而不是完整路径，导致 FileResourceProvider 无法找到文件。

### 问题 2: 最小 API 请求不同的验证码类型，都是返回随机的验证码
- **原因**: DefaultImageCaptchaApplication 在从预生成池获取验证码时，没有考虑请求的验证码类型，而是直接获取任意类型的验证码。

## 任务列表

### [x] 任务 1: 修复 FileResourceProvider 中的资源路径问题
- **优先级**: P0
- **描述**: 修改 FileResourceProvider 类，使其能够正确处理相对路径的资源。
- **成功标准**:
  - 扫描文件夹添加的资源能够被正确加载和使用。
- **测试要求**:
  - `programmatic` TR-1.1: 扫描包含背景图片的目录后，能够成功生成使用该图片的验证码。
  - `programmatic` TR-1.2: 扫描包含模板资源的目录后，能够成功生成使用该模板的验证码。

### [x] 任务 2: 修复 DefaultImageCaptchaApplication 中的验证码类型问题
- **优先级**: P0
- **描述**: 修改 DefaultImageCaptchaApplication 类，使其在从预生成池获取验证码时，考虑请求的验证码类型。
- **成功标准**:
  - 请求特定类型的验证码时，返回对应类型的验证码，而不是随机类型。
- **测试要求**:
  - `programmatic` TR-2.1: 请求 Slider 类型的验证码时，返回 Slider 类型的验证码。
  - `programmatic` TR-2.2: 请求 Rotate 类型的验证码时，返回 Rotate 类型的验证码。
  - `programmatic` TR-2.3: 请求 Concat 类型的验证码时，返回 Concat 类型的验证码。
  - `programmatic` TR-2.4: 请求 WordImageClick 类型的验证码时，返回 WordImageClick 类型的验证码。

### [x] 任务 3: 测试修复效果
- **优先级**: P1
- **描述**: 测试修复后的功能，确保两个问题都已解决。
- **成功标准**:
  - 扫描文件夹添加资源能够正常工作。
  - 请求不同类型的验证码时，返回对应类型的验证码。
- **测试要求**:
  - `programmatic` TR-3.1: 运行项目，测试扫描文件夹添加资源的功能。
  - `programmatic` TR-3.2: 运行项目，测试请求不同类型验证码的功能。

## 实现细节

### 任务 1 实现细节
1. 修改 FileResourceProvider 类的 GetResourceStream 方法，使其能够处理相对路径的资源。
2. 可能需要在 FileResourceProvider 中存储基础目录信息，以便在加载资源时使用。

### 任务 2 实现细节
1. 修改 ICaptchaPregenerationPool 接口，添加根据类型获取验证码的方法。
2. 修改 MemoryCaptchaPregenerationPool 类，实现根据类型获取验证码的方法。
3. 修改 DefaultImageCaptchaApplication 类，在从预生成池获取验证码时，使用请求的验证码类型。

## 风险评估

### 任务 1 风险
- **风险**: 修改 FileResourceProvider 可能会影响现有的资源加载逻辑。
- **缓解措施**: 确保修改后的代码能够向后兼容，同时添加适当的测试。

### 任务 2 风险
- **风险**: 修改预生成池的逻辑可能会影响验证码的生成性能。
- **缓解措施**: 确保修改后的代码能够高效地根据类型获取验证码，同时添加适当的测试。

## 预期完成时间

- 任务 1: 1 小时
- 任务 2: 1.5 小时
- 任务 3: 0.5 小时
- 总计: 3 小时