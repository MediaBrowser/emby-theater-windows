using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Mpdn.Config;

namespace Mpdn.RenderScript
{
    namespace Mpdn.ImageProcessor
    {
        public partial class ImageProcessorConfigDialog : ImageProcessorConfigDialogBase
        {
            private string m_ShaderPath;

            public ImageProcessorConfigDialog()
            {
                InitializeComponent();

                var descs = EnumHelpers.GetDescriptions<ImageProcessorUsage>();
                foreach (var desc in descs)
                {
                    comboBoxUsage.Items.Add(desc);
                }

                comboBoxUsage.SelectedIndex = 0;
            }

            protected override void LoadSettings()
            {
                m_ShaderPath = Settings.FullShaderPath;

                Add(Settings.ShaderFileNames);

                if (listBox.Items.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                comboBoxUsage.SelectedIndex = (int) Settings.ImageProcessorUsage;
            }

            protected override void SaveSettings()
            {
                Settings.ShaderFileNames = listBox.Items.Cast<string>().ToArray();
                Settings.ImageProcessorUsage = (ImageProcessorUsage)comboBoxUsage.SelectedIndex;
            }

            private void ButtonAddClick(object sender, EventArgs e)
            {
                openFileDialog.InitialDirectory = m_ShaderPath;
                if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                Add(openFileDialog.FileNames);
                UpdateButtons();
            }

            private void ButtonRemoveClick(object sender, EventArgs e)
            {
                RemoveItem();
                UpdateButtons();
            }

            private void ButtonClearClick(object sender, EventArgs e)
            {
                listBox.Items.Clear();
                UpdateButtons();
            }

            private void ButtonUpClick(object sender, EventArgs e)
            {
                MoveItem((int) Direction.Up);
                UpdateButtons();
            }

            private void ButtonDownClick(object sender, EventArgs e)
            {
                MoveItem((int) Direction.Down);
                UpdateButtons();
            }

            private void RemoveItem()
            {
                var index = listBox.SelectedIndex;
                listBox.Items.RemoveAt(index);
                listBox.SelectedIndex = index < listBox.Items.Count ? index : listBox.Items.Count - 1;
            }

            private void MoveItem(int direction)
            {
                var index = listBox.SelectedIndex;
                var item = listBox.Items[index];
                listBox.Items.RemoveAt(index);
                listBox.Items.Insert(index + direction, item);
                listBox.SelectedIndex = index + direction;
            }

            private void Add(IEnumerable<string> fileNames)
            {
                foreach (var fileName in fileNames)
                {
                    listBox.Items.Add(GetRelativePath(m_ShaderPath, fileName));
                }
            }

            private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
            {
                UpdateButtons();
            }

            private void UpdateButtons()
            {
                var index = listBox.SelectedIndex;
                var count = listBox.Items.Count;

                buttonRemove.Enabled = index >= 0;
                buttonUp.Enabled = index > 0;
                buttonDown.Enabled = index >= 0 && index < count - 1;
                buttonClear.Enabled = count > 0;
            }

            private static string GetRelativePath(string rootPath, string filename)
            {
                if (!Path.IsPathRooted(filename))
                    return filename;

                if (!filename.StartsWith(rootPath))
                    throw new InvalidOperationException("Unable to include an external shader file");

                return filename.Remove(0, rootPath.Length + 1);
            }

            private enum Direction
            {
                Up = -1,
                Down = 1
            }
        }

        public class ImageProcessorConfigDialogBase : ScriptConfigDialog<ImageProcessor>
        {
        }
    }
}