# ClipPilot

ClipPilot 是一个 Windows 桌面端轻量悬浮工具，用来快速处理文件、文件夹路径和剪贴板截图。

## 当前功能

- 桌面悬浮卡通助手
  - 默认只显示人物
  - 检测到内容后显示操作气泡
  - 双击人物可展开 / 收起气泡
  - 拖动人物可移动窗口
- 文件 / 文件夹拖拽
  - 拖入文件后复制路径
  - 拖入多个文件后复制多行路径
  - 拖入文件夹后复制文件夹路径
- 剪贴板监听
  - 使用 Windows `WM_CLIPBOARDUPDATE` 消息监听剪贴板变化
  - 支持文本路径识别
  - 支持资源管理器复制的文件列表
  - 支持剪贴板图片识别
- 截图处理
  - 微信截图或其他截图进入剪贴板后，自动保存为临时 PNG
  - 临时目录：`%TEMP%\ClipPilot\`
  - 可复制截图路径或复制截图文件
- 快捷操作
  - 复制文件
  - 复制路径
  - 复制名称
  - 打开文件
  - 打开目录 / 文件夹
- 托盘菜单
  - 显示悬浮窗
  - 隐藏悬浮窗
  - 设置
  - 退出
- 设置项
  - 是否启用剪贴板监听
  - 是否启用拖拽复制
  - 是否显示操作成功提示

## 技术栈

- C#
- WPF
- .NET 10 Windows
- Windows Forms NotifyIcon 托盘
- Win32 剪贴板更新消息监听

## 开发环境

- Windows 10/11
- Visual Studio 2022 或 Rider
- .NET SDK 10

## 运行

```bash
dotnet run --project "copyutil/copyutil.csproj"
```

## 构建

```bash
dotnet build "copyutil/copyutil.csproj"
```

## 发布单文件 exe

```bash
dotnet publish "copyutil/copyutil.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "publish"
```

发布后运行：

```text
publish/copyutil.exe
```

## 配置文件

窗口位置和设置项保存在：

```text
%AppData%\ClipPilot\settings.json
```

## 说明

项目中的需求文档、MVP 功能清单等规划文件仅用于本地开发，不作为 GitHub 仓库内容维护。当前功能说明以本 README 为准。
