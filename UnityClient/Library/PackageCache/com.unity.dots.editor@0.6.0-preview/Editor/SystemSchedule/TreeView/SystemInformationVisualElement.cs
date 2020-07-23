using UnityEngine.UIElements;

namespace Unity.Entities.Editor
{
    class SystemInformationVisualElement : BindableElement, IBinding, IPoolable
    {
        public World World;
        SystemTreeViewItem m_Target;
        public SystemScheduleTreeView TreeView { get; set; }

        public SystemTreeViewItem Target
        {
            get => m_Target;
            set
            {
                if (m_Target == value)
                    return;
                m_Target = value;
                Update();
            }
        }

        readonly Toggle m_SystemEnableToggle;
        readonly VisualElement m_Icon;
        readonly Label m_SystemNameLabel;
        readonly Label m_EntityMatchLabel;
        readonly Label m_RunningTimeLabel;

        public SystemInformationVisualElement()
        {
            Resources.Templates.CommonResources.AddStyles(this);
            Resources.Templates.SystemScheduleItem.Clone(this);
            binding = this;

            AddToClassList(UssClasses.DotsEditorCommon.CommonResources);
            AddToClassList(UssClasses.Resources.SystemSchedule);

            m_SystemEnableToggle = this.Q<Toggle>(className: UssClasses.SystemScheduleWindow.Items.Enabled);
            m_SystemEnableToggle.RegisterCallback<ChangeEvent<bool>, SystemInformationVisualElement>(
                OnSystemTogglePress, this);

            m_Icon = this.Q(className: UssClasses.SystemScheduleWindow.Items.Icon);

            m_SystemNameLabel = this.Q<Label>(className: UssClasses.SystemScheduleWindow.Items.SystemName);
            m_EntityMatchLabel = this.Q<Label>(className: UssClasses.SystemScheduleWindow.Items.Matches);
            m_RunningTimeLabel = this.Q<Label>(className: UssClasses.SystemScheduleWindow.Items.Time);
        }

        static void SetText(Label label, string text)
        {
            if (label.text != text)
            {
                label.text = text;
            }
        }

        public void Update()
        {
            if (null == Target)
                return;

            if (m_Target.System != null && m_Target.System.World == null)
                return;

            if (string.Empty == GetSystemClass(m_Target?.System))
            {
                m_Icon.style.display = DisplayStyle.None;
            }
            else
            {
                m_Icon.style.display = DisplayStyle.Flex;
            }

            SetText(m_SystemNameLabel, Target.GetSystemName(World));
            SetText(m_EntityMatchLabel, Target.GetEntityMatches());
            SetText(m_RunningTimeLabel, Target.GetRunningTime());
            SetSystemClass(m_Icon, m_Target?.System);

            if (Target.System == null) // player loop system without children
            {
                SetEnabled(Target.HasChildren);
                m_SystemEnableToggle.style.display = DisplayStyle.None;
            }
            else
            {
                this.SetEnabled(true);
                m_SystemEnableToggle.style.display = DisplayStyle.Flex;
                var systemState = Target.System?.Enabled ?? true;
                if (m_SystemEnableToggle.value != systemState)
                {
                    m_SystemEnableToggle.SetValueWithoutNotify(systemState);
                }

                var groupState = systemState && Target.GetParentState();

                m_SystemNameLabel.SetEnabled(groupState);
                m_EntityMatchLabel.SetEnabled(groupState);
                m_RunningTimeLabel.SetEnabled(groupState);
            }
        }

        static void SetSystemClass(VisualElement element, ComponentSystemBase system)
        {
            switch (system)
            {
                case null:
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.CommandBufferIcon);
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.SystemGroupIcon);
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.SystemIcon);
                    break;
                case EntityCommandBufferSystem _:
                    element.AddToClassList(UssClasses.SystemScheduleWindow.Items.CommandBufferIcon);
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.SystemGroupIcon);
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.SystemIcon);
                    break;
                case ComponentSystemGroup _:
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.CommandBufferIcon);
                    element.AddToClassList(UssClasses.SystemScheduleWindow.Items.SystemGroupIcon);
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.SystemIcon);
                    break;
                case ComponentSystemBase _:
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.CommandBufferIcon);
                    element.RemoveFromClassList(UssClasses.SystemScheduleWindow.Items.SystemGroupIcon);
                    element.AddToClassList(UssClasses.SystemScheduleWindow.Items.SystemIcon);
                    break;
            }
        }

        static string GetSystemClass(ComponentSystemBase system)
        {
            switch (system)
            {
                case null:
                    return "";
                case EntityCommandBufferSystem _:
                    return UssClasses.SystemScheduleWindow.Items.CommandBufferIcon;
                case ComponentSystemGroup _:
                    return UssClasses.SystemScheduleWindow.Items.SystemGroupIcon;
                case ComponentSystemBase _:
                    return UssClasses.SystemScheduleWindow.Items.SystemIcon;
            }
        }

        static void OnSystemTogglePress(ChangeEvent<bool> evt, SystemInformationVisualElement item)
        {
            if (item.Target.System != null)
            {
                item.Target.SetSystemState(evt.newValue);
            }
            else
            {
                item.Target.SetPlayerLoopSystemState(evt.newValue);
            }
        }

        public void PreUpdate()
        {
        }

        public void Release()
        {
        }

        public void Reset()
        {
            World = null;
            Target = null;
            TreeView = null;
        }

        public void ReturnToPool()
        {
            SystemSchedulePool.ReturnToPool(TreeView, this);
        }
    }
}
