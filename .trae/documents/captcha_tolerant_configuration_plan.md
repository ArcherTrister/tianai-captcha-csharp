# 验证码验证精度可配置化改造计划

## 任务分解与优先级

### [ ] 任务 1: 分析当前验证精度实现
- **Priority**: P0
- **Depends On**: None
- **Description**:
  - 分析 SimpleImageCaptchaValidator 类中当前的验证精度实现
  - 了解各种验证码类型的验证精度处理逻辑
- **Success Criteria**:
  - 完全理解当前验证精度的实现逻辑
  - 识别需要修改的关键代码位置
- **Test Requirements**:
  - `programmatic` TR-1.1: 确认当前验证精度的默认值和使用方式
  - `human-judgement` TR-1.2: 确认需要修改的代码位置和逻辑

### [ ] 任务 2: 修改 SimpleImageCaptchaValidator 类
- **Priority**: P0
- **Depends On**: 任务 1
- **Description**:
  - 添加每种验证码类型的验证精度配置属性
  - 修改验证精度的获取逻辑，确保未设置时使用默认值 0.005
  - 处理为零的情况，视为未设置
- **Success Criteria**:
  - 每种验证码类型都有独立的验证精度配置属性
  - 未设置时使用默认值 0.005
  - 为零的情况视为未设置
- **Test Requirements**:
  - `programmatic` TR-2.1: 验证未设置时使用默认值 0.005
  - `programmatic` TR-2.2: 验证为零的情况视为未设置
  - `programmatic` TR-2.3: 验证设置了值时使用设置的值

### [ ] 任务 3: 修改 RotateImageCaptchaInfo 类
- **Priority**: P1
- **Depends On**: 任务 2
- **Description**:
  - 修改 RotateImageCaptchaInfo 类的默认验证精度逻辑
  - 确保未设置时使用默认值 0.005
- **Success Criteria**:
  - RotateImageCaptchaInfo 类使用新的默认验证精度 0.005
- **Test Requirements**:
  - `programmatic` TR-3.1: 验证 RotateImageCaptchaInfo 未设置时使用默认值 0.005

### [ ] 任务 4: 编写测试用例
- **Priority**: P1
- **Depends On**: 任务 2, 任务 3
- **Description**:
  - 为修改后的验证精度逻辑编写测试用例
  - 测试各种情况下的验证精度使用
- **Success Criteria**:
  - 所有测试用例通过
  - 覆盖未设置、为零、设置了值等各种情况
- **Test Requirements**:
  - `programmatic` TR-4.1: 所有测试用例通过
  - `human-judgement` TR-4.2: 测试用例覆盖所有关键场景

### [ ] 任务 5: 验证改造结果
- **Priority**: P2
- **Depends On**: 任务 4
- **Description**:
  - 运行所有测试，确保改造后的代码正常工作
  - 验证各种验证码类型的验证精度配置是否生效
- **Success Criteria**:
  - 所有测试通过
  - 验证精度配置功能正常工作
- **Test Requirements**:
  - `programmatic` TR-5.1: 所有测试通过
  - `human-judgement` TR-5.2: 验证精度配置功能正常工作

## 实现细节

### 任务 2 实现细节
1. 在 SimpleImageCaptchaValidator 类中添加以下属性：
   - `public float SliderTolerant { get; set; } = 0.003f;`（保持不变）
   - `public float RotateTolerant { get; set; } = 0.005f;`
   - `public float ConcatTolerant { get; set; } = 0.005f;`
   - `public float WordImageClickTolerant { get; set; } = 0.005f;`

2. 修改 DefaultTolerant 属性的默认值为 0.005f

3. 修改 AddPercentage 方法，确保处理为零的情况：
   ```csharp
   float tolerant = info.Tolerant.HasValue && info.Tolerant.Value > 0 ? info.Tolerant.Value : SliderTolerant;
   ```

4. 修改 AddClickCheckData 方法，添加默认验证精度逻辑

5. 修改 Valid 方法，根据验证码类型使用对应的验证精度配置

### 任务 3 实现细节
1. 修改 RotateImageCaptchaInfo 类的 DefaultTolerant 常量为 0.005f

## 测试场景

1. 测试滑块验证码：
   - 未设置 Tolerant
   - 设置 Tolerant 为 0
   - 设置 Tolerant 为非零值

2. 测试旋转验证码：
   - 未设置 Tolerant
   - 设置 Tolerant 为 0
   - 设置 Tolerant 为非零值

3. 测试文字点击验证码：
   - 未设置 Tolerant
   - 设置 Tolerant 为 0
   - 设置 Tolerant 为非零值

4. 测试拼接验证码：
   - 未设置 Tolerant
   - 设置 Tolerant 为 0
   - 设置 Tolerant 为非零值
