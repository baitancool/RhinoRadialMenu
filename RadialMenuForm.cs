using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rhino;

namespace RadialMenu
{
    public class RadialMenuForm : Form
    {
        #region Win32 API
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int ULW_ALPHA = 0x02;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x, y; }
        [StructLayout(LayoutKind.Sequential)]
        private struct SIZE { public int cx, cy; }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BLENDFUNCTION { public byte BlendOp, BlendFlags, SourceConstantAlpha, AlphaFormat; }
        #endregion

        private RadialMenuSettings _settings;
        private Image _logoImage;

        private Point _center;
        private int _hoveredCategory = -1;
        private int _hoveredLayer = -1;
        private int _hoveredIndex = -1;

        private string _selectedCommand = null;
        private bool _showingSettings = false;

        // 命令选择事件
        public event Action<string> CommandSelected;

        private DateTime _lastCenterClickTime = DateTime.MinValue;
        private DateTime _lastClickTime = DateTime.MinValue;
        private int _lastClickCategory = -1;
        private int _lastClickLayer = -1;
        private int _lastClickIndex = -1;

        // 延迟执行定时器
        private System.Windows.Forms.Timer _clickTimer;
        private int _pendingLayer = -1;
        private int _pendingCategory = -1;
        private int _pendingIndex = -1;

        public RadialMenuForm(Point screenPosition)
        {
            _settings = RadialMenuSettings.Load();
            LoadLogo();

            // 初始化定时器 - 350ms后执行命令
            _clickTimer = new System.Windows.Forms.Timer();
            _clickTimer.Interval = 350;
            _clickTimer.Tick += OnClickTimerTick;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;

            int outerRadius = GetOuterRadius();
            int size = outerRadius * 2 + 60;
            Size = new Size(size, size);
            _center = new Point(size / 2, size / 2);
            Location = new Point(screenPosition.X - size / 2, screenPosition.Y - size / 2);

            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
            Deactivate += (s, e) => { if (!_showingSettings) Close(); };
        }



        private int GetOuterRadius()
        {
            return _settings.InnerRadius + _settings.CategoryRingWidth + _settings.LayerWidth * _settings.LayerCount;
        }

