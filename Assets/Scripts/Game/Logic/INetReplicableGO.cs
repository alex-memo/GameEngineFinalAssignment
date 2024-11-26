using UnityEngine;

namespace Game.Logic
{
    public interface INetReplicableGO
    {
        public void Set(ulong _ownerID, ISpawnable _spawnable, int _id);
    }
}
