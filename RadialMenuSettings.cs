using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace RadialMenu
{
    [Serializable]
    public class MenuItemData
    {
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";

        public MenuItemData() { }
        public MenuItemData(string name, string command)
        {
            Name = name;
            Command = command;
        }

        public bool IsEmpty => string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Command);
    }

    [Serializable]
    public class RadialMenuSettings
    {
        // 层数 (1-3层)
        public int LayerCount { get; set; } = 2;

        // 8个分类名
        public List<string> CategoryNames { get; set; }

        // 第1层命令 (8分类 x 2命令 = 16个)
        public List<MenuItemData> Layer1Items { get; set; }
        // 第2层命令 (8分类 x 3命令 = 24个)
        public List<MenuItemData> Layer2Items { get; set; }
        // 第3层命令 (8分类 x 4命令 = 32个)
        public List<MenuItemData> Layer3Items { get; set; }

        // 颜色设置
        public int BackgroundAlpha { get; set; } = 140;
        public int BackgroundR { get; set; } = 30;
        public int BackgroundG { get; set; } = 34;
        public int BackgroundB { get; set; } = 42;

        public int HoverAlpha { get; set; } = 180;
        public int HoverR { get; set; } = 74;
        public int HoverG { get; set; } = 144;
        public int HoverB { get; set; } = 217;

        public int CenterAlpha { get; set; } = 200;
        public int CenterR { get; set; } = 20;
        public int CenterG { get; set; } = 24;
        public int CenterB { get; set; } = 32;

        // 尺寸
        public int InnerRadius { get; set; } = 45;
        public int CategoryRingWidth { get; set; } = 45;
        public int LayerWidth { get; set; } = 50;

        public Color BackgroundColor => Color.FromArgb(BackgroundAlpha, BackgroundR, BackgroundG, BackgroundB);
        public Color HoverColor => Color.FromArgb(HoverAlpha, HoverR, HoverG, HoverB);
        public Color CenterColor => Color.FromArgb(CenterAlpha, CenterR, CenterG, CenterB);

        public RadialMenuSettings()
        {
            // 构造函数不初始化，让Load方法处理
        }

        private void InitializeDefaultItems()
        {
            // 第1层默认命令 (每分类2个)
            if (Layer1Items == null || Layer1Items.Count == 0)
            {
                Layer1Items = new List<MenuItemData>
                {
                    new MenuItemData("移动", "_Move"), new MenuItemData("复制", "_Copy"),
                    new MenuItemData("旋转", "_Rotate"), new MenuItemData("旋转3D", "_Rotate3D"),
                    new MenuItemData("缩放", "_Scale"), new MenuItemData("缩放1D", "_Scale1D"),
                    new MenuItemData("直线", "_Line"), new MenuItemData("多段线", "_Polyline"),
                    new MenuItemData("挤出", "_ExtrudeCrv"), new MenuItemData("放样", "_Loft"),
                    new MenuItemData("方块", "_Box"), new MenuItemData("球体", "_Sphere"),
                    new MenuItemData("修剪", "_Trim"), new MenuItemData("分割", "_Split"),
                    new MenuItemData("群组", "_Group"), new MenuItemData("组合", "_Join")
                };
            }

            // 第2层默认命令 (每分类3个)
            if (Layer2Items == null || Layer2Items.Count == 0)
            {
                Layer2Items = new List<MenuItemData>
                {
                    // 变换 (3个)
                    new MenuItemData("阵列", "_Array"), new MenuItemData("镜像", "_Mirror"), new MenuItemData("定向", "_Orient"),
                    // 旋转 (3个)
                    new MenuItemData("扭转", "_Twist"), new MenuItemData("弯曲", "_Bend"), new MenuItemData("流动", "_Flow"),
                    // 缩放 (3个)
                    new MenuItemData("缩放2D", "_Scale2D"), new MenuItemData("拉伸", "_Stretch"), new MenuItemData("锥化", "_Taper"),
                    // 曲线 (3个)
                    new MenuItemData("圆弧", "_Arc"), new MenuItemData("圆", "_Circle"), new MenuItemData("矩形", "_Rectangle"),
                    // 曲面 (3个)
                    new MenuItemData("扫掠1", "_Sweep1"), new MenuItemData("扫掠2", "_Sweep2"), new MenuItemData("旋转成形", "_Revolve"),
                    // 实体 (3个)
                    new MenuItemData("圆柱", "_Cylinder"), new MenuItemData("圆锥", "_Cone"), new MenuItemData("圆环", "_Torus"),
                    // 编辑 (3个)
                    new MenuItemData("延伸", "_Extend"), new MenuItemData("偏移", "_Offset"), new MenuItemData("倒角", "_Fillet"),
                    // 组合 (3个)
                    new MenuItemData("布尔并", "_BooleanUnion"), new MenuItemData("布尔差", "_BooleanDifference"), new MenuItemData("布尔交", "_BooleanIntersection")
                };
            }

            // 第3层默认命令 (每分类4个)
            if (Layer3Items == null || Layer3Items.Count == 0)
            {
                Layer3Items = new List<MenuItemData>
                {
                    // 变换 (4个)
                    new MenuItemData("沿曲线阵列", "_ArrayCrv"), new MenuItemData("极轴阵列", "_ArrayPolar"), new MenuItemData("对齐", "_Align"), new MenuItemData("分布", "_Distribute"),
                    // 旋转 (4个)
                    new MenuItemData("沿曲线流动", "_FlowAlongSrf"), new MenuItemData("变形", "_CageEdit"), new MenuItemData("投影", "_Project"), new MenuItemData("拉回", "_Pull"),
                    // 缩放 (4个)
                    new MenuItemData("沿曲线缩放", "_ScaleByPlane"), new MenuItemData("剪切", "_Shear"), new MenuItemData("挤压", "_Squish"), new MenuItemData("展平", "_UnrollSrf"),
                    // 曲线 (4个)
                    new MenuItemData("椭圆", "_Ellipse"), new MenuItemData("螺旋线", "_Helix"), new MenuItemData("抛物线", "_Parabola"), new MenuItemData("样条曲线", "_InterpCrv"),
                    // 曲面 (4个)
                    new MenuItemData("嵌面", "_Patch"), new MenuItemData("网格曲面", "_NetworkSrf"), new MenuItemData("边缘曲面", "_EdgeSrf"), new MenuItemData("平面曲面", "_PlanarSrf"),
                    // 实体 (4个)
                    new MenuItemData("管道", "_Pipe"), new MenuItemData("金字塔", "_Pyramid"), new MenuItemData("椭球", "_Ellipsoid"), new MenuItemData("抛物面", "_Paraboloid"),
                    // 编辑 (4个)
                    new MenuItemData("炸开", "_Explode"), new MenuItemData("重建", "_Rebuild"), new MenuItemData("匹配", "_Match"), new MenuItemData("混接", "_BlendCrv"),
                    // 组合 (4个)
                    new MenuItemData("布尔分割", "_BooleanSplit"), new MenuItemData("合并", "_Merge"), new MenuItemData("衔接", "_Connect"), new MenuItemData("桥接", "_Bridge")
                };
            }

            // 分类名称
            if (CategoryNames == null || CategoryNames.Count == 0)
            {
                CategoryNames = new List<string>
                {
                    "变换", "旋转", "缩放", "曲线", "曲面", "实体", "编辑", "组合"
                };
            }
        }

        public void EnsureData()
        {
            if (CategoryNames == null) CategoryNames = new List<string>();
            if (Layer1Items == null) Layer1Items = new List<MenuItemData>();
            if (Layer2Items == null) Layer2Items = new List<MenuItemData>();
            if (Layer3Items == null) Layer3Items = new List<MenuItemData>();

            while (CategoryNames.Count < 8) CategoryNames.Add("");
            while (Layer1Items.Count < 16) Layer1Items.Add(new MenuItemData());
            while (Layer2Items.Count < 24) Layer2Items.Add(new MenuItemData());
            while (Layer3Items.Count < 32) Layer3Items.Add(new MenuItemData());
        }

        // 获取指定层指定分类的命令
        public MenuItemData GetItem(int layer, int category, int index)
        {
            EnsureData();
            if (layer == 1 && index < 2) return Layer1Items[category * 2 + index];
            if (layer == 2 && index < 3) return Layer2Items[category * 3 + index];
            if (layer == 3 && index < 4) return Layer3Items[category * 4 + index];
            return new MenuItemData();
        }

        public void SetItem(int layer, int category, int index, MenuItemData item)
        {
            EnsureData();
            if (layer == 1 && index < 2) Layer1Items[category * 2 + index] = item;
            else if (layer == 2 && index < 3) Layer2Items[category * 3 + index] = item;
            else if (layer == 3 && index < 4) Layer3Items[category * 4 + index] = item;
        }

        private static string SettingsPath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dir = Path.Combine(appData, "RadialMenu");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                return Path.Combine(dir, "settings.xml");
            }
        }

        public static RadialMenuSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var serializer = new XmlSerializer(typeof(RadialMenuSettings));
                    using (var fs = new FileStream(SettingsPath, FileMode.Open))
                    {
                        var settings = (RadialMenuSettings)serializer.Deserialize(fs);
                        settings.InitializeDefaultItems(); // 填充缺失的默认值
                        settings.EnsureData();
                        return settings;
                    }
                }
            }
            catch { }
            var newSettings = new RadialMenuSettings();
            newSettings.InitializeDefaultItems();
            newSettings.EnsureData();
            return newSettings;
        }

        public void Save()
        {
            try
            {
                EnsureData();
                var serializer = new XmlSerializer(typeof(RadialMenuSettings));
                using (var fs = new FileStream(SettingsPath, FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch { }
        }

        /// <summary>
        /// 导出命令配置到文件（只包含分类名和命令，不含颜色等设置）
        /// </summary>
        public void ExportCommands(string filePath)
        {
            EnsureData();
            var exportData = new CommandExportData
            {
                CategoryNames = new List<string>(CategoryNames),
                Layer1Items = new List<MenuItemData>(Layer1Items),
                Layer2Items = new List<MenuItemData>(Layer2Items),
                Layer3Items = new List<MenuItemData>(Layer3Items)
            };

            var serializer = new XmlSerializer(typeof(CommandExportData));
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fs, exportData);
            }
        }

        /// <summary>
        /// 从文件导入命令配置
        /// </summary>
        public void ImportCommands(string filePath)
        {
            var serializer = new XmlSerializer(typeof(CommandExportData));
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                var importData = (CommandExportData)serializer.Deserialize(fs);

                if (importData.CategoryNames != null && importData.CategoryNames.Count > 0)
                    CategoryNames = importData.CategoryNames;
                if (importData.Layer1Items != null && importData.Layer1Items.Count > 0)
                    Layer1Items = importData.Layer1Items;
                if (importData.Layer2Items != null && importData.Layer2Items.Count > 0)
                    Layer2Items = importData.Layer2Items;
                if (importData.Layer3Items != null && importData.Layer3Items.Count > 0)
                    Layer3Items = importData.Layer3Items;

                EnsureData();
            }
        }
    }

    /// <summary>
    /// 命令导出数据结构（只包含命令相关配置）
    /// </summary>
    [Serializable]
    public class CommandExportData
    {
        public List<string> CategoryNames { get; set; }
        public List<MenuItemData> Layer1Items { get; set; }
        public List<MenuItemData> Layer2Items { get; set; }
        public List<MenuItemData> Layer3Items { get; set; }
    }
}
