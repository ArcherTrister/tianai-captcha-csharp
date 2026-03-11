# Tianai.Captcha - 资源提供者升级计划

## 项目现状分析

当前项目的资源管理体系包括：

* **资源管理器**：`DefaultImageCaptchaResourceManager`

* **资源存储**：`InMemoryResourceStore`

* **资源提供者**：

  * `EmbeddedResourceProvider`：程序集嵌入资源提供者

  * `FileResourceProvider`：文件资源提供者

  * `UriResourceProvider`：URL 资源提供者

## 升级目标

1. **程序集提供者升级**：注册程序集后自动扫描添加背景和模板资源
2. **文件提供者升级**：给定目录后自动添加目录下的文件
3. **保持现有路径格式**：遵循项目中规划的路径格式

## 详细任务分解

### [ ] 任务 1：升级 EmbeddedResourceProvider
- **Priority**：P0
- **Depends On**：None
- **Description**：
  - 修改 `EmbeddedResourceProvider`，添加自动扫描功能
  - 当注册程序集时，自动扫描并添加背景图片、模板资源和字体资源
  - 支持扫描 Resources/BgImages/All/ 下的背景图片（All 表示对所有验证码类型都可用）
  - 支持扫描 Resources/BgImages/{Type}/ 下的背景图片（特定类型专用）
  - 支持扫描 Resources/Templates/All/ 下的模板资源（All 表示对所有验证码类型都可用）
  - 支持扫描 Resources/Templates/{Type}/ 下的模板资源（特定类型专用）
  - 支持扫描 Resources/Fonts/ 下的字体资源
- **Success Criteria**：
  - 注册程序集后，资源管理器中自动添加该程序集的背景、模板和字体资源
- **Test Requirements**：
  - `programmatic` TR-1.1：注册包含资源的程序集后，能随机获取到该程序集的背景图片
  - `programmatic` TR-1.2：注册包含资源的程序集后，能随机获取到该程序集的模板资源
  - `programmatic` TR-1.3：注册包含资源的程序集后，能随机获取到该程序集的字体资源

### [ ] 任务 2：升级 FileResourceProvider
- **Priority**：P0
- **Depends On**：None
- **Description**：
  - 修改 `FileResourceProvider`，添加目录扫描功能
  - 添加方法接收目录路径，自动扫描并添加目录下的文件
  - 支持扫描指定目录下的背景图片、模板资源和字体资源
  - 支持扫描 Resources/BgImages/All/ 下的背景图片（All 表示对所有验证码类型都可用）
  - 支持扫描 Resources/BgImages/{Type}/ 下的背景图片（特定类型专用）
  - 支持扫描 Resources/Templates/All/ 下的模板资源（All 表示对所有验证码类型都可用）
  - 支持扫描 Resources/Templates/{Type}/ 下的模板资源（特定类型专用）
  - 支持扫描 Resources/Fonts/ 下的字体资源
  - 遵循项目中规划的路径格式
- **Success Criteria**：
  - 调用目录扫描方法后，资源管理器中自动添加该目录下的背景、模板和字体资源
- **Test Requirements**：
  - `programmatic` TR-2.1：扫描包含背景图片的目录后，能随机获取到该目录的背景图片
  - `programmatic` TR-2.2：扫描包含模板的目录后，能随机获取到该目录的模板资源
  - `programmatic` TR-2.3：扫描包含字体资源的目录后，能随机获取到该目录的字体资源

### \[ ] 任务 3：修改资源管理器接口

* **Priority**：P1

* **Depends On**：任务 1, 任务 2

* **Description**：

  * 在 `IImageCaptchaResourceManager` 中添加资源扫描相关方法

  * 确保资源提供者的扫描功能能与资源管理器无缝集成

* **Success Criteria**：

  * 资源管理器能通过统一接口触发资源扫描

* **Test Requirements**：

  * `programmatic` TR-3.1：通过资源管理器接口能触发程序集资源扫描

  * `programmatic` TR-3.2：通过资源管理器接口能触发文件目录资源扫描

### \[ ] 任务 4：添加目录扫描工具类

* **Priority**：P1

* **Depends On**：None

* **Description**：

  * 创建 `ResourceScanner` 工具类，提供统一的资源扫描功能

  * 支持扫描程序集和文件系统中的资源

  * 遵循项目中规划的路径格式

* **Success Criteria**：

  * 工具类能正确识别和扫描背景图片和模板资源

* **Test Requirements**：

  * `programmatic` TR-4.1：能正确扫描程序集中的资源

  * `programmatic` TR-4.2：能正确扫描文件系统中的资源

