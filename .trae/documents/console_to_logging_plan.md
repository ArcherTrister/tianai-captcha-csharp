# Tianai.Captcha - Console 打印替换为日志打印计划

## [x] 任务 1: 为 ServiceCollectionExtensions 类添加日志记录
- **优先级**: P1
- **Depends On**: None
- **Description**:
  - 为 ServiceCollectionExtensions 类添加 ILogger<ServiceCollectionExtensions> 依赖
  - 将所有 Console.WriteLine 调用替换为相应的日志方法
- **Success Criteria**:
  - ServiceCollectionExtensions 类中不再使用 Console.WriteLine
  - 所有日志消息使用适当的日志级别（如 Information、Debug、Error）
- **Test Requirements**:
  - `programmatic` TR-1.1: 编译通过，无错误
  - `human-judgement` TR-1.2: 代码可读性良好，日志级别使用合理
- **Notes**: 由于 ServiceCollectionExtensions 是静态类，需要通过参数注入日志记录器

## [x] 任务 2: 为 EmbeddedResourceProvider 类添加日志记录
- **优先级**: P1
- **Depends On**: None
- **Description**:
  - 为 EmbeddedResourceProvider 类添加 ILogger<EmbeddedResourceProvider> 依赖
  - 将所有 Console.WriteLine 调用替换为相应的日志方法
- **Success Criteria**:
  - EmbeddedResourceProvider 类中不再使用 Console.WriteLine
  - 所有日志消息使用适当的日志级别
- **Test Requirements**:
  - `programmatic` TR-2.1: 编译通过，无错误
  - `human-judgement` TR-2.2: 代码可读性良好，日志级别使用合理
- **Notes**: 需要修改构造函数以注入日志记录器

## [x] 任务 3: 为 ResourceScanner 类添加日志记录
- **优先级**: P1
- **Depends On**: None
- **Description**:
  - 为 ResourceScanner 类添加 ILogger<ResourceScanner> 依赖
  - 将所有 Console.WriteLine 调用替换为相应的日志方法
- **Success Criteria**:
  - ResourceScanner 类中不再使用 Console.WriteLine
  - 所有日志消息使用适当的日志级别
- **Test Requirements**:
  - `programmatic` TR-3.1: 编译通过，无错误
  - `human-judgement` TR-3.2: 代码可读性良好，日志级别使用合理
- **Notes**: 需要修改构造函数以注入日志记录器

## [x] 任务 4: 为 UriResourceProvider 类添加日志记录
- **优先级**: P1
- **Depends On**: None
- **Description**:
  - 为 UriResourceProvider 类添加 ILogger<UriResourceProvider> 依赖
  - 将所有 Console.WriteLine 调用替换为相应的日志方法
- **Success Criteria**:
  - UriResourceProvider 类中不再使用 Console.WriteLine
  - 所有日志消息使用适当的日志级别
- **Test Requirements**:
  - `programmatic` TR-4.1: 编译通过，无错误
  - `human-judgement` TR-4.2: 代码可读性良好，日志级别使用合理
- **Notes**: 需要修改构造函数以注入日志记录器

## [x] 任务 5: 检查并更新示例项目
- **优先级**: P2
- **Depends On**: None
- **Description**:
  - 检查 samples/ResourceViewer/Program.cs 中的 Console.WriteLine 调用
  - 根据需要替换为日志记录
- **Success Criteria**:
  - 示例项目中的 Console.WriteLine 调用已适当处理
- **Test Requirements**:
  - `programmatic` TR-5.1: 编译通过，无错误
  - `human-judgement` TR-5.2: 代码可读性良好
- **Notes**: 示例项目可能保留 Console.WriteLine 用于控制台输出，但应确保日志记录也能正常工作

## [x] 任务 6: 运行测试以确保功能正常
- **优先级**: P1
- **Depends On**: 任务 1-5
- **Description**:
  - 运行项目的测试套件
  - 确保所有测试通过
- **Success Criteria**:
  - 所有测试通过
  - 没有因日志记录变更导致的功能问题
- **Test Requirements**:
  - `programmatic` TR-6.1: 所有测试通过
  - `human-judgement` TR-6.2: 测试输出中没有异常或错误

## 实现策略
1. 为每个需要日志记录的类添加 ILogger<T> 依赖
2. 使用依赖注入的方式获取日志记录器
3. 根据消息的性质选择适当的日志级别：
   - 信息性消息使用 Information 级别
   - 调试信息使用 Debug 级别
   - 错误信息使用 Error 级别
4. 保持日志消息的清晰性和一致性
5. 确保所有修改后的代码编译通过且测试通过