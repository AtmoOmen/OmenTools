# OmenTools

OmenTools 是一套面向 [Dalamud](https://github.com/goatcorp/Dalamud) 插件开发的综合性工具库，以**服务化架构**、**异步任务编排**和**游戏底层交互**三大能力为核心，将插件开发中常见的样板代码、生命周期管理和事件处理封装为可复用的基础设施。

---

## 快速开始

### 1. 初始化与卸载

在插件入口类中通过 `DService` 完成一键初始化和资源回收：

```csharp
public class MyPlugin : IDalamudPlugin
{
    public MyPlugin(IDalamudPluginInterface pluginInterface)
    {
        // 基础初始化
        DService.Init(pluginInterface);

        // 可选：禁用不需要的服务
        // DService.Init(pluginInterface, () => new DServiceInitOptions()
        //     .Disable<GamePacketManager>()
        //     .Disable<TooltipManager>());
    }

    public void Dispose()
    {
        DService.Uninit(); // 自动释放所有追踪资源
    }
}
```

`DService` 会自动完成以下工作：
- 通过 Dalamud 的依赖注入获取全部原生服务
- 自动扫描并实例化所有继承自 `OmenServiceBase` 的自定义服务
- 注册服务生命周期（按发现顺序 `Init`，按逆序 `Uninit`）
- 追踪所有 `TaskHelper`、`MemoryPatch` 和 `IDalamudHook`，在卸载时统一释放

### 2. 引入全局 Using（强烈推荐）

将 `Global/GlobalUsing.OmenTools.cs` 复制到项目中，可省去大量重复引用：

```csharp
// 自动引入 OmenTools 核心命名空间
// 自动引入 Dalamud 的 ImGui / 接口服务
// 自动引入自定义类型别名 (IObjectTable, IPlayerCharacter 等)
// 自动引入 Globals 静态工具方法
```

---

## 核心模块

### DService — 服务容器与生命周期中枢

`DService` 是整个工具库的根容器，以单例模式持有所有服务实例。

| 能力 | 说明 |
|------|------|
| `DService.Init()` | 初始化容器、注入 Dalamud 服务、实例化并启动所有 OmenService |
| `DService.Uninit()` | 逆序卸载服务、释放追踪资源 |
| `DService.Instance().GetOmenService<T>()` | 获取指定 OmenService 实例 |
| `DServiceInitOptions` | 通过 `Disable<T>()` 在初始化时排除特定服务 |

### OmenService — 插件服务基类

所有内部服务均继承自 `OmenServiceBase<T>`，具备以下能力：

- **单例访问**：`SomeService.Instance()` 全局获取
- **生命周期钩子**：`Init()` / `Uninit()` 自动调用
- **配置持久化**：内置 `LoadConfig<T>()` / `SaveConfig<T>()`，自动存取 `PluginConfigDirectory/OmenTools/Service/{ServiceName}.json`

自定义服务示例：

```csharp
public class MyFeatureService : OmenServiceBase<MyFeatureService>
{
    protected override void Init()
    {
        // 注册 Framework 回调、Hook、事件等
        FrameworkManager.Instance().Reg(OnUpdate, throttleMS: 16);
    }

    protected override void Uninit()
    {
        FrameworkManager.Instance().Unreg(OnUpdate);
    }

    private void OnUpdate(IFramework framework)
    {
        // 业务逻辑
    }
}
```

### TaskHelper — 异步任务队列

`TaskHelper` 是一个基于 `System.Threading.Channels` 的异步任务调度器，专为游戏插件场景设计：所有任务均在 **Framework 更新线程** 上执行，避免跨线程操作游戏状态。

```csharp
var taskHelper = new TaskHelper();

// 同步任务：返回 true 表示完成，false 表示下一帧继续
helper.Enqueue(() => {
    if (!IsConditionMet()) return false;
    DoSomething();
    return true;
}, name: "等待条件并执行", timeoutMS: 5000);

// 异步任务
helper.EnqueueAsync(async ct => {
    await Task.Delay(1000, ct);
    return true;
}, name: "延迟 1 秒");

// 优先级队列（weight 越大优先级越高）
helper.Enqueue(UrgentTask, weight: 10);
helper.Enqueue(NormalTask, weight: 0);

// 延迟插入
helper.DelayNext(500, "等待半秒");

// 全局控制
helper.Abort();                          // 放弃所有任务
helper.TimeoutMS = 10000;                // 默认超时
helper.TimeoutBehaviour = TaskAbortBehaviour.AbortAll; // 超时/异常时放弃全部
```

特性一览：
- **双队列架构**：高优先级队列 (`weight > 0`) 优先消费，同优先级按 FIFO
- **精细超时控制**：支持全局默认值 + 单任务覆盖
- **异常恢复策略**：`AbortAll`（放弃全部）或 `AbortCurrent`（仅跳过当前）
- **取消语义**：异步任务接收 `CancellationToken`，Dispose 时自动传播取消

### Managers — 游戏事件包装层

OmenTools 在 Dalamud 与 FFXIVClientStruct 提供的底层事件之上提供了二次封装，让事件的订阅和拦截更加便捷：

| 管理器 | 核心能力 |
|--------|---------|
| `FrameworkManager` | 带节流控制的 `IFramework.Update` 回调注册，精确到 Stopwatch Tick |
| `GamePacketManager` | 游戏网络封包 **PreSend / PostSend / PreReceive / PostReceive** 四阶段拦截；内置已知上行封包解析日志 |
| `UseActionManager` | 技能使用事件（前/后拦截），支持阻止发送 |
| `ChatManager` | 聊天消息收发监听与拦截 |
| `CommandManager` | 斜杠指令注册管理 |
| `TooltipManager` | 物品/技能 Tooltip 动态修改 |
| `WindowManager` | ImGui 窗口生命周期统一管理 |
| `FontManager` | 自定义字体加载与全局缩放 |
| `AtkEventManager` | UI (Addon) 事件监听与响应 |
| `DataShareManager` | 插件间数据共享（基于特性标记） |
| `IPCManager` | 跨插件 IPC 自动注册与调用 |

使用示例：

```csharp
// 封包拦截
GamePacketManager.Instance().RegPreSendPacket((ref bool prevented, int opcode, ref nint packet, ref bool prio) => {
    if (opcode == 0x123) prevented = true; // 阻止该封包
});

// 带节流的帧更新
FrameworkManager.Instance().Reg(MyUpdateLoop, throttleMS: 33); // ~30 FPS
```

### ImGuiOm — ImGui 控件与组件库

封装 Dear ImGui 的常用模式，减少样板代码：

```csharp
// 渐变动画颜色
ImGuiOm.GetGradientColor();

// 带图标的玩家信息渲染
ImGuiOm.RenderPlayerInfo("Player Name", "Server");

// 发光矩形（用于高亮按钮等）
ImGuiOm.AddGlowRect(drawList, min, max, colorU32, rounding, glowSize, steps);
```

**预制组件**：
- 选择器下拉框：`ActionSelectCombo`、`ItemSelectCombo`、`JobSelectCombo`、`ZoneSelectCombo`、`StatusSelectCombo` 等
- 地图渲染器：`ImGuiMapRenderer` + `ImGuiMapMarker`
- Markdown 渲染器：`ImGuiMarkdownRenderer`
- 日期选择器：`DatePicker`

### Extensions — 扩展方法集

涵盖 Dalamud 接口、`System` 类型、游戏对象、UI 节点等数十个扩展方法：

| 类别 | 典型扩展 |
|------|---------|
| `GameObjectExtension` | `TargetInteract()`, `IsReachable()`, `IsMTQ()`, `FindNearest()` |
| `AtkUnitBaseExtension` | UI 节点查找、点击、文本读取 |
| `VectorExtension` | `ToPlayerHeight()`, 各种坐标转换 |
| `SeStringExtension` | 游戏字符串解析与构造 |
| `LuminaSheetExtension` | Excel  sheet 查询简化 |
| `EnumExtension` | 枚举描述、遍历辅助 |

### Interop — 底层交互

| 模块 | 说明 |
|------|------|
| `MemoryPatch` | 基于签名或地址的内存补丁，支持 `??` / `**` 通配符，自动备份与恢复 |
| `ExecuteCommand` | 封装 100+ 游戏内 `/command` 的 `ExecuteCommand` 调用（传送、情感动作、副本、钓鱼等） |
| `AddonEvent` / `AgentEvent` | UI 与 Agent 事件的类型安全抽象基类 |
| `MovementInputController` | 移动输入模拟与控制 |
| `Windows Helpers` | 键鼠模拟、输入法控制、系统托盘通知 |

### Info — 数据与模型

- **封包定义**：上游（Upstream）与下游（Downstream）封包的强类型结构
- **游戏常量**：副本类型、图标 ID、坐标数据、物品来源查询模型
- **Lumina 扩展 Sheet**：游戏数据表中未直接暴露的额外 Sheet
- **JSON 转换器**：`Vector2/3/4`、`DateTime`、`TimeSpan`、`Version` 等常用类型的 Newtonsoft.Json 转换器
- **算法与集合**：`AhoCorasick` 多模式匹配、`LRUCache`

---

## 辅助工具

### Throttler — 高精度节流器

基于 `Stopwatch.GetTimestamp()` 实现，适用于高频事件去重：

```csharp
var throttler = new Throttler<string>();

if (throttler.Throttle("DoSomething", 500)) {
    // 首次进入或 500ms 后再次进入
}

if (throttler.Check("DoSomething")) {
    // 检查是否已过节流期
}
```

### DLog — 日志包装

对 `IPluginLog` 的轻量封装，统一日志输出风格：

```csharp
DLog.Debug("调试信息");
DLog.Error("发生错误", exception);
```

### Localization — 多格式本地化

支持 `properties`、`JSON`、`RESX` 等多种本地化文件格式，自动热重载。

---

## 许可证

MIT
