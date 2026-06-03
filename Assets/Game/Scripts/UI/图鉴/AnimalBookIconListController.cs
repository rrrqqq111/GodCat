using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekogamiRanch.UI
{
    [Serializable]
    public sealed class AnimalBookRarityColor
    {
        [SerializeField, Min(0), InspectorName("稀有度")] private int rarity;
        [SerializeField, InspectorName("颜色")] private Color color = Color.white;

        public int Rarity => rarity;
        public Color Color => color;
    }

    public static class AnimalBookRarityColorUtility
    {
        public static void Apply(Image image, int rarity, IReadOnlyList<AnimalBookRarityColor> rarityColors, Color fallbackColor)
        {
            if (image == null)
            {
                return;
            }

            var color = GetColor(rarity, rarityColors, fallbackColor);
            color.a = image.color.a;
            image.color = color;
        }

        public static Color GetColor(int rarity, IReadOnlyList<AnimalBookRarityColor> rarityColors, Color fallbackColor)
        {
            if (rarityColors == null)
            {
                return fallbackColor;
            }

            for (var i = 0; i < rarityColors.Count; i++)
            {
                var rarityColor = rarityColors[i];
                if (rarityColor != null && rarityColor.Rarity == rarity)
                {
                    return rarityColor.Color;
                }
            }

            return fallbackColor;
        }
    }

    public class AnimalBookIconListController : MonoBehaviour
    {
        [SerializeField, InspectorName("动物数据根目录")] private string animalDataRoot = "Assets/Game/Data/Animals";
        [SerializeField, InspectorName("滚动矩形")] private ScrollRect scrollRect;
        [SerializeField, InspectorName("内容根节点")] private RectTransform contentRoot;
        [SerializeField, InspectorName("图标模板")] private GameObject iconTemplate;
        [SerializeField, InspectorName("模板动物图标图像")] private Image templateAnimalIconImage;
        [SerializeField, InspectorName("选中动物显示面板")] private AnimalBookSelectedAnimalPanel selectedAnimalPanel;
        [SerializeField, InspectorName("模板稀有度变色图像")] private List<Image> templateRarityColorImages = new List<Image>();
        [SerializeField, InspectorName("稀有度颜色表")] private List<AnimalBookRarityColor> rarityColors = new List<AnimalBookRarityColor>();
        [SerializeField, InspectorName("默认稀有度颜色")] private Color fallbackRarityColor = Color.white;
        [SerializeField, InspectorName("隐藏模板")] private bool hideTemplate = true;
        [SerializeField, InspectorName("编辑器自动读取动物数据")] private bool autoLoadAnimalDataInEditor = true;
        [SerializeField, InspectorName("动物数据列表")] private List<AnimalData> animalCatalog = new List<AnimalData>();
        [SerializeField, InspectorName("分类绑定列表")] private List<CategoryBinding> categories = new List<CategoryBinding>();

        private readonly List<GameObject> spawnedIcons = new List<GameObject>();
        private bool listenersRegistered;

        private void Awake()
        {
            EnsureReferences();
            LoadAnimalCatalogIfNeeded();

            if (hideTemplate && iconTemplate != null)
            {
                iconTemplate.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EnsureReferences();
            LoadAnimalCatalogIfNeeded();
            RegisterListeners();
            RefreshSelectedCategory();
        }

        private void OnDisable()
        {
            UnregisterListeners();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureReferences();
            if (autoLoadAnimalDataInEditor)
            {
                RefreshAnimalCatalogFromAssets();
            }
        }
#endif

        public void RefreshSelectedCategory()
        {
            var selectedCategory = GetSelectedCategory();
            RefreshCategory(selectedCategory);
        }

        public void RefreshCategory(CategoryBinding category)
        {
            ClearSpawnedIcons();

            if (contentRoot == null || iconTemplate == null)
            {
                Debug.LogWarning("[AnimalBookIconListController] Content root or icon template is missing.", this);
                return;
            }

            var animals = FilterAnimals(category).ToList();
            if (animals.Count > 0)
            {
                SelectAnimal(animals[0]);
            }

            foreach (var animalData in animals)
            {
                CreateIcon(animalData);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            if (scrollRect != null)
            {
                scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }
        }

        private void EnsureReferences()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponentInChildren<ScrollRect>(true);
            }

            if (contentRoot == null && scrollRect != null)
            {
                contentRoot = scrollRect.content;
            }

            if (iconTemplate == null && contentRoot != null && contentRoot.childCount > 0)
            {
                iconTemplate = contentRoot.GetChild(0).gameObject;
            }

            if (templateAnimalIconImage == null && iconTemplate != null)
            {
                templateAnimalIconImage = FindBestTemplateIconImage(iconTemplate);
            }
        }

        private void RegisterListeners()
        {
            if (listenersRegistered)
            {
                return;
            }

            for (var i = 0; i < categories.Count; i++)
            {
                var toggle = categories[i].toggle;
                if (toggle == null)
                {
                    continue;
                }

                toggle.onValueChanged.RemoveListener(OnCategoryToggleChanged);
                toggle.onValueChanged.AddListener(OnCategoryToggleChanged);
            }

            listenersRegistered = true;
        }

        private void UnregisterListeners()
        {
            for (var i = 0; i < categories.Count; i++)
            {
                if (categories[i].toggle != null)
                {
                    categories[i].toggle.onValueChanged.RemoveListener(OnCategoryToggleChanged);
                }
            }

            listenersRegistered = false;
        }

        private void OnCategoryToggleChanged(bool isOn)
        {
            if (isOn)
            {
                RefreshSelectedCategory();
            }
        }

        private CategoryBinding GetSelectedCategory()
        {
            for (var i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                if (category.toggle != null && category.toggle.isOn)
                {
                    return category;
                }
            }

            return categories.Count > 0 ? categories[0] : null;
        }

        private IEnumerable<AnimalData> FilterAnimals(CategoryBinding category)
        {
            var animals = animalCatalog.Where(data => data != null);
            if (category != null && !category.showAll && !string.IsNullOrWhiteSpace(category.family))
            {
                var family = category.family.Trim();
                animals = animals.Where(data => string.Equals(data.Family, family, StringComparison.OrdinalIgnoreCase));
            }

            return animals
                .OrderBy(data => data.Rarity)
                .ThenBy(data => data.Family)
                .ThenBy(data => data.DisplayName)
                .ThenBy(data => data.Id);
        }

        private void CreateIcon(AnimalData animalData)
        {
            var iconObject = Instantiate(iconTemplate, contentRoot);
            iconObject.name = $"AnimalIcon_{animalData.Id}";
            iconObject.SetActive(true);
            spawnedIcons.Add(iconObject);

            var iconImage = FindSpawnedComponent(iconObject, templateAnimalIconImage);
            if (iconImage == null)
            {
                iconImage = FindBestTemplateIconImage(iconObject);
            }

            ApplyImage(iconImage, animalData.Icon);
            ApplyRarityColors(iconObject, animalData.Rarity);

            var button = iconObject.GetComponent<Button>();
            if (button == null)
            {
                button = iconObject.AddComponent<Button>();
            }

            if (button.targetGraphic == null)
            {
                button.targetGraphic = iconObject.GetComponent<Graphic>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectAnimal(animalData));
        }

        private void SelectAnimal(AnimalData animalData)
        {
            if (selectedAnimalPanel != null)
            {
                selectedAnimalPanel.Refresh(animalData);
            }
        }

        private void ApplyRarityColors(GameObject iconObject, int rarity)
        {
            for (var i = 0; i < templateRarityColorImages.Count; i++)
            {
                var image = FindSpawnedComponent(iconObject, templateRarityColorImages[i]);
                if (image == null && templateRarityColorImages[i] != null)
                {
                    Debug.LogWarning("[AnimalBookIconListController] Rarity color image must be on the icon template or one of its children.", this);
                    continue;
                }

                AnimalBookRarityColorUtility.Apply(image, rarity, rarityColors, fallbackRarityColor);
            }
        }

        private Image FindSpawnedComponent(GameObject spawnedObject, Image templateImage)
        {
            if (spawnedObject == null || templateImage == null || iconTemplate == null)
            {
                return null;
            }

            var path = GetRelativePath(iconTemplate.transform, templateImage.transform);
            if (path == null)
            {
                return null;
            }

            if (path.Length == 0)
            {
                return spawnedObject.GetComponent<Image>();
            }

            var target = spawnedObject.transform.Find(path);
            return target != null ? target.GetComponent<Image>() : null;
        }

        private static Image FindBestTemplateIconImage(GameObject root)
        {
            if (root == null)
            {
                return null;
            }

            var images = root.GetComponentsInChildren<Image>(true);
            if (images.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < images.Length; i++)
            {
                if (images[i] != null && images[i].gameObject != root)
                {
                    return images[i];
                }
            }

            return images[0];
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return null;
            }

            if (root == target)
            {
                return string.Empty;
            }

            var names = new List<string>();
            var current = target;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }

            if (current != root)
            {
                return null;
            }

            names.Reverse();
            return string.Join("/", names);
        }

        private void ClearSpawnedIcons()
        {
            for (var i = spawnedIcons.Count - 1; i >= 0; i--)
            {
                if (spawnedIcons[i] != null)
                {
                    DestroyIcon(spawnedIcons[i]);
                }
            }

            spawnedIcons.Clear();

            if (contentRoot == null || iconTemplate == null)
            {
                return;
            }

            for (var i = contentRoot.childCount - 1; i >= 0; i--)
            {
                var child = contentRoot.GetChild(i);
                if (child != null && child.gameObject != iconTemplate)
                {
                    DestroyIcon(child.gameObject);
                }
            }
        }

        private void DestroyIcon(GameObject iconObject)
        {
            if (iconObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(iconObject);
            }
            else
            {
                DestroyImmediate(iconObject);
            }
        }

        private static void ApplyImage(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.enabled = sprite != null;
            image.preserveAspect = true;
        }

        private void LoadAnimalCatalogIfNeeded()
        {
            if (animalCatalog.Count > 0)
            {
                return;
            }

#if UNITY_EDITOR
            if (autoLoadAnimalDataInEditor)
            {
                RefreshAnimalCatalogFromAssets();
            }
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh Animal Catalog From Assets")]
        private void RefreshAnimalCatalogFromAssets()
        {
            if (string.IsNullOrWhiteSpace(animalDataRoot))
            {
                return;
            }

            animalCatalog = AssetDatabase.FindAssets("t:AnimalData", new[] { animalDataRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AnimalData>)
                .Where(data => data != null)
                .OrderBy(data => data.Rarity)
                .ThenBy(data => data.Family)
                .ThenBy(data => data.DisplayName)
                .ThenBy(data => data.Id)
                .ToList();
        }
#endif

        [Serializable]
        public sealed class CategoryBinding
        {
            [InspectorName("标签")]
            public Toggle toggle;
            [InspectorName("动物家族")]
            public string family;
            [InspectorName("显示全部")]
            public bool showAll;
        }
    }
}
