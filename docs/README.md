# Godot 4 系统学习手册（Unity 开发者版）

这是一份独立的 Godot 学习资料，不依赖本仓库的玩法或代码。示例使用 Godot 4 的 .NET 版本和 C#；概念同样适用于 GDScript。Godot 小版本可能调整具体 API，以文末官方文档和编辑器自动补全为准。

## 阅读路线

1. [引擎全景与 Unity 概念对照](01-引擎全景.md)：引擎、场景树、节点、资源、服务器各负责什么。
2. [主循环与生命周期](02-主循环与生命周期.md)：谁驱动游戏、输入/物理/普通帧何时执行、暂停和时间。
3. [场景、节点、资源与通信](03-场景节点资源与通信.md)：组合、实例化、信号、组、Autoload 和资源共享。
4. [输入、物理与导航](04-输入物理与导航.md)：Input Map、碰撞体、层与掩码、射线、导航。
5. [坐标、相机与视口](05-坐标相机与视口.md)：局部/世界/屏幕坐标、Camera2D/Camera3D、CanvasLayer、SubViewport。
6. [渲染、材质与 Shader](06-渲染材质与Shader.md)：从场景到屏幕、2D/3D 管线、光照、材质与 Shader 实作。
7. [动画、UI、音频与粒子](07-动画UI音频与粒子.md)：常用表现模块如何组合。
8. [数据、调试、性能与发布](08-数据调试性能与发布.md)：保存、导入、Profiler、构建和导出。
9. [Unity 迁移速查与练习](09-Unity迁移与练习.md)：对照表、常见误区、可验证的练习路线。
10. [其他系统与深入方向](10-其他系统与深入方向.md)：TileMap、网络、国际化、线程、编辑器扩展和底层接口。

## 贯穿全书的总模型

```text
操作系统启动 Godot
  └─ SceneTree 主循环
       ├─ 输入事件 → UI/游戏逻辑
       ├─ 固定频率物理更新 → 碰撞与运动
       ├─ 普通帧更新 → 动画、相机、表现
       ├─ 渲染服务器 → GPU → 画面
       └─ 音频服务器 → 声卡
```

你写的 C# 脚本不是主循环本身。它们挂在节点上，由 SceneTree 在合适的阶段回调。节点组成场景，场景可以反复实例化；资源提供贴图、音频、材质和数据。这个关系比记 API 名字重要。

## 如何使用这份手册

- 每章先理解“职责”，再运行一个最小实验；不要一次记全所有节点。
- 阅读时在编辑器中打开 Remote 场景树、Inspector、Debugger 和 Profiler，对照运行时状态。
- 代码片段是独立示意；涉及节点路径、输入动作或资源路径的地方，要先按注释在编辑器创建对应内容。
- 学完每章，尝试用自己的话解释：谁拥有数据、谁触发行为、结果在哪一步变成画面或声音。

## 官方资料

- [Godot 官方文档](https://docs.godotengine.org/en/stable/)
- [Godot C#/.NET](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/)
- [Godot 类参考](https://docs.godotengine.org/en/stable/classes/)
- [Godot 渲染器概览](https://docs.godotengine.org/en/stable/tutorials/rendering/renderers.html)

本手册优先解释 Godot 4 的稳定概念。遇到 3.x 教程时尤其要核对节点名、物理 API、Shader 内置变量和导出流程。
