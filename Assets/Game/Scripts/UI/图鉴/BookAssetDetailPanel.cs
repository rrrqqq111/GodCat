using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class BookAssetDetailPanel : MonoBehaviour
    {
        [SerializeField, InspectorName("属性绑定列表")] private List<MemberBinding> memberBindings = new List<MemberBinding>();
        [SerializeField, InspectorName("空文本")] private string emptyText = string.Empty;
        [SerializeField, InspectorName("找不到字段时隐藏图像")] private bool hideImageWhenMissing = true;

        public void Refresh(ScriptableObject asset)
        {
            for (var i = 0; i < memberBindings.Count; i++)
            {
                memberBindings[i]?.Apply(asset, emptyText, hideImageWhenMissing);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < memberBindings.Count; i++)
            {
                memberBindings[i]?.Clear(emptyText, hideImageWhenMissing);
            }
        }

        [Serializable]
        public sealed class MemberBinding
        {
            [SerializeField, InspectorName("字段或属性名")] private string memberName;
            [SerializeField, InspectorName("前缀文本")] private string prefix;
            [SerializeField, InspectorName("后缀文本")] private string suffix;
            [SerializeField, InspectorName("目标文本")] private TMP_Text targetText;
            [SerializeField, InspectorName("目标图像")] private Image targetImage;
            [SerializeField, InspectorName("图像保持比例")] private bool preserveAspect = true;

            public void Apply(ScriptableObject asset, string emptyText, bool hideImageWhenMissing)
            {
                if (targetText != null)
                {
                    targetText.text = TryReadMember(asset, memberName, out var value)
                        ? FormatText(value, emptyText)
                        : emptyText;
                }

                if (targetImage != null)
                {
                    var sprite = ReadSprite(asset, memberName);
                    targetImage.sprite = sprite;
                    targetImage.preserveAspect = preserveAspect;
                    if (hideImageWhenMissing)
                    {
                        targetImage.enabled = sprite != null;
                    }
                }
            }

            public void Clear(string emptyText, bool hideImageWhenMissing)
            {
                if (targetText != null)
                {
                    targetText.text = emptyText;
                }

                if (targetImage != null)
                {
                    targetImage.sprite = null;
                    if (hideImageWhenMissing)
                    {
                        targetImage.enabled = false;
                    }
                }
            }

            private string FormatText(object value, string emptyText)
            {
                if (value == null)
                {
                    return emptyText;
                }

                var text = value switch
                {
                    string stringValue => stringValue,
                    UnityEngine.Object objectValue => objectValue != null ? objectValue.name : emptyText,
                    _ => value.ToString()
                };

                if (string.IsNullOrWhiteSpace(text))
                {
                    text = emptyText;
                }

                return $"{prefix}{text}{suffix}";
            }
        }

        private static Sprite ReadSprite(ScriptableObject asset, string memberName)
        {
            if (!TryReadMember(asset, memberName, out var value) || value == null)
            {
                return null;
            }

            if (value is Sprite sprite)
            {
                return sprite;
            }

            if (value is Texture2D texture)
            {
                return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }

            return null;
        }

        private static bool TryReadMember(ScriptableObject asset, string memberName, out object value)
        {
            value = null;
            if (asset == null || string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            var type = asset.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var property = type.GetProperty(memberName, flags);
            if (property != null)
            {
                value = property.GetValue(asset);
                return true;
            }

            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                value = field.GetValue(asset);
                return true;
            }

            return false;
        }
    }
}
