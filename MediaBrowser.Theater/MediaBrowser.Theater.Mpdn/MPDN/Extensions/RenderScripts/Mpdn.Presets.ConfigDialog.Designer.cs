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
namespace Mpdn.RenderScript
{
    namespace Mpdn.Presets
    {
        partial class PresetDialog
        {
            /// <summary> 
            /// Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary> 
            /// Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (components != null))
                {
                    components.Dispose();
                }
            }

            #region Component Designer generated code

            /// <summary> 
            /// Required method for Designer support - do not modify 
            /// the contents of this method with the code editor.
            /// </summary>
            private void InitializeComponent()
            {
            this.components = new System.ComponentModel.Container();
            this.listViewAvail = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuAvail = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.buttonConfigure = new System.Windows.Forms.Button();
            this.listViewChain = new System.Windows.Forms.ListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuChain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuConfigure = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.menuClear = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuGroup = new System.Windows.Forms.ToolStripMenuItem();
            this.menuUngroup = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonMinus = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonUp = new System.Windows.Forms.Button();
            this.buttonDown = new System.Windows.Forms.Button();
            this.panelReorder = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.NameLable = new System.Windows.Forms.Label();
            this.NameBox = new System.Windows.Forms.TextBox();
            this.menuAvail.SuspendLayout();
            this.menuChain.SuspendLayout();
            this.panelReorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewAvail
            // 
            this.listViewAvail.AllowDrop = true;
            this.listViewAvail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewAvail.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader1,
            this.columnHeader2});
            this.listViewAvail.ContextMenuStrip = this.menuAvail;
            this.listViewAvail.FullRowSelect = true;
            this.listViewAvail.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewAvail.Location = new System.Drawing.Point(3, 3);
            this.listViewAvail.Name = "listViewAvail";
            this.listViewAvail.Size = new System.Drawing.Size(397, 459);
            this.listViewAvail.TabIndex = 0;
            this.listViewAvail.UseCompatibleStateImageBehavior = false;
            this.listViewAvail.View = System.Windows.Forms.View.Details;
            this.listViewAvail.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ItemCopyDrag);
            this.listViewAvail.SelectedIndexChanged += new System.EventHandler(this.ListViewSelectedIndexChanged);
            this.listViewAvail.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListDragDropRemove);
            this.listViewAvail.DragEnter += new System.Windows.Forms.DragEventHandler(this.ListDragEnter);
            this.listViewAvail.DoubleClick += new System.EventHandler(this.ButtonAddClick);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader3.Width = 20;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Available Scripts";
            this.columnHeader1.Width = 136;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 271;
            // 
            // menuAvail
            // 
            this.menuAvail.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAdd});
            this.menuAvail.Name = "contextMenuStrip2";
            this.menuAvail.Size = new System.Drawing.Size(97, 26);
            // 
            // menuAdd
            // 
            this.menuAdd.Name = "menuAdd";
            this.menuAdd.Size = new System.Drawing.Size(96, 22);
            this.menuAdd.Text = "Add";
            this.menuAdd.Click += new System.EventHandler(this.ButtonAddClick);
            // 
            // labelCopyright
            // 
            this.labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCopyright.Location = new System.Drawing.Point(36, 467);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(201, 24);
            this.labelCopyright.TabIndex = 2;
            this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonConfigure
            // 
            this.buttonConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonConfigure.Enabled = false;
            this.buttonConfigure.Location = new System.Drawing.Point(3, 468);
            this.buttonConfigure.Name = "buttonConfigure";
            this.buttonConfigure.Size = new System.Drawing.Size(75, 23);
            this.buttonConfigure.TabIndex = 5;
            this.buttonConfigure.Text = "Configure...";
            this.buttonConfigure.UseVisualStyleBackColor = true;
            this.buttonConfigure.Click += new System.EventHandler(this.ButtonConfigureClick);
            // 
            // listViewChain
            // 
            this.listViewChain.AllowDrop = true;
            this.listViewChain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewChain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6});
            this.listViewChain.ContextMenuStrip = this.menuChain;
            this.listViewChain.FullRowSelect = true;
            this.listViewChain.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewChain.Location = new System.Drawing.Point(3, 3);
            this.listViewChain.Name = "listViewChain";
            this.listViewChain.Size = new System.Drawing.Size(394, 459);
            this.listViewChain.TabIndex = 4;
            this.listViewChain.UseCompatibleStateImageBehavior = false;
            this.listViewChain.View = System.Windows.Forms.View.Details;
            this.listViewChain.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ItemMoveDrag);
            this.listViewChain.SelectedIndexChanged += new System.EventHandler(this.ListViewChainSelectedIndexChanged);
            this.listViewChain.DragDrop += new System.Windows.Forms.DragEventHandler(this.ListDragDrop);
            this.listViewChain.DragEnter += new System.Windows.Forms.DragEventHandler(this.ListDragEnter);
            this.listViewChain.DoubleClick += new System.EventHandler(this.ButtonConfigureClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader4.Width = 20;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Script Chain";
            this.columnHeader5.Width = 136;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Description";
            this.columnHeader6.Width = 271;
            // 
            // menuChain
            // 
            this.menuChain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuConfigure,
            this.toolStripSeparator1,
            this.menuRemove,
            this.menuClear,
            this.menuSelectAll,
            this.toolStripSeparator2,
            this.menuGroup,
            this.menuUngroup});
            this.menuChain.Name = "contextMenuStrip1";
            this.menuChain.Size = new System.Drawing.Size(165, 170);
            this.menuChain.Opening += new System.ComponentModel.CancelEventHandler(this.MenuChainOpening);
            // 
            // menuConfigure
            // 
            this.menuConfigure.Name = "menuConfigure";
            this.menuConfigure.Size = new System.Drawing.Size(164, 22);
            this.menuConfigure.Text = "Configure...";
            this.menuConfigure.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuConfigureItemClicked);
            this.menuConfigure.Click += new System.EventHandler(this.ButtonConfigureClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(161, 6);
            // 
            // menuRemove
            // 
            this.menuRemove.Name = "menuRemove";
            this.menuRemove.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.menuRemove.Size = new System.Drawing.Size(164, 22);
            this.menuRemove.Text = "Remove";
            this.menuRemove.Click += new System.EventHandler(this.ButtonMinusClick);
            // 
            // menuClear
            // 
            this.menuClear.Name = "menuClear";
            this.menuClear.Size = new System.Drawing.Size(164, 22);
            this.menuClear.Text = "Clear";
            this.menuClear.Click += new System.EventHandler(this.ButtonClearClick);
            // 
            // menuSelectAll
            // 
            this.menuSelectAll.Name = "menuSelectAll";
            this.menuSelectAll.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.menuSelectAll.Size = new System.Drawing.Size(164, 22);
            this.menuSelectAll.Text = "Select All";
            this.menuSelectAll.Click += new System.EventHandler(this.SelectAll);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(161, 6);
            // 
            // menuGroup
            // 
            this.menuGroup.Name = "menuGroup";
            this.menuGroup.Size = new System.Drawing.Size(164, 22);
            this.menuGroup.Text = "Group";
            this.menuGroup.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.MenuGroupItemClicked);
            // 
            // menuUngroup
            // 
            this.menuUngroup.Name = "menuUngroup";
            this.menuUngroup.Size = new System.Drawing.Size(164, 22);
            this.menuUngroup.Text = "Ungroup";
            this.menuUngroup.Click += new System.EventHandler(this.MenuUngroupClicked);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdd.Enabled = false;
            this.buttonAdd.Location = new System.Drawing.Point(3, 468);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(27, 24);
            this.buttonAdd.TabIndex = 1;
            this.buttonAdd.Text = "+";
            this.toolTip1.SetToolTip(this.buttonAdd, "Add");
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
            // 
            // buttonMinus
            // 
            this.buttonMinus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMinus.Enabled = false;
            this.buttonMinus.Location = new System.Drawing.Point(370, 468);
            this.buttonMinus.Name = "buttonMinus";
            this.buttonMinus.Size = new System.Drawing.Size(27, 24);
            this.buttonMinus.TabIndex = 2;
            this.buttonMinus.Text = "-";
            this.toolTip1.SetToolTip(this.buttonMinus, "Remove");
            this.buttonMinus.UseVisualStyleBackColor = true;
            this.buttonMinus.Click += new System.EventHandler(this.ButtonMinusClick);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Enabled = false;
            this.buttonClear.Location = new System.Drawing.Point(337, 468);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(27, 24);
            this.buttonClear.TabIndex = 3;
            this.buttonClear.Text = "c";
            this.toolTip1.SetToolTip(this.buttonClear, "Clear");
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.ButtonClearClick);
            // 
            // buttonUp
            // 
            this.buttonUp.Dock = System.Windows.Forms.DockStyle.Left;
            this.buttonUp.Enabled = false;
            this.buttonUp.FlatAppearance.BorderSize = 0;
            this.buttonUp.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ControlLightLight;
            this.buttonUp.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUp.Location = new System.Drawing.Point(0, 0);
            this.buttonUp.Name = "buttonUp";
            this.buttonUp.Size = new System.Drawing.Size(24, 24);
            this.buttonUp.TabIndex = 0;
            this.buttonUp.Text = "▲";
            this.toolTip1.SetToolTip(this.buttonUp, "Move up");
            this.buttonUp.UseVisualStyleBackColor = true;
            this.buttonUp.Click += new System.EventHandler(this.ButtonUpClick);
            // 
            // buttonDown
            // 
            this.buttonDown.Dock = System.Windows.Forms.DockStyle.Right;
            this.buttonDown.Enabled = false;
            this.buttonDown.FlatAppearance.BorderSize = 0;
            this.buttonDown.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ControlLightLight;
            this.buttonDown.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.buttonDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDown.Location = new System.Drawing.Point(24, 0);
            this.buttonDown.Name = "buttonDown";
            this.buttonDown.Size = new System.Drawing.Size(24, 24);
            this.buttonDown.TabIndex = 1;
            this.buttonDown.Text = "▼";
            this.toolTip1.SetToolTip(this.buttonDown, "Move down");
            this.buttonDown.UseVisualStyleBackColor = true;
            this.buttonDown.Click += new System.EventHandler(this.ButtonDownClick);
            // 
            // panelReorder
            // 
            this.panelReorder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelReorder.Controls.Add(this.buttonUp);
            this.panelReorder.Controls.Add(this.buttonDown);
            this.panelReorder.Location = new System.Drawing.Point(84, 468);
            this.panelReorder.Name = "panelReorder";
            this.panelReorder.Size = new System.Drawing.Size(48, 24);
            this.panelReorder.TabIndex = 6;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(324, 468);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(243, 468);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 7;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(18, 18);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.splitter1);
            this.splitContainer.Panel1.Controls.Add(this.buttonMinus);
            this.splitContainer.Panel1.Controls.Add(this.buttonClear);
            this.splitContainer.Panel1.Controls.Add(this.listViewChain);
            this.splitContainer.Panel1.Controls.Add(this.buttonConfigure);
            this.splitContainer.Panel1.Controls.Add(this.NameLable);
            this.splitContainer.Panel1.Controls.Add(this.NameBox);
            this.splitContainer.Panel1.Controls.Add(this.panelReorder);
            this.splitContainer.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.listViewAvail);
            this.splitContainer.Panel2.Controls.Add(this.buttonAdd);
            this.splitContainer.Panel2.Controls.Add(this.buttonCancel);
            this.splitContainer.Panel2.Controls.Add(this.labelCopyright);
            this.splitContainer.Panel2.Controls.Add(this.buttonOk);
            this.splitContainer.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer.Size = new System.Drawing.Size(807, 495);
            this.splitContainer.SplitterDistance = 400;
            this.splitContainer.TabIndex = 9;
            this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitterMoved);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 495);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;
            // 
            // NameLable
            // 
            this.NameLable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NameLable.AutoSize = true;
            this.NameLable.Location = new System.Drawing.Point(138, 474);
            this.NameLable.Name = "NameLable";
            this.NameLable.Size = new System.Drawing.Size(35, 13);
            this.NameLable.TabIndex = 11;
            this.NameLable.Text = "Name";
            // 
            // NameBox
            // 
            this.NameBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NameBox.Location = new System.Drawing.Point(179, 470);
            this.NameBox.Name = "NameBox";
            this.NameBox.Size = new System.Drawing.Size(152, 20);
            this.NameBox.TabIndex = 10;
            this.NameBox.TextChanged += new System.EventHandler(this.NameChanged);
            this.NameBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NameKeyDown);
            this.NameBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.NamePreviewKeyDown);
            // 
            // PresetDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(843, 531);
            this.Controls.Add(this.splitContainer);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 460);
            this.Name = "PresetDialog";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "<INSERT TITLE>";
            this.Activated += new System.EventHandler(this.PresetDialogActivated);
            this.ResizeEnd += new System.EventHandler(this.DialogResizeEnd);
            this.menuAvail.ResumeLayout(false);
            this.menuChain.ResumeLayout(false);
            this.panelReorder.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

            }

            #endregion

            private System.Windows.Forms.ListView listViewAvail;
            private System.Windows.Forms.ColumnHeader columnHeader1;
            private System.Windows.Forms.ColumnHeader columnHeader2;
            private System.Windows.Forms.Label labelCopyright;
            private System.Windows.Forms.Button buttonConfigure;
            private System.Windows.Forms.ColumnHeader columnHeader3;
            private System.Windows.Forms.Button buttonMinus;
            private System.Windows.Forms.Button buttonAdd;
            private System.Windows.Forms.ListView listViewChain;
            private System.Windows.Forms.ColumnHeader columnHeader4;
            private System.Windows.Forms.ColumnHeader columnHeader5;
            private System.Windows.Forms.ColumnHeader columnHeader6;
            private System.Windows.Forms.ToolTip toolTip1;
            private System.Windows.Forms.Button buttonClear;
            private System.Windows.Forms.Panel panelReorder;
            private System.Windows.Forms.Button buttonUp;
            private System.Windows.Forms.Button buttonDown;
            private System.Windows.Forms.ToolStripMenuItem menuConfigure;
            private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
            private System.Windows.Forms.ToolStripMenuItem menuRemove;
            private System.Windows.Forms.ToolStripMenuItem menuClear;
            private System.Windows.Forms.ContextMenuStrip menuAvail;
            private System.Windows.Forms.ToolStripMenuItem menuAdd;
            private System.Windows.Forms.Button buttonCancel;
            private System.Windows.Forms.Button buttonOk;
            private System.Windows.Forms.ToolStripMenuItem menuSelectAll;
            private System.Windows.Forms.SplitContainer splitContainer;
            private System.Windows.Forms.Splitter splitter1;
            private System.Windows.Forms.TextBox NameBox;
            private System.Windows.Forms.Label NameLable;
            private System.Windows.Forms.ToolStripMenuItem menuGroup;
            private System.Windows.Forms.ToolStripMenuItem menuUngroup;
            private System.Windows.Forms.ContextMenuStrip menuChain;
            private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        }
    }
}  
