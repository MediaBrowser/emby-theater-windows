// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
using System;
using System.Drawing;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mpdn.RenderScript;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Mpdn.Presets
    {
        public partial class PresetDialog : PresetDialogBase
        {
            private const string SELECTED_INDICATOR_STR = "➔";

            /* Settings */

            #region Save / Load

            private int m_SelectedIndex = -1;
            protected int SelectedIndex
            {
                get { return m_SelectedIndex; }
                set
                {
                    if (0 <= value && value < listViewChain.Items.Count)
                    {
                        m_SelectedIndex = value;
                        foreach (ListViewItem item in listViewChain.SelectedItems)
                            item.Selected = false;
                        listViewChain.Items[value].Selected = true;
                    }
                    else
                        m_SelectedIndex = -1;
                }
            }

            public PresetDialog()
            {
                InitializeComponent();

                var renderScripts = PlayerControl.RenderScripts
                    .Where(script => script is IRenderChainUi)
                    .Select(x => (x as IRenderChainUi).CreateNew())
                    .Concat(new [] { RenderChainUi.Identity } )
                    .OrderBy(x => x.Category + SELECTED_INDICATOR_STR + x.Descriptor.Name);

                var groups = new Dictionary<string, ListViewGroup>();

                foreach (var script in renderScripts)
                if (script.Category.ToLowerInvariant() != "hidden")
                {
                    var item = listViewAvail.Items.Add(string.Empty);
                    item.SubItems.Add(script.Descriptor.Name);
                    item.SubItems.Add(script.Descriptor.Description);
                    item.Tag = script;

                    if (!groups.ContainsKey(script.Category))
                        groups.Add(
                            script.Category, 
                            listViewAvail.Groups.Add("", script.Category));

                    item.Group = groups[script.Category];
                }

                if (listViewAvail.Items.Count > 0)
                {
                    var firstItem = listViewAvail.Items[0];
                    firstItem.Text = SELECTED_INDICATOR_STR;
                    firstItem.Selected = true;
                }

                listViewAvail.Sort();

                var menuitem = (ToolStripMenuItem)menuChain.Items
                    .Find("menuGroup", false).First();

                foreach (var script in renderScripts)
                {
                    var group = script.Chain as PresetCollection;
                    if (group != null && group.AllowRegrouping)
                    {
                        var item = menuitem.DropDownItems.Add(script.Descriptor.Name);
                        item.Tag = script;
                    }
                }

                ResizeLists();
                UpdateButtons();
            }

            protected List<Preset> GatherPresets(ListView.ListViewItemCollection items)
            {
                var scripts = from item in listViewChain.Items.Cast<ListViewItem>()
                              select (Preset)item.Tag;
                return scripts.ToList();
            }

            protected List<Preset> GatherPresets(ListViewGroupCollection items)
            {
                var scripts = from item in listViewChain.Items.Cast<ListViewItem>()
                              select (Preset)item.Tag;
                return scripts.ToList();
            }

            protected override void LoadSettings()
            {
                AddPresets(Settings.Options);
                listViewChain.SelectedIndices.Clear();

                ResizeLists();
                UpdateButtons();
            }

            protected override void SaveSettings()
            {
                Settings.Options = GatherPresets(listViewChain.Items);
            }

            #endregion

            /* List Manipulation */

            #region Item manipulation

            private void AddPresets(IEnumerable<Preset> presets, int index = -1)
            {
                listViewChain.SelectedItems.Clear();
                foreach (var preset in presets)
                {
                    ListViewItem item;
                    if (index < 0)
                        item = listViewChain.Items.Add(string.Empty);
                    else
                        item = listViewChain.Items.Insert(index++,string.Empty);

                    item.SubItems.Add(preset.Name);
                    item.SubItems.Add(preset.Description);
                    item.Tag = preset;

                    listViewChain.SelectedIndices.Add(item.Index);
                }
                ResizeLists();
                UpdateButtons();
            }

            private void AddPreset(Preset preset, int index = -1)
            {
                AddPresets(new[] { preset }, index);
            }

            private void AddScripts(IEnumerable<IRenderChainUi> renderScripts, int index = -1) 
            {
				AddPresets(renderScripts.Select(script => script.MakeNewPreset()), index);
            }

            private void AddScript(IRenderChainUi renderScript, int index = -1)
            {
                AddScripts(new[] { renderScript }, index);
            }

            private void RemoveItem(ListViewItem selectedItem)
            {
                var preset = (Preset)selectedItem.Tag;

                var index = selectedItem.Index;
                selectedItem.Remove();
                if (index < listViewChain.Items.Count)
                {
                    listViewChain.Items[index].Selected = true;
                }
                else if (listViewChain.Items.Count > 0)
                {
                    listViewChain.Items[listViewChain.Items.Count - 1].Selected = true;
                }

                ResizeLists();
                UpdateButtons();
            }

            private void ConfigureItem(ListViewItem item)
            {
                var preset = (Preset)item.Tag;
                if (preset.HasConfigDialog() && preset.ShowConfigDialog(Owner))
                    UpdateItemText(item);
            }

            private enum MoveDirection
            {
                Up = -1,
                Down = 1
            };

            private static void MoveListViewItems(ListView listView, MoveDirection direction)
            {
                var valid = listView.SelectedItems.Count > 0 &&
                            ((direction == MoveDirection.Down &&
                              (listView.SelectedItems[listView.SelectedItems.Count - 1].Index < listView.Items.Count - 1))
                             || (direction == MoveDirection.Up && (listView.SelectedItems[0].Index > 0)));

                if (!valid)
                    return;

                var start = true;
                var firstIdx = 0;
                var items = new List<ListViewItem>();

                foreach (ListViewItem i in listView.SelectedItems)
                {
                    if (start)
                    {
                        firstIdx = i.Index;
                        start = false;
                    }
                    items.Add(i);
                }

                listView.BeginUpdate();

                foreach (ListViewItem i in listView.SelectedItems)
                {
                    i.Remove();
                }

                if (direction == MoveDirection.Up)
                {
                    var insertTo = firstIdx - 1;
                    foreach (var i in items)
                    {
                        i.Selected = true;
                        listView.Items.Insert(insertTo, i);
                        insertTo++;
                    }
                }
                else
                {
                    var insertTo = firstIdx + 1;
                    foreach (var i in items)
                    {
                        i.Selected = true;
                        listView.Items.Insert(insertTo, i);
                        insertTo++;
                    }
                }

                listView.EndUpdate();
                listView.Focus();
            }

            #endregion

            #region Selecting

            private void ListViewSelectedIndexChanged(object sender, EventArgs e)
            {
                foreach (ListViewItem i in listViewAvail.Items)
                {
                    i.Text = string.Empty;
                }

                if (listViewAvail.SelectedItems.Count > 0)
                {
                    var item = listViewAvail.SelectedItems[0];
                    var script = (IRenderChainUi)item.Tag;

                    item.Text = SELECTED_INDICATOR_STR;
                    labelCopyright.Text = script == null ? string.Empty : script.Descriptor.Copyright;
                }

                UpdateButtons();
            }

            private void ListViewChainSelectedIndexChanged(object sender, EventArgs e)
            {
                foreach (ListViewItem i in listViewChain.Items)
                {
                    i.Text = string.Empty;
                }

                if (listViewChain.SelectedItems.Count > 0)
                {
                    var item = listViewChain.SelectedItems[0];
                    var preset = (Preset)item.Tag;

                    m_SelectedIndex = item.Index;
                    item.Text = SELECTED_INDICATOR_STR;
                    NameBox.Text = preset.Name;
                }
                else
                    NameBox.Text = string.Empty;

                UpdateButtons();
            }

            private void SelectAll(object sender, EventArgs e)
            {
                foreach (ListViewItem item in listViewChain.Items)
                    item.Selected = true;
            }

            #endregion

            #region Drap / Drop

            private void ItemCopyDrag(object sender, ItemDragEventArgs e)
            {
                DoDragDrop((sender as ListView).SelectedItems, DragDropEffects.Copy);
            }

            private void ItemMoveDrag(object sender, ItemDragEventArgs e)
            {
                DoDragDrop((sender as ListView).SelectedItems, DragDropEffects.Move);
            }

            private void ListDragEnter(object sender, DragEventArgs e)
            {
                e.Effect = e.AllowedEffect;
            }

            private void ListDragDrop(object sender, DragEventArgs e)
            {
                Point cp = listViewChain.PointToClient(new Point(e.X, e.Y));
                ListViewItem dragToItem = listViewChain.GetItemAt(cp.X, cp.Y);
                bool after = (dragToItem != null) && listViewChain.GetItemRect(dragToItem.Index).Bottom - 8 <= cp.Y;
                var draggedItems = e.Data.GetData(typeof(ListView.SelectedListViewItemCollection)) as ListView.SelectedListViewItemCollection;
                if (draggedItems == null || draggedItems.Count == 0)
                    return;

                if (e.Effect == DragDropEffects.Copy)
                {
                    var items = draggedItems.Cast<ListViewItem>();
                    var index = dragToItem == null ? listViewChain.Items.Count : dragToItem.Index + (after ? 1 : 0);

                    AddScripts(items.Select(item => (item.Tag as IRenderChainUi)), index);
                }
                else if (e.Effect == DragDropEffects.Move)
                {
                    if (draggedItems.Contains(dragToItem))
                        return;

                    var items = new List<ListViewItem>();
                    foreach (ListViewItem item in draggedItems.Cast<ListViewItem>())
                    {
                        item.Remove();
                        items.Add(item);
                    }

                    var index = dragToItem == null ? listViewChain.Items.Count : dragToItem.Index + (after ? 1 : 0);
                    foreach (ListViewItem item in items)
                    {
                        listViewChain.Items.Insert(index, item);
                        index++;
                    }
                }

                listViewChain.Focus();
            }

            private void ListDragDropRemove(object sender, DragEventArgs e)
            {
                var draggedItems = e.Data.GetData(typeof(ListView.SelectedListViewItemCollection)) as ListView.SelectedListViewItemCollection;
                if (draggedItems == null)
                    return;

                if (e.Effect == DragDropEffects.Move)
                {
                    var items = new List<ListViewItem>();
                    foreach (ListViewItem item in draggedItems.Cast<ListViewItem>())
                        RemoveItem(item);
                }
            }

            #endregion

            /* Appearance */

            #region Appearance

            private void UpdateButtons()
            {
                buttonAdd.Enabled = listViewAvail.SelectedItems.Count > 0;
                buttonMinus.Enabled = listViewChain.SelectedItems.Count > 0;
                buttonClear.Enabled = listViewChain.Items.Count > 0;
                buttonUp.Enabled = listViewChain.SelectedItems.Count > 0 && listViewChain.SelectedItems[0].Index > 0;
                buttonDown.Enabled = listViewChain.SelectedItems.Count > 0 &&
                                     listViewChain.SelectedItems[listViewChain.SelectedItems.Count - 1].Index < listViewChain.Items.Count - 1;
                buttonConfigure.Enabled = buttonMinus.Enabled;
                
                menuAdd.Enabled = buttonAdd.Enabled;
                menuRemove.Enabled = buttonMinus.Enabled;
                menuClear.Enabled = buttonClear.Enabled;
                menuGroup.Enabled = buttonMinus.Enabled;
                menuUngroup.Enabled = buttonMinus.Enabled;

                if (listViewChain.SelectedItems.Count > 0)
                {
                    var item = listViewChain.SelectedItems[0];
                    var preset = item.Tag as Preset;
                    var chain = preset.Chain as PresetCollection;

                    buttonConfigure.Enabled = preset.HasConfigDialog();
                    menuUngroup.Enabled = chain != null && chain.AllowRegrouping;
                }

                menuConfigure.Enabled = buttonConfigure.Enabled;
            }

            private void UpdateItemText(ListViewItem item)
            {
                var preset = (Preset)item.Tag;

                while (item.SubItems.Count < 3) 
                    item.SubItems.Add(string.Empty);
                item.SubItems[1].Text = preset.Name;
                item.SubItems[2].Text = preset.Description;

                ResizeLists();
            }

            private void ResizeLists()
            {
                listViewChain.BeginUpdate();
                {
                    listViewChain.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listViewChain.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                listViewChain.EndUpdate();

                listViewAvail.BeginUpdate();
                {
                    listViewAvail.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                    listViewAvail.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                }
                listViewAvail.EndUpdate();
            }

            private void SplitterMoved(object sender, SplitterEventArgs e)
            {
                ResizeLists();
            }

            private void DialogResizeEnd(object sender, EventArgs e)
            {
                ResizeLists();
            }           

            #endregion

            /* Actions */

            #region Buttons

            private void ButtonConfigureClick(object sender, EventArgs e)
            {
                if (listViewChain.SelectedItems.Count <= 0)
                    return;

                var item = listViewChain.SelectedItems[0];
                ConfigureItem(item);
            }

            private void ButtonAddClick(object sender, EventArgs e)
            {
                foreach (ListViewItem item in listViewAvail.SelectedItems) 
                    AddScript((IRenderChainUi)item.Tag);
            }

            private void ButtonMinusClick(object sender, EventArgs e)
            {
                foreach (ListViewItem item in listViewChain.SelectedItems)
                    RemoveItem(item);
            }

            private void ButtonClearClick(object sender, EventArgs e)
            {
                while (listViewChain.Items.Count > 0)
                {
                    RemoveItem(listViewChain.Items[0]);
                }
                UpdateButtons();
            }

            private void ButtonUpClick(object sender, EventArgs e)
            {
                MoveListViewItems(listViewChain, MoveDirection.Up);
                UpdateButtons();
            }

            private void ButtonDownClick(object sender, EventArgs e)
            {
                MoveListViewItems(listViewChain, MoveDirection.Down);
                UpdateButtons();
            }

            private void PresetDialogActivated(object sender, EventArgs e)
            {
                buttonOk.Focus(); // Prevent people from accidentally removing scripts when pressing enter
            }

            #endregion

            #region Name Editing

            private void NameChanged(object sender, EventArgs e)
            {
                if (listViewChain.SelectedItems.Count > 0)
                {
                    var item = listViewChain.SelectedItems[0];
                    Preset preset = (Preset)item.Tag;

                    preset.Name = NameBox.Text;
                    UpdateItemText(item);
                }
            }

            private void NamePreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.IsInputKey = true;
                }
            }

            private void NameKeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SelectedIndex++;
                    if (SelectedIndex != -1)
                    {
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        NameBox.SelectAll();
                    }
                    else
                        AcceptButton.PerformClick();
                }
                else if (e.KeyCode == Keys.Down)
                {
                    SelectedIndex++;
                    if (SelectedIndex == -1 && listViewChain.Items.Count > 0)
                        SelectedIndex = 0;

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    NameBox.SelectAll();
                }
                else if (e.KeyCode == Keys.Up)
                {
                    SelectedIndex--;
                    if (SelectedIndex == -1)
                        SelectedIndex = listViewChain.Items.Count - 1;

                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    NameBox.SelectAll();
                }
            }

            #endregion

            #region (Un)Grouping

            private void MenuGroupItemClicked(object sender, ToolStripItemClickedEventArgs e)
            {
                var script = (IRenderChainUi)e.ClickedItem.Tag;
                var grouppreset = script.MakeNewPreset();
                var group = (PresetCollection)grouppreset.Chain;
                int index = (listViewChain.SelectedItems.Count > 0)
                    ? listViewChain.SelectedItems[0].Index
                    : -1;

                foreach (ListViewItem item in listViewChain.SelectedItems)
                {
                    var preset = (Preset)item.Tag;
                    group.Options.Add(preset);
                    RemoveItem(item);
                }
                AddPreset(grouppreset, index);
                listViewChain.SelectedIndices.Clear();
                listViewChain.SelectedIndices.Add(index);
            }

            private void MenuUngroupClicked(object sender, EventArgs e)
            {
                if (listViewChain.SelectedItems.Count == 1)
                {
                    var item = listViewChain.SelectedItems[0];
                    var preset = (Preset)item.Tag;
                    var group = preset.Chain as PresetCollection;
                    var index = item.Index;
                    
                    if (group == null || !group.AllowRegrouping) 
                        return;

                    RemoveItem(item);
                    AddPresets(group.Options, index);
                }
            }

            #endregion

            #region Configure Menu

            private void MenuChainOpening(object sender, System.ComponentModel.CancelEventArgs e)
            {
                var menuitem = (ToolStripMenuItem)menuChain.Items
                    .Find("menuConfigure", false).First();

                if (listViewChain.SelectedItems.Count <= 0)
                    return;

                var item = listViewChain.SelectedItems[0];
                var preset = (Preset)item.Tag;
                AddSubItems(menuitem, preset);
            }

            private void AddSubItems(ToolStripMenuItem menuitem, Preset preset)
            {
                menuitem.DropDownItemClicked -= menuConfigureItemClicked;
                menuitem.DropDownItems.Clear();
                menuitem.DropDownItemClicked += menuConfigureItemClicked;
                var presetCollection = preset.Chain as PresetCollection;
                if (presetCollection != null)
                {
                    var options = presetCollection.Options;

                    foreach (var option in options)
                    {
                        var item = (ToolStripMenuItem)menuitem.DropDownItems.Add(option.Name);
                        item.Tag = option;
                        AddSubItems(item, option);
                    }
                }
            }

            private void menuConfigureItemClicked(object sender, ToolStripItemClickedEventArgs e)
            {
                var preset = (Preset)e.ClickedItem.Tag;
                if (preset.HasConfigDialog()
                    && preset.ShowConfigDialog(Owner) 
                    && listViewChain.SelectedItems.Count > 0)
                    UpdateItemText(listViewChain.SelectedItems[0]);
            }

            #endregion

        }

        public class PresetDialogBase : ScriptConfigDialog<PresetCollection>
        {
        }
    }
}
