using UnityEngine;

public class ChangeCursor : MonoBehaviour
{
    [Header("鼠标贴图")]
    [SerializeField] private Texture2D cursorTexture;

    [Header("热点（点击位置）")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private const string CURSOR_SKIN_KEY = "CURSOR_SKIN_ENABLED";

    private void Start()
    {
        bool enabled = PlayerPrefs.GetInt(CURSOR_SKIN_KEY, 1) == 1;
        SetCursorSkinEnabled(enabled);
    }

    public void SetCursorSkinEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(CURSOR_SKIN_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (enabled)
        {
            ApplyCursor();
        }
        else
        {
            ResetCursor();
        }
    }

    public bool IsCursorSkinEnabled()
    {
        return PlayerPrefs.GetInt(CURSOR_SKIN_KEY, 1) == 1;
    }

    public void ApplyCursor()
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("没有设置鼠标贴图");
            return;
        }

        Cursor.visible = true;
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    public void ResetCursor()
    {
        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        // 如果关闭了这个对象，但设置里仍然启用鼠标皮肤，则不要恢复默认鼠标
        if (!IsCursorSkinEnabled())
        {
            ResetCursor();
        }
    }
}