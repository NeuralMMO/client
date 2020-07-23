using System.Linq;
using UnityEngine;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Tag a list field or property to add pagination.
    /// </summary>
    public class PaginationAttribute : InspectorAttribute
    {
        /// <summary>
        /// Returns the pagination sizes that should be displayed.
        /// </summary>
        public readonly int[] Sizes;
        
        /// <summary>
        /// Indicates if the pagination should be hidden when the list contains less elements than the minimal pagination
        /// size.
        /// </summary>
        public bool AutoHide = true;
        
        /// <summary>
        /// Constructs a new instance of <see cref="PaginationAttribute"/> with the specified pagination sizes.
        /// </summary>
        /// <param name="sizes">The number of elements to be displayed when using pagination.</param>
        public PaginationAttribute(params int[] sizes)
        {
            Sizes = sizes.Where(size => size > 0).OrderBy(i => i).ToArray();
            
            // Dig in to find invalid pagination sizes 
            if (Sizes.Length != sizes.Length)
            {
                Debug.LogWarning($"{nameof(PaginationAttribute)}: Invalid pagination sizes [{string.Join(", ", sizes.Except(Sizes))}]. Pagination sizes need to be greater than 0.");
            }
            
            if (Sizes.Length == 0)
            {
                Sizes = new[] {5, 10, 20};
            }
        }
    }
}