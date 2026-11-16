# Edge Rush · 商店品牌素材

制作与检查日期：2026-11-16。以下是**原创程序绘制的品牌插画**，不是游戏截图，也不是 Android 功能验收证据。尚未上传 TapTap；默认图标与两层Adaptive图标已通过 Unity 插件导入 `Assets/Branding/`。Android 六种尺寸共12层引用已回读确认，新 APK 资源验收另记。

| 交付 | 尺寸／格式 | 用途与状态 |
| --- | --- | --- |
| [icon-512.png](icon-512.png) | 512×512，PNG，24-bit RGB，不透明 | 商店图标候选；单台青色车和三车道，无文字／外部标记 |
| [cover-1920x1080.png](cover-1920x1080.png) | 1920×1080，PNG，24-bit RGB，不透明 | 详情页封面候选；唯一可见文字为 EDGE RUSH，没有 HUD、手机外框、现金或排名 |
| [adaptive-foreground-512.png](adaptive-foreground-512.png) | 512×512，PNG，32-bit RGBA，透明 | Android Adaptive 前景：只有原创车、局部轮子与阴影；全部非零 alpha 在中央安全区 |
| [adaptive-background-512.png](adaptive-background-512.png) | 512×512，PNG，24-bit RGB，不透明 | Android Adaptive 背景：只有道路，无车、阴影或文字 |
| [icon-512.svg](icon-512.svg)／[cover-1920x1080.svg](cover-1920x1080.svg) | 可编辑 SVG，文字已转轮廓 | 对应矢量几何；没有外链、嵌入照片或字体文件 |
| [render-brand-assets.ps1](render-brand-assets.ps1) | Windows PowerShell 5.1＋System.Drawing | 重现 PNG／SVG；不安装依赖、不联网、不启动 GUI |

## 设计来源

车辆轮廓为本次独立绘制，延续 [Palette.cs](../../Assets/Scripts/Art/Palette.cs) 中 Cyan Interceptor 的 `#26C4E8` 车身、`#78F4FF` 高光和 `#40E8FF` 辉光；深色双窗、成对白前灯、红尾灯、局部黑轮和小后翼与现有程序美术方向一致。背景使用 `#070914` 深蓝、`#1B1E2A` 路面及 `#FF48AA` 品红点缀。

构图参考仓库 [网站原创道路插画](../site/dist/assets/road-preview.svg) 的三车道与霓虹方向，重新绘制为品牌图；没有使用其标签或 UI 样式。没有下载照片、素材库图片、外部图标或使用 AI 生成图片。封面不能替代需要来自真实候选 APK 的商店截图。

标题轮廓由本机已安装的 **Arial Bold** 生成；未复制或分发字体文件，SVG 已将标题转换为轮廓。重现脚本在 Windows 上使用同一安装字体，字体版本不同可能导致轮廓略有差异。

## 封面安全区

1920×1080 画布中的 1760×920 安全区为 **x=80～1840，y=80～1000**。标题字形和主要车辆／阴影均处于该区域；道路、背景建筑和边缘光线属于可裁切的背景。

主要车辆连同最外层阴影约为 x=1251.64～1522.36、y=523.64～984.44。标题在左侧，前景车在右侧，裁切时仍须在 TapTap 当前上传预览中复核。不得把安全区引导线印进正式图片。

标题轮廓坐标回读：EDGE 为 x=161.278～730.678、y=315.222～470.671；RUSH 为 x=156.139～784.100、y=518.476～691.805，均在安全区内。

## 重现

在仓库根目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\docs\store-assets\render-brand-assets.ps1
```

默认重现本目录八个 PNG／SVG 输出；不会改游戏代码、Unity 图标、商店后台或系统设置。当前用户／机器保存的执行策略不变。只生成 Adaptive 两层及其 SVG、保留原图标／封面不重写时：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\docs\store-assets\render-brand-assets.ps1 -AdaptiveOnly
```

## Android Adaptive 两层

前景复用同一台原创车的几何，缩放为 0.9、位置为 `(256,241.6)`；没有再用不透明商店图标充当前景。512 画布中央 300×300 区域为 x/y=106～405。PNG 解码后实际非零 alpha 包围盒是 **x=171～340、y=112～399**，包括最外阴影；alpha 最小值为0、最大值为255，四周保持透明。背景单独绘制完整深色三车道，无车辆和文字。

对应 [前景 SVG](adaptive-foreground-512.svg) 与 [背景 SVG](adaptive-background-512.svg) 均已通过 XML 解析。已分别打开前景／背景 PNG 检查车辆与道路分离；显示工具可能将透明背景呈黑色，透明性以真实 PNG alpha 解码检查为准。安装器的实际合成、设备遮罩和 APK 图标仍需发布阶段验证，不以源文件检查代替。

本次使用 `-AdaptiveOnly`，原 `icon-512.png` 与 `cover-1920x1080.png` 的哈希和内容均保持不变。

## 检查与剩余验收

- 已打开并人工检查两个 PNG：车体、轮子和完整高对比标题正常，无异常大色块、朝原点延伸的条带或多余文字。
- 两个 PNG 的尺寸与 24-bit RGB 格式已回读；无透明通道。
- 两个 SVG 的 XML 解析通过；316 个多边形／折线顶点均在各自画布内。
- 标题转轮廓；PNG 与 SVG 使用同一套几何参数。脚本负坐标参数显式为数值，避免 PowerShell 字符串拼接产生越界几何。
- 根代理已逐图审查并采用当前候选；小尺寸／系统遮罩的设备显示、TapTap 当前上传裁切预览和规格校验仍待。
- Unity 默认图标引用已回读确认；APK 中实际图标、Launcher 与系统遮罩验收由对应构建记录另记。

| 文件 | bytes | SHA-256 |
| --- | --- | --- |
| icon-512.png | 30711 | `4EBC1EEC3515796A464A9AFE8F3BC8D98644A80D60EE98383EF33EBC7FC80853` |
| cover-1920x1080.png | 105748 | `3D7FED6ABE8C8545D9EBC48331A5887E0E32F694B41BD9DA05787318891310C5` |
| adaptive-foreground-512.png | 13822 | `B1BB0DCA8DBF42F88FB346DD4202202A2B97D755E9C10EEE1F18D8E740B35132` |
| adaptive-background-512.png | 19747 | `2709617B61AC4912DC6FA1BA87EF40075666E3B6EC313134146C6DEFBF662F6E` |

平台规格沿用 [09 上架资料草稿](../09-TapTap上架资料草稿.md) 的国际版官方核对结果；实际提交前仍核对当前后台。视频封面、真实玩法截图与录屏不在本次交付内。
