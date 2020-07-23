using System;
using UnityEngine;

namespace Unity.QuickSearch
{
    [Flags]
    enum AssetModification
    {
        Updated = 1,
        Removed = 1 << 1,
        Moved = 1 << 2
    }

    struct Transaction
    {
        public long timestamp;
        public Hash128 guid;
        public int state;

        public Transaction(string guid, int state)
        {
            timestamp = DateTime.Now.ToBinary();
            this.guid = Hash128.Parse(guid);
            this.state = state;
        }

        public Transaction(string guid, AssetModification state)
            : this(guid, (int)state)
        {}

        public override string ToString()
        {
            return $"[{DateTime.FromBinary(timestamp):u}] ({state}) {guid}";
        }

        public AssetModification GetState()
        {
            return (AssetModification)state;
        }
    }
}
