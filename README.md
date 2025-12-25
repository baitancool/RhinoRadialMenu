# 言覃犀牛快捷轮盘 / YanTan Rhino Radial Menu

## 简介 / Introduction

言覃犀牛快捷轮盘是一款为 Rhino 7 设计的快捷命令插件，提供八卦式径向菜单界面，让您快速访问常用命令。

YanTan Rhino Radial Menu is a quick command plugin designed for Rhino 7, providing an octagonal radial menu interface for fast access to frequently used commands.

**作者 / Author:** 言覃设计  
**微信 / WeChat:** baitancool  
**命令 / Command:** `ytzj`

---

## 功能特点 / Features

- 🎯 八方向分类，最多3层命令（72个命令位）
- 🖱️ 鼠标位置弹出，操作便捷
- ✏️ 双击编辑命令，自定义配置
- 🎨 可调节背景透明度和颜色
- 💾 自动保存配置

- 🎯 8-direction categories, up to 3 layers (72 command slots)
- 🖱️ Popup at mouse position, convenient operation
- ✏️ Double-click to edit commands, customizable
- 🎨 Adjustable background transparency and colors
- 💾 Auto-save configuration

---

## 安装 / Installation

1. 将 `RadialMenu.rhp` 文件拖入 Rhino 窗口
2. 或通过 `PlugInManager` 命令安装

1. Drag `RadialMenu.rhp` file into Rhino window
2. Or install via `PlugInManager` command

---

## 操作指南 / User Guide

### 打开轮盘 / Open Menu
- 输入命令 `ytzj` 并回车
- 建议设置快捷键（如 `Ctrl+Q`）

- Type command `ytzj` and press Enter
- Recommend setting a shortcut key (e.g., `Ctrl+Q`)

### 执行命令 / Execute Command
- **单击** 命令区域 → 执行该命令

- **Single click** on command area → Execute the command

### 编辑命令 / Edit Command
- **双击** 命令区域 → 打开编辑对话框
- 可修改显示名称和 Rhino 命令

- **Double-click** on command area → Open edit dialog
- Can modify display name and Rhino command

### 编辑分类名称 / Edit Category Name
- **双击** 内圈分类区域 → 编辑分类名称

- **Double-click** on inner category ring → Edit category name

### 打开设置 / Open Settings
- **双击** 中心 Logo → 打开设置界面
- 可调节：层数、背景透明度、颜色

- **Double-click** on center Logo → Open settings
- Adjustable: layers, background transparency, colors

### 移动轮盘 / Move Menu
- **拖拽** 中心 Logo → 移动轮盘位置

- **Drag** center Logo → Move menu position

### 关闭轮盘 / Close Menu
- 按 `ESC` 键
- 点击轮盘外部区域
- 右键点击

- Press `ESC` key
- Click outside the menu
- Right-click

---

## 默认命令 / Default Commands

### 第1层 / Layer 1 (每分类2个 / 2 per category)
| 分类 | 命令1 | 命令2 |
|------|-------|-------|
| 变换 | 移动 | 复制 |
| 旋转 | 旋转 | 旋转3D |
| 缩放 | 缩放 | 缩放1D |
| 曲线 | 直线 | 多段线 |
| 曲面 | 挤出 | 放样 |
| 实体 | 方块 | 球体 |
| 编辑 | 修剪 | 分割 |
| 组合 | 群组 | 组合 |

### 第2层 / Layer 2 (每分类3个 / 3 per category)
| 分类 | 命令1 | 命令2 | 命令3 |
|------|-------|-------|-------|
| 变换 | 阵列 | 镜像 | 定向 |
| 旋转 | 扭转 | 弯曲 | 流动 |
| 缩放 | 缩放2D | 拉伸 | 锥化 |
| 曲线 | 圆弧 | 圆 | 矩形 |
| 曲面 | 扫掠1 | 扫掠2 | 旋转成形 |
| 实体 | 圆柱 | 圆锥 | 圆环 |
| 编辑 | 延伸 | 偏移 | 倒角 |
| 组合 | 布尔并 | 布尔差 | 布尔交 |

### 第3层 / Layer 3 (每分类4个 / 4 per category)
| 分类 | 命令1 | 命令2 | 命令3 | 命令4 |
|------|-------|-------|-------|-------|
| 变换 | 沿曲线阵列 | 极轴阵列 | 对齐 | 分布 |
| 旋转 | 沿曲线流动 | 变形 | 投影 | 拉回 |
| 缩放 | 沿曲线缩放 | 剪切 | 挤压 | 展平 |
| 曲线 | 椭圆 | 螺旋线 | 抛物线 | 样条曲线 |
| 曲面 | 嵌面 | 网格曲面 | 边缘曲面 | 平面曲面 |
| 实体 | 管道 | 金字塔 | 椭球 | 抛物面 |
| 编辑 | 炸开 | 重建 | 匹配 | 混接 |
| 组合 | 布尔分割 | 合并 | 衔接 | 桥接 |

---

## 配置文件 / Configuration File

配置保存在 / Configuration saved at:
```
%APPDATA%\RadialMenu\settings.xml
```

删除此文件可恢复默认设置 / Delete this file to restore default settings.

---

## 联系作者 / Contact

更多插件请联系作者微信：**baitancool**

For more plugins, contact author WeChat: **baitancool**
