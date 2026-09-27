# 06｜渲染、材质与 Shader

## 1. 渲染系统的输入与输出

输入是场景中的可绘制对象、变换、网格/贴图、材质、灯光、相机、Viewport；输出是 Viewport 的颜色图像及相关缓冲。脚本通常更新输入状态，引擎负责收集、排序、裁剪、提交绘制命令。GPU 负责大量并行顶点和像素运算。

```text
C# 改变状态
  → SceneTree/渲染服务器收集对象
  → 相机和 Viewport 确定可见空间
  → 排序/裁剪/准备绘制调用
  → GPU 顶点处理 → 图元组装与裁剪 → 光栅化
  → 片元处理 → 深度/模板测试与混合
  → 后处理/UI 合成 → 显示
```

这是概念模型。具体通道与顺序取决于 2D/3D、透明度、光照、渲染器和效果；不要把它理解成所有对象都经过完全相同的一串函数。

## 2. 一张 Sprite 怎样变成像素

`Sprite2D` 可以理解为一个有纹理坐标的矩形，通常拆成两个三角形。矩形顶点先经变换得到屏幕位置。光栅化确定每个三角形覆盖哪些像素；片元阶段根据 UV 采样贴图并算出颜色；透明混合把前景颜色与已有画面合成。一个 Sprite 的 Shader 只处理其几何覆盖的区域，因此要画“超出轮廓的描边”时往往需要扩大绘制区域或使用额外通道。

对于 2D，遮挡次序一般更依赖 CanvasLayer、`z_index`、树顺序与 Y Sort，而不是把所有 Sprite 当成 3D 模型使用深度缓冲。透明像素和混合方式也会影响结果。

## 3. 3D 渲染多了什么

3D 网格经过 Model → View → Projection 变换。相机视锥外的对象可被剔除；三角形光栅化后，深度测试用于决定哪个表面在前面。材质提供基础颜色、金属度、粗糙度、法线等参数；灯光和环境让表面产生明暗。透明物体通常需要另外排序和混合，因此比不透明物体更容易有排序或性能问题。

PBR（基于物理的渲染）是材质与光照的一套近似模型，不等于“必须写 Shader”。使用 `StandardMaterial3D` 已能获得大多数常规效果。2D 的 `CanvasItemMaterial`、`Light2D`、法线贴图也可提供基础光照。[Godot 渲染架构](https://docs.godotengine.org/en/stable/engine_details/architecture/internal_rendering_architecture.html)

## 4. Shader、Material、Texture、Uniform

| 名称 | 作用 |
| --- | --- |
| Texture | 图片数据；颜色、法线、噪声等都可以是纹理 |
| Shader | 定义顶点、片元或光照计算的程序 |
| Material | 选择 Shader 并保存供它使用的参数 |
| Uniform | 由 CPU/材质传入 Shader 的参数，通常在同一次绘制中保持不变 |
| Varying | 从顶点阶段传给片元阶段、在三角形内插值的数据 |

Shader 是运行在 GPU 管线中的程序，C# 是 CPU 侧的游戏逻辑。两者通过材质参数、纹理等数据连接。Shader 不应承担生命值、AI 或存档规则。

Godot 主要 Shader 类型：`canvas_item` 用于 2D/Control，`spatial` 用于 3D，另有粒子、天空等专门类型。`vertex()` 处理顶点；`fragment()` 处理图元覆盖的片元；`light()` 允许自定义光照。你只需实现需要改写的部分，引擎提供默认行为。[Shader 参考](https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/)

## 5. 从零构建一个 2D Shader

目标：精灵受伤时逐渐闪红。

1. 场景中创建 `Sprite2D`，指定贴图。
2. Inspector 的 Material 新建 `ShaderMaterial`。
3. 在该材质的 Shader 新建 Shader，类型为 CanvasItem。
4. 写入以下代码，保存为 `.gdshader`；在材质 Inspector 可看到参数。
5. C# 在受伤时修改 `hit_amount`，使它从 1 回落到 0。

```glsl
shader_type canvas_item;

uniform vec4 hit_color : source_color = vec4(1.0, 0.2, 0.2, 1.0);
uniform float hit_amount : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    // Godot 4 的 canvas_item 在此处已把默认贴图颜色计入 COLOR。
    vec4 base = COLOR;
    COLOR = vec4(mix(base.rgb, hit_color.rgb, hit_amount), base.a);
}
```

```csharp
using Godot;

public partial class HitFlash : Sprite2D
{
    private ShaderMaterial _material = default!;

    public override void _Ready()
    {
        // 若多个精灵共用一份材质，直接改参数会影响所有实例。
        _material = (ShaderMaterial)Material.Duplicate();
        Material = _material;
    }

    public void Flash()
    {
        _material.SetShaderParameter("hit_amount", 1.0f);
        CreateTween().TweenMethod(
            Callable.From<float>(v => _material.SetShaderParameter("hit_amount", v)),
            1.0f, 0.0f, 0.2f);
    }
}
```

此示例要求 Material 已在 Inspector 指向 `ShaderMaterial`。若在 2D 中还要保留 `Modulate` 的影响，使用片元入口的 `COLOR` 再加工是合适的；直接 `COLOR = texture(TEXTURE, UV)` 会跳过部分默认调色。Godot 4 的 `canvas_item` 光照与 Godot 3 不同，不要照搬旧教程。[CanvasItem Shader 内置变量](https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/canvas_item_shader.html)

## 6. 一个最小 3D Shader

```glsl
shader_type spatial;
render_mode unshaded;

uniform vec4 tint : source_color = vec4(0.2, 0.7, 1.0, 1.0);

void fragment() {
    ALBEDO = tint.rgb;
}
```

`unshaded` 让材质不参与通常的光照计算，适合先验证 Shader 是否生效。去掉它、再试 `StandardMaterial3D` 的金属度和粗糙度，可以逐步理解材质与光照。不要把 2D 的 `COLOR` 和 3D 的 `ALBEDO` 混用。[Spatial Shader 参考](https://docs.godotengine.org/en/stable/tutorials/shaders/shader_reference/spatial_shader.html)

## 7. 渲染器与图形 API

Godot 4 的常见渲染器是 Forward+、Mobile、Compatibility。它们决定可用的图形特性和性能取舍；RenderingDevice、Vulkan、Direct3D 12、Metal、OpenGL 是更底层的后端/驱动概念。**项目选 Forward+ 不等于项目是 3D，也不等于开发者需要写底层 GPU 命令。** 先按目标平台和实际效果选择，后期用目标设备实测。[渲染器概览](https://docs.godotengine.org/en/stable/tutorials/rendering/renderers.html)

## 8. 性能从哪里花掉

常见成本包括：CPU 侧大量节点/脚本处理、频繁绘制调用和状态切换、过大的透明区域与粒子、昂贵的全屏 Shader、多盏动态灯光与阴影、高分辨率 Viewport、多相机重复绘制。不要只看 FPS 猜原因；用 Profiler、帧时间和目标硬件验证。

**自测**：想让敌人走到屏幕右边，应改 Shader 还是世界位置？想让敌人受伤闪红，应改哪一个参数？给精灵加 Shader 后为什么它不能自动画出超出自身矩形的大片光晕？
