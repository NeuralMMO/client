using System.Linq;
using JetBrains.Annotations;
using Unity.Properties.UI.Internal;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [UI]
    class CustomInspectorDatabaseTests
    {
        public abstract class BaseType
        {
            public const string Label = "find-my-label";
        }
        
        public class NoInspectorDerivedType : BaseType
        {
        }    

        public class HasInspectorDerivedType : BaseType
        {
        }    
        
        [UsedImplicitly]
        public class BaseTypeInspector : Inspector<BaseType>
        {
            public override VisualElement Build()
            {
                var label = new Label(nameof(BaseType));
                label.AddToClassList(BaseType.Label);
                return label;
            }
        }
        
        [UsedImplicitly]
        public class DerivedType2Inspector : Inspector<HasInspectorDerivedType>
        {
            public override VisualElement Build()
            {
                var label = new Label(nameof(HasInspectorDerivedType));
                label.AddToClassList(BaseType.Label);
                return label;
            }
        }

        public class ASD
        {
#pragma warning disable 649
            public BaseType Type = new NoInspectorDerivedType();
#pragma warning restore 649
        }
        
        [Test]
        public void TypeWithCustomInspector_WhenNoSpecializedInspectorExists_UsesDeclaredTypeInspector()
        {
            var element = new PropertyElement();
            var instance = new ASD();
            instance.Type = new NoInspectorDerivedType();
            element.SetTarget(instance);
            Assert.That(element.Q<Label>(className:BaseType.Label).text, Is.EqualTo(nameof(BaseType)));
            element.ClearTarget();
            instance.Type = new HasInspectorDerivedType();
            element.SetTarget(instance);
            Assert.That(element.Q<Label>(className:BaseType.Label).text, Is.EqualTo(nameof(HasInspectorDerivedType)));
        }
    
        [Test]
        public void CanGetInspectorForType()
        {
            Assert.That(CustomInspectorDatabase.GetRootInspector<NoInspectorType>(), Is.Null);
            var i0 = CustomInspectorDatabase.GetRootInspector<SingleInspectorType>();
            Assert.That(i0, Is.Not.Null);
            Assert.That(i0, Is.TypeOf<SingleInspectorTypeInspector>());

            var i1 = CustomInspectorDatabase.GetRootInspector<MultipleInspectorsType>();
            Assert.That(i1, Is.Not.Null);
            Assert.That(i1, Is.TypeOf<MultipleInspectorsTypeInspector>());

            Assert.That(CustomInspectorDatabase.GetRootInspector<NoInspectorButDrawerType>(), Is.Null);

            var i2 = CustomInspectorDatabase.GetRootInspector<InspectorAndDrawerType>();
            Assert.That(i2, Is.Not.Null);
            Assert.That(i2, Is.TypeOf<InspectorAndDrawerTypeInspector>());
        }

        [Test]
        public void CanGetPropertyDrawerForType()
        {
            Assert.That(CustomInspectorDatabase.GetPropertyDrawer<NoInspectorType, DrawerAttribute>(), Is.Null);
            Assert.That(CustomInspectorDatabase.GetPropertyDrawer<SingleInspectorType, DrawerAttribute>(), Is.Null);
            Assert.That(CustomInspectorDatabase.GetPropertyDrawer<MultipleInspectorsType, DrawerAttribute>(), Is.Null);

            var d0 = CustomInspectorDatabase.GetPropertyDrawer<NoInspectorButDrawerType, DrawerAttribute>();
            Assert.That(d0, Is.Not.Null);
            Assert.That(d0, Is.TypeOf<NoInspectorButDrawerTypeDrawer>());
            
            var d1 = CustomInspectorDatabase.GetPropertyDrawer<InspectorAndDrawerType, DrawerAttribute>();
            Assert.That(d1, Is.Not.Null);
            Assert.That(d1, Is.TypeOf<InspectorAndDrawerTypeTypeDrawer>());
        }

        [Test]
        public void CanGetListForInspectorTypes()
        {
            var l0 = CustomInspectorDatabase.GetInspectorTypes<NoInspectorType>().ToList(); 
            Assert.That(l0.Count, Is.EqualTo(0));

            var l1 = CustomInspectorDatabase.GetInspectorTypes<SingleInspectorType>().ToList(); 
            Assert.That(l1.Count, Is.EqualTo(1));
            Assert.That(l1[0], Is.EqualTo(typeof(SingleInspectorTypeInspector)));
            
            var l2 = CustomInspectorDatabase.GetInspectorTypes<MultipleInspectorsType>().ToList(); 
            Assert.That(l2.Count, Is.EqualTo(2));
            Assert.That(l2[0], Is.EqualTo(typeof(MultipleInspectorsTypeInspector)));
            Assert.That(l2[1], Is.EqualTo(typeof(MultipleInspectorsTypeInspectorWithTag)));
            
            var l3 = CustomInspectorDatabase.GetInspectorTypes<NoInspectorButDrawerType>().ToList(); 
            Assert.That(l3.Count, Is.EqualTo(1));
            Assert.That(l3[0], Is.EqualTo(typeof(NoInspectorButDrawerTypeDrawer)));
            
            var l4 = CustomInspectorDatabase.GetInspectorTypes<InspectorAndDrawerType>().ToList(); 
            Assert.That(l4.Count, Is.EqualTo(3));
            Assert.That(l4[0], Is.EqualTo(typeof(InspectorAndDrawerTypeInspector)));
            Assert.That(l4[1], Is.EqualTo(typeof(InspectorAndDrawerTypeTypeDrawer)));
            Assert.That(l4[2], Is.EqualTo(typeof(InspectorAndDrawerTypeTypeDrawerWithTag)));
        }
        
        [Test]
        public void CanGetInspectorWithConstraints()
        {
            var i0 = CustomInspectorDatabase.GetInspector<NoInspectorType>(
                InspectorConstraint.AssignableTo<IUserInspectorTag>()); 
            Assert.That(i0, Is.Null);

            var i1 = CustomInspectorDatabase.GetInspector<MultipleInspectorsType>(
                InspectorConstraint.AssignableTo<IUserInspectorTag>());
            Assert.That(i1, Is.Not.Null);
            Assert.That(i1, Is.TypeOf<MultipleInspectorsTypeInspectorWithTag>());
            
            var i3 = CustomInspectorDatabase.GetInspector<InspectorAndDrawerType>(
                InspectorConstraint.AssignableTo<IPropertyDrawer>(),
                InspectorConstraint.AssignableTo<IUserInspectorTag>());
            Assert.That(i3, Is.Not.Null);
            Assert.That(i3, Is.TypeOf<InspectorAndDrawerTypeTypeDrawerWithTag>());
            
            var i4 = CustomInspectorDatabase.GetInspector<InspectorAndDrawerType>(
                InspectorConstraint.AssignableTo<IPropertyDrawer>(),
                InspectorConstraint.AssignableTo<IAnotherUserInspectorTag>());
            Assert.That(i4, Is.Not.Null);
            Assert.That(i4, Is.TypeOf<InspectorAndDrawerTypeTypeDrawer>());
        }
    }
}
