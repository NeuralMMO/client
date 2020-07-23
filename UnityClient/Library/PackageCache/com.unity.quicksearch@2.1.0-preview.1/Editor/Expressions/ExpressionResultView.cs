using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.QuickSearch
{
    class ExpressionResultView : IMGUIContainer, ISearchView
    {
        private readonly IResultView m_View;
        private readonly List<int> m_Selection = new List<int>();

        public SearchExpression expression { get; private set; }
        public ISearchList results => expression;
        public SearchContext context => expression.context;
        public SearchSelection selection => new SearchSelection(m_Selection, results);
        public float itemIconSize { get; set; } = 1f;
        public DisplayMode displayMode => DisplayMode.List;
        public bool multiselect { get; set; } = false;
        public Action<SearchItem, bool> selectCallback => OnItemSelected;
        public Func<SearchItem, bool> filterCallback => null;
        public Action<SearchItem> trackingCallback => null;

        public ExpressionResultView(SearchExpression expression)
        {
            style.overflow = Overflow.Hidden;
            onGUIHandler = OnGUI;

            this.expression = expression;
            m_View = new ListView(this);
        }

        public void AddSelection(params int[] selection)
        {
            m_Selection.AddRange(selection.Where(s => !m_Selection.Contains(s)));
        }

        public void ExecuteAction(SearchAction action, SearchItem[] items, bool endSearch = true)
        {
            selectCallback?.Invoke(items.FirstOrDefault(), false);
        }

        public void Refresh()
        {
            Repaint();
        }

        public void Repaint()
        {
            panel?.visualTree.MarkDirtyRepaint();
        }

        public void SetSearchText(string searchText, TextCursorPlacement moveCursor = TextCursorPlacement.MoveLineEnd)
        {
            context.searchText = searchText;
            Refresh();
        }

        public void SetSelection(params int[] selection)
        {
            m_Selection.Clear();
            m_Selection.AddRange(selection);
        }

        public void ShowItemContextualMenu(SearchItem item, Rect contextualActionPosition)
        {
            // Nothing to do.
        }

        public void Close()
        {
            // Nothing to do.
        }

        public void PopFilterWindow()
        {
            throw new NotSupportedException();
        }

        private void OnItemSelected(SearchItem selectedItem, bool canceled)
        {
            if (selectedItem == null || canceled)
                return;

            var provider = selectedItem.provider;
            var selectAction = provider.actions.FirstOrDefault(a => a.id == "select");
            if (selectAction != null && selectAction.handler != null)
                selectAction.handler(selectedItem);
            else if (selectAction != null && selectAction.execute != null)
                selectAction.execute(new SearchItem[] { selectedItem });
            else
                selectedItem.provider?.trackSelection?.Invoke(selectedItem, context);
        }

        private void OnGUI()
        {
            expression.context.searchView = this;
            m_View.HandleInputEvent(Event.current, m_Selection);
            m_View.Draw(m_Selection, resolvedStyle.width);
            DrawStats();
        }

        public void Draw(Rect previewArea)
        {
            expression.context.searchView = this;
            m_View.Draw(previewArea, m_Selection);
        }

        public string GetPreviewString()
        {
            if (expression.pending)
                return $"Still searching but we've already found {expression.Count} results...";
            if (expression.Count == 0)
                return "No result";
            return $"Found {expression.Count} results from {expression.requestCount} request(s) in {expression.elapsedTime} ms";
        }

        public void DrawStats()
        {
            using (new GUILayout.HorizontalScope(Styles.debugToolbar))
            {
                GUILayout.Label(GetPreviewString(), Styles.debugLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Refresh", Styles.debugToolbarButton))
                    expression.Evaluate();
            }
        }

        public void SelectSearch()
        {
            // Nothing to select
        }
    }
}
