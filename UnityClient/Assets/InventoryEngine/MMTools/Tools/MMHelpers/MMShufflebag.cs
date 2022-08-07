using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class to use to get more controlled randomness, taking values out of the bag randomly, and never getting them again.
    /// 
    /// Usage :
    /// 
    /// var shuffleBag = new ShuffleBag(40);
    /// for (int i = 0; i<40; i++)
    /// {
    ///     newValue = something;
    ///     shuffleBag.Add(newValue, amount);
    /// }
    /// 
    /// then :
    /// float something = shuffleBag.Pick();
    /// 
    /// </summary>
    public class MMShufflebag : MonoBehaviour
    {
        public int Capacity { get { return _contents.Capacity; } }
        public int Size { get { return _contents.Count; } }

        protected List<float> _contents;
        protected float _currentItem;
        protected int _currentIndex = -1;

        /// <summary>
        /// Initializes the shufflebag
        /// </summary>
        /// <param name="initialCapacity"></param>
        public MMShufflebag(int initialCapacity)
        {
            _contents = new List<float>(initialCapacity);
        }

        /// <summary>
        /// Adds the specified quantity of the item to the bag
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        public void Add(float item, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                _contents.Add(item);
            }
            _currentIndex = Size - 1;
        }

        /// <summary>
        /// Returns a random item from the bag
        /// </summary>
        /// <returns></returns>
        public float Pick()
        {
            if (_currentIndex < 1)
            {
                _currentIndex = Size - 1;
                _currentItem = _contents[0];
                return _currentItem;
            }

            int position = UnityEngine.Random.Range(0, _currentIndex);

            _currentItem = _contents[position];
            _contents[position] = _contents[_currentIndex];
            _contents[_currentIndex] = _currentItem;
            _currentIndex--;

            return _currentItem;
        }
    }
}
