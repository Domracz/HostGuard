using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class NotificationManager
{
    private struct Toast
    {
        public GameObject Obj;
        public float ExpireTime;
    }

    private static readonly List<Toast> _toasts = new();
    private const int MaxToasts = 4;
    private const float ToastDuration = 6f;
    private const float ToastHeight = 0.28f;
    private const float ToastPadding = 0.04f;

    public static void Show(string message)
    {
        if (!HostGuardConfig.ShowNotifications.Value) return;
        if (HudManager.Instance == null) return;

        // Enforce max visible toasts
        while (_toasts.Count >= MaxToasts)
            DismissOldest();

        var obj = CreateToast(message);
        // Newest toast goes to top (index 0)
        _toasts.Insert(0, new Toast { Obj = obj, ExpireTime = Time.time + ToastDuration });
        RepositionAll();
    }

    /// <summary>
    /// Call once per frame from HudUpdatePatch to auto-dismiss expired toasts.
    /// </summary>
    public static void Update()
    {
        bool changed = false;
        for (int i = _toasts.Count - 1; i >= 0; i--)
        {
            if (_toasts[i].Obj == null)
            {
                _toasts.RemoveAt(i);
                changed = true;
                continue;
            }
            if (Time.time >= _toasts[i].ExpireTime)
            {
                Object.Destroy(_toasts[i].Obj);
                _toasts.RemoveAt(i);
                changed = true;
            }
        }
        if (changed) RepositionAll();
    }

    private static GameObject CreateToast(string message)
    {
        var root = new GameObject("HG_Toast");
        root.transform.SetParent(HudManager.Instance.transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localScale = Vector3.one;

        float toastW = 2.8f;

        // Background
        var bg = new GameObject("BG");
        bg.transform.SetParent(root.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(toastW, ToastHeight, 1f);
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquare();
        sr.color = new Color(0.12f, 0.12f, 0.15f, 0.7f);
        sr.sortingOrder = 510;
        if (HostGuardUI.HudSpriteMaterial != null) sr.material = HostGuardUI.HudSpriteMaterial;
        if (!string.IsNullOrEmpty(HostGuardUI.HudSortingLayer)) sr.sortingLayerName = HostGuardUI.HudSortingLayer;

        // Label
        var label = new GameObject("L");
        label.transform.SetParent(root.transform);
        label.transform.localPosition = Vector3.zero;
        label.transform.localScale = Vector3.one;
        var tmp = label.AddComponent<TextMeshPro>();
        tmp.text = message;
        tmp.fontSize = 1.2f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;
        tmp.sortingOrder = 511;
        tmp.enableWordWrapping = false;
        tmp.fontStyle = FontStyles.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var rt = tmp.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(toastW - 0.1f, ToastHeight);
        rt.pivot = new Vector2(0f, 0.5f);
        label.transform.localPosition = new Vector3(-toastW / 2f + 0.08f, 0f, 0f);

        return root;
    }

    private static void RepositionAll()
    {
        var cam = Camera.main;
        if (cam == null) return;
        float top = cam.orthographicSize;
        float left = -cam.orthographicSize * cam.aspect;

        for (int i = 0; i < _toasts.Count; i++)
        {
            if (_toasts[i].Obj == null) continue;
            float x = left + 1.5f;
            float y = top - 0.2f - i * (ToastHeight + ToastPadding);
            _toasts[i].Obj.transform.localPosition = new Vector3(x, y, -50f);
        }
    }

    private static void DismissOldest()
    {
        if (_toasts.Count == 0) return;
        int last = _toasts.Count - 1;
        var oldest = _toasts[last];
        _toasts.RemoveAt(last);
        if (oldest.Obj != null) Object.Destroy(oldest.Obj);
    }

    private static Sprite? _sqSpr;
    private static Sprite GetSquare()
    {
        if (_sqSpr != null) return _sqSpr;
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _sqSpr = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sqSpr;
    }
}
