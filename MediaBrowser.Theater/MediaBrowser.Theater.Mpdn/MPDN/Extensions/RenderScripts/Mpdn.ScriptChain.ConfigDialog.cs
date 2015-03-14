using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Mpdn.ScriptChain
    {
        public partial class ScriptChainDialog : ScriptChainDialogBase
        {
            private const string SELECTED_INDICATOR_STR = "➔";

            public ScriptChainDialog()
            {
                InitializeComponent();

                var renderScripts = PlayerControl.RenderScripts
                    .Where(script => script is IRenderChainUi)
                    .Select(x => (x as IRenderChainUi).CreateNew())
                    .OrderBy(x => x.Descriptor.Name);

                foreach (var script in renderScripts)
                {
                    var item = listViewAvail.Items.Add(string.Empty);
                    item.SubItems.Add(script.Descriptor.Name);
                    item.SubItems.Add(script.Descriptor.Description);
                    item.Tag = script;
                }

                if (listViewAvail.Items.Count > 0)
                {
                    var firstItem = listViewAvail.Items[0];
                    firstItem.Text = SELECTED_INDICATOR_STR;
                    firstItem.Selected = true;
                }

                listViewAvail.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewAvail.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listViewChain.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listViewChain.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                for (var i = 0; i < listViewChain.Columns.Count; i++)
                {
                    var col = listViewAvail.Columns[i];
                    listViewChain.Columns[i].Width = col.Width;
                }

                UpdateButtons();
            }

            protected override void LoadSettings()
            {
                AddScripts(Settings.ScriptList);
                UpdateButtons();
            }

            protected override void SaveSettings()
            {
                var scripts = from item in listViewChain.Items.Cast<ListViewItem>()
                    select (IRenderChainUi) item.Tag;
                Settings.ScriptList = scripts.ToList();
            }

            private void AddScripts(IEnumerable<IRenderChainUi> renderScripts)
            {
                foreach (var script in renderScripts)
                {
                    var item = listViewChain.Items.Add(string.Empty);
                    item.SubItems.Add(script.Descriptor.Name);
                    item.SubItems.Add(script.Descriptor.Description);
                    item.Tag = script;
                }
            }

            private void RemoveScript(ListViewItem selectedItem)
            {
                var renderScript = (IRenderChainUi) selectedItem.Tag;

                renderScript.Destroy();

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
                UpdateButtons();
            }

            private void ListViewSelectedIndexChanged(object sender, EventArgs e)
            {
                foreach (ListViewItem i in listViewAvail.Items)
                {
                    i.Text = string.Empty;
                }

                UpdateButtons();

                if (listViewAvail.SelectedItems.Count <= 0)
                    return;

                var item = listViewAvail.SelectedItems[0];

                item.Text = SELECTED_INDICATOR_STR;

                var script = (IRenderChainUi) item.Tag;
                labelCopyright.Text = script == null ? string.Empty : script.Descriptor.Copyright;
            }

            private void UpdateButtons()
            {
                buttonAdd.Enabled = listViewAvail.SelectedItems.Count > 0;
                buttonMinus.Enabled = listViewChain.SelectedItems.Count > 0;
                buttonClear.Enabled = listViewChain.Items.Count > 0;
                panelReorder.Visible = listViewChain.Items.Count > 0;
                buttonUp.Enabled = listViewChain.SelectedItems.Count > 0 && listViewChain.SelectedItems[0].Index > 0;
                buttonDown.Enabled = listViewChain.SelectedItems.Count > 0 &&
                                     listViewChain.SelectedItems[0].Index < listViewChain.Items.Count - 1;
                buttonConfigure.Enabled = buttonMinus.Enabled && (listViewChain.SelectedItems[0].Tag as IRenderScriptUi).HasConfigDialog();

                menuAdd.Enabled = buttonAdd.Enabled;
                menuRemove.Enabled = buttonMinus.Enabled;
                menuClear.Enabled = buttonClear.Enabled;
                menuConfigure.Enabled = buttonConfigure.Enabled;
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
                    item.Text = SELECTED_INDICATOR_STR;

                    var s = (IRenderChainUi) item.Tag;
                    buttonConfigure.Enabled = s != null && s.HasConfigDialog();
                }

                UpdateButtons();
            }

            private void ButtonConfigureClick(object sender, EventArgs e)
            {
                if (listViewChain.SelectedItems.Count <= 0)
                    return;

                var item = listViewChain.SelectedItems[0];
                var script = (IRenderChainUi) item.Tag;
                if (script.HasConfigDialog() && script.ShowConfigDialog(Owner))
                    UpdateItemText(item, script);
            }

            private void ButtonAddClick(object sender, EventArgs e)
            {
                if (listViewAvail.SelectedItems.Count == 0)
                    return;

                AddScript(listViewAvail.SelectedItems[0]);
            }

            private void ButtonMinusClick(object sender, EventArgs e)
            {
                if (listViewChain.SelectedItems.Count == 0)
                    return;

                RemoveScript(listViewChain.SelectedItems[0]);
            }

            private void ButtonClearClick(object sender, EventArgs e)
            {
                while (listViewChain.Items.Count > 0)
                {
                    RemoveScript(listViewChain.Items[0]);
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

            private void AddScript(ListViewItem selectedItem)
            {
                var item = (ListViewItem) selectedItem.Clone();
                item.Text = string.Empty;

                var scriptRenderer = (IRenderChainUi) item.Tag;
                var renderScript = scriptRenderer.CreateNew();

                item.Tag = renderScript;
                UpdateItemText(item, renderScript);
                listViewChain.Items.Add(item);
                UpdateButtons();
            }

            private static void UpdateItemText(ListViewItem item, IRenderChainUi renderScript)
            {
                item.SubItems[1].Text = renderScript.Descriptor.Name;
                item.SubItems[2].Text = renderScript.Descriptor.Description;
            }

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

            private enum MoveDirection
            {
                Up = -1,
                Down = 1
            };
        }

        public class ScriptChainDialogBase : ScriptConfigDialog<ScriptChain>
        {
        }
    }
}