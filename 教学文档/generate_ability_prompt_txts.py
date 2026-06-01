from __future__ import annotations

import re
import shutil
import json
from dataclasses import dataclass, field
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
OUT_ROOT = ROOT / "教学文档" / "提示词文档大全"


@dataclass
class AbilityAsset:
    path: Path
    guid: str
    name: str
    ability_id: str
    desc: str
    trigger_type: str
    impact_type: str
    effect_type: str
    effect_script_id: str
    effect_params: dict[str, str]
    sub_guids: list[str] = field(default_factory=list)


@dataclass
class AnimalAsset:
    path: Path
    guid: str
    animal_id: str
    animal_name: str
    family: str
    rarity: str
    base_money: str
    description: str
    ability_guid: str


def read_meta_guid(asset_path: Path) -> str:
    meta_path = asset_path.with_suffix(asset_path.suffix + ".meta")
    if not meta_path.exists():
        return ""
    match = re.search(r"^guid:\s*([0-9a-fA-F]+)\s*$", meta_path.read_text(encoding="utf-8", errors="replace"), re.M)
    return match.group(1) if match else ""


def unquote_yaml(value: str) -> str:
    value = value.strip()
    if value.startswith('"') and value.endswith('"'):
        try:
            return json.loads(value)
        except json.JSONDecodeError:
            value = value[1:-1]
    if "\\u" in value:
        try:
            value = value.encode("utf-8").decode("unicode_escape")
        except UnicodeDecodeError:
            pass
    return value


def get_scalar(text: str, key: str, default: str = "") -> str:
    match = re.search(rf"^[ \t]*{re.escape(key)}:[ \t]*(.*)$", text, re.M)
    return unquote_yaml(match.group(1)) if match else default


def get_guid_field(text: str, key: str) -> str:
    match = re.search(rf"^[ \t]*{re.escape(key)}:[ \t]*\{{[^}}]*guid:[ \t]*([0-9a-fA-F]+)", text, re.M)
    return match.group(1) if match else ""


def get_sub_guids(text: str) -> list[str]:
    lines = text.splitlines()
    result: list[str] = []
    in_block = False
    for line in lines:
        if re.match(r"^[ \t]*subAbilities:[ \t]*$", line):
            in_block = True
            continue
        if in_block:
            if re.match(r"^[ \t]{2}-[ \t]*\{", line):
                match = re.search(r"guid:[ \t]*([0-9a-fA-F]+)", line)
                if match:
                    result.append(match.group(1))
                continue
            if re.match(r"^[ \t]{2}[A-Za-z_]\w*:", line):
                break
    return result


def parse_params(text: str) -> dict[str, str]:
    params: dict[str, str] = {}
    lines = text.splitlines()
    in_block = False
    for line in lines:
        if re.match(r"^[ \t]*effectParams:[ \t]*$", line):
            in_block = True
            continue
        if not in_block:
            continue
        if re.match(r"^[ \t]{2}[A-Za-z_]\w*:", line):
            break
        item = re.match(r"^[ \t]{4}([A-Za-z_]\w*):[ \t]*(.*)$", line)
        if item:
            key, value = item.groups()
            params[key] = unquote_yaml(value)
    return params


def parse_ability(path: Path) -> AbilityAsset:
    text = path.read_text(encoding="utf-8", errors="replace")
    return AbilityAsset(
        path=path,
        guid=read_meta_guid(path),
        name=get_scalar(text, "m_Name", path.stem),
        ability_id=get_scalar(text, "id", path.stem),
        desc=get_scalar(text, "desc"),
        trigger_type=get_scalar(text, "triggerType"),
        impact_type=get_scalar(text, "impactType"),
        effect_type=get_scalar(text, "effectType"),
        effect_script_id=get_scalar(text, "effectScriptId"),
        effect_params=parse_params(text),
        sub_guids=get_sub_guids(text),
    )


def parse_animal(path: Path) -> AnimalAsset:
    text = path.read_text(encoding="utf-8", errors="replace")
    return AnimalAsset(
        path=path,
        guid=read_meta_guid(path),
        animal_id=get_scalar(text, "id", path.stem),
        animal_name=get_scalar(text, "animalName", path.stem),
        family=get_scalar(text, "family", path.parent.name),
        rarity=get_scalar(text, "rarity"),
        base_money=get_scalar(text, "baseMoney"),
        description=get_scalar(text, "description"),
        ability_guid=get_guid_field(text, "ability"),
    )


