# Vibecoding 提示词与使用方法（学生版）

> 本文档用于猫神牧场教学项目。目标是让你使用 vibecoding 工具新增动物、能力、道具或玩具内容，而不是修改游戏核心框架。

## 1. 使用 vibecoding 前先记住

这个项目已经搭好了基础框架。你主要负责新增内容：

- 动物资源 `AnimalData`
- 能力资源 `AbilityData`
- 能力脚本 `IAbilityEffect`
- 注册表中的一条注册项
- 后续课程中的道具资源、玩具资源和对应效果脚本

你不应该让工具随意修改：

- `RanchManager`
- `RanchMap`
- `MapCell`
- `RanchTurnService`
- `RanchSettlementService`
- `RanchOfferService`
- UI 脚本
- 抽卡逻辑
- 结算逻辑
- 地图绑定逻辑

如果 vibecoding 工具提出要改这些脚本，先停止，不要接受。

## 2. 推荐使用流程

每次只让工具做一个小任务。

推荐顺序：

1. 先让工具阅读样例。
2. 再让工具列出计划。
3. 确认它只会新增或修改允许的文件。
4. 让工具生成内容。
5. 回到 Unity 等待编译。
6. 检查 Console 是否有红色报错。
7. 运行游戏测试能力是否生效。

不要一次让工具生成很多动物。先做一个，跑通，再做下一个。

## 3. 通用总控提示词

每次开始任务前，建议先复制下面这段。

```text
你现在在一个 Unity 教学项目中工作，项目名是猫神牧场教学项目。

你的任务只限于新增教学内容，不允许重构或修改游戏核心逻辑。

必须遵守：
1. 不要修改 RanchManager、RanchMap、MapCell、RanchTurnService、RanchSettlementService、RanchOfferService。
2. 不要修改抽卡逻辑、结算逻辑、地图绑定逻辑、UI 刷新逻辑。
3. 不要删除已有资源或脚本。
4. 不要为了让功能运行而擅自改核心框架。
5. 如果发现缺少接入点，只说明缺口，不要擅自补核心逻辑。
6. 生成内容前，先列出准备新增或修改的文件清单。
7. 只允许修改我明确允许的文件。

请优先参考这三个样例：
- 家猪 pig
- 野猪 boar
- 马 horse

请按照项目现有结构和命名方式生成内容。
```

## 4. 新增完整动物内容包

当你要新增一个“动物 + 能力资源 + 能力脚本”的完整内容时，使用这个提示词。

```text
请为猫神牧场教学项目新增一个完整动物内容包。

动物设定：
- 动物中文名：【填写动物中文名】
- 动物英文 ID：【填写英文 ID，例如 Rabbit】
- 家族 family：【填写家族，例如 Hoofed】
- 稀有度 rarity：【填写 0 到 4】
- 基础收益 baseMoney：【填写数值】
- 能力名称：【填写能力名】
- 能力效果：【用一句话说明能力做什么】
- 触发时机 triggerType：【例如 SettlementPrepare / DayStart / Moved】
- 作用范围 impactType：【例如 Self / Adjacent / Row / Field】
- 效果类型 effectType：【例如 AddMoney / Breed / Move / Transform】

请生成或修改：
1. 一个 AnimalData 资源
2. 一个 AbilityData 资源
3. 一个新的能力脚本，必须实现 IAbilityEffect
4. AbilityEffectRegistry 中追加一条注册项

文件位置要求：
- AnimalData 放在 Assets/Game/Data/Animals/[Family]/
- AbilityData 放在 Assets/Game/Data/Abilities/Animals/[Family]/
- 能力脚本放在 Assets/Game/Scripts/Abilities/[Family]/

严格限制：
- 不要修改 RanchManager
- 不要修改 RanchMap
- 不要修改 MapCell
- 不要修改 RanchTurnService
- 不要修改 RanchSettlementService
- 不要修改 RanchOfferService
- 不要修改 UI 脚本
- 不要修改抽卡逻辑
- 不要修改结算顺序

生成前请先列出准备新增或修改的文件清单。
生成后请说明：
1. AnimalData 的关键字段
2. AbilityData 的关键字段
3. effectScriptId 和注册表 key 是否一致
4. 我应该如何在 Unity 中测试
```

