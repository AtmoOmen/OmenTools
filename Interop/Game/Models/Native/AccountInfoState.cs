namespace OmenTools.Interop.Game.Models.Native;

public enum AccountInfoState
{
    Uninitialized      = -2, // 尚未构造或已释放
    ReadingCacheFile   = -1, // 正在异步读取本地缓存文件
    CacheFileInvalid   = 0,  // 缓存内容解析失败, 角色列表被清空
    UpdatedFromServer  = 1,  // 由登录响应直接写入
    CacheFileProcessed = 2   // 本地缓存文件处理结束
}
