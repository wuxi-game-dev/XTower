# 09｜Unity 迁移速查与练习

先完成[00｜从双击到第一帧](00-从双击到第一帧.md)的观察实验，再用本章与 Unity 经验对照。否则很容易只把 API 名称换掉，却没看懂 Godot 的窗口、SceneTree、Viewport 与渲染服务之间的关系。

## 1. 从 Unity 转 Godot 的关键改念

Unity 更容易被理解成“GameObject 挂多个 Component”；Godot 则倾向“特定类型的节点构成树，再把树保存为场景”。`Sprite2D`、`CollisionShape2D`、`Camera2D` 常是不同节点。脚本通常继承节点类型并挂在对应节点上。

| 以前会问 | 在 Godot 中先问 |
| --- | --- |
| “该写在哪个 MonoBehaviour？” | “哪个节点拥有这项行为与生命周期？” |
| “要不要做一个 Prefab？” | “这棵节点树是否值得保存为独立 Scene？” |
| “这个配置要不要 ScriptableObject？” | “是否需要一个可共享、可保存的 Resource？” |
| “在哪里 FindObjectOfType？” | “固定引用、组或信号是否更合适？” |
| “RenderTexture 怎样接？” | “是否需要 SubViewport 和 ViewportTexture？” |
| “Layer 怎么设置？” | “这是物理层、渲染可见层还是 CanvasLayer？” |

## 2. 常用 API 速查

| 目的 | Godot C# |
| --- | --- |
| 普通帧更新 | `public override void _Process(double delta)` |
| 固定物理更新 | `public override void _PhysicsProcess(double delta)` |
| 取子节点 | `GetNode<T>("Path")` |
| 实例化场景 | `packedScene.Instantiate<T>()` 后 `AddChild()` |
| 延迟释放节点 | `QueueFree()` |
| 获取主场景树 | `GetTree()` |
| 输入动作 | `Input.IsActionPressed("action")` |
| 2D 世界位置 | `GlobalPosition` |
| 订阅信号 | `button.Pressed += Handler` |
| 改 Shader 参数 | `shaderMaterial.SetShaderParameter("name", value)` |
| 调试输出 | `GD.Print(...)`、`GD.PushError(...)` |

以上是入口而非一对一替代。Godot 的暂停、输入传播、资源共享和生命周期与 Unity 的细节不同，不能机械迁移旧代码。

### Godot C# 的额外习惯

- 使用 **Godot .NET 版编辑器**和匹配的 .NET SDK。写完新的节点类、`[GlobalClass]` 或 `[Signal]` 后，通常需要先构建，编辑器才识别新类型与信号。
- GDScript 文档使用 `snake_case`，C# API 通常使用 `PascalCase`：`get_tree()` 对应 `GetTree()`，`queue_free()` 对应 `QueueFree()`。但 Input Map 中的动作名、资源路径与节点名仍是项目里的字符串。
- `[Export]` 把支持的类型暴露到 Inspector；其值跟随场景/资源保存，适合设计时配置。不是任意 .NET 对象都能直接导出。可用 `Resource`、Godot 类型或普通类型中的受支持成员建模。[C# 导出属性](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_exports.html)
- Godot 对象有引擎侧生命周期。`QueueFree()` 后，不应仅凭 C# 引用非空就认定节点还可用；必要时检查 `GodotObject.IsInstanceValid()`。普通 .NET 对象与 Godot 节点的所有权规则不同。
- `GD.Print()`、信号、场景树操作都涉及 Godot 运行环境。纯计算代码可保留为普通 C# 类，方便独立测试；对 Node 的集成还需在引擎里验证。

## 3. 最容易踩的十个坑

1. **忘记把节点加入 SceneTree**：实例化对象还不等于已运行。
2. **把所有东西写到一个 Autoload**：全局依赖会越来越难检查。
3. **在 `_Process` 中做碰撞运动**：角色运动与物理查询应按物理步组织。
4. **混用 `Position` 和 `GlobalPosition`**：父节点变换后误差暴露。
5. **用屏幕鼠标位置当世界坐标**：相机滚动或缩放后生成位置错误。
6. **给所有实例改同一份材质/资源**：共享资源造成联动。
7. **让 Sprite 负责碰撞**：图片可见不等于有 CollisionShape。
8. **让 UI 每帧扫描树**：用信号推送状态变化更清楚。
9. **只在编辑器内测试**：导出后路径、资源和窗口行为可能不同。
10. **照搬 Godot 3 教程**：4.x 的物理、Shader、TileMap、导出等 API 已变化。

## 4. 建议的动手课程：四个小实验

### 实验 A：验证主循环与场景

先做第 00 章的 `BootProbe`；随后给根场景加 `Timer`。运行时观察 Remote 场景树，让 Timer 每秒生成一个相同场景实例，确认“模板”和“运行实例”的区别。

完成标准：你能说出生命周期顺序，知道为什么 `_Process` 和 `_PhysicsProcess` 次数可能不同。

### 实验 B：输入、物理与相机

按第 04 章建一个有碰撞形状的 `CharacterBody2D`，再建地面 `StaticBody2D`。用 Input Map 绑定移动与跳跃，添加 `Camera2D`，最后用 `CanvasLayer` 放一个固定在屏幕左上角的速度标签。

完成标准：角色与地面碰撞；相机跟随时 HUD 不移动；你能解释 `GlobalPosition` 与屏幕位置的区别。

### 实验 C：渲染与 Shader

给 `Sprite2D` 做第 06 章的受伤闪红 Shader。先手动改 Inspector 参数，再从 C# 改。放两个相同精灵，对比共用材质与复制材质时的行为。试着给背景加一个 `PointLight2D` 或在 3D 场景给网格加 `StandardMaterial3D`。

完成标准：能区分 CPU 逻辑、材质参数与 GPU Shader；能说明相机和 Shader 各改变什么。

### 实验 D：一次完整交互

创建按钮 → 点击后生成目标 → 玩家碰到目标时扣血 → 更新 HUD → 播放音效和动画 → 生命归零时显示结束界面 → 保存最高分到 `user://`。最后导出到目标桌面平台运行。

完成标准：整条链路在导出版本也成立，而不只是编译通过。

## 5. 你应能回答的架构问题

- “可复用配置”与“每个实例当前状态”各放哪里？
- 玩家按键如何变成位移，再怎样变成屏幕像素？
- UI 点击为什么有时会阻止世界点击？
- 角色死亡后，是谁负责音效、动画、奖励和节点移除？
- 两个 Viewport 各自渲染相同场景，会有什么成本？
- 某个效果掉帧，你如何区分脚本、物理与 GPU 的瓶颈？

能够亲手完成四个实验并回答这些问题，就掌握了 Godot 常用系统的骨架。之后学习复杂状态机、程序化关卡、网络、底层 RenderingDevice 或自定义引擎模块，会有清晰的落点。

## 6. 继续查资料的路径

优先查 [官方 Step by step](https://docs.godotengine.org/en/stable/getting_started/step_by_step/)、[Your first 2D game](https://docs.godotengine.org/en/stable/getting_started/first_2d_game/)、[C#/.NET](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/) 和 [类参考](https://docs.godotengine.org/en/stable/classes/)。看视频教程时确认它用的是 Godot 4 而非 3，并对照当前版本文档。
