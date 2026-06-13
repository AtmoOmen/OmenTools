using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace OmenTools.Extensions;

public static unsafe class CharacterManagerExtension
{
    extension(scoped ref CharacterManager manager)
    {
        /// <summary>
        ///     需传入 static 方法指针
        /// </summary>
        public BattleChara* FindFirst(delegate* managed<BattleChara*, bool> predicate)
        {
            fixed (CharacterManager* ptr = &manager)
            {
                if (ptr == null) return null;

                foreach (var p in ptr->BattleCharas)
                {
                    if (p.Value == null) continue;
                    if (predicate(p.Value))
                        return p.Value;
                }
            }

            return null;
        }

        public BattleChara* FindFirst(Func<nint, bool> predicate)
        {
            fixed (CharacterManager* ptr = &manager)
            {
                if (ptr == null) return null;

                foreach (var p in ptr->BattleCharas)
                {
                    if (p.Value == null) continue;
                    if (predicate((nint)p.Value))
                        return p.Value;
                }
            }

            return null;
        }

        /// <summary>
        ///     需传入 static 方法指针, 结果写入调用方提供的列表, 避免分配
        /// </summary>
        public void FindAll(delegate* managed<BattleChara*, bool> predicate, List<nint> results)
        {
            results.Clear();

            fixed (CharacterManager* ptr = &manager)
            {
                if (ptr == null) return;

                foreach (var p in ptr->BattleCharas)
                {
                    if (p.Value == null) continue;
                    if (predicate(p.Value))
                        results.Add((nint)p.Value);
                }
            }
        }

        public void FindAll(Func<nint, bool> predicate, List<nint> results)
        {
            results.Clear();

            fixed (CharacterManager* ptr = &manager)
            {
                if (ptr == null) return;

                foreach (var p in ptr->BattleCharas)
                {
                    if (p.Value == null) continue;
                    if (predicate((nint)p.Value))
                        results.Add((nint)p.Value);
                }
            }
        }

        /// <summary>
        ///     需传入 static 方法指针
        /// </summary>
        public List<nint> FindAll(delegate* managed<BattleChara*, bool> predicate)
        {
            var results = new List<nint>();

            fixed (CharacterManager* ptr = &manager)
            {
                if (ptr == null) return results;

                foreach (var p in ptr->BattleCharas)
                {
                    if (p.Value == null) continue;
                    if (predicate(p.Value))
                        results.Add((nint)p.Value);
                }
            }

            return results;
        }

        public List<nint> FindAll(Func<nint, bool> predicate)
        {
            var results = new List<nint>();

            fixed (CharacterManager* ptr = &manager)
            {
                if (ptr == null) return results;

                foreach (var p in ptr->BattleCharas)
                {
                    if (p.Value == null) continue;
                    if (predicate((nint)p.Value))
                        results.Add((nint)p.Value);
                }
            }

            return results;
        }
    }
}
