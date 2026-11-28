# OmenTools

OmenTools 是一套面向 [Dalamud](https://github.com/goatcorp/Dalamud) 插件开发的综合性工具库，封装大量可复用的基础设施。

> [!NOTE]
> 需要同时引用 [FFXIVClientStructs.Expansion](https://github.com/AtmoOmen/FFXIVClientStructs.Expansion) 项目并置于同一级才可以正确加载。

> [!WARNING]
> 本项目的根本性质是个人开发工具箱，服务于个人开发需求。
> 本人不会乱删能力，但会快速迭代任何我认为可以修改、优化或重构的能力，其中可能会移除过时的能力、重命名、调整命名空间或重新组织结构。
> 若无法接受，不建议引用本项目，而是自由挑选其中你真的需要的能力合并到你自己的项目中。

---

## 快速开始

### 1. 初始化与卸载

在插件入口类中通过 `DService` 完成一键初始化和资源回收。

```csharp
public class MyPlugin : IAsyncDalamudPlugin
{
    private IDalamudPluginInterface PI { get; init; }

    public MyPlugin(IDalamudPluginInterface pluginInterface)
    {
        // 获取 IDalamudPluginInterface
        PI = pluginInterface;
    }

    public async Task LoadAsync
    (
        CancellationToken cancellationToken
    )
    {
        // 基础初始化
        await DService.InitAsync(pluginInterface, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DService.UninitAsync().ConfigureAwait(false);
    }
}
```

### 2. 添加全局引用

将 `Global/GlobalUsing.OmenTools.cs` 复制到项目中，可省去大量重复引用。

---

## 配套项目

### [AddonToolkit](https://github.com/AtmoOmen/AddonToolkit)

高效、安全、现代化且完善的游戏原生界面组件的 C# 封装库，用于制作各种原生界面与组件。

---

## 许可证

MIT
