using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekogamiRanch.UI
{
    public class BookToggleFolderAssetLoader : MonoBehaviour
    {
        [SerializeField, InspectorName("触发标签")] private Toggle toggle;
        [SerializeField, InspectorName("资源文件夹")] private string assetFolder;
        [SerializeField, InspectorName("资源类型名")] private string assetTypeName = "ScriptableObject";
        [SerializeField, InspectorName("名称字段名")] private string nameFieldName = "DisplayName";
        [SerializeField, InspectorName("图像字段名")] private string iconFieldName = "Icon";
        [SerializeField, InspectorName("滚动矩形")] private ScrollRect scrollRect;
        [SerializeField, InspectorName("内容根节点")] private RectTransform contentRoot;
        [SerializeField, InspectorName("图标模板")] private GameObject iconTemplate;
        [SerializeField, InspectorName("模板图像组件")] private Image templateIconImage;
        [SerializeField, InspectorName("模板名称文本")] private TMP_Text templateNameText;
        [SerializeField, InspectorName("隐藏模板")] private bool hideTemplate = true;
        [SerializeField, InspectorName("刷新后自动选择第一个")] private bool autoSelectFirstAsset = true;
        [SerializeField, InspectorName("编辑器自动读取资源")] private bool autoLoadAssetsInEditor = true;
        [SerializeField, InspectorName("缓存资源列表")] private List<ScriptableObject> cachedAssets = new List<ScriptableObject>();
        [SerializeField, InspectorName("点击资源事件")] private UnityEvent<ScriptableObject> onAssetClicked;

        private readonly List<GameObject> spawnedIcons = new List<GameObject>();
        private bool listenerRegistered;

        private void Awake()
        {
            EnsureReferences();
            RefreshCachedAssetsIfNeeded();

            if (hideTemplate && iconTemplate != null)
            {
                iconTemplate.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EnsureReferences();
            RefreshCachedAssetsIfNeeded();
            RegisterListener();

            if (toggle == null || toggle.isOn)
            {
                RefreshList();
            }
        }

        private void OnDisable()
        {
            UnregisterListener();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureReferences();
            if (autoLoadAssetsInEditor)
            {
                RefreshCachedAssetsFromFolder();
            }
        }
#endif

        public void RefreshList()
        {
            ClearSpawnedIcons();

            if (contentRoot == null || iconTemplate == null)
            {
                Debug.LogWarning("[BookToggleFolderAssetLoader] Content root or icon template is missing.", this);
                return;
            }

            var sortedAssets = SortAssets(cachedAssets).ToList();
            foreach (var asset in sortedAssets)
            {
                CreateIcon(asset);
            }

            if (autoSelectFirstAsset && sortedAssets.Count > 0)
            {
                SelectAsset(sortedAssets[0]);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
            if (scrollRect != null)
            {
                scrollRect.normalizedPosition = new Vector2(0f, 1f);
            }
        }

        private void EnsureReferences()
        {
            if (toggle == null)
            {
                toggle = GetComponent<Toggle>();
            }

            if (scrollRect == null)
            {
                scrollRect = GetComponentInParent<ScrollRect>();
            }

            if (contentRoot == null && scrollRect != null)
            {
                contentRoot = scrollRect.content;
            }

            if (iconTemplate == null && contentRoot != null && contentRoot.childCount > 0)
            {
                iconTemplate = contentRoot.GetChild(0).gameObject;
            }

            if (templateIconImage == null && iconTemplate != null)
            {
                templateIconImage = FindBestTemplateIconImage(iconTemplate);
            }

            if (templateNameText == null && iconTemplate != null)
            {
                templateNameText = iconTemplate.GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void RegisterListener()
        {
            if (listenerRegistered || toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            listenerRegistered = true;
        }

        private void UnregisterListener()
        {
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }

            listenerRegistered = false;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                RefreshList();
            }
        }

        private void CreateIcon(ScriptableObject asset)
        {
            if (asset == null)
            {
                return;
            }

            var iconObject = Instantiate(iconTemplate, contentRoot);
            iconObject.name = $"BookIcon_{asset.name}";
            iconObject.SetActive(true);
            spawnedIcons.Add(iconObject);

            var iconImage = FindSpawnedImage(iconObject, templateIconImage);
            if (iconImage == null)
            {
                iconImage = FindBestTemplateIconImage(iconObject);
            }

            ApplyImage(iconImage, ReadSprite(asset, iconFieldName));

            var nameText = FindSpawnedText(iconObject, templateNameText);
            if (nameText == null)
            {
                nameText = iconObject.GetComponentInChildren<TMP_Text>(true);
            }

            if (nameText != null)
            {
                nameText.text = ReadName(asset, nameFieldName);
            }

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
            button.onClick.AddListener(() => SelectAsset(asset));
        }

        private void SelectAsset(ScriptableObject asset)
        {
            if (asset != null)
            {
                onAssetClicked?.Invoke(asset);
            }
        }

        private Image FindSpawnedImage(GameObject spawnedObject, Image templateImage)
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

        private TMP_Text FindSpawnedText(GameObject spawnedObject, TMP_Text templateText)
        {
            if (spawnedObject == null || templateText == null || iconTemplate == null)
            {
                return null;
            }

            var path = GetRelativePath(iconTemplate.transform, templateText.transform);
            if (path == null)
            {
                return null;
            }

            if (path.Length == 0)
            {
                return spawnedObject.GetComponent<TMP_Text>();
            }

            var target = spawnedObject.transform.Find(path);
            return target != null ? target.GetComponent<TMP_Text>() : null;
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

        private void RefreshCachedAssetsIfNeeded()
        {
#if UNITY_EDITOR
            if (autoLoadAssetsInEditor && cachedAssets.Count == 0)
            {
                RefreshCachedAssetsFromFolder();
            }
#endif
        }

        private static IEnumerable<ScriptableObject> SortAssets(IEnumerable<ScriptableObject> assets)
        {
            return assets
                .Where(asset => asset != null)
                .OrderBy(asset => ReadName(asset, "DisplayName"))
                .ThenBy(asset => asset.name);
        }

        private static string ReadName(ScriptableObject asset, string memberName)
        {
            if (asset == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(memberName) && TryReadMember(asset, memberName, out string value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (TryReadMember(asset, "DisplayName", out string displayName) && !string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            if (TryReadMember(asset, "Name", out string name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            if (TryReadMember(asset, "Id", out string id) && !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return asset.name;
        }

        private static Sprite ReadSprite(ScriptableObject asset, string memberName)
        {
            if (asset == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            if (TryReadMember(asset, memberName, out Sprite sprite))
            {
                return sprite;
            }

            if (TryReadMember(asset, memberName, out Texture2D texture) && texture != null)
            {
                return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }

            return null;
        }

        private static bool TryReadMember<T>(ScriptableObject asset, string memberName, out T value)
        {
            value = default;
            if (asset == null || string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            var type = asset.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var property = type.GetProperty(memberName, flags);
            if (property != null && typeof(T).IsAssignableFrom(property.PropertyType))
            {
                value = (T)property.GetValue(asset);
                return true;
            }

            var field = type.GetField(memberName, flags);
            if (field != null && typeof(T).IsAssignableFrom(field.FieldType))
            {
                value = (T)field.GetValue(asset);
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("刷新资源缓存")]
        private void RefreshCachedAssetsFromFolder()
        {
            cachedAssets.Clear();
            if (string.IsNullOrWhiteSpace(assetFolder))
            {
                return;
            }

            var filter = string.IsNullOrWhiteSpace(assetTypeName) ? "t:ScriptableObject" : $"t:{assetTypeName.Trim()}";
            cachedAssets = AssetDatabase.FindAssets(filter, new[] { assetFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ScriptableObject>)
                .Where(asset => asset != null)
                .OrderBy(asset => ReadName(asset, nameFieldName))
                .ThenBy(asset => asset.name)
                .ToList();
        }
#endif
    }
}
