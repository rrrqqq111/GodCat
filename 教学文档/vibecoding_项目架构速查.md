# Vibecoding 项目架构速查

本文给自动改代码工具用，目标是快速定位“谁负责什么、谁调用谁、加新动物要改哪里”。

## 1. 项目主入口

### 全局调控
- `Assets/Game/Scripts/Ranch/RanchManager.cs`
- 角色：场景入口、全局门面、服务组合根
- 负责：
  - 初始化 `RanchMap`
  - 创建各类 service
  - 暴露对外 API 给 UI / 能力 / 道具 / 测试脚本
  - 转发事件监听

### 回合状态
- `Assets/Game/Scripts/Ranch/RanchGameState.cs`
- 角色：全局状态机
- 负责：
  - `Day`
  - `Money`
  - `Cans`
  - `Phase`
  - 测试模式状态

### 回合推进
- `Assets/Game/Scripts/Ranch/RanchTurnService.cs`
- 角色：控制白天、结算、下一天切换
- 负责：
  - `NextDay()`
  - 白天结算 / 过渡阶段
  - 触发日启动道具/玩具

### 日结算
- `Assets/Game/Scripts/Ranch/RanchSettlementService.cs`
- 角色：能力触发和收益结算中心
- 负责：
  - 白天/阶段结算
  - 动物能力触发
  - 被吃/被移除后的连锁处理
  - 结算报告文本

## 2. 目录职责

### `Assets/Game/Scripts/Ranch`
核心游戏编排层。

常见脚本归属：
- `RanchMap.cs`：地图、格子、摆放、移动、交换
- `MapCell.cs`：单格渲染与占用状态
- `RanchAnimalService.cs`：动物名册 + 地图部署
- `RanchAnimalLifecycleService.cs`：动物移除生命周期
- `RanchAnimalSpawnService.cs`：随机生成、替换、按家族筛选
- `RanchOfferService.cs`：开局/商店动物池
- `RanchEconomyService.cs`：钱、罐头等经济值
- `RanchItemService.cs`：道具持有与使用
- `RanchRewardService.cs`：随机奖励发放
- `RanchToyService.cs`：玩具注册与触发
- `MapObjectService.cs`：地块上物体系统
- `RanchEventHub.cs`：统一事件中心
- `RanchTextFormatter.cs`：UI 文本拼装

### `Assets/Game/Scripts/Animals`
- `Animal.cs`：动物运行时实例
- `AnimalData.cs`：动物配置资产

### `Assets/Game/Scripts/Abilities`
- `AbilityData.cs`：能力配置资产
- `ConfiguredAnimalAbility.cs`：配置驱动的能力运行体
- `AbilityEffectRegistry.cs`：能力效果注册表
- `AnimalAbilityFactory.cs`：把配置转成可执行能力
- `AbilityTargetResolver.cs`：能力目标解析
- `Prey/`：捕食相关规则、结果、目标解析

### `Assets/Game/Scripts/Items`
- `ItemData.cs`：道具配置
- `RanchItemService.cs`：持有、使用、触发
- `ItemEffectRegistry.cs`：道具效果注册表

### `Assets/Game/Scripts/Toys`
- `ToyData.cs`：玩具配置
- `RanchToyService.cs`：玩具触发
- `ToyEffectRegistry.cs`：玩具效果注册表

### `Assets/Game/Scripts/MapObjects`
- `MapCellObjectData.cs`：地块物体配置
- `MapCellObjectRuntime.cs`：运行时对象
- `MapCellObjectEffectRegistry.cs`：地块物体效果注册表
- `MapCellObjectSpawner.cs`：地图物体投放工具

### `Assets/Game/Scripts/UI`
- `RanchUIController.cs`：主 UI 刷新
- `AnimalDetailPanel.cs`：详情
- `AnimalOfferPanel.cs`：可选动物
- `ItemPanelController.cs`：道具面板
- `ToyPanelController.cs`：玩具面板
- `AnimalRemoveButtonPanel.cs`：移除按钮

## 3. 核心调用链

### 运行初始化
`RanchManager.Start()`  
-> `InitializeFromScene()`  
-> `RefreshContentPools()`  
-> `Initialize(...)`  
-> `RanchMap.Initialize(...)`  
-> `CreateServices()`  
-> `SeedAnimals(...)`  
-> `CreateTurnService()`

### 日常回合
`RanchUIController` / 外部按钮  
-> `RanchManager.NextDay()`  
-> `RanchTurnService.NextDay()`  
-> `RanchSettlementService.ResolveDailySettlement(...)`  
-> `state.SetPhase(...)`

### 动物能力触发
`RanchSettlementService`  
-> `animal.Ability.TryExecute(...)`  
-> `ConfiguredAnimalAbility`  
-> `AbilityEffectRegistry.TryGet(...)`  
-> 具体 `XXXAbilityEffect.Execute(...)`

### 捕食
`RanchManager.TryPrey(...)`  
-> `RanchPreyService.TryPrey(...)`  
-> `RanchAnimalLifecycleService.TryRemove(...)`  
-> `RanchSettlementService` 连锁触发能力

