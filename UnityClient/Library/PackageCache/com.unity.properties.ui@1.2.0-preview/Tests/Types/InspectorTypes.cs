using UnityEngine;

namespace Unity.Properties.UI.Tests
{
    public class DrawerAttribute : UnityEngine.PropertyAttribute {}
    
    public interface IUserInspectorTag {}
    public interface IAnotherUserInspectorTag {}
    
    public class NoInspectorType {}
    
    public class SingleInspectorType {}
    public class SingleInspectorTypeInspector : Inspector<SingleInspectorType> {}
    
    public class MultipleInspectorsType {}
    public class MultipleInspectorsTypeInspector : Inspector<MultipleInspectorsType> {}
    public class MultipleInspectorsTypeInspectorWithTag : Inspector<MultipleInspectorsType>, IUserInspectorTag {}
    

    public class NoInspectorButDrawerType {}
    public class NoInspectorButDrawerTypeDrawer : PropertyDrawer<NoInspectorButDrawerType, DrawerAttribute> {}
    
    public class InspectorAndDrawerType {}
    public class InspectorAndDrawerTypeInspector : Inspector<InspectorAndDrawerType> {}
    
    public class InspectorAndDrawerTypeTypeDrawer : PropertyDrawer<InspectorAndDrawerType, DrawerAttribute>, IAnotherUserInspectorTag {}
    
    public class InspectorAndDrawerTypeTypeDrawerWithTag : PropertyDrawer<InspectorAndDrawerType, DrawerAttribute>, IUserInspectorTag {}
}