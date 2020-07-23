using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.QuickSearch
{
    /// <summary>
    /// A view able to display a <see cref="ISearchList"/> of <see cref="SearchItem"/>s.
    /// </summary>
    interface IResultView
    {
        /// <summary>Items to be displayed.</summary>
        ISearchList items { get; }

        /// <summary>Item size in pixels.</summary>
        float itemSize { get; }

        /// <summary>Scroll position of the content area of the view.</summary>
        Vector2 scrollPosition { get; }

        /// <summary>Search view that contains the text area where a query is entered.</summary>
        ISearchView searchView { get; }

        /// <summary>Rect of the result view.</summary>
        Rect rect { get; }

        /// <summary>
        /// Indicates if the result view should focus the selected index.
        /// </summary>
        bool focusSelectedItem { get; set; }

        /// <summary>
        /// Indicates if a scrollbar is displayed
        /// </summary>
        bool scrollbarVisible { get; }

        /// <summary>
        /// Draw the items in a specified rect area specifying which items are selected.
        /// </summary>
        /// <param name="rect">Rect of the drawing area.</param>
        /// <param name="selection">Indexes of items to draw as selected</param>
        void Draw(Rect rect, ICollection<int> selection);

        /// <summary>
        /// Draw the items specifying which items are selected.
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="sliderPos"></param>
        /// <param name="focusSelectedItem"></param>
        void Draw(ICollection<int> selection, float sliderPos);

        /// <summary>
        ///  Get how many items can be shown in the display area depending on the <see cref="IResultView.itemSize"/>
        /// </summary>
        /// <returns>Returns the number of visible items.</returns>
        int GetDisplayItemCount();

        /// <summary>
        /// Handle user input events.
        /// </summary>
        /// <param name="evt"></param>
        void HandleInputEvent(Event evt, List<int> selection);
    }

    abstract class ResultView : IResultView
    {
        const int k_ResetSelectionIndex = -1;

        protected Vector2 m_ScrollPosition;
        protected bool m_PrepareDrag;
        protected Vector3 m_DragStartPosition;
        protected int m_MouseDownItemIndex;
        protected double m_ClickTime = 0;
        protected Rect m_DrawItemsRect = Rect.zero;

        public ResultView(ISearchView hostView)
        {
            searchView = hostView;
        }

        public ISearchList items => searchView.results;
        public float itemSize => searchView.itemIconSize;
        public Vector2 scrollPosition { get => m_ScrollPosition; set => m_ScrollPosition = value; }
        public ISearchView searchView { get; private set; }
        public SearchContext context => searchView.context;
        public Rect rect => m_DrawItemsRect;
        public bool focusSelectedItem { get; set; }
        protected bool compactView => itemSize == 0;
        public bool scrollbarVisible { get; protected set; }

        public abstract int GetDisplayItemCount();
        public abstract void Draw(Rect rect, ICollection<int> selection);

        public void Draw(ICollection<int> selection, float sliderPos)
        {
            GUILayout.Box(String.Empty, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.Width(sliderPos-2f));
            if (Event.current.type == EventType.Repaint)
                m_DrawItemsRect = GUILayoutUtility.GetLastRect();

            Draw(m_DrawItemsRect, selection);
        }

        protected bool IsDragClicked(Event evt)
        {
            if (evt.type != EventType.DragExited)
                return false;
            return IsDragClicked(evt.mousePosition);
        }

        protected bool IsDragClicked(Vector2 mousePosition)
        {
            var dragDistance = Vector2.Distance(m_DragStartPosition, mousePosition);
            #if false // Used to debug miss double clicks.
            Debug.Log($"Drag distance: {dragDistance}");
            #endif
            return dragDistance < 15;
        }

        protected void HandleMouseDown(int clickedItemIndex)
        {
            m_PrepareDrag = true;
            m_MouseDownItemIndex = clickedItemIndex;
            m_DragStartPosition = Event.current.mousePosition;
        }

        protected void HandleMouseUp(int clickedItemIndex, int itemTotalCount)
        {
            var evt = Event.current;
            var mouseDownItemIndex = m_MouseDownItemIndex;
            m_MouseDownItemIndex = -1;

            if (AutoComplete.IsHovered(evt.mousePosition))
                return;

            if (clickedItemIndex >= 0 && clickedItemIndex < itemTotalCount)
            {
                if (evt.button == 0 && mouseDownItemIndex == clickedItemIndex)
                {
                    var ctrl = evt.control || evt.command;
                    var now = EditorApplication.timeSinceStartup;
                    if (searchView.multiselect && evt.modifiers.HasFlag(EventModifiers.Shift))
                    {
                        int min = searchView.selection.MinIndex();
                        int max = searchView.selection.MaxIndex();

                        if (clickedItemIndex > min)
                        {
                            if (ctrl && clickedItemIndex > max)
                            {
                                var r = 0;
                                var range = new int[clickedItemIndex - max];
                                for (int i = max+1; i <= clickedItemIndex; ++i)
                                    range[r++] = i;
                                searchView.AddSelection(range);
                            }
                            else
                            {
                                var r = 0;
                                var range = new int[clickedItemIndex - min + 1];
                                for (int i = min; i <= clickedItemIndex; ++i)
                                    range[r++] = i;
                                searchView.SetSelection(range);
                            }
                        }
                        else if (clickedItemIndex < max)
                        {
                            if (ctrl && clickedItemIndex < min)
                            {
                                var r = 0;
                                var range = new int[min - clickedItemIndex];
                                for (int i = min-1; i >= clickedItemIndex; --i)
                                    range[r++] = i;
                                searchView.AddSelection(range);
                            }
                            else
                            {
                                var r = 0;
                                var range = new int[max - clickedItemIndex + 1];
                                for (int i = max; i >= clickedItemIndex; --i)
                                    range[r++] = i;
                                searchView.SetSelection(range);
                            }
                        }
                    }
                    else if (searchView.multiselect && ctrl)
                    {
                        searchView.AddSelection(clickedItemIndex);
                    }
                    else
                        searchView.SetSelection(clickedItemIndex);

                    if ((now - m_ClickTime) < 0.3)
                    {
                        var item = items.ElementAt(clickedItemIndex);
                        if (item.provider.actions.Count > 0)
                            searchView.ExecuteAction(item.provider.actions[0], new []{item});
                        GUIUtility.ExitGUI();
                    }
                    SearchField.Focus();
                    evt.Use();
                    m_ClickTime = now;
                }
                else if (evt.button == 1)
                {
                    var item = items.ElementAt(clickedItemIndex);
                    var contextRect = new Rect(evt.mousePosition, new Vector2(1, 1));
                    if (item.provider.openContextual == null || !item.provider.openContextual(searchView.selection, contextRect))
                    {
                        if (searchView.selection.Count <= 1)
                            searchView.ShowItemContextualMenu(item, contextRect);
                    }
                }
            }

            // Reset drag content
            m_PrepareDrag = false;
            DragAndDrop.PrepareStartDrag();
        }

        protected void HandleMouseDrag(int dragIndex, int itemTotalCount)
        {
            if (IsDragClicked(Event.current.mousePosition))
                return;
            if (dragIndex >= 0 && dragIndex < itemTotalCount)
            {
                var item = items.ElementAt(dragIndex);
                if (item.provider?.startDrag != null)
                {
                    item.provider.startDrag(item, searchView.context);
                    m_PrepareDrag = false;

                    Event.current.Use();
                    #if UNITY_EDITOR_OSX
                    searchView.Close();
                    GUIUtility.ExitGUI();
                    #endif
                }
            }
        }

        protected void HandleKeyEvent(Event evt, List<int> selection)
        {
            var ctrl = evt.control || evt.command;
            var selectedIndex = selection.Count == 0 ? k_ResetSelectionIndex : selection.Last();
            var firstIndex = selection.Count == 0 ? k_ResetSelectionIndex : selection.First();
            var lastIndex = selectedIndex;
            var multiselect = searchView.multiselect;
            var results = searchView.results;
            if (evt.keyCode == KeyCode.DownArrow)
            {
                if (multiselect && evt.modifiers.HasFlag(EventModifiers.Shift) && selection.Count > 0)
                {
                    if (lastIndex >= firstIndex)
                    {
                        if (lastIndex < results.Count - 1)
                            searchView.AddSelection(lastIndex + 1);
                    }
                    else
                        selection.Remove(lastIndex);
                }
                else if (lastIndex < results.Count - 1)
                    searchView.SetSelection(selectedIndex + 1);
                evt.Use();
            }
            else if (evt.keyCode == KeyCode.UpArrow)
            {
                if (selectedIndex >= 0)
                {
                    if (multiselect && evt.modifiers.HasFlag(EventModifiers.Shift) && selection.Count > 0)
                    {
                        if (firstIndex < lastIndex)
                            selection.Remove(lastIndex);
                        else if (lastIndex > 0)
                            searchView.AddSelection(lastIndex - 1);
                    }
                    else
                        searchView.SetSelection(selectedIndex - 1);
                    if (selectedIndex - 1 < 0)
                        searchView.SelectSearch();
                    evt.Use();
                }
            }
            else if (ctrl && evt.keyCode == KeyCode.End || evt.keyCode == KeyCode.PageDown)
            {
                HandlePageDown(evt, selection);
            }
            else if (ctrl && evt.keyCode == KeyCode.Home || evt.keyCode == KeyCode.PageUp)
            {
                HandlePageUp(evt, selection);
            }
            else if (evt.keyCode == KeyCode.RightArrow && evt.modifiers.HasFlag(EventModifiers.Alt))
            {
                if (selectedIndex != -1 && selection.Count <= 1)
                {
                    var item = results.ElementAt(selectedIndex);
                    var menuPositionY = (selectedIndex + 1) * Styles.itemRowHeight - scrollPosition.y + Styles.itemRowHeight / 2.0f;
                    searchView.ShowItemContextualMenu(item, new Rect(rect.xMax - Styles.actionButtonSize, menuPositionY, 1, 1));
                    evt.Use();
                }
            }
            else if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return)
            {
                if (selectedIndex == -1 && results.Count > 0)
                    selectedIndex = 0;

                if (selectedIndex != -1)
                {
                    var item = results.ElementAt(selectedIndex);
                    if (item.provider.actions.Count > 0)
                    {
                        SearchAction action = item.provider.actions[0];
                        if (context.actionId != null)
                        {
                            action = SearchService.GetAction(item.provider, context.actionId);
                        }
                        else if (evt.modifiers.HasFlag(EventModifiers.Alt))
                        {
                            var actionIndex = 1;
                            if (evt.modifiers.HasFlag(EventModifiers.Control))
                            {
                                actionIndex = 2;
                                if (evt.modifiers.HasFlag(EventModifiers.Shift))
                                    actionIndex = 3;
                            }
                            action = item.provider.actions[Math.Max(0, Math.Min(actionIndex, item.provider.actions.Count - 1))];
                        }

                        if (action != null)
                        {
                            evt.Use();
                            searchView.ExecuteAction(action, searchView.selection.ToArray());
                            GUIUtility.ExitGUI();
                        }
                    }
                }
            }
            else if (!EditorGUIUtility.editingTextField)
                SearchField.Focus();

            var newSelection = selection.Count == 0 ? k_ResetSelectionIndex : selection.Last();
            if (selectedIndex != newSelection)
                focusSelectedItem = true;
        }

        protected virtual void HandlePageDown(Event evt, List<int> selection)
        {
            var ctrl = evt.control || evt.command;
            var jumpAtIndex = k_ResetSelectionIndex;

            if (evt.keyCode == KeyCode.PageDown)
            {
                jumpAtIndex = GetLastVisibleItemIndex();
                if (selection.Count > 0 && selection.Last() == jumpAtIndex)
                    jumpAtIndex += GetVisibleItemCount();
                jumpAtIndex = Math.Min(jumpAtIndex, searchView.results.Count - 1);
            }
            else if (ctrl && evt.keyCode == KeyCode.End)
            {
                jumpAtIndex = searchView.results.Count - 1;
            }

            if (searchView.multiselect && evt.modifiers.HasFlag(EventModifiers.Shift) && selection.Count > 0)
            {
                var selectedIndex = selection.Count == 0 ? k_ResetSelectionIndex : selection.First();
                searchView.SetSelection(GenerateSelectionRange(selectedIndex, jumpAtIndex));
            }
            else
            {
                searchView.SetSelection(jumpAtIndex);
            }
            evt.Use();
        }

        protected virtual void HandlePageUp(Event evt, List<int> selection)
        {
            var ctrl = evt.control || evt.command;
            var jumpAtIndex = k_ResetSelectionIndex;

            if (evt.keyCode == KeyCode.PageUp)
            {
                jumpAtIndex = GetFirstVisibleItemIndex();
                if (selection.Count > 0 && selection.Last() == jumpAtIndex)
                    jumpAtIndex -= GetVisibleItemCount();
                jumpAtIndex = Math.Max(0, jumpAtIndex);
            }
            else if (ctrl && evt.keyCode == KeyCode.Home)
            {
                jumpAtIndex = 0;
            }

            if (searchView.multiselect && evt.modifiers.HasFlag(EventModifiers.Shift) && selection.Count > 0)
            {
                var selectedIndex = selection.Count == 0 ? k_ResetSelectionIndex : selection.First();
                searchView.SetSelection(GenerateSelectionRange(selectedIndex, jumpAtIndex));
            }
            else
            {
                searchView.SetSelection(jumpAtIndex);
            }
            evt.Use();
        }

        private int[] GenerateSelectionRange(int first, int last)
        {
            int r = 0;
            var diff = last - first;
            var range = new int[Math.Abs(diff) + 1];
            if (diff >= 0)
            {
                for (int i = first; i <= last; ++i)
                    range[r++] = i;
            }
            else
            {
                for (int i = first; i >= last; --i)
                    range[r++] = i;
            }
            return range;
        }

        protected virtual float GetRowHeight()
        {
            return compactView ? Styles.itemRowHeight / 2.0f : Styles.itemRowHeight;
        }

        protected virtual int GetFirstVisibleItemIndex()
        {
            var rowHeight = GetRowHeight();
            return Math.Max(0, Mathf.CeilToInt(m_ScrollPosition.y / rowHeight));
        }

        protected virtual int GetLastVisibleItemIndex()
        {
            var firstVisibleIndex = GetFirstVisibleItemIndex();
            var itemVisibleCount = GetVisibleItemCount();
            return firstVisibleIndex + itemVisibleCount - 1;
        }

        protected virtual int GetVisibleItemCount()
        {
            var itemCount = items.Count;
            var rowHeight = GetRowHeight();
            var availableHeight = rect.height;
            return Math.Max(0, Math.Min(itemCount, Mathf.FloorToInt(availableHeight / rowHeight)));
        }

        public void HandleInputEvent(Event evt, List<int> selection)
        {
            if (evt.type == EventType.KeyDown)
                HandleKeyEvent(evt, selection);
        }
    }
}