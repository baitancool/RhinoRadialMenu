using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RadialMenu
{
    public class SettingsForm : Form
    {
        private RadialMenuSettings _settings;

        private NumericUpDown numLayerCount;
        private TrackBar trackBgAlpha;
        private Button btnBgColor;
        private Button btnHoverColor;
        private Panel previewBg;
        private Panel previewHover;
        private Label lblBgAlpha;
        private Label lblInfo;

        public SettingsForm(RadialMenuSettings settings)
        {
            _settings = settings;
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            Text = "言覃犀牛快捷轮盘设置";
            Size = new Size(380, 380);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            BackColor = Color.FromArgb(45, 45, 48);
            ForeColor = Color.White;

            int y = 20;

            // 层数设置
            AddLabel("命令层数:", 20, y);
            numLayerCount = new NumericUpDown { Location = new Point(100, y - 3), Width = 60, Minimum = 1, Maximum = 3, BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White };
            numLayerCount.ValueChanged += (s, e) => UpdateInfo();
            Controls.Add(numLayerCount);

            lblInfo = new Label { Location = new Point(170, y), AutoSize = true, ForeColor = Color.FromArgb(180, 180, 180) };
            Controls.Add(lblInfo);

            y += 45;

            // 背景透明度
            AddLabel("背景透明度:", 20, y);
            trackBgAlpha = new TrackBar { Location = new Point(100, y - 5), Width = 160, Minimum = 50, Maximum = 255, TickFrequency = 25 };
            trackBgAlpha.ValueChanged += (s, e) => { lblBgAlpha.Text = trackBgAlpha.Value.ToString(); };
            lblBgAlpha = new Label { Location = new Point(270, y), AutoSize = true, Text = "140" };
            Controls.Add(trackBgAlpha);
            Controls.Add(lblBgAlpha);

            y += 55;

            // 背景颜色
            AddLabel("背景颜色:", 20, y);
            previewBg = new Panel { Location = new Point(100, y - 2), Size = new Size(40, 24), BorderStyle = BorderStyle.FixedSingle };
            btnBgColor = new Button { Text = "选择", Location = new Point(150, y - 4), Width = 60, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };
            btnBgColor.Click += (s, e) => PickColor(previewBg);
            Controls.Add(previewBg);
            Controls.Add(btnBgColor);

            y += 40;

            // 悬停颜色
            AddLabel("悬停颜色:", 20, y);
            previewHover = new Panel { Location = new Point(100, y - 2), Size = new Size(40, 24), BorderStyle = BorderStyle.FixedSingle };
            btnHoverColor = new Button { Text = "选择", Location = new Point(150, y - 4), Width = 60, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };
            btnHoverColor.Click += (s, e) => PickColor(previewHover);
            Controls.Add(previewHover);
            Controls.Add(btnHoverColor);

            y += 45;

            // 导入/导出配置
            AddLabel("命令配置:", 20, y);
            var btnExport = new Button { Text = "导出", Location = new Point(100, y - 4), Width = 70, Height = 26, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };
            var btnImport = new Button { Text = "导入", Location = new Point(180, y - 4), Width = 70, Height = 26, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };
            btnExport.Click += (s, e) => ExportConfig();
            btnImport.Click += (s, e) => ImportConfig();
            Controls.Add(btnExport);
            Controls.Add(btnImport);

            y += 40;

            // 加入QQ群按钮
            var btnQQGroup = new Button { Text = "加入QQ交流群", Location = new Point(100, y - 4), Width = 150, Height = 26, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(50, 150, 80) };
            btnQQGroup.Click += (s, e) => JoinQQGroup();
            Controls.Add(btnQQGroup);

            y += 50;

            // 确定/取消按钮
            var btnOk = new Button { Text = "确定", Location = new Point(90, y), Width = 90, Height = 32, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(74, 144, 217) };
            var btnCancel = new Button { Text = "取消", Location = new Point(200, y), Width = 90, Height = 32, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };
            btnOk.Click += (s, e) => SaveSettings();
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private void ExportConfig()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "导出轮盘命令配置";
                sfd.Filter = "轮盘配置文件 (*.rmcfg)|*.rmcfg|XML文件 (*.xml)|*.xml";
                sfd.DefaultExt = "rmcfg";
                sfd.FileName = "RadialMenu_Commands";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _settings.ExportCommands(sfd.FileName);
                        MessageBox.Show("配置导出成功！\n\n文件位置：" + sfd.FileName, "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ImportConfig()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "导入轮盘命令配置";
                ofd.Filter = "轮盘配置文件 (*.rmcfg)|*.rmcfg|XML文件 (*.xml)|*.xml|所有文件 (*.*)|*.*";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _settings.ImportCommands(ofd.FileName);
                        LoadSettings(); // 刷新界面
                        MessageBox.Show("配置导入成功！\n\n已加载命令配置，点击确定保存。", "导入成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("导入失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddLabel(string text, int x, int y)
        {
            Controls.Add(new Label { Text = text, Location = new Point(x, y), AutoSize = true });
        }

        private void UpdateInfo()
        {
            int layers = (int)numLayerCount.Value;
            int total = 0;
            for (int i = 1; i <= layers; i++) total += 8 * (i + 1);
            lblInfo.Text = string.Format("共{0}个命令位", total);
        }

        private void LoadSettings()
        {
            numLayerCount.Value = _settings.LayerCount;
            trackBgAlpha.Value = _settings.BackgroundAlpha;
            lblBgAlpha.Text = _settings.BackgroundAlpha.ToString();
            previewBg.BackColor = Color.FromArgb(_settings.BackgroundR, _settings.BackgroundG, _settings.BackgroundB);
            previewHover.BackColor = Color.FromArgb(_settings.HoverR, _settings.HoverG, _settings.HoverB);
            UpdateInfo();
        }

        private void SaveSettings()
        {
            _settings.LayerCount = (int)numLayerCount.Value;
            _settings.BackgroundAlpha = trackBgAlpha.Value;
            _settings.BackgroundR = previewBg.BackColor.R;
            _settings.BackgroundG = previewBg.BackColor.G;
            _settings.BackgroundB = previewBg.BackColor.B;
            _settings.HoverR = previewHover.BackColor.R;
            _settings.HoverG = previewHover.BackColor.G;
            _settings.HoverB = previewHover.BackColor.B;
            _settings.Save();
        }

        private void PickColor(Panel preview)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = preview.BackColor;
                cd.FullOpen = true;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    preview.BackColor = cd.Color;
                }
            }
        }

        private void JoinQQGroup()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://qm.qq.com/q/jKUHVNsT2U",
                    UseShellExecute = true
                });
            }
            catch
            {
                MessageBox.Show("请手动访问：https://qm.qq.com/q/jKUHVNsT2U\n\n或搜索QQ群加入【言覃设计插件交流群】", "加入QQ群", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