## 5. 只新增动物资源

如果能力已经存在，只想新增一个动物资源，用这个。

```text
请只新增一个 AnimalData 动物资源，不要写新脚本，不要修改核心逻辑。

动物设定：
- 动物中文名：【填写】
- 动物英文 ID：【填写】
- family：【填写】
- rarity：【填写 0 到 4】
- baseMoney：【填写】
- description：【填写】
- 绑定能力 AbilityData：【填写已有能力资源名】

要求：
1. 只创建 AnimalData。
2. 不要创建新的能力脚本。
3. 不要修改 AbilityEffectRegistry。
4. 不要修改 RanchManager 或抽卡逻辑。
5. 如果找不到指定 AbilityData，请告诉我，不要自己乱改脚本。

请先列出文件清单，再生成资源。
```

## 6. 只新增能力资源

如果能力脚本已经存在，只需要做一个新的能力资源，用这个。

```text
请只新增一个 AbilityData 能力资源，不要修改核心逻辑。

能力设定：
- 能力 ID：【填写】
- desc：【填写能力描述】
- triggerType：【填写】
- impactType：【填写】
- effectType：【填写】
- effectScriptId：【填写已有能力脚本注册名】
- effectParams：【填写参数】

要求：
1. AbilityData 放在 Assets/Game/Data/Abilities/Animals/[Family]/。
2. effectScriptId 必须使用 AbilityEffectRegistry 中已经注册的 key。
3. 不要新增能力脚本。
4. 不要修改 AbilityEffectRegistry。
5. 不要修改抽卡、结算、地图、UI。

如果 effectScriptId 没有注册，请只提示缺少注册，不要擅自改代码。
```

## 7. 只新增能力脚本

如果动物资源和能力资源已经建好，但缺少能力脚本，用这个。

```text
请只新增一个能力脚本，并在 AbilityEffectRegistry 中追加注册项。

脚本设定：
- 脚本类名：【例如 RabbitAbilityEffect】
- 所属家族文件夹：【例如 Hoofed】
- 对应 AbilityData.effectScriptId：【例如 RabbitAbilityEffect】
- 能力效果：【描述能力逻辑】

允许修改：
1. 新增 Assets/Game/Scripts/Abilities/[Family]/[ClassName].cs
2. 在 Assets/Game/Scripts/Abilities/AbilityEffectRegistry.cs 末尾追加一条注册项

禁止修改：
- RanchManager
- RanchMap
- MapCell
- RanchTurnService
- RanchSettlementService
- RanchOfferService
- UI 脚本
- 已有样例能力脚本

脚本要求：
1. 必须实现 IAbilityEffect。
2. Execute 方法中要做空值检查。
3. 成功执行时返回 true。
4. 未满足条件时返回 false。
5. 不要引入新的框架结构。

请先列出计划，再生成代码。
```

## 8. 新增道具内容

后续课程如果要做道具，用这个提示词。

```text
请为猫神牧场教学项目新增一个道具内容。

道具设定：
- 道具中文名：【填写】
- 道具 ID：【填写】
- rarity：【填写 0 到 4】
- category：【填写】
- triggerType：【填写】
- effectScriptId：【填写】
- 效果说明：【填写】
- effectParams：【填写】

请生成或修改：
1. 一个 ItemData 资源，放在 Assets/Game/Data/Items/
2. 如果确实需要新效果，新增一个 IItemEffect 脚本
3. 如果新增了效果脚本，只能在 ItemEffectRegistry 中追加注册项

禁止修改：
- RanchManager
- RanchItemService
- 抽卡逻辑
- 结算逻辑
- UI 逻辑

请先列出文件清单，再生成。
```

## 9. 新增玩具内容

后续课程如果要做玩具，用这个提示词。

```text
请为猫神牧场教学项目新增一个玩具内容。

玩具设定：
- 玩具中文名：【填写】
- 玩具 ID：【填写】
- rarity：【填写 0 到 4】
- slotType：【填写】
- triggerType：【填写】
- effectScriptId：【填写】
- 效果说明：【填写】
- effectParams：【填写】

请生成或修改：
1. 一个 ToyData 资源，放在 Assets/Game/Data/Toys/
2. 如果确实需要新效果，新增一个 IToyEffect 脚本
3. 如果新增了效果脚本，只能在 ToyEffectRegistry 中追加注册项

禁止修改：
- RanchManager
- RanchToyService
- 抽卡逻辑
- 结算逻辑
- UI 逻辑

请先列出文件清单，再生成。
```