### \[ ] 任务 5：编写测试用例

* **Priority**：P2

* **Depends On**：任务 1, 任务 2, 任务 3, 任务 4

* **Description**：

  * 为升级后的功能编写单元测试

  * 测试程序集资源扫描功能

  * 测试文件目录资源扫描功能

  * 测试资源管理器的集成功能

* **Success Criteria**：

  * 所有测试用例通过

* **Test Requirements**：

  * `programmatic` TR-5.1：所有测试用例执行通过

### [ ] 任务 6：更新文档和示例
- **Priority**：P2
- **Depends On**：任务 1, 任务 2, 任务 3, 任务 4, 任务 5
- **Description**：
  - 更新项目文档，说明新的资源扫描功能
  - 更新示例项目，展示如何使用新功能
- **Success Criteria**：
  - 文档和示例能清晰展示新功能的使用方法
- **Test Requirements**：
  - `human-judgement` TR-6.1：文档内容完整且清晰
  - `human-judgement` TR-6.2：示例项目能正常运行

### [ ] 任务 7：升级 UriResourceProvider
- **Priority**：P1
- **Depends On**：任务 3
- **Description**：
  - 升级 `UriResourceProvider`，添加批量 URL 资源加载功能
  - 添加 URL 资源缓存机制，减少网络请求
  - 支持异步加载 URL 资源，提高性能
  - 增强错误处理机制，处理网络异常
  - 添加资源验证功能，验证 URL 资源的有效性和格式
  - 支持 HTTP 认证和超时配置
- **Success Criteria**：
  - URL 资源提供者能批量加载资源
  - URL 资源能被缓存以减少网络请求
  - 能处理网络异常和无效 URL
- **Test Requirements**：
  - `programmatic` TR-7.1：能从配置列表中批量加载 URL 资源
  - `programmatic` TR-7.2：重复加载相同 URL 资源时使用缓存
  - `programmatic` TR-7.3：能正确处理网络异常

### [ ] 任务 8：添加资源缓存机制
- **Priority**：P1
- **Depends On**：任务 1, 任务 2
- **Description**：
  - 在资源存储中添加缓存机制，避免重复扫描
  - 缓存扫描结果，提高资源访问性能
  - 提供缓存清理和更新机制
- **Success Criteria**：
  - 资源扫描结果能被缓存
  - 重复访问相同资源时使用缓存
  - 能手动清理和更新缓存
- **Test Requirements**：
  - `programmatic` TR-8.1：重复扫描相同资源时使用缓存
  - `programmatic` TR-8.2：缓存能被正确清理和更新

### [ ] 任务 9：添加资源验证功能
- **Priority**：P1
- **Depends On**：任务 1, 任务 2
- **Description**：
  - 添加资源验证功能，验证扫描到的资源是否有效
  - 验证图片资源的格式和大小
  - 验证字体资源的有效性
- **Success Criteria**：
  - 无效资源能被检测并过滤
  - 资源验证不影响正常资源的加载
- **Test Requirements**：
  - `programmatic` TR-9.1：能检测并过滤无效的图片资源
  - `programmatic` TR-9.2：能检测并过滤无效的字体资源

### [ ] 任务 10：支持资源过滤
- **Priority**：P2
- **Depends On**：任务 1, 任务 2
- **Description**：
  - 添加资源过滤功能，根据配置过滤不需要的资源
  - 支持按文件类型、大小、名称等过滤
  - 提供过滤规则配置接口
- **Success Criteria**：
  - 能根据配置过滤不需要的资源
  - 过滤规则能正确应用
- **Test Requirements**：
  - `programmatic` TR-10.1：能根据文件类型过滤资源
  - `programmatic` TR-10.2：能根据大小过滤资源

## 路径格式规范

遵循项目中现有的路径格式：
- **背景图片**：
  - `Resources/BgImages/All/`：所有验证码类型都可用的背景图片
  - `Resources/BgImages/{Type}/`：特定验证码类型专用的背景图片（如 `Resources/BgImages/Slider/`）
- **模板资源**：
  - `Resources/Templates/All/`：所有验证码类型都可用的模板资源
  - `Resources/Templates/{Type}/`：特定验证码类型专用的模板资源（如 `Resources/Templates/Slider/`）
- **字体资源**：`Resources/Fonts/`

## 预期效果

升级完成后，用户可以：

1. 注册程序集后自动获取其中的资源
2. 指定目录后自动获取其中的资源
3. 无需手动添加每个资源文件
4. 保持与现有代码的兼容性

