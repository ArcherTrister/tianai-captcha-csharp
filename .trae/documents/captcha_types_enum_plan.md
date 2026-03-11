# Tianai.Captcha - CaptchaTypes 改为枚举类型的实现计划

## [ ] 任务 1: 创建 CaptchaType 枚举类型
- **优先级**: P0
- **依赖**: 无
- **描述**:
  - 在 Constants.cs 文件中，将 `CaptchaTypes` 静态类替换为 `CaptchaType` 枚举
  - 为枚举添加与原常量相同的值
  - 实现枚举到字符串的转换方法
- **成功标准**:
  - 枚举类型创建成功，包含所有原有的验证码类型
  - 提供了枚举到字符串的转换方法
- **测试要求**:
  - `programmatic` TR-1.1: 枚举类型能正确编译
  - `programmatic` TR-1.2: 枚举到字符串的转换能正确返回与原常量相同的值
- **备注**:
  - 保留原有的字符串值，确保兼容性

## [ ] 任务 2: 更新 ResourceScanner.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 ResourceScanner.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - ResourceScanner.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-2.1: ResourceScanner.cs 文件能正确编译
  - `programmatic` TR-2.2: 资源扫描功能正常工作
- **备注**:
  - 注意处理字符串比较的地方，确保使用正确的转换方法

## [ ] 任务 3: 更新 Program.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 Program.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - Program.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-3.1: Program.cs 文件能正确编译
  - `programmatic` TR-3.2: 示例 Web API 能正常启动
- **备注**:
  - 注意处理数组和字典使用的地方

## [ ] 任务 4: 更新 ServiceCollectionExtensions.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 ServiceCollectionExtensions.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - ServiceCollectionExtensions.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-4.1: ServiceCollectionExtensions.cs 文件能正确编译
- **备注**:
  - 注意处理方法调用的地方

## [ ] 任务 5: 更新测试文件中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新测试文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - 所有测试文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-5.1: 所有测试文件能正确编译
  - `programmatic` TR-5.2: 所有测试能正常通过
- **备注**:
  - 注意处理测试数据和断言的地方

## [ ] 任务 6: 更新 DefaultImageCaptchaApplication.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 DefaultImageCaptchaApplication.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - DefaultImageCaptchaApplication.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-6.1: DefaultImageCaptchaApplication.cs 文件能正确编译
- **备注**:
  - 注意处理方法调用和默认值的地方

## [ ] 任务 7: 更新 SimpleImageCaptchaValidator.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 SimpleImageCaptchaValidator.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - SimpleImageCaptchaValidator.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-7.1: SimpleImageCaptchaValidator.cs 文件能正确编译
- **备注**:
  - 注意处理默认值和验证逻辑的地方

## [ ] 任务 8: 更新 MultiImageCaptchaGenerator.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 MultiImageCaptchaGenerator.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - MultiImageCaptchaGenerator.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-8.1: MultiImageCaptchaGenerator.cs 文件能正确编译
- **备注**:
  - 注意处理默认值和构造函数的地方

## [ ] 任务 9: 更新 GenerateParam.cs 和 ImageCaptchaInfo.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 GenerateParam.cs 和 ImageCaptchaInfo.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - 两个文件都能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-9.1: 两个文件都能正确编译
- **备注**:
  - 注意处理属性默认值和 getter/setter 的地方

## [ ] 任务 10: 更新 CaptchaTypeClassifier.cs 中的引用
- **优先级**: P0
- **依赖**: 任务 1
- **描述**:
  - 更新 CaptchaTypeClassifier.cs 文件中对 `CaptchaTypes` 的引用
  - 将字符串常量替换为枚举值，并使用转换方法获取字符串
- **成功标准**:
  - CaptchaTypeClassifier.cs 文件能正确编译
  - 所有对 `CaptchaTypes` 的引用都已更新为使用新的枚举类型
- **测试要求**:
  - `programmatic` TR-10.1: CaptchaTypeClassifier.cs 文件能正确编译
- **备注**:
  - 注意处理类型判断逻辑的地方

## [ ] 任务 11: 运行完整测试套件
- **优先级**: P1
- **依赖**: 所有前面的任务
- **描述**:
  - 运行项目的完整测试套件，确保所有测试都能通过
- **成功标准**:
  - 所有测试都能通过
- **测试要求**:
  - `programmatic` TR-11.1: 所有测试通过
- **备注**:
  - 确保没有遗漏任何引用更新

## [ ] 任务 12: 清理和验证
- **优先级**: P1
- **依赖**: 任务 11
- **描述**:
  - 清理不再使用的代码
  - 验证所有更改是否符合预期
- **成功标准**:
  - 代码干净，没有未使用的引用
  - 所有功能正常工作
- **测试要求**:
  - `programmatic` TR-12.1: 代码编译成功
  - `human-judgment` TR-12.2: 代码结构清晰，符合最佳实践
- **备注**:
  - 确保没有引入任何新的问题
