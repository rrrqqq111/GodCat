using UnityEngine;

namespace NekogamiRanch.Talents
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Talent Data", fileName = "TalentData")]
    public class TalentData : ScriptableObject
    {
        [SerializeField, InspectorName("天赋名称")] private string talentName;
        [SerializeField, TextArea, InspectorName("天赋描述")] private string description;
        [SerializeField, InspectorName("天赋图片")] private Sprite icon;

        public string TalentName => string.IsNullOrWhiteSpace(talentName) ? name : talentName;
        public string DisplayName => TalentName;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite TalentImage => icon;
    }
}
