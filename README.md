# OmenTools

个人自用工具库，破坏性变更随时可能不期而至。

## 主要功能

本工具库旨在简化 Dalamud 插件开发流程，主要包含以下功能模块：

*   **DService 服务管家**：提供统一的依赖注入与服务生命周期管理，集成常用的 Dalamud 服务与自定义管理器。
*   **TaskHelper 任务队列**：强大的异步任务管理系统，支持多步操作、超时控制、异常处理及任务撤销。
*   **大量游戏事件包装管理 (Managers)**：涵盖技能使用、状态变更、数据包等大量包装，并提供可订阅、修改的事件。
*   **UI 组件库 (ImGuiOm)**：提供基于 ImGui 的封装控件，包括自定义按钮、带图标的文本、渐变色处理及玩家信息渲染等。
*   **实用工具集 (Helpers)**：
    *   **MemoryPatch**：简易的内存补丁管理。
    *   **Throttler**：操作节流控制，防止高频触发。
    *   **Lumina 封装**：简化游戏数据表的读取与查询。
*   **丰富的扩展方法 (Extensions)**：涵盖 `Dalamud` 接口、`System` 类型及游戏对象的各类便捷扩展。

## 初始化与卸载

本项目通过 `DService` 类进行统一的生命周期管理。请在插件入口类中按照以下方式调用：

1.  **初始化**：在插件构造函数中，传入 `IDalamudPluginInterface` 实例并调用 `DService.Init` 方法。
2.  **卸载**：在插件的 `Dispose` 方法中，调用 `DService.Uninit` 方法以释放资源。

```csharp
public class MyPlugin : IDalamudPlugin
{
    public MyPlugin(IDalamudPluginInterface pluginInterface)
    {
        // 初始化 OmenTools
        DService.Init(pluginInterface);
        
        // 其他初始化代码...
    }

    public void Dispose()
    {
        // 卸载 OmenTools，释放资源
        DService.Uninit();
        
        // 其他清理代码...
    }
}
```

## 引用建议

建议将本项目中的 `GlobalUsing.OmenTools.cs` 文件复制到目标项目中。该文件包含了常用的全局引用配置，能够简化代码编写，方便直接调用各类工具方法与扩展函数。
