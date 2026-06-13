using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace OmenTools.Extensions;

public static unsafe class EventObjectManagerExtension
{
    extension(scoped ref EventObjectManager manager)
    {
        /// <summary>
        ///     需传入 static 方法指针
        /// </summary>
        public GameObject* FindFirst(delegate* managed<GameObject*, bool> predicate)
        {
            fixed (EventObjectManager* ptr = &manager)
            {
                if (ptr == null) return null;

                foreach (var p in ptr->EventObjects)
                {
                    if (p.Value == null) continue;
                    if (predicate(p.Value))
                        return p.Value;
                }
            }

            return null;
        }

        public GameObject* FindFirst(Func<nint, bool> predicate)
        {
            fixed (EventObjectManager* ptr = &manager)
            {
                if (ptr == null) return null;

                foreach (var p in ptr->EventObjects)
                {
                    if (p.Value == null) continue;
                    if (predicate((nint)p.Value))
                        return p.Value;
                }
            }

            return null;
        }
    }
}
