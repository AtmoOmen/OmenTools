# OmenTools

OmenTools 是一套面向 [Dalamud](https://github.com/goatcorp/Dalamud) 插件开发的综合性工具库，封装大量可复用的基础设施。

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

---

## 扩展项目

### [OmenTools.KamiToolKit](https://github.com/AtmoOmen/OmenTools.KamiToolKit)

提供 KamiToolKit 绘制的原生界面、节点扩展。

## 许可证

MIT
