using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ElievPlugInNO1NO2
{
    public class FamilyListForm : Form
    {
        private readonly ListBox _listBox;
        private readonly Button _okButton;
        private readonly Button _cancelButton;

        public string SelectedFamilyPath { get; private set; }

        public FamilyListForm(string folderPath, List<string> familyFullPaths)
        {
            Text = "בחר משפחה מתוך התיקייה";
            Width = 600;
            Height = 450;
            StartPosition = FormStartPosition.CenterScreen;

            var titleLabel = new Label
            {
                Text = $"תיקייה: {folderPath}",
                Dock = DockStyle.Top,
                Height = 30
            };

            _listBox = new ListBox
            {
                Dock = DockStyle.Fill
            };

            var items = familyFullPaths
                .Select(p => new FamilyItem(Path.GetFileName(p), p))
                .ToList();

            _listBox.DataSource = items;
            _listBox.DisplayMember = nameof(FamilyItem.DisplayName);

            _listBox.DoubleClick += (s, e) => ConfirmSelection();

            _okButton = new Button
            {
                Text = "OK",
                Width = 100
            };
            _okButton.Click += (s, e) => ConfirmSelection();

            _cancelButton = new Button
            {
                Text = "Cancel",
                Width = 100,
                DialogResult = DialogResult.Cancel
            };

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft
            };

            panel.Controls.Add(_cancelButton);
            panel.Controls.Add(_okButton);

            Controls.Add(_listBox);
            Controls.Add(panel);
            Controls.Add(titleLabel);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void ConfirmSelection()
        {
            var selected = _listBox.SelectedItem as FamilyItem;
            if (selected == null)
            {
                MessageBox.Show("בחר פריט מהרשימה.");
                return;
            }

            SelectedFamilyPath = selected.FullPath;
            DialogResult = DialogResult.OK;
            Close();
        }

        private class FamilyItem
        {
            public string DisplayName { get; }
            public string FullPath { get; }

            public FamilyItem(string displayName, string fullPath)
            {
                DisplayName = displayName;
                FullPath = fullPath;
            }
        }
    }
}
