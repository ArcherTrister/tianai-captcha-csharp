# 调试日志打印功能实现计划

## 项目概述
为 Tianai.Captcha 项目的关键节点添加调试状态下的日志打印功能，包括资源加载、验证码生成、验证过程等重要环节。

## 实现目标
- 在关键节点添加详细的调试日志
- 确保日志记录不会影响生产环境性能
- 提供清晰的日志层次结构，便于调试和问题定位

## 详细任务计划

### [ ] 任务 1: 为资源管理相关类添加日志
- **Priority**: P1
- **Depends On**: None
- **Description**:
  - 为 DefaultImageCaptchaResourceManager 添加资源管理相关日志
  - 为 ResourceScanner 添加资源扫描和加载日志
  - 为 ResourceValidator 添加资源验证日志
- **Success Criteria**:
  - 资源加载过程有详细的调试日志
  - 资源扫描过程有详细的调试日志
  - 资源验证过程有详细的调试日志
- **Test Requirements**:
  - `programmatic` TR-1.1: 资源扫描时能看到详细的调试日志
  - `programmatic` TR-1.2: 资源加载失败时能看到错误日志
  - `human-judgement` TR-1.3: 日志内容清晰易懂，包含必要的上下文信息

### [ ] 任务 2: 为验证码生成相关类添加日志
- **Priority**: P1
- **Depends On**: Task 1
- **Description**:
  - 为 DefaultImageCaptchaApplication 添加验证码生成和验证日志
  - 为 AbstractImageCaptchaGenerator 及其实现类添加生成过程日志
  - 为 ImageCaptchaUtils 添加图片处理日志
- **Success Criteria**:
  - 验证码生成过程有详细的调试日志
  - 验证码验证过程有详细的调试日志
  - 图片处理过程有详细的调试日志
- **Test Requirements**:
  - `programmatic` TR-2.1: 验证码生成时能看到详细的调试日志
  - `programmatic` TR-2.2: 验证码验证时能看到详细的调试日志
  - `human-judgement` TR-2.3: 日志内容清晰易懂，包含必要的上下文信息

### [ ] 任务 3: 为缓存和预生成池相关类添加日志
- **Priority**: P2
- **Depends On**: Task 2
- **Description**:
  - 为 ICacheStore 实现类添加缓存操作日志
  - 为 CaptchaPregenerationService 添加预生成服务日志
  - 为 ICaptchaPregenerationPool 实现类添加预生成池操作日志
- **Success Criteria**:
  - 缓存操作过程有详细的调试日志
  - 预生成服务过程有详细的调试日志
  - 预生成池操作过程有详细的调试日志
- **Test Requirements**:
  - `programmatic` TR-3.1: 缓存操作时能看到详细的调试日志
  - `programmatic` TR-3.2: 预生成服务运行时能看到详细的调试日志
  - `human-judgement` TR-3.3: 日志内容清晰易懂，包含必要的上下文信息

### [ ] 任务 4: 为资源提供者相关类添加日志
- **Priority**: P2
- **Depends On**: Task 1
- **Description**:
  - 为 EmbeddedResourceProvider 添加嵌入式资源提供日志
  - 为 FileResourceProvider 添加文件资源提供日志
  - 为 UriResourceProvider 添加 URI 资源提供日志
- **Success Criteria**:
  - 嵌入式资源提供过程有详细的调试日志
  - 文件资源提供过程有详细的调试日志
  - URI 资源提供过程有详细的调试日志
- **Test Requirements**:
  - `programmatic` TR-4.1: 资源提供时能看到详细的调试日志
  - `programmatic` TR-4.2: 资源提供失败时能看到错误日志
  - `human-judgement` TR-4.3: 日志内容清晰易懂，包含必要的上下文信息

### [ ] 任务 5: 验证日志功能
- **Priority**: P1
- **Depends On**: Task 1, Task 2, Task 3, Task 4
- **Description**:
  - 运行项目并验证所有关键节点的日志打印功能
  - 确保日志在调试模式下正常输出
  - 确保日志在生产模式下不会影响性能
- **Success Criteria**:
  - 所有关键节点的日志都能正常输出
  - 日志内容清晰易懂，包含必要的上下文信息
  - 日志不会影响生产环境性能
- **Test Requirements**:
  - `programmatic` TR-5.1: 运行项目时能看到所有关键节点的调试日志
  - `programmatic` TR-5.2: 生产模式下日志不会过度输出
  - `human-judgement` TR-5.3: 日志内容清晰易懂，便于调试和问题定位

## 实现策略
1. 使用现有的 Microsoft.Extensions.Logging 库为实例类添加日志
2. 对于静态类等无法使用 ILogger 的情况，使用 Debug.WriteLine 等方式代替
3. 为每个类添加适当的日志记录器
4. 在关键方法中添加详细的调试日志
5. 使用不同的日志级别：Debug、Information、Warning、Error
6. 确保日志内容包含必要的上下文信息，如资源路径、验证码类型、操作结果等

## 注意事项
1. 确保日志记录不会影响生产环境性能
2. 确保日志内容清晰易懂，便于调试和问题定位
3. 确保日志不会包含敏感信息
4. 确保日志记录符合项目的代码风格和规范