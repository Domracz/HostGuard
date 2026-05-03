using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public static class HostGuardUI
{
    private static GameObject? _lobbyButton;
    private static GameObject? _panel;
    private static GameObject? _rowsContainer;
    private static GameObject? _presetsContainer;
    private static readonly List<Action> _refreshCbs = new();
    private static int _configPresetNum = 1;
    private static int _gamePresetNum = 1;
    private static bool _settingsTab = true;
    private static bool _gamePresetsMode = false;
    private static string? _renamingPreset = null;
    private static string _renameInput = "";
    private static TextMeshPro? _renameInputTmp = null;
    private static Action? _renameConfirmAction = null;

    // List expand/collapse state
    private static bool _showExactNames = false;
    private static bool _showExactWords = false;
    private static string? _addingToList = null;
    private static string _addInput = "";
    private static TextMeshPro? _addInputTmp = null;
    private static Action? _addConfirmAction = null;

    // Scroll state
    private static float _scrollY = 0f;
    private static float _scrollMax = 0f;
    private static float _totalContentHeight = 0f;

    // Viewport bounds (world Y coordinates for clipping)
    private static float _viewportTop = 0f;
    private static float _viewportBot = 0f;

    private static readonly Color PanelBg = new(0.12f, 0.12f, 0.15f, 1f);
    private static readonly Color TabActive = new(0.2f, 0.55f, 0.2f, 1f);
    private static readonly Color TabInactive = new(0.25f, 0.25f, 0.28f, 1f);
    private static readonly Color BtnGreen = new(0.2f, 0.6f, 0.2f, 1f);
    private static readonly Color HdrColor = new(0.9f, 0.9f, 0.25f, 1f);
    private static readonly Color Dim = new(0.45f, 0.45f, 0.45f, 1f);
    private static readonly Color SubTabActive = new(0.15f, 0.45f, 0.6f, 1f);
    private static readonly Color SubTabInactive = new(0.2f, 0.2f, 0.23f, 1f);

    private static float _camLeft, _camRight, _camTop, _camBot;
    private static bool _camReady;
    private static Material? _hudSpriteMaterial;
    private static string _hudSortingLayer = "";
    private static bool _sortingLayerDiscovered;

    // Panel layout (computed from camera)
    private static float _panelCX, _panelLeft, _panelW, _panelH, _contentTop;

    public static bool IsPanelOpen => _panel != null && _panel.activeSelf;

    public static void CreateLobbyButton(Transform hudParent)
    {
        if (_lobbyButton != null) Object.Destroy(_lobbyButton);
        _lobbyButton = new GameObject("HG_Button");
        _lobbyButton.transform.SetParent(hudParent);
        _lobbyButton.layer = 5;
        _lobbyButton.transform.localPosition = new Vector3(-3.8f, -2.5f, -50f);
        _lobbyButton.transform.localScale = Vector3.one;
        MakeBg(_lobbyButton.transform, Vector3.zero, 1.1f, 0.38f, BtnGreen, 10);
        MakeLabel(_lobbyButton.transform, "HostGuard", Vector3.zero, 2.2f, Color.white, TextAlignmentOptions.Center, 11);
        var c = _lobbyButton.AddComponent<BoxCollider2D>(); c.size = new Vector2(1.1f, 0.38f);
        var p = AddButton(_lobbyButton); p.OnClick.AddListener((Action)Toggle);
        _lobbyButton.SetActive(false);
    }

    public static void UpdateVisibility()
    {
        if (_lobbyButton == null) return;

        if (!_camReady)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                float h = cam.orthographicSize; float w = h * cam.aspect;
                _camLeft = -w; _camRight = w; _camTop = h; _camBot = -h;
                _camReady = true;
                _lobbyButton.transform.localPosition = new Vector3(_camLeft + 0.65f, _camBot + 0.25f, -50f);
            }
        }

        if (!_sortingLayerDiscovered && HudManager.Instance != null)
        {
            var renderers = HudManager.Instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Count; i++)
            {
                var sr = renderers[i];
                if (sr.sprite != null && sr.enabled)
                {
                    _hudSortingLayer = sr.sortingLayerName ?? "";
                    _hudSpriteMaterial = sr.material;
                    break;
                }
            }
            _sortingLayerDiscovered = true;
        }

        bool show = AmongUsClient.Instance != null
                 && AmongUsClient.Instance.AmHost
                 && !AmongUsClient.Instance.IsGameStarted;
        if (_lobbyButton.activeSelf != show) _lobbyButton.SetActive(show);

        if (IsPanelOpen && PlayerControl.LocalPlayer != null)
        {
            PlayerControl.LocalPlayer.moveable = false;
            try { PlayerControl.LocalPlayer.NetTransform.Halt(); } catch { }
        }

        // Scroll when panel open and settings tab visible
        if (IsPanelOpen && _settingsTab && _rowsContainer != null)
        {
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
            {
                _scrollY = Mathf.Clamp(_scrollY - scroll * 0.3f, 0f, _scrollMax);
                _rowsContainer.transform.localPosition = new Vector3(0f, _scrollY, 0f);
                ClipToViewport();
            }
        }

        // Rename keyboard input
        if (_renamingPreset != null && _renameInputTmp != null)
        {
            string typed = Input.inputString;
            for (int ci = 0; ci < typed.Length; ci++)
            {
                char c = typed[ci];
                if (c == '\b') // backspace
                {
                    if (_renameInput.Length > 0)
                        _renameInput = _renameInput.Substring(0, _renameInput.Length - 1);
                }
                else if (c == '\r' || c == '\n') // enter
                {
                    _renameConfirmAction?.Invoke();
                    return;
                }
                else if (c == 27) // escape
                {
                    _renamingPreset = null;
                    BuildPresets();
                    return;
                }
                else if (!char.IsControl(c))
                {
                    _renameInput += c;
                }
            }
            _renameInputTmp.text = "New name: " + _renameInput + "_";
        }

        // List add keyboard input
        if (_addingToList != null && _addInputTmp != null)
        {
            string typed2 = Input.inputString;
            for (int ci2 = 0; ci2 < typed2.Length; ci2++)
            {
                char ch = typed2[ci2];
                if (ch == '\b')
                {
                    if (_addInput.Length > 0)
                        _addInput = _addInput.Substring(0, _addInput.Length - 1);
                }
                else if (ch == '\r' || ch == '\n')
                {
                    _addConfirmAction?.Invoke();
                    return;
                }
                else if (ch == 27)
                {
                    _addingToList = null;
                    _addInputTmp = null;
                    _addConfirmAction = null;
                    RebuildSettings();
                    return;
                }
                else if (!char.IsControl(ch))
                {
                    _addInput += ch;
                }
            }
            _addInputTmp.text = "Add: " + _addInput + "_";
        }
    }

    private static void Toggle()
    {
        if (_panel == null) BuildPanel();
        if (_panel == null) return;
        bool open = !_panel.activeSelf;
        _panel.SetActive(open);
        if (!open && PlayerControl.LocalPlayer != null)
            PlayerControl.LocalPlayer.moveable = true;
    }

    private static void Close()
    {
        if (_panel != null) _panel.SetActive(false);
        if (PlayerControl.LocalPlayer != null) PlayerControl.LocalPlayer.moveable = true;
    }

    // ==================== BUILD PANEL ====================

    private static void BuildPanel()
    {
        if (_panel != null) Object.Destroy(_panel);
        _refreshCbs.Clear();
        _scrollY = 0f;

        if (!_camReady) return;

        if (!_sortingLayerDiscovered && HudManager.Instance != null)
        {
            var renderers = HudManager.Instance.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Count; i++)
            {
                var sr = renderers[i];
                if (sr.sprite != null && sr.enabled)
                {
                    _hudSortingLayer = sr.sortingLayerName ?? "";
                    _hudSpriteMaterial = sr.material;
                    break;
                }
            }
            _sortingLayerDiscovered = true;
        }

        _panel = new GameObject("HG_Panel");
        _panel.transform.SetParent(_lobbyButton!.transform.parent);
        _panel.transform.localPosition = Vector3.zero;
        _panel.transform.localScale = Vector3.one;

        float screenW = _camRight - _camLeft;
        float screenH = _camTop - _camBot;
        _panelW = screenW * 0.30f;
        _panelH = screenH * 0.72f;
        float panelLeft = _camLeft + screenW * 0.02f;
        _panelLeft = panelLeft;
        _panelCX = panelLeft + _panelW / 2f;
        float panelTop = _panelH / 2f;
        _contentTop = panelTop - 0.95f;

        // Viewport: content visible between just below tabs and just above panel bottom
        _viewportTop = _contentTop + 0.05f;
        _viewportBot = -_panelH / 2f + 0.1f;

        // Border + Background
        MakeBg(_panel.transform, new Vector3(_panelCX, 0f, -99f), _panelW + 0.06f, _panelH + 0.06f, new Color(0.4f, 0.4f, 0.45f, 1f), 490);
        MakeBg(_panel.transform, new Vector3(_panelCX, 0f, -98.9f), _panelW, _panelH, PanelBg, 491);

        // Title
        MakeLabel(_panel.transform, "HostGuard Settings", new Vector3(_panelCX, panelTop - 0.22f, -100f),
            2.0f, Color.white, TextAlignmentOptions.Center, 500);

        // Close
        var closeObj = MakeLabel(_panel.transform, "X", new Vector3(_panelLeft + _panelW - 0.18f, panelTop - 0.22f, -100f),
            1.8f, new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 500);
        closeObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.3f);
        AddButton(closeObj).OnClick.AddListener((Action)Close);

        // Tabs
        float tabY = panelTop - 0.55f;
        MakeTabBtn("Settings", _panelCX - _panelW * 0.2f, tabY, true, () => SwitchTab(true));
        MakeTabBtn("Presets", _panelCX + _panelW * 0.2f, tabY, false, () => SwitchTab(false));

        // Settings content (scrollable)
        _rowsContainer = new GameObject("Rows");
        _rowsContainer.transform.SetParent(_panel.transform);
        _rowsContainer.transform.localPosition = Vector3.zero;
        _rowsContainer.transform.localScale = Vector3.one;
        RenderAllSettings();
        ClipToViewport();

        // Presets content
        _presetsContainer = new GameObject("Presets");
        _presetsContainer.transform.SetParent(_panel.transform);
        _presetsContainer.transform.localPosition = Vector3.zero;
        _presetsContainer.transform.localScale = Vector3.one;
        BuildPresets();
        _presetsContainer.SetActive(false);

        _panel.SetActive(false);
    }

    private static void MakeTabBtn(string label, float x, float y, bool active, Action onClick)
    {
        var btn = new GameObject("Tab_" + label);
        btn.transform.SetParent(_panel!.transform);
        btn.transform.localPosition = new Vector3(x, y, -100f);
        btn.transform.localScale = Vector3.one;
        float tw = _panelW * 0.35f;
        MakeBg(btn.transform, Vector3.zero, tw, 0.25f, active ? TabActive : TabInactive, 500);
        MakeLabel(btn.transform, label, Vector3.zero, 1.4f, Color.white, TextAlignmentOptions.Center, 501);
        btn.AddComponent<BoxCollider2D>().size = new Vector2(tw, 0.25f);
        AddButton(btn).OnClick.AddListener((Action)onClick);
    }

    private static void SwitchTab(bool settings)
    {
        _settingsTab = settings;
        if (_rowsContainer != null) _rowsContainer.SetActive(settings);
        if (_presetsContainer != null) { _presetsContainer.SetActive(!settings); if (!settings) BuildPresets(); }
        UpdateTabVisual("Tab_Settings", settings);
        UpdateTabVisual("Tab_Presets", !settings);
    }

    /// <summary>
    /// Hides rows that are outside the viewport (above header or below panel bottom).
    /// Called after each scroll update.
    /// </summary>
    private static void ClipToViewport()
    {
        if (_rowsContainer == null) return;
        float containerY = _rowsContainer.transform.localPosition.y;

        for (int i = 0; i < _rowsContainer.transform.childCount; i++)
        {
            var child = _rowsContainer.transform.GetChild(i);
            // World Y = child's local Y + container offset
            float worldY = child.localPosition.y + containerY;
            bool visible = worldY <= _viewportTop && worldY >= _viewportBot;
            child.gameObject.SetActive(visible);
        }
    }

    private static void UpdateTabVisual(string name, bool active)
    {
        if (_panel == null) return;
        var t = _panel.transform.Find(name);
        if (t == null) return;
        var bg = t.Find("BG")?.GetComponent<SpriteRenderer>();
        if (bg != null) bg.color = active ? TabActive : TabInactive;
    }

    // ==================== RENDER ALL SETTINGS (scrollable) ====================

    private static void RenderAllSettings()
    {
        if (_rowsContainer == null) return;
        _refreshCbs.Clear();

        float y = _contentTop;
        float rowH = 0.26f;

        // --- NAME FILTER ---
        HdrToggle(y, "NAME FILTER", HostGuardConfig.NameFilterEnabled); y -= 0.3f;
        Row(y, "Default Names", HostGuardConfig.KickDefaultNames, HostGuardConfig.BanForDefaultName); y -= rowH;
        Row(y, "Strict Casing", HostGuardConfig.StrictDefaultNameCasing, null); y -= rowH;
        Row(y, "Bad Names", HostGuardConfig.BanForBadName, null); y -= rowH;
        y = RenderWordList(y, "Exact Match Names", HostGuardConfig.GetBadNameWordsList(),
            ref _showExactNames, "exactNames", HostGuardConfig.AddBadNameWord, HostGuardConfig.RemoveBadNameWord);

        // --- CHAT FILTER ---
        HdrToggle(y, "CHAT FILTER", HostGuardConfig.ChatFilterEnabled); y -= 0.3f;
        BanKickRow(y, "Banned Words", HostGuardConfig.BanForBannedWords); y -= rowH;
        y = RenderWordList(y, "Exact Match Words", HostGuardConfig.GetBannedWordsList(),
            ref _showExactWords, "exactWords", HostGuardConfig.AddBannedWord, HostGuardConfig.RemoveBannedWord);

        // --- BOT PROTECTION ---
        HdrToggle(y, "BOT PROTECTION", HostGuardConfig.BotProtectionEnabled); y -= 0.3f;
        Row(y, "Known Bots Ban", HostGuardConfig.BanKnownBots, null); y -= rowH;
        Row(y, "Cosmetic Detect", HostGuardConfig.CosmeticDetectionEnabled, HostGuardConfig.BanForSuspiciousCosmetics); y -= rowH;
        TxtRow(y, "Bot Names (comma-separated)", HostGuardConfig.KnownBotNames); y -= rowH;
        TxtRow(y, "Bot URLs (comma-separated)", HostGuardConfig.KnownBotUrls); y -= rowH;

        // --- FLOOD PROTECTION ---
        HdrToggle(y, "FLOOD PROTECTION", HostGuardConfig.FloodProtectionEnabled); y -= 0.3f;
        Row(y, "Meeting Spam", HostGuardConfig.MeetingSpamKick, null); y -= rowH;
        Row(y, "Auto-Lock", HostGuardConfig.AutoLockOnFlood, null); y -= rowH;
        NumRow(y, "Join Threshold", HostGuardConfig.FloodJoinThreshold, 1, 20, 1); y -= rowH;
        NumRow(y, "Join Window", HostGuardConfig.FloodJoinWindowSeconds, 1, 30, 1); y -= rowH;
        NumRow(y, "Leave Thresh", HostGuardConfig.RapidLeaveThreshold, 1, 10, 1); y -= rowH;
        NumRow(y, "Mtg Threshold", HostGuardConfig.MeetingSpamThreshold, 1, 10, 1); y -= rowH;
        NumRow(y, "Mtg Window", HostGuardConfig.MeetingSpamWindowSeconds, 5, 60, 5); y -= rowH;
        NumRow(y, "Lock Duration", HostGuardConfig.AutoLockDurationSeconds, 0, 300, 5); y -= rowH;

        // --- ANTI-CHEAT ---
        HdrToggle(y, "ANTI-CHEAT", HostGuardConfig.AntiCheatEnabled); y -= 0.3f;
        BanKickRow(y, "Invalid RPC", HostGuardConfig.BanOnInvalidRpc); y -= rowH;
        NumRow(y, "Chat Limit", HostGuardConfig.ChatRateLimit, 1, 20, 1); y -= rowH;
        NumRow(y, "Limit Window", HostGuardConfig.ChatRateLimitWindowSeconds, 1, 30, 1); y -= rowH;

        // --- MIN LEVEL ---
        HdrToggle(y, "MIN LEVEL", HostGuardConfig.MinLevelEnabled); y -= 0.3f;
        NumRow(y, "Min Level", HostGuardConfig.MinLevel, 0, 100, 1); y -= rowH;
        BanKickRow(y, "Low Level", HostGuardConfig.BanForLowLevel); y -= rowH;

        // --- GENERAL ---
        MakeLabel(_rowsContainer.transform, "GENERAL", new Vector3(_panelLeft + 0.12f, y, -100f),
            1.2f, HdrColor, TextAlignmentOptions.Left, 501); y -= 0.3f;
        Row(y, "Auto-Start", HostGuardConfig.AutoStartEnabled, null); y -= rowH;
        NumRow(y, "Start Players", HostGuardConfig.AutoStartPlayerCount, 0, 15, 1); y -= rowH;
        Row(y, "Rules on Start", HostGuardConfig.SendRulesOnLobbyStart, null); y -= rowH;
        Row(y, "Join Notifs", HostGuardConfig.VerboseJoinNotifications, null); y -= rowH;
        TxtRow(y, "Ban List URL", HostGuardConfig.BanListUrl); y -= rowH;

        // --- LOBBY ---
        MakeLabel(_rowsContainer.transform, "LOBBY", new Vector3(_panelLeft + 0.12f, y, -100f),
            1.2f, HdrColor, TextAlignmentOptions.Left, 501); y -= 0.3f;
        Row(y, "Auto Return", HostGuardConfig.AutoReturnToLobby, null); y -= rowH;
        FloatRow(y, "Return Delay", HostGuardConfig.AutoReturnDelay, 0f, 30f, 0.5f); y -= rowH;
        Row(y, "Auto Start If Closing", HostGuardConfig.AutoStartIfClosing, null); y -= rowH;
        NumRow(y, "Auto Start Threshold (s)", HostGuardConfig.AutoStartThreshold, 5, 120, 5); y -= rowH;
        NumRow(y, "Min Players", HostGuardConfig.MinPlayersToAutoStart, 1, 15, 1); y -= rowH;

        // --- WHITELIST ---
        MakeLabel(_rowsContainer.transform, "WHITELIST", new Vector3(_panelLeft + 0.12f, y, -100f),
            1.2f, HdrColor, TextAlignmentOptions.Left, 501); y -= 0.28f;
        var wlCodes = HostGuardConfig.GetWhitelistedCodes().ToList();
        if (wlCodes.Count == 0)
        {
            MakeLabel(_rowsContainer.transform, "(empty)", new Vector3(_panelLeft + 0.12f, y, -100f),
                1.0f, Dim, TextAlignmentOptions.Left, 501); y -= 0.22f;
        }
        else
        {
            foreach (var code in wlCodes)
            {
                string c = code;
                MakeLabel(_rowsContainer.transform, c, new Vector3(_panelLeft + 0.12f, y, -100f),
                    0.95f, Color.white, TextAlignmentOptions.Left, 501);
                var rmObj = MakeLabel(_rowsContainer.transform, "[X]",
                    new Vector3(_panelLeft + _panelW - 0.3f, y, -100f), 0.95f,
                    new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 501);
                rmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
                AddButton(rmObj).OnClick.AddListener((Action)(() =>
                {
                    HostGuardConfig.RemoveFromWhitelist(c);
                    RebuildSettings();
                }));
                y -= 0.22f;
            }
        }

        // Whitelist export/import/folder buttons
        MakeActionBtn(y, "Export Whitelist", () =>
        {
            var codes = HostGuardConfig.GetWhitelistedCodes();
            var filename = $"whitelist_{SanitizeNickname()}_export.txt";
            File.WriteAllLines(Path.Combine(BepInEx.Paths.ConfigPath, filename), codes);
            ChatHelper.SendLocalMessage($"[HostGuard] Whitelist exported to {filename}");
        }); y -= 0.26f;
        MakeActionBtn(y, "Import Whitelist", () =>
        {
            var files = Directory.GetFiles(BepInEx.Paths.ConfigPath, "whitelist_*_import.txt");
            if (files.Length == 0)
            {
                ChatHelper.SendLocalMessage("[HostGuard] No import file found. Export a list, rename '_export' to '_import', place it in the BepInEx config folder, then press Import.");
                return;
            }
            var existing = HostGuardConfig.GetWhitelistedCodes();
            int added = 0, dupes = 0;
            foreach (var line in File.ReadAllLines(files[0]))
            {
                var code = line.Trim();
                if (code.Length == 0 || code.Contains(",")) continue;
                if (existing.Contains(code)) { dupes++; continue; }
                HostGuardConfig.AddToWhitelist(code);
                added++;
            }
            ChatHelper.SendLocalMessage($"[HostGuard] Whitelist import: {added} codes added, {dupes} duplicates skipped.");
            RebuildSettings();
        }); y -= 0.26f;
        MakeActionBtn(y, "Open Folder", () =>
        {
            Process.Start("explorer.exe", BepInEx.Paths.ConfigPath);
        }); y -= 0.26f;

        // --- BLACKLIST ---
        MakeLabel(_rowsContainer.transform, "BLACKLIST", new Vector3(_panelLeft + 0.12f, y, -100f),
            1.2f, HdrColor, TextAlignmentOptions.Left, 501); y -= 0.28f;
        var blCodes = Blacklist.GetAll();
        if (blCodes.Count == 0)
        {
            MakeLabel(_rowsContainer.transform, "(empty)", new Vector3(_panelLeft + 0.12f, y, -100f),
                1.0f, Dim, TextAlignmentOptions.Left, 501); y -= 0.22f;
        }
        else
        {
            foreach (var code in blCodes)
            {
                string c = code;
                MakeLabel(_rowsContainer.transform, c, new Vector3(_panelLeft + 0.12f, y, -100f),
                    0.95f, Color.white, TextAlignmentOptions.Left, 501);
                var rmObj = MakeLabel(_rowsContainer.transform, "[X]",
                    new Vector3(_panelLeft + _panelW - 0.3f, y, -100f), 0.95f,
                    new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 501);
                rmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
                AddButton(rmObj).OnClick.AddListener((Action)(() =>
                {
                    Blacklist.Remove(c);
                    RebuildSettings();
                }));
                y -= 0.22f;
            }
        }

        // Blacklist export/import/folder buttons
        MakeActionBtn(y, "Export Blacklist", () =>
        {
            var codes = Blacklist.GetAll();
            var filename = $"blacklist_{SanitizeNickname()}_export.txt";
            File.WriteAllLines(Path.Combine(BepInEx.Paths.ConfigPath, filename), codes);
            ChatHelper.SendLocalMessage($"[HostGuard] Blacklist exported to {filename}");
        }); y -= 0.26f;
        MakeActionBtn(y, "Import Blacklist", () =>
        {
            var files = Directory.GetFiles(BepInEx.Paths.ConfigPath, "blacklist_*_import.txt");
            if (files.Length == 0)
            {
                ChatHelper.SendLocalMessage("[HostGuard] No import file found. Export a list, rename '_export' to '_import', place it in the BepInEx config folder, then press Import.");
                return;
            }
            int added = 0, dupes = 0;
            foreach (var line in File.ReadAllLines(files[0]))
            {
                var code = line.Trim();
                if (code.Length == 0 || code.Contains(",")) continue;
                if (Blacklist.Add(code)) added++;
                else dupes++;
            }
            ChatHelper.SendLocalMessage($"[HostGuard] Blacklist import: {added} codes added, {dupes} duplicates skipped.");
            RebuildSettings();
        }); y -= 0.26f;
        MakeActionBtn(y, "Open Folder", () =>
        {
            Process.Start("explorer.exe", BepInEx.Paths.ConfigPath);
        }); y -= 0.26f;

        // Calculate scroll range
        _totalContentHeight = _contentTop - y;
        float visibleH = _panelH - 1.2f; // panel height minus title+tabs area
        _scrollMax = Mathf.Max(0f, _totalContentHeight - visibleH);
    }

    private static void RebuildSettings(bool resetScroll = false)
    {
        if (_rowsContainer == null) return;
        if (resetScroll) _scrollY = 0f;
        for (int i = _rowsContainer.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(_rowsContainer.transform.GetChild(i).gameObject);
        _rowsContainer.transform.localPosition = new Vector3(0f, _scrollY, 0f);
        RenderAllSettings();
        ClipToViewport();
    }

    // --- Row builders (place directly at y position) ---

    private static void HdrToggle(float y, string title, ConfigEntry<bool> enableCfg)
    {
        if (_rowsContainer == null) return;
        MakeLabel(_rowsContainer.transform, title, new Vector3(_panelLeft + 0.12f, y, -100f),
            1.2f, HdrColor, TextAlignmentOptions.Left, 501);
        MakeSmallToggle(_rowsContainer.transform, enableCfg,
            new Vector3(_panelLeft + _panelW - 0.5f, y, -100f), "ON", "OFF", Color.green, Palette.ImpostorRed);
    }

    private static void Row(float y, string label, ConfigEntry<bool> cfg, ConfigEntry<bool>? banCfg)
    {
        if (_rowsContainer == null) return;
        float lx = _panelLeft + 0.12f;
        float togX = _panelLeft + _panelW - 0.9f;
        MakeLabel(_rowsContainer.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);
        MakeSmallToggle(_rowsContainer.transform, cfg, new Vector3(togX, y, -100f), "ON", "OFF", Color.green, Palette.ImpostorRed);
        if (banCfg != null)
            MakeSmallToggle(_rowsContainer.transform, banCfg, new Vector3(togX + 0.45f, y, -100f), "BAN", "KICK",
                new Color(0.85f, 0.25f, 0.1f), new Color(0.85f, 0.7f, 0.1f));
    }

    private static void BanKickRow(float y, string label, ConfigEntry<bool> banCfg)
    {
        if (_rowsContainer == null) return;
        float lx = _panelLeft + 0.12f;
        float togX = _panelLeft + _panelW - 0.9f;
        MakeLabel(_rowsContainer.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);
        MakeSmallToggle(_rowsContainer.transform, banCfg, new Vector3(togX, y, -100f), "BAN", "KICK",
            new Color(0.85f, 0.25f, 0.1f), new Color(0.85f, 0.7f, 0.1f));
    }

    private static float RenderWordList(float y, string title, List<string> items,
        ref bool showState, string listId, Func<string, bool> addFunc, Func<string, bool> removeFunc)
    {
        if (_rowsContainer == null) return y;
        float lx = _panelLeft + 0.12f;

        // Header label + count
        MakeLabel(_rowsContainer.transform, $"{title} ({items.Count})", new Vector3(lx, y, -100f),
            1.05f, Color.white, TextAlignmentOptions.Left, 501);

        // Show/Hide toggle button
        string capturedId = listId;
        var togObj = MakeLabel(_rowsContainer.transform, showState ? "[Hide]" : "[Show]",
            new Vector3(_panelLeft + _panelW - 0.5f, y, -100f), 0.95f,
            new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.Center, 501);
        togObj.AddComponent<BoxCollider2D>().size = new Vector2(0.45f, 0.2f);
        AddButton(togObj).OnClick.AddListener((Action)(() =>
        {
            if (capturedId == "exactNames") _showExactNames = !_showExactNames;
            else if (capturedId == "exactWords") _showExactWords = !_showExactWords;
            RebuildSettings();
        }));
        y -= 0.24f;

        if (!showState)
            return y;

        // Render each item with [X] remove button
        if (items.Count == 0)
        {
            MakeLabel(_rowsContainer.transform, "(empty)", new Vector3(lx + 0.1f, y, -100f),
                0.9f, Dim, TextAlignmentOptions.Left, 501);
            y -= 0.2f;
        }
        else
        {
            for (int i = 0; i < items.Count; i++)
            {
                string item = items[i];
                MakeLabel(_rowsContainer.transform, item, new Vector3(lx + 0.1f, y, -100f),
                    0.9f, Color.white, TextAlignmentOptions.Left, 501);
                string capturedItem = item;
                Func<string, bool> rmFn = removeFunc;
                var rmObj = MakeLabel(_rowsContainer.transform, "[X]",
                    new Vector3(_panelLeft + _panelW - 0.3f, y, -100f), 0.9f,
                    new Color(1f, 0.3f, 0.3f), TextAlignmentOptions.Center, 501);
                rmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.18f);
                AddButton(rmObj).OnClick.AddListener((Action)(() =>
                {
                    rmFn(capturedItem);
                    RebuildSettings();
                }));
                y -= 0.2f;
            }
        }

        // Add input or [+ Add] button
        if (_addingToList == capturedId)
        {
            var inputLabel = MakeLabel(_rowsContainer.transform, "Add: _",
                new Vector3(lx + 0.1f, y, -100f), 0.9f, Color.yellow, TextAlignmentOptions.Left, 501);
            _addInputTmp = inputLabel.GetComponent<TextMeshPro>();

            Func<string, bool> addFn = addFunc;
            _addConfirmAction = () =>
            {
                if (!string.IsNullOrWhiteSpace(_addInput))
                    addFn(_addInput.Trim());
                _addingToList = null;
                _addInputTmp = null;
                _addConfirmAction = null;
                _addInput = "";
                RebuildSettings();
            };

            var okObj = MakeLabel(_rowsContainer.transform, "[OK]",
                new Vector3(_panelLeft + _panelW - 0.5f, y, -100f), 0.9f,
                Color.green, TextAlignmentOptions.Center, 501);
            okObj.AddComponent<BoxCollider2D>().size = new Vector2(0.35f, 0.18f);
            AddButton(okObj).OnClick.AddListener((Action)(() => _addConfirmAction?.Invoke()));

            var cancelObj = MakeLabel(_rowsContainer.transform, "[X]",
                new Vector3(_panelLeft + _panelW - 0.2f, y, -100f), 0.9f,
                Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
            cancelObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.18f);
            AddButton(cancelObj).OnClick.AddListener((Action)(() =>
            {
                _addingToList = null;
                _addInputTmp = null;
                _addConfirmAction = null;
                _addInput = "";
                RebuildSettings();
            }));
            y -= 0.22f;
        }
        else
        {
            string addId = capturedId;
            var addObj = MakeLabel(_rowsContainer.transform, "[+ Add]",
                new Vector3(lx + 0.1f, y, -100f), 0.9f,
                Color.green, TextAlignmentOptions.Left, 501);
            addObj.AddComponent<BoxCollider2D>().size = new Vector2(0.5f, 0.18f);
            AddButton(addObj).OnClick.AddListener((Action)(() =>
            {
                _addingToList = addId;
                _addInput = "";
                RebuildSettings();
            }));
            y -= 0.22f;
        }

        return y;
    }

    private static void TxtRow(float y, string label, ConfigEntry<string> cfg)
    {
        if (_rowsContainer == null) return;
        float lx = _panelLeft + 0.12f;
        MakeLabel(_rowsContainer.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);
        string display = cfg.Value;
        if (display.Length > 30) display = display.Substring(0, 27) + "...";
        MakeLabel(_rowsContainer.transform, display, new Vector3(lx + _panelW * 0.45f, y, -100f),
            0.85f, Dim, TextAlignmentOptions.Left, 501);
    }

    private static void NumRow(float y, string label, ConfigEntry<int> cfg, int min, int max, int step)
    {
        if (_rowsContainer == null) return;
        float lx = _panelLeft + 0.12f;
        float numX = _panelLeft + _panelW - 0.9f;
        MakeLabel(_rowsContainer.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);

        int v = cfg.Value;
        var valObj = MakeLabel(_rowsContainer.transform, v.ToString(), new Vector3(numX, y, -100f),
            1.15f, Color.white, TextAlignmentOptions.Center, 502);
        var vt = valObj.GetComponent<TextMeshPro>();

        var minObj = MakeLabel(_rowsContainer.transform, "<", new Vector3(numX - 0.25f, y, -100f),
            1.3f, Palette.ImpostorRed, TextAlignmentOptions.Center, 502);
        minObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        AddButton(minObj).OnClick.AddListener((Action)(() => { v = Math.Max(min, v - step); cfg.Value = v; if (vt) vt.text = v.ToString(); }));

        var plusObj = MakeLabel(_rowsContainer.transform, ">", new Vector3(numX + 0.25f, y, -100f),
            1.3f, Color.green, TextAlignmentOptions.Center, 502);
        plusObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        AddButton(plusObj).OnClick.AddListener((Action)(() => { v = Math.Min(max, v + step); cfg.Value = v; if (vt) vt.text = v.ToString(); }));
    }

    private static string SanitizeNickname()
    {
        var name = PlayerControl.LocalPlayer?.Data?.PlayerName;
        if (string.IsNullOrWhiteSpace(name)) name = "host";
        return Regex.Replace(name, @"[^a-zA-Z0-9_\-]", "_");
    }

    private static void FloatRow(float y, string label, ConfigEntry<float> cfg, float min, float max, float step)
    {
        if (_rowsContainer == null) return;
        float lx = _panelLeft + 0.12f;
        float numX = _panelLeft + _panelW - 0.9f;
        MakeLabel(_rowsContainer.transform, label, new Vector3(lx, y, -100f), 1.05f, Color.white, TextAlignmentOptions.Left, 501);

        float v = cfg.Value;
        var valObj = MakeLabel(_rowsContainer.transform, v.ToString("F1"), new Vector3(numX, y, -100f),
            1.15f, Color.white, TextAlignmentOptions.Center, 502);
        var vt = valObj.GetComponent<TextMeshPro>();

        var minObj = MakeLabel(_rowsContainer.transform, "<", new Vector3(numX - 0.25f, y, -100f),
            1.3f, Palette.ImpostorRed, TextAlignmentOptions.Center, 502);
        minObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        AddButton(minObj).OnClick.AddListener((Action)(() => { v = Math.Max(min, v - step); cfg.Value = v; if (vt) vt.text = v.ToString("F1"); }));

        var plusObj = MakeLabel(_rowsContainer.transform, ">", new Vector3(numX + 0.25f, y, -100f),
            1.3f, Color.green, TextAlignmentOptions.Center, 502);
        plusObj.AddComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.2f);
        AddButton(plusObj).OnClick.AddListener((Action)(() => { v = Math.Min(max, v + step); cfg.Value = v; if (vt) vt.text = v.ToString("F1"); }));
    }

    private static void MakeActionBtn(float y, string label, Action onClick)
    {
        if (_rowsContainer == null) return;
        var btn = new GameObject("ActBtn");
        btn.transform.SetParent(_rowsContainer.transform);
        btn.transform.localPosition = new Vector3(_panelCX, y, -100f);
        btn.transform.localScale = Vector3.one;
        float bw = _panelW * 0.7f;
        MakeBg(btn.transform, Vector3.zero, bw, 0.2f, new Color(0.2f, 0.35f, 0.55f, 1f), 501);
        MakeLabel(btn.transform, label, Vector3.zero, 1.0f, Color.white, TextAlignmentOptions.Center, 502);
        btn.AddComponent<BoxCollider2D>().size = new Vector2(bw, 0.2f);
        AddButton(btn).OnClick.AddListener(onClick);
    }

    private static void MakeSmallToggle(Transform parent, ConfigEntry<bool> cfg, Vector3 pos,
        string onTxt, string offTxt, Color onCol, Color offCol)
    {
        var obj = new GameObject("Tog");
        obj.transform.SetParent(parent);
        obj.transform.localPosition = pos;
        obj.transform.localScale = Vector3.one;
        var bgObj = MakeBg(obj.transform, Vector3.zero, 0.38f, 0.16f, cfg.Value ? onCol : offCol, 501);
        var bgSR = bgObj.GetComponent<SpriteRenderer>();
        var txtObj = MakeLabel(obj.transform, cfg.Value ? onTxt : offTxt, Vector3.zero, 1.0f, Color.white, TextAlignmentOptions.Center, 502);
        var txtTMP = txtObj.GetComponent<TextMeshPro>();
        obj.AddComponent<BoxCollider2D>().size = new Vector2(0.38f, 0.16f);
        AddButton(obj).OnClick.AddListener((Action)(() =>
        {
            cfg.Value = !cfg.Value;
            if (bgSR) bgSR.color = cfg.Value ? onCol : offCol;
            if (txtTMP) txtTMP.text = cfg.Value ? onTxt : offTxt;
        }));
        _refreshCbs.Add(() =>
        {
            if (bgSR) bgSR.color = cfg.Value ? onCol : offCol;
            if (txtTMP) txtTMP.text = cfg.Value ? onTxt : offTxt;
        });
    }

    // ==================== PRESETS ====================

    private static void BuildPresets()
    {
        if (_presetsContainer == null) return;

        // Clear all children
        for (int i = _presetsContainer.transform.childCount - 1; i >= 0; i--)
            Object.Destroy(_presetsContainer.transform.GetChild(i).gameObject);

        float y = _contentTop;

        // Sub-tab buttons: Config | Game
        float stW = _panelW * 0.3f;

        var configTab = new GameObject("PSub_Config");
        configTab.transform.SetParent(_presetsContainer.transform);
        configTab.transform.localPosition = new Vector3(_panelCX - _panelW * 0.17f, y, -100f);
        configTab.transform.localScale = Vector3.one;
        MakeBg(configTab.transform, Vector3.zero, stW, 0.22f, !_gamePresetsMode ? SubTabActive : SubTabInactive, 501);
        MakeLabel(configTab.transform, "Config", Vector3.zero, 1.2f, Color.white, TextAlignmentOptions.Center, 502);
        configTab.AddComponent<BoxCollider2D>().size = new Vector2(stW, 0.22f);
        AddButton(configTab).OnClick.AddListener((Action)(() => { _gamePresetsMode = false; BuildPresets(); }));

        var gameTab = new GameObject("PSub_Game");
        gameTab.transform.SetParent(_presetsContainer.transform);
        gameTab.transform.localPosition = new Vector3(_panelCX + _panelW * 0.17f, y, -100f);
        gameTab.transform.localScale = Vector3.one;
        MakeBg(gameTab.transform, Vector3.zero, stW, 0.22f, _gamePresetsMode ? SubTabActive : SubTabInactive, 501);
        MakeLabel(gameTab.transform, "Game", Vector3.zero, 1.2f, Color.white, TextAlignmentOptions.Center, 502);
        gameTab.AddComponent<BoxCollider2D>().size = new Vector2(stW, 0.22f);
        AddButton(gameTab).OnClick.AddListener((Action)(() => { _gamePresetsMode = true; BuildPresets(); }));

        y -= 0.38f;

        // Save button
        string saveLabel = _gamePresetsMode ? "[ Save Game Settings ]" : "[ Save Config ]";
        var saveObj = MakeLabel(_presetsContainer.transform, saveLabel,
            new Vector3(_panelCX, y, -100f), 1.3f, Color.green, TextAlignmentOptions.Center, 501);
        saveObj.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, 0.25f);
        AddButton(saveObj).OnClick.AddListener((Action)(() =>
        {
            if (_gamePresetsMode)
            {
                if (GamePresetManager.SavePreset("Game " + _gamePresetNum++))
                    ChatHelper.SendLocalMessage("Game settings saved.");
                else
                    ChatHelper.SendLocalMessage("Could not save — game options not available.");
            }
            else
            {
                PresetManager.SavePreset("Config " + _configPresetNum++);
                ChatHelper.SendLocalMessage("Config preset saved.");
            }
            BuildPresets();
        }));

        y -= 0.38f;

        // Open Folder / Refresh buttons
        string folderPath = _gamePresetsMode ? GamePresetManager.GetPresetsDir() : PresetManager.GetPresetsDir();

        var openObj = MakeLabel(_presetsContainer.transform, "Open Folder",
            new Vector3(_panelCX - _panelW * 0.17f, y, -100f), 1.0f, new Color(0.5f, 0.8f, 1f), TextAlignmentOptions.Center, 501);
        openObj.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.2f);
        AddButton(openObj).OnClick.AddListener((Action)(() =>
        {
            Process.Start("explorer.exe", folderPath);
            ChatHelper.SendLocalMessage("[HostGuard] Preset folder opened.");
        }));

        var refreshObj = MakeLabel(_presetsContainer.transform, "Refresh",
            new Vector3(_panelCX + _panelW * 0.17f, y, -100f), 1.0f, new Color(1f, 0.8f, 0.3f), TextAlignmentOptions.Center, 501);
        refreshObj.AddComponent<BoxCollider2D>().size = new Vector2(0.6f, 0.2f);
        AddButton(refreshObj).OnClick.AddListener((Action)(() =>
        {
            _renamingPreset = null;
            BuildPresets();
            ChatHelper.SendLocalMessage("[HostGuard] Preset list refreshed.");
        }));

        y -= 0.32f;

        // Preset list
        var presets = _gamePresetsMode ? GamePresetManager.GetPresetNames() : PresetManager.GetPresetNames();

        if (presets.Count == 0)
        {
            MakeLabel(_presetsContainer.transform, "No presets yet.\nDrop .txt files into the folder, then press Refresh.",
                new Vector3(_panelCX, y, -100f), 1.0f, Dim, TextAlignmentOptions.Center, 501);
        }
        else
        {
            for (int idx = 0; idx < presets.Count; idx++)
            {
                string n = presets[idx];

                // If renaming this preset, show input row instead
                if (_renamingPreset == n)
                {
                    _renameInput = "";
                    var inputLabel = MakeLabel(_presetsContainer.transform, "New name: _",
                        new Vector3(_panelLeft + 0.15f, y, -100f), 1.0f, Color.yellow, TextAlignmentOptions.Left, 501);
                    _renameInputTmp = inputLabel.GetComponent<TextMeshPro>();

                    string capturedName = n;
                    _renameConfirmAction = () =>
                    {
                        if (string.IsNullOrWhiteSpace(_renameInput))
                        {
                            ChatHelper.SendLocalMessage("[HostGuard] Name cannot be empty.");
                            return;
                        }
                        bool ok = _gamePresetsMode
                            ? GamePresetManager.RenamePreset(capturedName, _renameInput)
                            : PresetManager.RenamePreset(capturedName, _renameInput);
                        if (ok)
                            ChatHelper.SendLocalMessage($"[HostGuard] Preset renamed to '{_renameInput}'.");
                        _renamingPreset = null;
                        _renameInputTmp = null;
                        _renameConfirmAction = null;
                        BuildPresets();
                    };

                    var confirmObj = MakeLabel(_presetsContainer.transform, "OK",
                        new Vector3(_panelCX + _panelW * 0.3f, y, -100f), 1.0f, Color.green, TextAlignmentOptions.Center, 501);
                    confirmObj.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 0.2f);
                    AddButton(confirmObj).OnClick.AddListener((Action)(() => _renameConfirmAction?.Invoke()));

                    var cancelObj = MakeLabel(_presetsContainer.transform, "X",
                        new Vector3(_panelCX + _panelW * 0.42f, y, -100f), 1.0f, Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
                    cancelObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.2f);
                    AddButton(cancelObj).OnClick.AddListener((Action)(() =>
                    {
                        _renamingPreset = null;
                        _renameInputTmp = null;
                        _renameConfirmAction = null;
                        BuildPresets();
                    }));

                    y -= 0.28f;
                    continue;
                }

                MakeLabel(_presetsContainer.transform, n, new Vector3(_panelLeft + 0.15f, y, -100f),
                    1.1f, Color.white, TextAlignmentOptions.Left, 501);

                var loadObj = MakeLabel(_presetsContainer.transform, "Load",
                    new Vector3(_panelCX + _panelW * 0.1f, y, -100f), 1.0f, Color.green, TextAlignmentOptions.Center, 501);
                loadObj.AddComponent<BoxCollider2D>().size = new Vector2(0.4f, 0.2f);
                AddButton(loadObj).OnClick.AddListener((Action)(() =>
                {
                    if (_gamePresetsMode)
                    {
                        if (GamePresetManager.LoadPreset(n))
                            ChatHelper.SendLocalMessage($"Game preset '{n}' loaded.");
                        else
                            ChatHelper.SendLocalMessage("Could not load — game options not available.");
                    }
                    else
                    {
                        if (PresetManager.LoadPreset(n))
                        {
                            RebuildSettings();
                            ChatHelper.SendLocalMessage($"Config preset '{n}' loaded.");
                        }
                    }
                }));

                string renameTarget = n;
                var renObj = MakeLabel(_presetsContainer.transform, "Ren",
                    new Vector3(_panelCX + _panelW * 0.27f, y, -100f), 1.0f, new Color(1f, 0.8f, 0.3f), TextAlignmentOptions.Center, 501);
                renObj.AddComponent<BoxCollider2D>().size = new Vector2(0.35f, 0.2f);
                AddButton(renObj).OnClick.AddListener((Action)(() =>
                {
                    _renamingPreset = renameTarget;
                    BuildPresets();
                }));

                var delObj = MakeLabel(_presetsContainer.transform, "X",
                    new Vector3(_panelCX + _panelW * 0.42f, y, -100f), 1.0f, Palette.ImpostorRed, TextAlignmentOptions.Center, 501);
                delObj.AddComponent<BoxCollider2D>().size = new Vector2(0.25f, 0.2f);
                AddButton(delObj).OnClick.AddListener((Action)(() =>
                {
                    if (_gamePresetsMode)
                        GamePresetManager.DeletePreset(n);
                    else
                        PresetManager.DeletePreset(n);
                    BuildPresets();
                }));

                y -= 0.28f;
            }
        }
    }

    // ==================== PRIMITIVES ====================

    private static GameObject MakeBg(Transform parent, Vector3 pos, float w, float h, Color color, int order)
    {
        var o = new GameObject("BG");
        o.transform.SetParent(parent); o.transform.localPosition = pos; o.transform.localScale = new Vector3(w, h, 1f);
        var sr = o.AddComponent<SpriteRenderer>();
        sr.sprite = GetSquare(); sr.color = color; sr.sortingOrder = order;
        if (_hudSpriteMaterial != null) sr.material = _hudSpriteMaterial;
        if (!string.IsNullOrEmpty(_hudSortingLayer)) sr.sortingLayerName = _hudSortingLayer;
        return o;
    }

    private static GameObject MakeLabel(Transform parent, string text, Vector3 pos,
        float size, Color color, TextAlignmentOptions align, int order)
    {
        var o = new GameObject("L");
        o.transform.SetParent(parent); o.transform.localPosition = pos; o.transform.localScale = Vector3.one;
        var tmp = o.AddComponent<TextMeshPro>();
        tmp.text = text; tmp.fontSize = size; tmp.alignment = align; tmp.color = color;
        tmp.sortingOrder = order; tmp.enableWordWrapping = false; tmp.fontStyle = FontStyles.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var rt = tmp.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(3f, 0.3f);
        if (align == TextAlignmentOptions.Left) rt.pivot = new Vector2(0f, 0.5f);
        else if (align == TextAlignmentOptions.Right) rt.pivot = new Vector2(1f, 0.5f);
        else rt.pivot = new Vector2(0.5f, 0.5f);
        return o;
    }

    private static PassiveButton AddButton(GameObject obj)
    {
        var p = obj.AddComponent<PassiveButton>();
        p.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        p.OnMouseOut = new UnityEvent(); p.OnMouseOver = new UnityEvent();
        return p;
    }

    private static Sprite? _sqSpr;
    private static Sprite GetSquare()
    {
        if (_sqSpr != null) return _sqSpr;
        var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, Color.white); tex.Apply();
        _sqSpr = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sqSpr;
    }

    public static void Cleanup()
    {
        if (_panel != null) Object.Destroy(_panel);
        if (_lobbyButton != null) Object.Destroy(_lobbyButton);
        _panel = null; _lobbyButton = null; _rowsContainer = null; _presetsContainer = null;
        _refreshCbs.Clear(); _camReady = false;
        _renamingPreset = null; _renameInput = ""; _renameInputTmp = null; _renameConfirmAction = null;
        _addingToList = null; _addInput = ""; _addInputTmp = null; _addConfirmAction = null;
        _showExactNames = false; _showExactWords = false;
    }
}