def script_summary(script_name: str, animal_name: str, ability: AbilityAsset, source: str) -> str:
    params = ability.effect_params
    money = params.get("money", "0")
    count = params.get("count", "0")
    target_family = params.get("targetFamily", "None")
    max_count = params.get("maxCount", "0")
    max_rarity = params.get("maxRarity", "0")
    item_count = params.get("itemCount", "0")
    transform = params.get("transformChancePercent", "100")

    custom: dict[str, str] = {
        "PigAbilityEffect": f"当效果类型为 SellSelf 时，出售自身，成功后给牧场增加 {money} 金币。",
        "SheepAbilityEffect": f"当效果类型为 SellSelf 时，出售自身，成功后给牧场增加 {money} 金币。",
        "BoarAbilityEffect": f"统计场上野猪数量，若不少于 2 只则加 {money} 金币；再从相邻目标中找 Pig，按 {transform}% 概率把一只 Pig 转化为当前动物的数据。",
        "HorseAbilityEffect": f"沿当前行向右寻找连续空格，移动到最右可达空格；每移动 1 格获得 {money} 金币。",
        "GazelleAbilityEffect": f"寻找上方第一个有动物的相邻格，与其交换位置；交换成功后获得 {money} 金币。",
        "DonkeyAbilityEffect": f"如果场上存在 Horse 或 Zebra，则获得 {money} 金币。",
        "AlpacaAbilityEffect": f"给所有 targets 添加额外金币倍率，倍率取 effectParams.money={money}，是否可叠加取 AbilityData.stackable。",
        "ZebraAbilityEffect": f"给自己永久增加 {money} 点基础收益。",
        "WaterBuffaloAbilityEffect": f"只有自己站在 Puddle 地块上才触发，成功后给自己永久增加 {money} 点基础收益。",
        "CamelAbilityEffect": "只有自己站在 Sand 地块上才触发；随机选择一个相邻动物，手动触发该动物自己的能力。",
        "MuskOxAbilityEffect": f"扫描同一行其他动物，给其中 family 为 Hoofed 的动物永久增加 {money} 点基础收益。",
        "CalfAbilityEffect": "当 effectType 为 GrowUp 时，从 effectParams 的成长目标 A/B/C 及权重中随机选择一个，调用 RanchManager.GrowAnimal 成长。",
        "LambAbilityEffect": "当 effectType 为 GrowUp 时，从 effectParams 的成长目标 A/B/C 及权重中随机选择一个，调用 RanchManager.GrowAnimal 成长。",
        "GoatAbilityEffect": "当自己被移除时，在被移除坐标附近或随机空地生成 effectParams.animalData 指定的后代动物。",
        "CowAbilityEffect": "当相邻动物被捕食时，如果被捕食动物是水牛/奶牛/麝牛，则在空地生成 effectParams.animalData 指定的动物。",
        "CapreolusAbilityEffect": f"遍历 targets，找到后出售目标动物，每成功出售一只获得 {money} 金币。",
        "ElkAbilityEffect": f"根据同家族数量决定加成，给同家族目标永久增加基础收益，数值受 minMultiplier/maxMultiplier 约束。",
        "HippoAbilityEffect": f"统计相邻地块中的 Puddle 数量，按数量给自己永久增加基础收益，每个水洼 {money} 点。",
        "ReindeerAbilityEffect": f"给稀有度不高于 maxRarity={max_rarity} 的 targets 永久增加 {money} 点基础收益。",
        "RhinoAbilityEffect": f"尝试生成一只 family 为 {target_family} 的随机动物，并给新动物 {money} 点基础收益加成。",
        "RainbowUnicornAbilityEffect": f"调用 RanchManager.TryAddRandomItem，尝试获得随机道具，次数取 itemCount={item_count}，至少 1 次。",
        "GiraffeAbilityEffect": f"这是被动保护能力：创建全局保护规则；保护成功时给保护者永久增加 {money} 点基础收益。",
        "TigerAbilityEffect": f"捕食相邻 Hoofed 动物，成功捕食后每吃一只给自己永久增加 {money} 点基础收益。",
        "CrocodileAbilityEffect": "捕食相邻 Hoofed 动物；成功后按自己的进化等级获得金币，至少 1 金币。",
        "SaltwaterCrocodileAbilityEffect": "捕食全场 Hoofed 动物，随机选目标；成功后按自己的进化等级获得罐头，至少 1 个。",
        "GrayWolfAbilityEffect": f"捕食相邻 Hoofed 动物，随机选 1 个；成功时获得 {money} 金币。",
        "LionessAbilityEffect": f"捕食相邻 Hoofed 动物，随机选 1 个；成功时获得 {money} 金币。",
        "OwlAbilityEffect": f"捕食全场稀有度不高于 maxRarity={max_rarity} 的动物，随机选 1 个；成功后按猎物基础收益乘以 {money} 给自己加永久基础收益。",
        "SnowLeopardAbilityEffect": f"捕食相邻稀有度不高于 maxRarity={max_rarity} 的动物，随机选 1 个；成功后按猎物基础收益乘以 {money} 给自己加永久基础收益。",
        "BrownBearAbilityEffect": f"捕食相邻非 Carnivora 动物，随机选 1 个；成功后给自己永久增加 {money} 点基础收益。",
        "CheetahAbilityEffect": f"向右冲刺最多 count={count} 格，移动到最远可达空格；成功后获得 {money} 金币。",
        "CoyoteAbilityEffect": f"统计自身 family 数量，若数量不超过 maxCount={max_count} 获得 {money} 金币，否则扣 {params.get('penalty', money)} 金币。",
        "BadgerAbilityEffect": f"统计场上 Badger 数量，若不超过 1 只获得 {money} 金币，否则扣除同等惩罚。",
        "HyenaAbilityEffect": f"普通能力按同家族数量给钱；伏击能力则在相邻动物被移除时捕食附近 Hoofed 目标，并获得 {money} 金币。",
        "MaleLionAbilityEffect": f"若场上存在 Lioness，则给自己永久增加 {money} 点基础收益。",
        "PandaAbilityEffect": f"随机选择一个 target，给它永久增加 {money} 点基础收益。",
        "RedPandaAbilityEffect": "向右侧相邻空格生成一只不属于自身 family 的随机动物。",
        "RaccoonAbilityEffect": f"当 effectType 为 CooldownMoney 时获得 {money} 金币，冷却由 ConfiguredAnimalAbility 统一管理。",
        "SheepdogAbilityEffect": "这是被动保护能力：创建保护 Hoofed 动物、抵御 Carnivora 捕食者的规则。",
        "SkunkAbilityEffect": f"如果相邻存在空格则获得 {money} 金币，否则扣除惩罚值。",
        "TanukiAbilityEffect": f"随机选择一个 target，把目标替换成自身动物数据；成功后获得 {money} 金币。",
        "ChickenAbilityEffect": "当 effectType 为 GrowUp 时，从 effectParams 的成长目标 A/B/C 及权重中随机选择一个，调用 RanchManager.GrowAnimal 成长。",
        "HenAbilityEffect": "检查周围是否有 Rooster；如果有，则在空格生成 effectParams.animalData 指定的小鸡。",
        "RoosterAbilityEffect": "统计相邻 Hen 数量；没有 Hen 时扣除自身基础收益，有 Hen 时按数量加钱。",
        "GooseAbilityEffect": "这是被动保护能力：创建保护相邻 Bird 动物的规则，使相邻鸟类不能被吃掉。",
        "TurkeyAbilityEffect": f"统计自身周围相邻动物数量，按数量乘以 {money} 获得金币。",
        "SwanCountAbilityEffect": f"当场上同 ID 动物数量达到 count={count} 时，获得 {money} 金币。",
        "SwanPuddleAbilityEffect": f"如果相邻地块存在 Puddle，则获得 {money} 金币。",
        "FlamingoAbilityEffect": "根据 effectType 分支：AddBaseMoneyPerSameAnimalGroup 按同种动物每 count 只获得一倍自身基础收益；BreedOnAdjacentPuddle 则在相邻 Puddle 时繁殖自身。",
        "BreedAbilityEffect": "通用繁殖效果：当 effectType 为 Breed 或 Reproduce 时，在相邻空格或随机空格生成后代，默认后代为自己。",
    }

    if script_name in custom:
        return custom[script_name]

    if "TryPrey" in source:
        return "这是捕食类能力：构造 PreyContext 和 PreyTargetRule，调用 RanchManager.TryPrey，并根据结果给予收益或成长。"
    if "TryMoveAnimal" in source:
        return "这是移动类能力：根据地图坐标寻找目标格，调用 RanchManager.TryMoveAnimal，成功后给予奖励。"
    if "TrySwapAnimals" in source:
        return "这是交换位置类能力：找到目标动物，调用 RanchManager.TrySwapAnimals，成功后给予奖励。"
    if "GrowAnimal" in source:
        return "这是成长类能力：根据参数选择成长目标，并调用 RanchManager.GrowAnimal。"
    if "AddPermanentBaseMoneyBonus" in source:
        return "这是永久基础收益加成能力：满足条件后调用 AddPermanentBaseMoneyBonus。"
    if "AddMoney" in source:
        return f"这是金币能力：满足条件后调用 RanchManager.AddMoney，主要数值来自 effectParams.money={money}。"
    return f"请逐行复刻 {script_name} 的逻辑，保留空值检查、条件判断和返回值语义。"


