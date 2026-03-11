# 资源存储重构实现计划

## 任务概述
根据用户需求，对 Tianai.Captcha 项目的资源存储系统进行重构，主要包括：
1. 移除字符串重载方法，无需支持旧代码
2. 为不同的资源定义不同的类型对象和存储器
3. 明确添加资源的方法，不同资源对应不同的添加方法，便于使用枚举类型，减少 toString 调用

## 详细任务分解

### [x] 任务 1: 重构 ICrudResourceStore 接口
- **优先级**: P0
- **Depends On**: None
- **Description**:
  - 移除 ICrudResourceStore 接口中的字符串重载方法
  - 保留基于 CaptchaType 枚举的方法
- **Success Criteria**:
  - ICrudResourceStore 接口只包含基于 CaptchaType 枚举的方法
  - 移除所有字符串类型的重载方法
- **Test Requirements**:
  - `programmatic` TR-1.1: 接口编译通过
  - `human-judgement` TR-1.2: 接口定义清晰，只包含必要的方法

### [x] 任务 2: 重构 InMemoryResourceStore 实现
- **优先级**: P0
- **Depends On**: 任务 1
- **Description**:
  - 移除 InMemoryResourceStore 中的字符串重载方法
  - 修改内部存储结构，使用 CaptchaType 作为键而非字符串
  - 移除所有 toString() 调用
- **Success Criteria**:
  - InMemoryResourceStore 只实现基于 CaptchaType 枚举的方法
  - 内部存储使用 CaptchaType 作为键
  - 无 toString() 调用
- **Test Requirements**:
  - `programmatic` TR-2.1: 代码编译通过
  - `programmatic` TR-2.2: 所有现有功能正常工作
  - `human-judgement` TR-2.3: 代码结构清晰，无冗余方法

### [x] 任务 3: 重构 DefaultImageCaptchaResourceManager
- **优先级**: P1
- **Depends On**: 任务 1, 任务 2
- **Description**:
  - 修改 DefaultImageCaptchaResourceManager 中的方法，移除对字符串类型的支持
  - 确保所有调用都使用 CaptchaType 枚举
- **Success Criteria**:
  - DefaultImageCaptchaResourceManager 只使用 CaptchaType 枚举
  - 无字符串类型的方法调用
- **Test Requirements**:
  - `programmatic` TR-3.1: 代码编译通过
  - `programmatic` TR-3.2: 资源管理功能正常

### [x] 任务 4: 重构 ResourceScanner
- **优先级**: P1
- **Depends On**: 任务 1, 任务 2
- **Description**:
  - 修改 ResourceScanner 中的方法，移除对字符串类型的支持
  - 确保所有调用都使用 CaptchaType 枚举
- **Success Criteria**:
  - ResourceScanner 只使用 CaptchaType 枚举
  - 无字符串类型的方法调用
- **Test Requirements**:
  - `programmatic` TR-4.1: 代码编译通过
  - `programmatic` TR-4.2: 资源扫描功能正常

### [x] 任务 5: 重构其他相关代码
- **优先级**: P2
- **Depends On**: 任务 1, 任务 2
- **Description**:
  - 检查并修改其他使用字符串类型资源方法的代码
  - 确保所有调用都使用 CaptchaType 枚举
- **Success Criteria**:
  - 所有代码都使用 CaptchaType 枚举
  - 无字符串类型的资源方法调用
- **Test Requirements**:
  - `programmatic` TR-5.1: 整个项目编译通过
  - `programmatic` TR-5.2: 所有功能正常工作

## 实现注意事项
1. 确保所有修改都保持向后兼容性，虽然用户要求无需支持旧代码，但应确保现有功能不受影响
2. 移除字符串重载方法时，确保所有调用点都已更新
3. 使用 CaptchaType 作为键时，确保哈希和相等性比较正确
4. 测试所有功能，确保资源存储和管理功能正常工作

## 预期成果
- 更清晰的资源存储接口和实现
- 减少 toString() 调用，提高性能
- 更类型安全的资源管理系统
- 更明确的资源添加方法