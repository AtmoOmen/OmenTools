using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.System.Memory;

namespace OmenTools.Extensions;

public static unsafe class MemorySpaceExtension
{
    extension(scoped ref IMemorySpace memorySpace)
    {
        /// <summary>
        ///     在当前内存空间中分配并构造原生对象，检查构造结果是否有效。
        /// </summary>
        /// <typeparam name="T">实现原生构造接口的非托管类型。</typeparam>
        /// <returns>已构造的原生对象地址。</returns>
        /// <exception cref="OutOfMemoryException">分配失败。</exception>
        public T* CreateChecked<T>() where T : unmanaged, ICreatable<T>
        {
            var memory = memorySpace.Create<T>();

            return memory is null ?
                       throw new OutOfMemoryException($"无法为 {typeof(T)} 分配内存。") :
                       memory;
        }

        /// <summary>
        ///     在当前内存空间中按指定对齐方式分配内存，并检查分配结果。
        /// </summary>
        /// <param name="size">要分配的字节数。</param>
        /// <param name="alignment">内存对齐字节数。</param>
        /// <returns>分配的内存地址。</returns>
        /// <exception cref="OutOfMemoryException">非零大小的分配失败。</exception>
        public void* MallocChecked(ulong size, ulong alignment = 8)
        {
            var memory = memorySpace.Malloc(size, alignment);

            if (memory is null &&
                size is not 0)
                throw new OutOfMemoryException($"无法分配 {size} 字节内存。");

            return memory;
        }

        /// <summary>
        ///     在当前内存空间中分配一个结构，并将全部字节清零。
        /// </summary>
        /// <typeparam name="T">要分配的非托管类型。</typeparam>
        /// <returns>已清零的结构地址。</returns>
        /// <exception cref="OutOfMemoryException">分配失败。</exception>
        public T* MallocZeroed<T>() where T : unmanaged
        {
            var memoryPointer = memorySpace.Malloc<T>();

            if (memoryPointer is null)
                throw new OutOfMemoryException($"无法为 {typeof(T)} 分配内存。");

            NativeMemory.Clear(memoryPointer, (nuint)sizeof(T));

            return memoryPointer;
        }

        /// <summary>
        ///     在当前内存空间中分配一组结构，并将全部字节清零。
        /// </summary>
        /// <typeparam name="T">要分配的非托管类型。</typeparam>
        /// <param name="count">要分配的元素数量。</param>
        /// <returns>已清零的数组地址；数量为零时返回 <see langword="null" />。</returns>
        /// <exception cref="ArgumentOutOfRangeException">数量为负数。</exception>
        /// <exception cref="OutOfMemoryException">分配失败。</exception>
        public T* AllocateZeroedArray<T>
        (
            int count
        ) where T : unmanaged
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count is 0) return null;

            var blockSize     = checked((nuint)sizeof(T) * (nuint)count);
            var memoryPointer = (T*)memorySpace.Malloc(blockSize, 8);

            if (memoryPointer is null)
                throw new OutOfMemoryException($"无法为 {count} 个 {typeof(T)} 分配内存。");

            NativeMemory.Clear(memoryPointer, blockSize);

            return memoryPointer;
        }

        /// <summary>
        ///     在当前内存空间中分配一组结构，并将全部字节清零。
        /// </summary>
        /// <typeparam name="T">要分配的非托管类型。</typeparam>
        /// <param name="count">要分配的元素数量。</param>
        /// <returns>已清零的数组地址；数量为零时返回 <see langword="null" />。</returns>
        public T* AllocateZeroedArray<T>
        (
            uint count
        ) where T : unmanaged
            => memorySpace.AllocateZeroedArray<T>(checked((int)count));

        /// <summary>
        ///     在当前内存空间中重新分配一组结构。
        /// </summary>
        /// <typeparam name="T">要重新分配的非托管类型。</typeparam>
        /// <param name="memory">原有内存地址。</param>
        /// <param name="newCount">新的元素数量。</param>
        /// <returns>重新分配后的数组地址。</returns>
        /// <exception cref="ArgumentOutOfRangeException">数量为负数。</exception>
        /// <exception cref="OutOfMemoryException">分配失败且数量不为零。</exception>
        public T* Reallocate<T>
        (
            void* memory,
            int   newCount
        ) where T : unmanaged
        {
            ArgumentOutOfRangeException.ThrowIfNegative(newCount);
            return memorySpace.Reallocate<T>(memory, (uint)newCount);
        }

        /// <summary>
        ///     在当前内存空间中重新分配一组结构。
        /// </summary>
        /// <typeparam name="T">要重新分配的非托管类型。</typeparam>
        /// <param name="memory">原有内存地址。</param>
        /// <param name="newCount">新的元素数量。</param>
        /// <returns>重新分配后的数组地址。</returns>
        /// <exception cref="OutOfMemoryException">分配失败且数量不为零。</exception>
        public T* Reallocate<T>
        (
            void* memory,
            uint  newCount
        ) where T : unmanaged
        {
            var blockSize     = checked((ulong)sizeof(T) * newCount);
            var memoryPointer = (T*)memorySpace.AlignedRealloc(memory, blockSize, 16);

            if (memoryPointer is null && newCount is not 0)
                throw new OutOfMemoryException($"无法为 {newCount} 个 {typeof(T)} 重新分配内存。");

            return memoryPointer;
        }

        /// <summary>
        ///     复制一组连续结构。
        /// </summary>
        /// <typeparam name="T">要复制的非托管类型。</typeparam>
        /// <param name="oldBuffer">源地址。</param>
        /// <param name="newBuffer">目标地址。</param>
        /// <param name="count">要复制的元素数量。</param>
        /// <exception cref="ArgumentOutOfRangeException">数量为负数。</exception>
        public static void Copy<T>
        (
            T*  oldBuffer,
            T*  newBuffer,
            int count
        ) where T : unmanaged
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            IMemorySpace.Copy(oldBuffer, newBuffer, (uint)count);
        }

        /// <summary>
        ///     复制一组连续结构。
        /// </summary>
        /// <typeparam name="T">要复制的非托管类型。</typeparam>
        /// <param name="oldBuffer">源地址。</param>
        /// <param name="newBuffer">目标地址。</param>
        /// <param name="count">要复制的元素数量。</param>
        public static void Copy<T>
        (
            T*   oldBuffer,
            T*   newBuffer,
            uint count
        ) where T : unmanaged
            => NativeMemory.Copy(oldBuffer, newBuffer, checked((nuint)sizeof(T) * count));
    }
}
