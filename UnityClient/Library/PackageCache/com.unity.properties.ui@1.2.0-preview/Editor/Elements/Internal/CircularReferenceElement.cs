using System;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Internal
{
    class CircularReferenceElement<T> : BindableElement, IBinding
    {
        readonly PropertyElement m_Root;
        readonly PropertyPath m_Path;
        readonly PropertyPath m_PathToReference;
        readonly T m_Value;
        
        IProperty GetProperty() => m_Root.TryGetProperty(m_Path, out var property) ? property : default;
        
        public CircularReferenceElement(PropertyElement root, IProperty property, T value, PropertyPath path, PropertyPath pathToReference)
        {
            binding = this;
            m_Root = root;
            m_Path = path;
            m_PathToReference = pathToReference;
            m_Value = value;
            name = m_PathToReference.ToString();
            style.flexDirection = FlexDirection.Row;
            Resources.Templates.CircularReference.Clone(this);

            var label = this.Q<Label>(className: UssClasses.CircularReferenceElement.Label);
            label.text = GuiFactory.GetDisplayName(property);
                label.AddManipulator(
                new ContextualMenuManipulator(evt =>
                {
                    var prop = GetProperty();
                    if (null == prop)
                        return;
                    
                    var inspectorOptions = property.GetAttribute<InspectorOptionsAttribute>();
                    
                    if (property.IsReadOnly || true == inspectorOptions?.HideResetToDefault)
                    {
                        return;
                    }

                    evt.menu.AppendAction(
                        "Reset to default",
                        p => ReloadWithInstance(default),
                        p => prop.HasAttribute<CreateInstanceOnInspectionAttribute>()
                            ? DropdownMenuAction.Status.Disabled
                            : DropdownMenuAction.Status.Normal);
                }));
            
            this.Q<Button>(className: UssClasses.CircularReferenceElement.Path).text = "ref: " + pathToReference + $" ({TypeUtility.GetResolvedTypeName(value.GetType())})";
            this.Q(className: UssClasses.CircularReferenceElement.Icon).tooltip = $"Circular reference found for path: `{pathToReference}`";
            
            RegisterCallback<MouseEnterEvent>(OnEnter);
            RegisterCallback<MouseLeaveEvent>(OnLeave);
        }

        void OnEnter(MouseEnterEvent evt)
        {
            m_Root.StartHighlightAtPath(m_PathToReference);
        }
        
        void OnLeave(MouseLeaveEvent evt)
        {
            m_Root.StopHighlightAtPath(m_PathToReference);
        }

        public void PreUpdate()
        {
        }

        public void Update()
        {
            try
            {
                if (!m_Root.TryGetValue<T>(m_PathToReference, out var value))
                {
                    return;
                }

                if (ReferenceEquals(m_Value, value))
                    return;

                ReloadWithInstance(value);
            }
            catch (Exception )
            {
                
            }
        }

        public void Release()
        {
        }
        
        void ReloadWithInstance(T value)
        {
            m_Root.SwapWithInstance(m_Path, this, value);
        }
    }
}