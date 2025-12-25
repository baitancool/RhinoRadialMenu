using System;
using System.Drawing;
using System.Windows.Forms;
using Rhino;
using Rhino.Commands;

namespace RadialMenu
{
    [System.Runtime.InteropServices.Guid("D8E6B4A2-5C3F-4A1D-9E7B-6F2C8D4A3E5B")]
    public class YtzjCommand : Command
    {
        public override string EnglishName => "ytzj";

        // 当前打开的轮盘窗口
        private static RadialMenuForm _currentForm = null;

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // 每次运行命令输出作者信息
                RhinoApp.WriteLine("更多插件联系作者微信baitancool");

                // 如果已有轮盘打开，先关闭
                if (_currentForm != null && !_currentForm.IsDisposed)
                {
                    _currentForm.Close();
                    _currentForm = null;
                }

                Point mousePos = Cursor.Position;
                _currentForm = new RadialMenuForm(mousePos);
                
                // 设置命令执行回调
                _currentForm.CommandSelected += (command) =>
                {
                    if (!string.IsNullOrEmpty(command))
                    {
                        RhinoApp.SendKeystrokes(command + "\n", true);
                    }
                };

                // 非模态显示，设置Rhino为Owner
                _currentForm.Show(new RhinoWindowWrapper());

                return Result.Success;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("轮盘异常: {0}", ex.Message);
                return Result.Failure;
            }
        }
    }

    /// <summary>
    /// Rhino窗口包装器，用于设置Owner
    /// </summary>
    internal class RhinoWindowWrapper : IWin32Window
    {
        public IntPtr Handle => RhinoApp.MainWindowHandle();
    }
}
