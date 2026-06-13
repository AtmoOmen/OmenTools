using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace OmenTools.Extensions;

public static unsafe class CharacterManagerExtension
{
    extension(scoped ref CharacterManager manager)
    {
        /// <summary>
        /// 按谓词查找首个匹配的战斗单位。
        /// 函数指针版本 — 零分配，需传入 static 方法指针。
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
    }
}