## 10. 让工具先检查而不是直接改

如果你不确定项目现在缺什么，用这个提示词。

```text
请只检查当前内容是否接入完整，不要修改任何文件。

请检查：
1. AnimalData 是否绑定了 AbilityData。
2. AbilityData.effectScriptId 是否填写。
3. AbilityEffectRegistry 是否注册了这个 effectScriptId。
4. 能力脚本类名是否和注册名一致。
5. 是否存在会导致能力不触发的明显问题。

只输出检查结果和建议，不要修改文件。
```

## 11. 编译报错时的修复提示词

如果 Unity Console 出现红色编译错误，不要让工具乱改一堆文件。用这个。

```text
Unity 出现编译错误。请只修复和本次新增能力相关的错误。

错误信息：
【粘贴 Unity Console 中的错误】

限制：
1. 只能修改本次新增的能力脚本。
2. 如果必须修改 AbilityEffectRegistry，只能修正本次新增的注册项。
3. 不要修改 RanchManager、RanchMap、RanchSettlementService、RanchOfferService、UI 脚本。
4. 不要重构项目。

请先解释错误原因，再给出最小修改方案。
```

## 12. 能力不触发时的排查提示词

如果游戏能运行，但是能力没有效果，用这个。

```text
当前动物能力没有触发。请只做排查，不要直接修改文件。

动物资源：
【填写 AnimalData 名称】

能力资源：
【填写 AbilityData 名称】

能力脚本：
【填写 AbilityEffect 类名】

请检查：
1. AnimalData.ability 是否绑定正确。
2. AbilityData.triggerType 是否符合触发时机。
3. AbilityData.effectScriptId 是否和 AbilityEffectRegistry key 完全一致。
4. AbilityEffectRegistry 是否注册了对应脚本。
5. Execute 方法是否因为条件判断返回 false。
6. effectParams 是否填写合理。

请输出排查清单，不要修改核心逻辑。
```

## 13. 要求工具输出文件清单

每次生成前都可以补一句：

```text
在真正修改前，请先列出你准备新增或修改的文件清单。没有得到我确认前，不要生成或修改文件。
```

如果工具列出的文件里出现这些，就要警惕：

```text
RanchManager.cs
RanchMap.cs
MapCell.cs
RanchTurnService.cs
RanchSettlementService.cs
RanchOfferService.cs
RanchUIController.cs
AnimalOfferPanel.cs
RanchHUD.cs
```

除非老师明确要求，否则不要让工具修改它们。

## 14. 推荐的课堂使用方式

### 第一步：照样例做一个简单动物

目标：创建一个新动物，绑定一个简单加钱能力。

推荐能力效果：

```text
每天结算前，如果自己在场上，则获得 2 金币。
```

这类能力最容易测试。

### 第二步：做一个和位置有关的能力

目标：让学生理解 `impactType` 和 `targets`。

推荐能力效果：

```text
结算前，让相邻动物获得额外金币。
```

### 第三步：做一个移动类能力

目标：让学生理解地图坐标和移动。

推荐能力效果：

```text
结算前向右移动一格，成功后获得金币。
```

### 第四步：做一个组合能力

目标：让学生综合使用资源字段、参数和脚本判断。

推荐能力效果：

```text
如果周围有指定家族动物，则获得金币；否则不触发。
```

## 15. 提交前检查清单

每次完成内容后，确认：

- 新动物资源已创建。
- 新能力资源已创建。
- 新能力脚本已创建。
- 注册表已追加注册项。
- `AnimalData.ability` 已绑定能力资源。
- `AbilityData.effectScriptId` 和注册表 key 完全一致。
- Unity Console 没有红色报错。
- Play 模式下动物能出现。
- 点击结算后能力能触发。
- 没有修改核心逻辑脚本。

最后再记住一句话：

```text
先让工具做内容，不要让工具改系统。
```
