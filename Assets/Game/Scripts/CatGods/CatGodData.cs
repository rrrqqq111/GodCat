using UnityEngine;

namespace NekogamiRanch.CatGods
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Cat God Data", fileName = "CatGodData")]
    public class CatGodData : ScriptableObject
    {
        [SerializeField, InspectorName("猫神名称")] private string catGodName;
        [SerializeField, InspectorName("技能名称")] private string skillName;
        [SerializeField, InspectorName("技能类型")] private CatGodSkillType skillType = CatGodSkillType.Passive;
        [SerializeField, Min(0), InspectorName("主动技能CD")] private int activeSkillCooldownDays;
        [SerializeField, TextArea, InspectorName("技能描述")] private string skillDescription;
        [SerializeField, InspectorName("猫神图片")] private Sprite catGodImage;
        [SerializeField, InspectorName("猫神头像")] private Sprite icon;

        public string CatGodName => string.IsNullOrWhiteSpace(catGodName) ? name : catGodName;
        public string SkillName => skillName;
        public CatGodSkillType SkillType => skillType;
        public int ActiveSkillCooldownDays => skillType == CatGodSkillType.Active ? Mathf.Max(0, activeSkillCooldownDays) : 0;
        public string SkillDescription => skillDescription;
        public Sprite CatGodImage => catGodImage;
        public bool IsActiveSkill => skillType == CatGodSkillType.Active;
        public Sprite Icon => catGodImage;
    }
}
