using System.Runtime.CompilerServices;
using Dalamud.Utility;
using Lumina.Text;
using Lumina.Text.ReadOnly;

namespace OmenTools.Extensions;

public static class SeStringBuilderExtension
{
    extension(SeStringBuilder)
    {
        /// <summary>
        ///     从共享对象池租借一个 <see cref="SeStringBuilder" /> 实例，并返回用于自动归还的释放句柄。
        /// </summary>
        /// <param name="builder">租借得到的原生 <see cref="SeStringBuilder" /> 实例。</param>
        /// <returns>释放时自动将实例归还至共享对象池的句柄。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RentedSeStringBuilder Rent(out SeStringBuilder builder)
        {
            var rented = new RentedSeStringBuilder();
            builder = rented.Builder;

            return rented;
        }

        /// <summary>
        ///     从共享对象池租借 <see cref="SeStringBuilder" /> 执行构建并生成 <see cref="ReadOnlySeString" />，结束后自动归还。
        /// </summary>
        /// <param name="action">使用租借实例进行内容构建的委托。</param>
        /// <returns>构建完成的 <see cref="ReadOnlySeString" />。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySeString Build(Action<SeStringBuilder> action)
        {
            using var rented = new RentedSeStringBuilder();
            action(rented.Builder);

            return rented.Builder.ToReadOnlySeString();
        }

        /// <summary>
        ///     携带状态参数从共享对象池租借 <see cref="SeStringBuilder" /> 执行构建并生成 <see cref="ReadOnlySeString" />，结束后自动归还。
        /// </summary>
        /// <typeparam name="TState">传入构建委托的状态类型。</typeparam>
        /// <param name="state">传递给构建委托的状态参数，用于避免闭包分配。</param>
        /// <param name="action">使用租借实例与状态参数进行内容构建的委托。</param>
        /// <returns>构建完成的 <see cref="ReadOnlySeString" />。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySeString Build<TState>(TState state, Action<SeStringBuilder, TState> action)
        {
            using var rented = new RentedSeStringBuilder();
            action(rented.Builder, state);

            return rented.Builder.ToReadOnlySeString();
        }

        /// <summary>
        ///     从共享对象池租借 <see cref="SeStringBuilder" /> 执行自定义构建并返回结果，结束后自动归还。
        /// </summary>
        /// <typeparam name="TResult">构建结果类型。</typeparam>
        /// <param name="func">使用租借实例计算结果的委托。</param>
        /// <returns>委托返回的结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult Use<TResult>(Func<SeStringBuilder, TResult> func)
        {
            using var rented = new RentedSeStringBuilder();

            return func(rented.Builder);
        }

        /// <summary>
        ///     携带状态参数从共享对象池租借 <see cref="SeStringBuilder" /> 执行自定义构建并返回结果，结束后自动归还。
        /// </summary>
        /// <typeparam name="TState">传入构建委托的状态类型。</typeparam>
        /// <typeparam name="TResult">构建结果类型。</typeparam>
        /// <param name="state">传递给构建委托的状态参数，用于避免闭包分配。</param>
        /// <param name="func">使用租借实例与状态参数计算结果的委托。</param>
        /// <returns>委托返回的结果。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult Use<TState, TResult>(TState state, Func<SeStringBuilder, TState, TResult> func)
        {
            using var rented = new RentedSeStringBuilder();

            return func(rented.Builder, state);
        }
    }
}
