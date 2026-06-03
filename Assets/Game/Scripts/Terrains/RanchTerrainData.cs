using UnityEngine;

namespace NekogamiRanch.Terrains
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Terrain Data", fileName = "RanchTerrainData")]
    public class RanchTerrainData : ScriptableObject
    {
        [SerializeField, InspectorName("地形ID")] private string id;
        [SerializeField, InspectorName("地形名称")] private string terrainName;
        [SerializeField, TextArea, InspectorName("描述")] private string description;
        [SerializeField, InspectorName("图标")] private Sprite icon;
        [SerializeField, InspectorName("地块贴图")] private Sprite tileSprite;
        [SerializeField, InspectorName("尺寸倍率")] private Vector2 sizeMultiplier = Vector2.one;
        [SerializeField, InspectorName("更新碰撞尺寸")] private bool updateColliderSize;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string Name => string.IsNullOrWhiteSpace(terrainName) ? name : terrainName;
        public string DisplayName => Name;
        public string Description => description;
        public Sprite Icon => icon;
        public Sprite TileSprite => tileSprite != null ? tileSprite : icon;
        public bool UpdateColliderSize => updateColliderSize;

        public Vector2 SizeMultiplier
        {
            get
            {
                return new Vector2(
                    sizeMultiplier.x > 0f ? sizeMultiplier.x : 1f,
                    sizeMultiplier.y > 0f ? sizeMultiplier.y : 1f);
            }
        }
    }
}
