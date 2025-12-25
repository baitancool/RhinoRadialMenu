using Rhino;
using Rhino.PlugIns;

namespace RadialMenu
{
    [System.Runtime.InteropServices.Guid("F3A7C2E1-9D4B-4F8E-A6C3-7B2E5D1F9A4C")]
    public class RadialMenuPlugin : PlugIn
    {
        public RadialMenuPlugin()
        {
            Instance = this;
        }

        public static RadialMenuPlugin Instance { get; private set; }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            RhinoApp.WriteLine("✓ 言覃犀牛快捷轮盘已加载 | 作者微信: baitancool | 快捷键: ytzj");
            return LoadReturnCode.Success;
        }
    }
}
