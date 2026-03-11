# 验证码预生成池实现计划

## 项目背景
当前验证码系统采用按需生成模式，每次请求时实时生成验证码，可能导致响应延迟。为了提高性能和响应速度，需要引入验证码预生成池的概念，提前生成并缓存验证码，当池水位低于阈值时自动补充。

## 实现目标
1. 引入验证码预生成池
2. 实现定时检查池水位机制
3. 当水位低于阈值时批量生成补充
4. 保持与现有API的兼容性

## 详细任务分解

### [ ] 任务1: 设计验证码预生成池接口和数据结构
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - 设计 `ICaptchaPregenerationPool` 接口
  - 设计预生成池的数据结构，包含验证码ID、类型、生成时间等信息
  - 定义池的容量、阈值等配置参数
- **Success Criteria**:
  - 接口设计合理，包含必要的方法
  - 数据结构能有效存储预生成的验证码
- **Test Requirements**:
  - `programmatic` TR-1.1: 接口定义完整，包含添加、获取、检查水位等方法
  - `human-judgement` TR-1.2: 接口设计符合面向对象原则，命名规范

### [ ] 任务2: 实现验证码预生成池的具体实现
- **Priority**: P0
- **Depends On**: 任务1
- **Description**:
  - 实现 `MemoryCaptchaPregenerationPool` 类
  - 实现线程安全的池操作
  - 实现验证码的添加、获取、过期处理等功能
- **Success Criteria**:
  - 池操作线程安全
  - 能正确管理预生成的验证码
  - 能处理验证码过期情况
- **Test Requirements**:
  - `programmatic` TR-2.1: 多线程环境下池操作正常
  - `programmatic` TR-2.2: 过期验证码能被正确清理
  - `programmatic` TR-2.3: 池水位计算准确

### [ ] 任务3: 实现定时检查和批量生成机制
- **Priority**: P0
- **Depends On**: 任务2
- **Description**:
  - 实现 `CaptchaPregenerationService` 服务
  - 使用后台定时任务检查池水位
  - 当水位低于阈值时，批量生成验证码补充
- **Success Criteria**:
  - 定时检查机制正常工作
  - 批量生成逻辑正确
  - 能根据配置参数动态调整生成策略
- **Test Requirements**:
  - `programmatic` TR-3.1: 定时任务能按配置执行
  - `programmatic` TR-3.2: 当水位低于阈值时能触发批量生成
  - `programmatic` TR-3.3: 批量生成的验证码数量符合预期

### [ ] 任务4: 集成预生成池到现有验证码应用
- **Priority**: P1
- **Depends On**: 任务3
- **Description**:
  - 修改 `DefaultImageCaptchaApplication`，优先从预生成池获取验证码
  - 当预生成池为空时，回退到实时生成
  - 保持现有API接口不变
- **Success Criteria**:
  - 能从预生成池获取验证码
  - 池为空时能正常回退到实时生成
  - 现有API调用不受影响
- **Test Requirements**:
  - `programmatic` TR-4.1: 调用 `GenerateCaptcha` 能优先从预生成池获取
  - `programmatic` TR-4.2: 池为空时能正常生成新验证码
  - `programmatic` TR-4.3: 所有现有API测试通过

### [ ] 任务5: 添加配置选项和依赖注入支持
- **Priority**: P1
- **Depends On**: 任务4
- **Description**:
  - 在 `ImageCaptchaOptions` 中添加预生成池相关配置
  - 实现依赖注入配置，支持预生成池服务的注册
  - 提供配置示例和文档
- **Success Criteria**:
  - 配置选项能正确加载
  - 依赖注入能正常工作
  - 配置文档完整
- **Test Requirements**:
  - `programmatic` TR-5.1: 配置选项能正确读取和应用
  - `programmatic` TR-5.2: 依赖注入注册成功
  - `human-judgement` TR-5.3: 配置文档清晰易懂

### [ ] 任务6: 编写单元测试和集成测试
- **Priority**: P2
- **Depends On**: 任务5
- **Description**:
  - 编写预生成池的单元测试
  - 编写定时检查服务的测试
  - 编写集成测试，验证完整流程
- **Success Criteria**:
  - 测试覆盖率达到80%以上
  - 所有测试通过
  - 测试用例覆盖主要场景
- **Test Requirements**:
  - `programmatic` TR-6.1: 单元测试覆盖率达到80%以上
  - `programmatic` TR-6.2: 集成测试通过
  - `programmatic` TR-6.3: 性能测试显示响应速度提升

### [ ] 任务7: 性能优化和监控
- **Priority**: P2
- **Depends On**: 任务6
- **Description**:
  - 优化预生成池的内存使用
  - 监控预生成池的状态和性能
  - 提供性能指标和监控数据
- **Success Criteria**:
  - 内存使用合理
  - 性能监控正常
  - 能提供有意义的性能指标
- **Test Requirements**:
  - `programmatic` TR-7.1: 内存使用在合理范围内
  - `programmatic` TR-7.2: 性能监控数据准确
  - `human-judgement` TR-7.3: 监控指标易于理解和使用

## 技术方案

### 预生成池设计
- **数据结构**: 使用线程安全的队列存储预生成的验证码
- **过期处理**: 定期清理过期的验证码
- **水位管理**: 维护当前池容量、最低阈值等指标

### 定时检查机制
- **实现方式**: 使用 `IHostedService` 实现后台定时任务
- **检查频率**: 可配置，默认每30秒检查一次
- **批量生成**: 当水位低于阈值时，批量生成到目标容量

### 集成方案
- **透明集成**: 保持现有API不变，内部优先使用预生成池
- **回退机制**: 当预生成池为空时，回退到实时生成
- **配置灵活**: 支持启用/禁用预生成池，以及调整相关参数

## 预期效果
1. 验证码生成响应速度显著提升
2. 系统负载更加均衡
3. 高峰期性能更加稳定
4. 与现有系统无缝集成

## 风险评估
- **内存使用**: 预生成池会占用额外内存，需要合理配置容量
- **过期管理**: 需要确保过期验证码能及时清理，避免内存泄漏
- **线程安全**: 多线程环境下需要确保池操作的线程安全性
- **配置合理性**: 需要根据实际负载调整预生成池的大小和阈值

## 实施计划
1. 完成接口和数据结构设计 (任务1)
2. 实现预生成池核心功能 (任务2)
3. 实现定时检查和批量生成 (任务3)
4. 集成到现有应用 (任务4)
5. 添加配置和依赖注入支持 (任务5)
6. 编写测试和验证 (任务6)
7. 性能优化和监控 (任务7)

预计总工作量: 约2-3个工作日