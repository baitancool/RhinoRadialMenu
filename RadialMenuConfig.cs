using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RadialMenu
{
    /// <summary>
    /// 轮盘菜单项（子分支）
    /// </summary>
    public class RadialMenuItem
    {
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";
        public string Icon { get; set; } = "⚡";
        public bool Enabled { get; set; } = true;

        public RadialMenuItem() { }
        public RadialMenuItem(string name, string command, string icon = "⚡")
        {
            Name = name;
            Command = command;
            Icon = icon;
        }
    }

    /// <summary>
    /// 主分支（包含3个子分支）
    /// </summary>
    public class RadialMenuBranch
    {
        public string Name { get; set; } = "";
        public string Icon { get; set; } = "☰";
        public List<RadialMenuItem> Items { get; set; } = new List<RadialMenuItem>();

        public RadialMenuBranch()
        {
            Items = new List<RadialMenuItem>
            {
                new RadialMenuItem(),
                new RadialMenuItem(),
                new RadialMenuItem()
            };
        }

        public RadialMenuBranch(string name, string icon, RadialMenuItem item1, RadialMenuItem item2, RadialMenuItem item3)
        {
            Name = name;
            Icon = icon;
            Items = new List<RadialMenuItem> { item1, item2, item3 };
        }
    }

    /// <summary>
    /// 轮盘配置
    /// </summary>
    [Serializable]
    public class RadialMenuConfig
    {
        public int OuterRadius { get; set; } = 180;
        public int MiddleRadius { get; set; } = 100;
        public int InnerRadius { get; set; } = 45;
        public int BackgroundAlpha { get; set; } = 180;

        /// <summary>
        /// 8个主分支
        /// </summary>
        public List<RadialMenuBranch> Branches { get; set; } = new List<RadialMenuBranch>();

        /// <summary>
        /// Logo图片路径
        /// </summary>
        public string LogoPath { get; set; } = "";

        public RadialMenuConfig()
        {
            // 默认8个方向的命令组
            Branches = new List<RadialMenuBranch>
            {
                // 上 (0°)
                new RadialMenuBranch("变换", "⬆",
                    new RadialMenuItem("移动", "_Move", "↑"),
                    new RadialMenuItem("复制", "_Copy", "⊕"),
                    new RadialMenuItem("阵列", "_Array", "▦")),

                // 右上 (45°)
                new RadialMenuBranch("旋转", "↗",
                    new RadialMenuItem("旋转", "_Rotate", "⟳"),
                    new RadialMenuItem("旋转3D", "_Rotate3D", "🔄"),
                    new RadialMenuItem("定向", "_Orient", "⤵")),

                // 右 (90°)
                new RadialMenuBranch("缩放", "➡",
                    new RadialMenuItem("缩放", "_Scale", "⤡"),
                    new RadialMenuItem("缩放1D", "_Scale1D", "↔"),
                    new RadialMenuItem("缩放2D", "_Scale2D", "⬌")),

                // 右下 (135°)
                new RadialMenuBranch("曲线", "↘",
                    new RadialMenuItem("直线", "_Line", "╱"),
                    new RadialMenuItem("多段线", "_Polyline", "⌇"),
                    new RadialMenuItem("曲线", "_Curve", "〰")),

                // 下 (180°)
                new RadialMenuBranch("曲面", "⬇",
                    new RadialMenuItem("挤出", "_ExtrudeCrv", "▭"),
                    new RadialMenuItem("放样", "_Loft", "◎"),
                    new RadialMenuItem("扫掠", "_Sweep1", "≋")),

                // 左下 (225°)
                new RadialMenuBranch("实体", "↙",
                    new RadialMenuItem("方块", "_Box", "▢"),
                    new RadialMenuItem("球体", "_Sphere", "●"),
                    new RadialMenuItem("圆柱", "_Cylinder", "⬭")),

                // 左 (270°)
                new RadialMenuBranch("编辑", "⬅",
                    new RadialMenuItem("修剪", "_Trim", "✂"),
                    new RadialMenuItem("分割", "_Split", "⫽"),
                    new RadialMenuItem("炸开", "_Explode", "✧")),

                // 左上 (315°)
                new RadialMenuBranch("组合", "↖",
                    new RadialMenuItem("群组", "_Group", "▣"),
                    new RadialMenuItem("组合", "_Join", "⊞"),
                    new RadialMenuItem("布尔并", "_BooleanUnion", "⊕"))
            };
        }

        private static string ConfigPath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string configDir = Path.Combine(appData, "RadialMenu");
                if (!Directory.Exists(configDir))
                    Directory.CreateDirectory(configDir);
                return Path.Combine(configDir, "config.xml");
            }
        }

        public static string LogoFilePath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "RadialMenu", "logo.png");
            }
        }

        public static RadialMenuConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RadialMenuConfig));
                    using (FileStream fs = new FileStream(ConfigPath, FileMode.Open))
                    {
                        return (RadialMenuConfig)serializer.Deserialize(fs);
                    }
                }
            }
            catch { }
            return new RadialMenuConfig();
        }

        public void Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(RadialMenuConfig));
                using (FileStream fs = new FileStream(ConfigPath, FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch { }
        }
    }
}