        private void LoadLogo()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream("RadialMenu.logo.png");
                if (stream != null)
                {
                    _logoImage = Image.FromStream(stream);
                    return;
                }
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.ToLower().Contains("logo"))
                    {
                        using (var s = assembly.GetManifestResourceStream(name))
                        {
                            if (s != null) { _logoImage = Image.FromStream(s); return; }
                        }
                    }
                }
            }
            catch { }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80000; // WS_EX_LAYERED
                return cp;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            UpdateWindow();
        }

        private void UpdateWindow()
        {
            using (Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    g.Clear(Color.Transparent);
                    DrawMenu(g);
                }
                SetBitmap(bmp);
            }
        }

        private void DrawMenu(Graphics g)
        {
            Color bgColor = _settings.BackgroundColor;
            Color hoverColor = _settings.HoverColor;
            Color centerColor = _settings.CenterColor;

            int innerR = _settings.InnerRadius;
            int catR = innerR + _settings.CategoryRingWidth;
            float sectorAngle = 45f;

            // 绘制各层命令（从外到内绘制，这样内层会覆盖外层边缘）
            for (int layer = _settings.LayerCount; layer >= 1; layer--)
            {
                int itemCount = layer + 1; // 第1层2个，第2层3个，第3层4个
                float subAngle = sectorAngle / itemCount;
                int layerInnerR = catR + _settings.LayerWidth * (layer - 1);
                int layerOuterR = catR + _settings.LayerWidth * layer;

                for (int cat = 0; cat < 8; cat++)
                {
                    float catStartAngle = -90f + cat * 45f - 22.5f;

                    for (int idx = 0; idx < itemCount; idx++)
                    {
                        var item = _settings.GetItem(layer, cat, idx);
                        float itemStartAngle = catStartAngle + idx * subAngle;
                        bool isHovered = (_hoveredCategory == cat && _hoveredLayer == layer && _hoveredIndex == idx);

                        using (GraphicsPath path = CreateSectorPath(_center, layerInnerR, layerOuterR, itemStartAngle, subAngle))
                        {
                            Color fillColor = isHovered ? hoverColor : bgColor;
                            using (SolidBrush brush = new SolidBrush(fillColor))
                            {
                                g.FillPath(brush, path);
                            }
                            using (Pen pen = new Pen(isHovered ? Color.FromArgb(255, 120, 180, 255) : Color.FromArgb(60, 255, 255, 255), isHovered ? 2f : 1f))
                            {
                                g.DrawPath(pen, path);
                            }
                        }

                        // 命令名称
                        float midAngle = (itemStartAngle + subAngle / 2f) * (float)Math.PI / 180f;
                        float textRadius = (layerInnerR + layerOuterR) / 2f;
                        float textX = _center.X + textRadius * (float)Math.Cos(midAngle);
                        float textY = _center.Y + textRadius * (float)Math.Sin(midAngle);

                        string displayName = item.IsEmpty ? "" : item.Name;
                        using (Font font = new Font("Microsoft YaHei UI", 8f))
                        using (SolidBrush brush = new SolidBrush(isHovered ? Color.White : Color.FromArgb(220, 255, 255, 255)))
                        {
                            StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                            g.DrawString(displayName, font, brush, textX, textY, sf);
                        }
                    }
                }
            }

            // 绘制分类圈
            for (int cat = 0; cat < 8; cat++)
            {
                float catStartAngle = -90f + cat * 45f - 22.5f;
                bool isCatHovered = (_hoveredCategory == cat && _hoveredLayer == 0);

                using (GraphicsPath path = CreateSectorPath(_center, innerR, catR, catStartAngle, sectorAngle))
                {
                    Color fillColor = isCatHovered
                        ? Color.FromArgb(Math.Min(bgColor.A + 30, 255), bgColor.R + 15, bgColor.G + 15, bgColor.B + 15)
                        : Color.FromArgb(bgColor.A - 20, Math.Max(bgColor.R - 5, 0), Math.Max(bgColor.G - 5, 0), Math.Max(bgColor.B - 5, 0));
                    using (SolidBrush brush = new SolidBrush(fillColor))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(50, 255, 255, 255), 1f))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // 分类名称
                float catMidAngle = (-90f + cat * 45f) * (float)Math.PI / 180f;
                float catTextRadius = (innerR + catR) / 2f;
                float catTextX = _center.X + catTextRadius * (float)Math.Cos(catMidAngle);
                float catTextY = _center.Y + catTextRadius * (float)Math.Sin(catMidAngle);

                using (Font font = new Font("Microsoft YaHei UI", 9f, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(240, 255, 255, 255)))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(_settings.CategoryNames[cat], font, brush, catTextX, catTextY, sf);
                }
            }

            // 中心Logo
            Rectangle centerRect = new Rectangle(_center.X - innerR, _center.Y - innerR, innerR * 2, innerR * 2);

            if (_logoImage != null)
            {
                using (GraphicsPath clipPath = new GraphicsPath())
                {
                    clipPath.AddEllipse(centerRect);
                    g.SetClip(clipPath);
                    g.DrawImage(_logoImage, centerRect);
                    g.ResetClip();
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(centerColor))
                {
                    g.FillEllipse(brush, centerRect);
                }
                using (Font font = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, _settings.HoverR, _settings.HoverG, _settings.HoverB)))
                {
                    StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("言覃\n助手", font, brush, _center.X, _center.Y, sf);
                }
            }

            using (Pen pen = new Pen(Color.FromArgb(200, _settings.HoverR, _settings.HoverG, _settings.HoverB), 2f))
            {
                g.DrawEllipse(pen, centerRect);
            }
        }

        private void SetBitmap(Bitmap bitmap)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = SelectObject(memDc, hBitmap);

            POINT pointSource = new POINT { x = 0, y = 0 };
            POINT topPos = new POINT { x = Left, y = Top };
            SIZE size = new SIZE { cx = bitmap.Width, cy = bitmap.Height };
            BLENDFUNCTION blend = new BLENDFUNCTION { BlendOp = 0, BlendFlags = 0, SourceConstantAlpha = 255, AlphaFormat = 1 };

            UpdateLayeredWindow(Handle, screenDc, ref topPos, ref size, memDc, ref pointSource, 0, ref blend, ULW_ALPHA);

            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
            ReleaseDC(IntPtr.Zero, screenDc);
        }

        private GraphicsPath CreateSectorPath(Point center, int innerRadius, int outerRadius, float startAngle, float sweepAngle)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle outerRect = new Rectangle(center.X - outerRadius, center.Y - outerRadius, outerRadius * 2, outerRadius * 2);
            Rectangle innerRect = new Rectangle(center.X - innerRadius, center.Y - innerRadius, innerRadius * 2, innerRadius * 2);
            path.AddArc(outerRect, startAngle, sweepAngle);
            path.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
            path.CloseFigure();
            return path;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var (cat, layer, idx) = GetHoveredItem(e.Location);
            if (cat != _hoveredCategory || layer != _hoveredLayer || idx != _hoveredIndex)
            {
                _hoveredCategory = cat;
                _hoveredLayer = layer;
                _hoveredIndex = idx;
                UpdateWindow();
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                float dx = e.X - _center.X;
                float dy = e.Y - _center.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                if (dist <= _settings.InnerRadius)
                {
                    DateTime now = DateTime.Now;
                    if ((now - _lastCenterClickTime).TotalMilliseconds < 400)
                    {
                        _lastCenterClickTime = DateTime.MinValue;
                        ShowSettings();
                        return;
                    }
                    _lastCenterClickTime = now;
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
                }
            }
        }

        private void ShowSettings()
        {
            _showingSettings = true;
            using (var settingsForm = new SettingsForm(_settings))
            {
                settingsForm.ShowDialog();
                _settings = RadialMenuSettings.Load();

                // 重新计算尺寸
                int outerRadius = GetOuterRadius();
                int newSize = outerRadius * 2 + 60;
                Size = new Size(newSize, newSize);
                _center = new Point(newSize / 2, newSize / 2);
                Location = new Point(Location.X + (Width - newSize) / 2, Location.Y + (Height - newSize) / 2);

                UpdateWindow();
            }
            _showingSettings = false;
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var (cat, layer, idx) = GetHoveredItem(e.Location);

                // 双击分类圈编辑分类名称
                if (cat >= 0 && layer == 0)
                {
                    DateTime now = DateTime.Now;
                    if (cat == _lastClickCategory && layer == _lastClickLayer && (now - _lastClickTime).TotalMilliseconds < 400)
                    {
                        ShowCategoryEditDialog(cat);
                        _lastClickTime = DateTime.MinValue;
                        return;
                    }
                    _lastClickTime = now;
                    _lastClickCategory = cat;
                    _lastClickLayer = layer;
                    _lastClickIndex = -1;
                    return;
                }

                if (cat >= 0 && layer >= 1 && idx >= 0)
                {
                    DateTime now = DateTime.Now;
                    // 双击编辑命令 (350ms内)
                    if (cat == _lastClickCategory && layer == _lastClickLayer && idx == _lastClickIndex && (now - _lastClickTime).TotalMilliseconds < 350)
                    {
                        // 取消待执行的命令
                        _clickTimer.Stop();
                        _pendingLayer = -1;
                        _pendingCategory = -1;
                        _pendingIndex = -1;
                        _lastClickTime = DateTime.MinValue;
                        ShowEditDialog(layer, cat, idx);
                        return;
                    }

                    // 记录点击位置
                    _lastClickTime = now;
                    _lastClickCategory = cat;
                    _lastClickLayer = layer;
                    _lastClickIndex = idx;

                    // 延迟执行命令，等待可能的双击
                    _pendingLayer = layer;
                    _pendingCategory = cat;
                    _pendingIndex = idx;
                    _clickTimer.Stop();
                    _clickTimer.Start();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                Close();
            }
        }

        private void OnClickTimerTick(object sender, EventArgs e)
        {
            _clickTimer.Stop();

            if (_pendingLayer >= 1 && _pendingCategory >= 0 && _pendingIndex >= 0)
            {
                var item = _settings.GetItem(_pendingLayer, _pendingCategory, _pendingIndex);
                if (!item.IsEmpty)
                {
                    string cmd = item.Command;
                    _selectedCommand = cmd;
                    
                    // 先关闭窗口
                    Close();
                    
                    // 然后触发命令选择事件
                    CommandSelected?.Invoke(cmd);
                }
            }

            _pendingLayer = -1;
            _pendingCategory = -1;
            _pendingIndex = -1;
        }

        private void ShowCategoryEditDialog(int category)
        {
            _showingSettings = true;

            using (Form editForm = new Form())
            {
                editForm.Text = "编辑分类名称";
                editForm.Size = new Size(300, 120);
                editForm.StartPosition = FormStartPosition.CenterParent;
                editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                editForm.MaximizeBox = false;
                editForm.MinimizeBox = false;
                editForm.TopMost = true;
                editForm.BackColor = Color.FromArgb(45, 45, 48);
                editForm.ForeColor = Color.White;

                var lblName = new Label { Text = "分类名称:", Location = new Point(20, 20), AutoSize = true };
                var txtName = new TextBox { Text = _settings.CategoryNames[category], Location = new Point(100, 17), Width = 150, BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White };

                var btnOk = new Button { Text = "确定", Location = new Point(60, 55), Width = 80, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(74, 144, 217) };
                var btnCancel = new Button { Text = "取消", Location = new Point(160, 55), Width = 80, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };

                editForm.Controls.AddRange(new Control[] { lblName, txtName, btnOk, btnCancel });
                editForm.AcceptButton = btnOk;
                editForm.CancelButton = btnCancel;

                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    _settings.CategoryNames[category] = txtName.Text;
                    _settings.Save();
                }
            }

            _showingSettings = false;
            UpdateWindow();
        }

        private void ShowEditDialog(int layer, int category, int index)
        {
            _showingSettings = true; // 防止Deactivate关闭轮盘

            var item = _settings.GetItem(layer, category, index);

            using (Form editForm = new Form())
            {
                editForm.Text = "编辑命令";
                editForm.Size = new Size(350, 160);
                editForm.StartPosition = FormStartPosition.CenterParent;
                editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                editForm.MaximizeBox = false;
                editForm.MinimizeBox = false;
                editForm.TopMost = true;
                editForm.BackColor = Color.FromArgb(45, 45, 48);
                editForm.ForeColor = Color.White;

                var lblName = new Label { Text = "显示名称:", Location = new Point(20, 20), AutoSize = true };
                var txtName = new TextBox { Text = item.Name, Location = new Point(100, 17), Width = 200, BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White };

                var lblCmd = new Label { Text = "Rhino命令:", Location = new Point(20, 55), AutoSize = true };
                var txtCmd = new TextBox { Text = item.Command, Location = new Point(100, 52), Width = 200, BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White };

                var btnOk = new Button { Text = "确定", Location = new Point(100, 90), Width = 80, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(74, 144, 217) };
                var btnCancel = new Button { Text = "取消", Location = new Point(200, 90), Width = 80, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(70, 70, 75) };

                editForm.Controls.AddRange(new Control[] { lblName, txtName, lblCmd, txtCmd, btnOk, btnCancel });
                editForm.AcceptButton = btnOk;
                editForm.CancelButton = btnCancel;

                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    _settings.SetItem(layer, category, index, new MenuItemData(txtName.Text, txtCmd.Text));
                    _settings.Save();
                }
            }
            
            _showingSettings = false;
            UpdateWindow();
        }

        private (int category, int layer, int index) GetHoveredItem(Point mousePos)
        {
            float dx = mousePos.X - _center.X;
            float dy = mousePos.Y - _center.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            int innerR = _settings.InnerRadius;
            int catR = innerR + _settings.CategoryRingWidth;

            if (distance <= innerR) return (-1, -1, -1);

            float angle = (float)(Math.Atan2(dy, dx) * 180 / Math.PI);
            if (angle < 0) angle += 360;
            angle = (angle + 90 + 22.5f) % 360;
            int category = (int)(angle / 45) % 8;

            if (distance <= catR) return (category, 0, -1);

            // 检查各层
            for (int layer = 1; layer <= _settings.LayerCount; layer++)
            {
                int layerInnerR = catR + _settings.LayerWidth * (layer - 1);
                int layerOuterR = catR + _settings.LayerWidth * layer;

                if (distance > layerInnerR && distance <= layerOuterR)
                {
                    int itemCount = layer + 1;
                    float subAngle = 45f / itemCount;
                    float catAngle = angle - category * 45;
                    int index = (int)(catAngle / subAngle) % itemCount;
                    return (category, layer, index);
                }
            }

            return (-1, -1, -1);
        }

        public string SelectedCommand => _selectedCommand;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _logoImage?.Dispose();
                _clickTimer?.Stop();
                _clickTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