### 地块物体
`RanchManager.TryAddMapObject(...)`  
-> `MapObjectService.TryAddMapCellObject(...)`

动物移动后：
`RanchAnimalService`  
-> `ResolveMovedAbility(...)`  
-> `RanchSettlementService.ResolveMovedAbility(...)`  
-> `MapObjectService.TryConsumeNearbyMapObjects(...)`

## 4. 监听与事件

统一事件中心：
- `Assets/Game/Scripts/Ranch/RanchEventHub.cs`

`RanchManager` 只做事件透传，常见监听入口：
- `StateChanged`
- `OnPreyAttempt`
- `OnPreyProtected`
- `OnPreySuccess`
- `OnPreyFailed`
- `OnAnimalPreyed`
- `OnAnimalRemoved`
- `OnAnimalSold`
- `OnAnimalGrown`
- `OnAnimalTransformed`
- `OnAnimalCooldownReduced`
- `OnAnimalEvolutionProgressed`
- `OnAnimalEvolutionLeveledUp`
- `OnMapObjectAdded`
- `OnMapObjectRemoved`
- `OnMapObjectConsumed`

UI 一般监听 `StateChanged` 做刷新。

## 5. 新增动物的基本流程

### 1) 新建动物配置
创建 `AnimalData` 资产，通常要填：
- `Id`
- `Name`
- `Family`
- `Rarity`
- `BaseMoney`
- `Ability`
- `Description`
- `Icon`

### 2) 准备能力配置
如果动物有能力：
- 新建/复用 `AbilityData`
- 填 `EffectType`
- 填 `EffectScriptId`
- 配 `TriggerType`
- 配 `EffectParams`

### 3) 新增能力脚本
在合适目录下新增：
- `Assets/Game/Scripts/Abilities/Hoofed/XXXAbilityEffect.cs`
- `Assets/Game/Scripts/Abilities/Carnivora/XXXAbilityEffect.cs`
- `Assets/Game/Scripts/Abilities/Bird/XXXAbilityEffect.cs`
- 或 `General/`

实现后要注册到：
- `Assets/Game/Scripts/Abilities/AbilityEffectRegistry.cs`

### 4) 若需要新目标逻辑
改这些地方之一：
- `AbilityTargetResolver.cs`
- `ConfiguredAnimalAbility.cs`
- `Prey/` 相关规则

### 5) 若涉及出生/替换/家族随机
改：
- `RanchAnimalSpawnService.cs`
- `RanchOfferService.cs`
- `RanchContentCatalog.cs`

### 6) 若涉及地图交互
改：
- `RanchSettlementService.cs`
- `MapObjectService.cs`
- `MapCell.cs`
- `RanchMap.cs`

## 6. 命名规则

### 动物
- 配置：`AnimalData`
- 运行体：`Animal`
- 能力类：`XXXAbilityEffect`
- 配置 `Id` 建议和能力效果名保持一致或强相关

### 能力
- 效果脚本名建议统一成 `XXXAbilityEffect`
- `EffectScriptId` 通常直接对应类名
- `TriggerType` 表示触发时机，不是效果名
- `EffectType` 表示具体语义分支，很多现有脚本靠它做二次分流

### 道具
- `ItemData`
- `ItemEffectRegistry`
- `XXXItemEffect`

### 玩具
- `ToyData`
- `ToyEffectRegistry`
- `XXXToyEffect`

### 地块物体
- `MapCellObjectData`
- `MapCellObjectRuntime`
- `MapCellObjectEffectRegistry`
- `XXXMapCellObjectEffect`

## 7. 改代码时的优先级

优先改 service，不优先改 `RanchManager`。

常见顺序：
1. 找到业务归属 service
2. 看有没有已有 registry / resolver
3. 只在 `RanchManager` 留一个薄转发
4. 保持 UI 监听不变

## 8. 典型注意点

- `RanchManager` 是门面，不是业务本体
- `RanchEventHub` 是事件总线，不要把监听散到各处
- `AbilityEffectRegistry` / `ItemEffectRegistry` / `ToyEffectRegistry` / `MapCellObjectEffectRegistry` 都是新增功能的必改点
- 新动物最好只补配置和效果脚本，别直接在 UI 层拼逻辑

## 9. 最快定位法

如果你要加功能，先问自己它属于哪一类：
- 回合推进 -> `RanchTurnService`
- 结算/能力 -> `RanchSettlementService`
- 动物部署/移动 -> `RanchAnimalService`
- 动物出生/替换 -> `RanchAnimalSpawnService`
- 动物增删流程 -> `RanchAnimalLifecycleService`
- 道具 -> `RanchItemService`
- 玩具 -> `RanchToyService`
- 地块物体 -> `MapObjectService`
- UI 刷新 -> `RanchUIController` / 各面板

如果还是不确定，先看 `RanchManager` 的对应转发方法，再顺着 service 往下找。