def important_params(params: dict[str, str]) -> str:
    keys = [
        "money", "count", "maxCount", "minMultiplier", "maxMultiplier", "maxRarity", "itemCount",
        "initialCooldownDays", "cooldownDays", "cooldownReductionAmount", "cooldownReductionTileType",
        "durationDays", "transformChancePercent", "type", "target", "targetFamily",
        "animalData", "growUpAnimalDataA", "growUpAnimalDataB", "growUpAnimalDataC",
        "growUpWeightA", "growUpWeightB", "growUpWeightC", "penalty"
    ]
    lines = []
    for key in keys:
        value = params.get(key)
        if value not in (None, "", "{fileID: 0}", "0", "None"):
            lines.append(f"- {key}: {value}")
    return "\n".join(lines) if lines else "- 当前能力主要依赖脚本内规则，参数可按样例保留默认值。"


def txt_name(name: str) -> str:
    invalid = '<>:"/\\|?*'
    result = ''.join('_' if c in invalid else c for c in name).strip()
    return result or "未命名动物"


def main():
    abilities = [parse_ability(p) for p in (ROOT / "Assets/Game/Data/Abilities/Animals").rglob("*.asset")]
    ability_by_guid = {a.guid: a for a in abilities if a.guid}
    animals = [parse_animal(p) for p in (ROOT / "Assets/Game/Data/Animals").rglob("*.asset")]
    scripts_by_name = {
        p.stem: p for p in (ROOT / "Assets/Game/Scripts/Abilities").rglob("*AbilityEffect.cs")
    }

    if OUT_ROOT.exists():
        shutil.rmtree(OUT_ROOT)
    OUT_ROOT.mkdir(parents=True, exist_ok=True)

    index_lines = ["# 提示词文档大全索引", "", "按家族文件夹分类，每个 txt 文件对应一个动物。", ""]

    for animal in sorted(animals, key=lambda a: (a.family, a.animal_name, a.animal_id)):
        root_ability = ability_by_guid.get(animal.ability_guid)
        if not root_ability:
            continue

        related = [root_ability]
        for sub_guid in root_ability.sub_guids:
            sub = ability_by_guid.get(sub_guid)
            if sub:
                related.append(sub)

        effect_names = []
        for ability in related:
            if ability.effect_script_id and ability.effect_script_id not in effect_names:
                effect_names.append(ability.effect_script_id)

        family_dir = OUT_ROOT / animal.family
        family_dir.mkdir(parents=True, exist_ok=True)
        out_path = family_dir / f"{txt_name(animal.animal_name)}.txt"
        index_lines.append(f"- {animal.family}/{animal.animal_name}.txt")

        content: list[str] = []
        content.append(f"{animal.animal_name}（{animal.animal_id}）能力脚本生成提示词")
        content.append("=" * 40)
        content.append("")
        content.append("【使用方式】")
        content.append("把下面整段提示词复制给 vibecoding 工具。要求工具先列出文件清单，确认只会新增/修改允许的文件，再让它生成。")
        content.append("")
        content.append("【提示词开始】")
        content.append("")
        content.append("你现在在 Unity 项目《猫神牧场教学项目》中工作。")
        content.append("请根据下面的动物设定、能力资源字段和脚本行为，生成这个动物对应的能力脚本与必要注册。")
        content.append("")
        content.append("硬性限制：")
        content.append("1. 不要修改 RanchManager、RanchMap、MapCell、RanchTurnService、RanchSettlementService、RanchOfferService。")
        content.append("2. 不要修改抽卡逻辑、结算逻辑、地图绑定逻辑、UI 逻辑。")
        content.append("3. 不要删除已有资源或脚本。")
        content.append("4. 如果需要注册能力，只能在 AbilityEffectRegistry 的字典末尾追加注册项。")
        content.append("5. 新脚本必须实现 IAbilityEffect；如果说明是被动保护能力，还要实现 IPassiveProtectionEffect。")
        content.append("6. 生成前先列出准备新增或修改的文件清单。")
        content.append("")
        content.append("允许修改：")
        content.append(f"- Assets/Game/Scripts/Abilities/{animal.family}/ 下新增或修正本动物能力脚本。")
        content.append("- Assets/Game/Scripts/Abilities/AbilityEffectRegistry.cs 中追加本能力注册项。")
        content.append("- 如资源尚未存在，可以新增对应 AnimalData 与 AbilityData，但不要改核心逻辑。")
        content.append("")
        content.append("动物资源设定：")
        content.append(f"- 动物中文名: {animal.animal_name}")
        content.append(f"- 动物 ID: {animal.animal_id}")
        content.append(f"- 家族 family: {animal.family}")
        content.append(f"- 稀有度 rarity: {animal.rarity}")
        content.append(f"- 基础收益 baseMoney: {animal.base_money}")
        content.append(f"- 动物描述: {animal.description}")
        content.append(f"- 动物资源路径: {animal.path.as_posix()}")
        content.append("")
        content.append("能力资源字段：")
        for ability in related:
            content.append(f"- 能力资源: {ability.name}")
            content.append(f"  - 路径: {ability.path.as_posix()}")
            content.append(f"  - id: {ability.ability_id}")
            content.append(f"  - desc: {ability.desc}")
            content.append(f"  - triggerType: {ability.trigger_type}")
            content.append(f"  - impactType: {ability.impact_type}")
            content.append(f"  - effectType: {ability.effect_type}")
            content.append(f"  - effectScriptId: {ability.effect_script_id or '空，表示这是容器能力；请展开 subAbilities'}")
            content.append("  - 关键 effectParams:")
            for line in important_params(ability.effect_params).splitlines():
                content.append(f"    {line}")
        content.append("")
        content.append("需要生成或复刻的能力脚本行为：")
        if not effect_names:
            content.append("- 这个动物的主能力是容器能力，请根据上面的 subAbilities 分别生成对应效果脚本。")
        for effect_name in effect_names:
            script_path = scripts_by_name.get(effect_name)
            source = script_path.read_text(encoding="utf-8", errors="replace") if script_path and script_path.exists() else ""
            ability_for_effect = next((a for a in related if a.effect_script_id == effect_name), related[-1])
            content.append(f"- {effect_name}:")
            content.append(f"  - 建议脚本路径: Assets/Game/Scripts/Abilities/{animal.family}/{effect_name}.cs")
            content.append(f"  - 注册 key: \"{effect_name}\"")
            content.append(f"  - 行为要求: {script_summary(effect_name, animal.animal_name, ability_for_effect, source)}")
        content.append("")
        content.append("代码风格要求：")
        content.append("- namespace 使用 NekogamiRanch.Abilities。")
        content.append("- 必要 using 通常包括 System、System.Collections.Generic、System.Linq、UnityEngine、NekogamiRanch.Animals、NekogamiRanch.Ranch、NekogamiRanch.Abilities.Prey。")
        content.append("- Execute 方法必须先检查 context、context.Owner、context.RanchManager、abilityData 是否为空。")
        content.append("- 未满足条件时返回 false，成功应用效果时返回 true。")
        content.append("- 数值优先读取 abilityData.EffectParams，不要写死在核心管理器里。")
        content.append("- 不要新增新的框架、服务或管理器。")
        content.append("")
        content.append("注册要求：")
        for effect_name in effect_names:
            content.append(f"- 在 AbilityEffectRegistry 的 Effects 字典中追加: {{ \"{effect_name}\", new {effect_name}() }},")
        content.append("- AbilityData.effectScriptId 必须和注册 key 完全一致。")
        content.append("")
        content.append("测试要求：")
        content.append("1. Unity 编译无红色报错。")
        content.append("2. AnimalData.ability 正确绑定 AbilityData。")
        content.append("3. AbilityData.effectScriptId 能在 AbilityEffectRegistry 找到。")
        content.append("4. 进入 Play 模式，放置或抽到该动物。")
        content.append("5. 触发对应 triggerType，观察能力是否按描述执行。")
        content.append("")
        content.append("输出要求：")
        content.append("1. 先列出准备新增/修改文件。")
        content.append("2. 再生成代码。")
        content.append("3. 最后说明如何在 Unity 中测试。")
        content.append("")
        content.append("【提示词结束】")
        content.append("")
        out_path.write_text("\n".join(content), encoding="utf-8")

    (OUT_ROOT / "索引.md").write_text("\n".join(index_lines) + "\n", encoding="utf-8")
    print(f"generated {len(list(OUT_ROOT.rglob('*.txt')))} prompt files in {OUT_ROOT}")


if __name__ == "__main__":
    main()
