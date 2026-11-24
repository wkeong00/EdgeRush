# Android 内部 APK：BlueStacks 基础初测

测试日期：2026-11-25（Asia/Singapore）。只验证现有内部包在一个模拟器上的观察行为；不替代正式签名包、真机或真实广告验收。

## 1. 环境与输入产物

| 项目 | 实际值／范围 |
| --- | --- |
| 模拟器 | 既有BlueStacks App Player **5.22.167.1006**，实例Pie64 |
| 主程序 | `C:\Program Files\BlueStacks_nxt\HD-Player.exe` |
| Android API／设备型号 | 本轮未读取Android About或ADB属性，不用Pie64名称冒充独立系统版本核验 |
| APK | `Builds/Android/EdgeRush-0.1.0-dev-ready.apk` |
| 绝对路径 | `C:\Users\wkeong\Desktop\TapTap project\EdgeRush\Builds\Android\EdgeRush-0.1.0-dev-ready.apk` |
| 大小 | 37,320,388 bytes（已有静态记录，本轮使用同一文件） |
| 本轮重新计算SHA256 | `DD7E547A128A5EDE0BFBB9D5EE2C2CC162F065BCD9B999D0B63EA8CE89255148` |
| 已有包身份 | `com.gizmobyte.edgerush`，0.1.0／code1，min26／target36，ARM64-only；沿用[静态产物记录](2026-11-16-android-dev-build.md) |
| 包性质 | Development、Android Debug签名、Mock广告；本轮没有重建、改包或签出Release |

启动既有模拟器，在My games桌面未看到Edge Rush；通过侧栏Install APK和Windows文件选择框选择上述精确文件。安装后观察到Edge Rush图标。没有卸载旧应用、清存档、启用ADB或更改安全／隐私设置。

## 2. 实际观察与结果

| 步骤 | 观察到的行为 | 本次结论 |
| --- | --- | --- |
| 安装与launcher | 出现Edge Rush名称与青色车辆图标 | 此模拟器安装及该launcher图标显示通过 |
| 首次启动 | 进入竖屏主菜单；金币0、BEST0、Play／Garage／Daily／Missions可见 | 启动与主菜单显示通过；未量化冷启动耗时 |
| Play／左移教学 | MOVE LEFT；左侧点击后车辆左移并进入MOVE RIGHT | 左点击输入初测通过 |
| 右移教学 | 向右拖动后车辆回中间，进入MAKE A CLOSE CALL | 模拟器鼠标拖动转触控初测通过，不替代实体触摸 |
| 擦边教学推进 | 在该步骤左侧点击，随后进入正式跑局倒数 | 教学可推进；未独立量化擦边判定窗口或证明整套公平性 |
| 首局结算 | RUN OVER，分数2／BEST2、25m、0 close calls、peak x1、+0金币 | 正常碰撞与结算显示通过 |
| 重新开始 | Run Again进入新跑局倒数 | 重开入口通过 |
| 暂停／继续 | 暂停面板显示SCORE2；Resume后显示倒数3且保留分数，之后继续跑局 | 主动暂停与继续初测通过 |
| 第二局结算 | RUN OVER，分数3／BEST3、38m、0 close calls、+0金币 | 第二次结算与纪录更新观察通过 |
| 每日领取 | Day1 ready，Claim50后显示CLAIMED及+50 COINS | 免费每日奖励初测通过 |
| Daily Mock奖励 | 点Watch Ad+30，显示TEST AD／REWARDED VIDEO／Editor / Development only／DAILYBONUS-WATCHING；倒数结束后+30 COINS、2 LEFT、冷却提示 | Mock完成路径通过；不是广告网络视频或收益 |
| 金币余额 | 返回主菜单显示80，车库显示80 | 50+30实际观察对账通过 |
| 车库 | 初始Cyan Interceptor为Selected；下一款Magenta Viper价格300，显示220 MORE COINS | 页面／价格显示通过；未购买，不代表全部外观解锁验收 |
| 移除任务并重开 | 仅划走Edge Rush最近任务，其他卡片保留；从桌面再打开后BEST3、80金币仍在 | 一次重开持久化初测通过；不声称完成系统重启或升级 |
| 每日状态持久化 | 重开后Daily仍CLAIMED、广告2 LEFT | 当日领取及次数保留通过 |

本轮只用正常UI和正常奖励获取数据，没有修改存档、设置伪分数或解锁广告车。最后返回主菜单，内部测试包仍保留在模拟器中。

## 3. 尚未验收

- 实体Android触控、返回／后台切换、音频／震动、安全区、多分辨率和不同设备GPU。
- 20分钟以上稳定性、FPS／内存／温度、冷启动量化、16KB设备运行。
- 正式签名干净安装与同签名升级、Debug包切Release的处理。
- 完整教程重试／跳过／重播、全部任务与外观、长期经济、无解路段与难度。
- Mock跳过／提前关闭／失败、复活／结算翻倍的所有路径、重复回调与延迟回调。
- 正式Provider资格、真实SDK依赖／隐私／广告回调／无填充／断网和正式收入。
- 真实候选包的商店截图／录屏：本轮没有导出或上传素材。

屏幕左侧带`Advertisement`的广告区域由BlueStacks自身呈现，不能记为Edge Rush已接入广告或获得收益。游戏内本轮看到的是明确标记Development only的Mock弹窗。

## 4. 下一步

已有模拟器安装和基础流程证据，可停止重复首次安装工作。保留本机测试数据；继续完成TapTap条目、准备真机测试者，并等待适用于TapTap-only／马来西亚个人的广告资格证据。正式SDK和候选Release完成后，重新执行所需设备与奖励异常验收。
