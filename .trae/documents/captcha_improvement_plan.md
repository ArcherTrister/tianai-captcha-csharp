# Tianai Captcha - 验证码系统改进计划

## 问题分析
1. **使用字符串表示验证码类型的风险**：当前代码中部分地方使用字符串来表示验证码类型，容易导致书写错误
2. **资源管理问题**：缺少专门的方法来添加背景图、模板、字体资源，三者没有独立管理，导致添加字体资源时可能报错

## 改进计划

### [x] 任务1：优先使用CaptchaType枚举
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - 修改InMemoryResourceStore，将string type参数改为CaptchaType枚举
  - 修改相关的方法签名和内部实现
  - 修改DefaultImageCaptchaApplication中的相关方法，使用CaptchaType枚举
  - 确保所有使用字符串表示验证码类型的地方都改为使用CaptchaType枚举
- **Success Criteria**:
  - 所有验证码类型相关的参数和变量都使用CaptchaType枚举
  - 代码中不再使用字符串来表示验证码类型
- **Test Requirements**:
  - `programmatic` TR-1.1: 编译通过，无类型错误
  - `programmatic` TR-1.2: 所有测试用例通过
  - `human-judgement` TR-1.3: 代码可读性提高，类型安全增强

### [x] 任务2：提供专门的资源添加方法
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - 在IImageCaptchaResourceManager接口中添加专门的方法来添加背景图、模板、字体资源
  - 在DefaultImageCaptchaResourceManager中实现这些方法
  - 确保这些方法能够正确地将资源添加到资源存储中
- **Success Criteria**:
  - IImageCaptchaResourceManager接口中添加了专门的资源添加方法
  - DefaultImageCaptchaResourceManager实现了这些方法
  - 可以通过这些方法独立管理背景图、模板、字体资源
- **Test Requirements**:
  - `programmatic` TR-2.1: 编译通过，无类型错误
  - `programmatic` TR-2.2: 可以通过新方法添加不同类型的资源
  - `human-judgement` TR-2.3: 资源管理API更加清晰易用

### [x] 任务3：修改资源存储实现
- **Priority**: P1
- **Depends On**: 任务1
- **Description**:
  - 修改InMemoryResourceStore，使用CaptchaType枚举作为资源存储的键
  - 确保资源存储能够正确处理不同类型的资源
- **Success Criteria**:
  - InMemoryResourceStore使用CaptchaType枚举作为键
  - 资源存储能够正确处理不同类型的资源
- **Test Requirements**:
  - `programmatic` TR-3.1: 编译通过，无类型错误
  - `programmatic` TR-3.2: 资源存储能够正确存储和获取不同类型的资源

### [x] 任务4：更新相关方法和调用
- **Priority**: P1
- **Depends On**: 任务1, 任务2
- **Description**:
  - 更新所有调用资源管理相关方法的代码
  - 确保所有方法调用都使用正确的参数类型
- **Success Criteria**:
  - 所有调用资源管理相关方法的代码都已更新
  - 所有方法调用都使用正确的参数类型
- **Test Requirements**:
  - `programmatic` TR-4.1: 编译通过，无类型错误
  - `programmatic` TR-4.2: 所有测试用例通过

### [x] 任务5：测试和验证
- **Priority**: P2
- **Depends On**: 任务1, 任务2, 任务3, 任务4
- **Description**:
  - 运行所有测试用例
  - 验证验证码系统的功能正常
  - 验证资源管理功能正常
- **Success Criteria**:
  - 所有测试用例通过
  - 验证码系统功能正常
  - 资源管理功能正常
- **Test Requirements**:
  - `programmatic` TR-5.1: 所有测试用例通过
  - `human-judgement` TR-5.2: 系统运行正常，无错误
