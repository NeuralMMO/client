using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    public class MMObjectPool : MonoBehaviour
    {
        [MMReadOnly]
        public List<GameObject> PooledGameObjects;
    }
}
