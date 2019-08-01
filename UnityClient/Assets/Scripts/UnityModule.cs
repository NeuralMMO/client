using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoBehaviorExtension {
    public class UnityModule: MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        // Unpack data from nested dict
        protected static object Unpack(string key, object val){
            Dictionary<string, object> value = val as Dictionary<string, object>;
            return value[key];
        }
      
        protected static object UnpackList(List<string> keys, object value){
            foreach (string key in keys) {
                value = Unpack(key, value);
            }
            return value;
        }

        // For use with base object
        protected Transform Get(string name){
            return this.transform.Find(name);
        }

        protected GameObject GetObject(string name){
            return this.Get(name).gameObject;
        }

        // For use with provided object
        protected static Transform Get(GameObject obj, string name){
            return obj.transform.Find(name);
        }

        protected static GameObject GetObject(GameObject obj, string name){
            return Get(obj, name).gameObject;
        }

        // For use with provided transform
        protected static Transform Get(Transform transform, string name){
            return transform.Find(name);
        }

        protected static GameObject GetObject(Transform transform, string name){
            return Get(transform, name).gameObject;
        }
    }
}