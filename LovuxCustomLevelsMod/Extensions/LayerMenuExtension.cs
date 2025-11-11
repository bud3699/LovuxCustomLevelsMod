using DG.Tweening;
using HarmonyLib;
using LovuxPatcher;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LayerMenuExtensions
{

    public static InputField inputField;
    public static GameObject versionTextObj;

    public static GameObject customLevelLoadButtonObj;
    public static GameObject customLevelUIRoot;

    public static GameObject customLevelCodeRoot;

    private static Text levelCodeText;

    private static IEnumerator WaitAndChangeSprite(MenuButtonSandbox sandboxBtn, Sprite sprite)
    {
        yield return new WaitWhile(() => UIManager.isMenuMoving);

        var traverse = Traverse.Create(sandboxBtn);
        var buttonIcon = traverse.Field("buttonIcon").GetValue<Image>();

        if (buttonIcon != null) { buttonIcon.sprite = sprite; }
    }

    /*
    public static void CalculateMenuExtended(this object layerMenuInstance)
    {
        Debug.Log("Using Extended Calculate Menu");
        var traverse = Traverse.Create(layerMenuInstance);

        var topButtons = traverse.Field("topButtons").GetValue<ButtonArray[]>();
        var activeButtons = traverse.Field("activeButtons").GetValue<List<ComponentButton>>();
        var levelButtons = traverse.Field("levelButtons").GetValue<List<ComponentButton>>();
        var peakTopButtons = traverse.Field("peakTopButtons").GetValue<int>();
        var gapTopButtons = traverse.Field("gapTopButtons").GetValue<int>();
        var openedMenuSizeField = traverse.Field("openedMenuSize");
        var levelNumber = traverse.Field("levelNumber").GetValue<RectTransform>();

        if (topButtons == null || activeButtons == null)
        {
            Debug.LogError("[Patch] CalculateMenuExtended failed: topButtons or activeButtons is null");
            return;
        }

        activeButtons.Clear();

        if (!SteamManager.Initialized)
        {
            topButtons[3].active = false;
            topButtons[4].active = false;
        }
        else if (SteamApps.BIsDlcInstalled((AppId_t)3895810u))
        {
            topButtons[3].active = true;
        }
        else
        {
            topButtons[3].active = false;
        }

        foreach (var btn in topButtons.Where(b => b.active))
        {
            activeButtons.Add(btn.button);
        }

        foreach (var btn in topButtons.Where(b => !b.active))
        {
            btn.button.gameObject.SetActive(false);
        }

        for (int i = 0; i < activeButtons.Count; i++)
        {
            activeButtons[i].rectTransform.DOAnchorPosY(peakTopButtons - gapTopButtons * i, 0f).Play();
        }

        var newSize = new Vector2(120f, 120 * activeButtons.Count) + Vector2.one * 4f;
        openedMenuSizeField.SetValue(newSize);

        int gameMode = (int)Traverse.Create(typeof(Director)).Field("gameMode").GetValue();

        for (int j = 0; j < activeButtons.Count; j++)
        {
            if (j > 0)
                activeButtons[j].navigationButtons[Vector2.left] = activeButtons[j - 1];
            if (j < activeButtons.Count - 1)
                activeButtons[j].navigationButtons[Vector2.right] = activeButtons[j + 1];

            if (gameMode != 3 && levelButtons != null && levelButtons.Count > 0)
                activeButtons[j].navigationButtons[Vector2.down] = levelButtons[0];
            else
                activeButtons[j].navigationButtons.Remove(Vector2.down);
        }

        if (gameMode != 3 && levelButtons != null)
        {
            foreach (var button in levelButtons)
                button.gameObject.SetActive(true);
        }
        else if (levelButtons != null)
        {
            foreach (var button in levelButtons)
                button.gameObject.SetActive(false);
        }

        foreach (var button in levelButtons)
        {
            button.navigationButtons[Vector2.left] = activeButtons[0];
        }

        CreateCustomLevelLoadButtonUI();
    }
    */

    public static void CreateCustomLevelLoadButtonUI()
    {
        if (UIManager.instance == null || UIManager.instance.menuLayer == null)
        {
            Debug.LogWarning("No active menu layer or UIManager instance.");
            return;
        }

        if (customLevelUIRoot == null && SteamApps.BIsDlcInstalled((AppId_t)3895810u))
        {
            customLevelUIRoot = new GameObject("CustomLevelSection");
            customLevelUIRoot.transform.SetParent(UIManager.instance.menuLayer.transform, false);

            Sprite menuSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Menu");

            if (menuSprite == null)
            {
                Debug.LogError("Menu sprite not found.");
                return;
            }

            var bg = customLevelUIRoot.AddComponent<Image>();
            bg.sprite = menuSprite;
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;

            var bgRect = customLevelUIRoot.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(1f, 1f);
            bgRect.anchorMax = new Vector2(1f, 1f);
            bgRect.pivot = new Vector2(1f, 1f);
            bgRect.anchoredPosition = new Vector2(-50f, -30f);
            bgRect.sizeDelta = new Vector2(280f, 100f);
            bgRect.localScale = Vector3.zero;

            customLevelLoadButtonObj = new GameObject("CustomLevelLoadButton");
            customLevelLoadButtonObj.transform.SetParent(customLevelUIRoot.transform, false);

            var loadButtonRect = customLevelLoadButtonObj.AddComponent<RectTransform>();
            loadButtonRect.anchorMin = new Vector2(1f, 1f);
            loadButtonRect.anchorMax = new Vector2(1f, 1f);
            loadButtonRect.pivot = new Vector2(1f, 1f);
            loadButtonRect.anchoredPosition = new Vector2(-20f, -20f);
            loadButtonRect.sizeDelta = new Vector2(80f, 40f);

            var loadButtonImage = customLevelLoadButtonObj.AddComponent<Image>();
            loadButtonImage.color = new Color(1f, 1f, 1f, 0.9f);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(customLevelLoadButtonObj.transform, false);

            var text = textGO.AddComponent<Text>();
            text.text = "Load";
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.black;
            text.fontSize = 32;

            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            GameObject inputObj = new GameObject("MenuInputField", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputObj.transform.SetParent(customLevelUIRoot.transform, false);
            inputRect.sizeDelta = new Vector2(160, 60);
            inputRect.anchorMin = new Vector2(0f, 1f);
            inputRect.anchorMax = new Vector2(0f, 1f);
            inputRect.pivot = new Vector2(0f, 1f);
            inputRect.anchoredPosition = new Vector2(10f, -10f);

            Image inputImage = inputObj.GetComponent<Image>();
            inputImage.sprite = menuSprite;
            inputImage.type = Image.Type.Sliced;
            inputImage.color = Color.white;


            GameObject inputTextObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform inputTextRect = inputTextObj.GetComponent<RectTransform>();
            inputTextObj.transform.SetParent(inputObj.transform, false);
            inputTextRect.sizeDelta = new Vector2(160, 60);
            inputTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputTextRect.pivot = new Vector2(0.5f, 0.5f);
            inputTextRect.anchoredPosition = Vector2.zero;

            Text inputText = inputTextObj.GetComponent<Text>();
            inputText.text = "";
            inputText.alignment = TextAnchor.MiddleCenter;
            inputText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            inputText.color = Color.black;
            inputText.fontSize = 30;

            inputField = inputObj.GetComponent<InputField>();
            inputField.textComponent = inputText;
            inputField.text = "5LOR1L";


            versionTextObj = new GameObject("ModVersionText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            RectTransform versionTextRect = versionTextObj.GetComponent<RectTransform>();
            versionTextObj.transform.SetParent(customLevelUIRoot.transform, false);
            versionTextRect.sizeDelta = new Vector2(300, 30);
            versionTextRect.anchorMin = new Vector2(0.5f, 0f);
            versionTextRect.anchorMax = new Vector2(0.5f, 0f);
            versionTextRect.pivot = new Vector2(0.5f, 0f);
            versionTextRect.anchoredPosition = new Vector2(-30f, 2f);

            Text versionText = versionTextObj.GetComponent<Text>();
            versionText.text = $"Mod Version: {Main.ModVersion}   |   Mod by: bud3699";
            versionText.alignment = TextAnchor.MiddleRight;
            versionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            versionText.color = new Color(0.5f, 0.5f, 0.5f);
            versionText.fontSize = 14;

            var loadButton = customLevelLoadButtonObj.AddComponent<Button>();

            loadButton.onClick.AddListener(() =>
            {
                if (UIManager.instance != null && Director.instance != null)
                {
                    var sandboxBtn = GameObject.FindObjectsOfType<MenuButtonSandbox>().FirstOrDefault();
                    var sandboxTraverse = Traverse.Create(sandboxBtn);
                    var toggleMapping = sandboxTraverse.Field("toggleMapping").GetValue<Dictionary<GameMode, (GameMode targetMode, Sprite targetIcon)>>();

                    Director.instance?.StartCoroutine(LoadCustomLevelCoroutine(inputField, sandboxBtn, toggleMapping));
                }
                else
                {
                    Debug.LogWarning("UIManager or Director instance is null, cannot load custom level.");
                }
            });

        }
    }

    public static void CreateLevelCodeUI()
    {
        if (UIManager.instance == null || UIManager.instance.menuLayer == null)
        {
            Debug.LogWarning("No active menu layer or UIManager instance.");
            return;
        }

        customLevelCodeRoot = new GameObject("LevelCodeUIRoot");
        customLevelCodeRoot.transform.SetParent(UIManager.instance.menuLayer.transform, false);

        Sprite menuSprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(s => s.name == "Menu");
        if (menuSprite == null)
        {
            Debug.LogError("Menu sprite not found.");
            return;
        }

        var bg = customLevelCodeRoot.AddComponent<Image>();
        bg.sprite = menuSprite;
        bg.type = Image.Type.Sliced;
        bg.color = Color.white;

        var bgRect = customLevelCodeRoot.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(1f, 1f);
        bgRect.anchorMax = new Vector2(1f, 1f);
        bgRect.pivot = new Vector2(1f, 1f);
        bgRect.anchoredPosition = new Vector2(-75f, -140f);
        bgRect.sizeDelta = new Vector2(220f, 60f);
        bgRect.localScale = Vector3.zero;

        GameObject textObj = new GameObject("LevelCodeText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObj.transform.SetParent(customLevelCodeRoot.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        levelCodeText = textObj.GetComponent<Text>();
        levelCodeText.text = "";
        levelCodeText.alignment = TextAnchor.MiddleCenter;
        levelCodeText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        levelCodeText.color = Color.black;
        levelCodeText.fontSize = 28;
    }

    public static void SetLevelCodeText(string newText)
    {
        if (levelCodeText != null)
        {
            levelCodeText.text = newText;
        }
        else
        {
            Debug.LogWarning("LevelCodeText not initialized.");
        }
    }


    private static IEnumerator LoadCustomLevelCoroutine(InputField inputField, MenuButtonSandbox sandboxBtn, Dictionary<GameMode, (GameMode targetMode, Sprite targetIcon)> toggleMapping)
    {
        /*
            if (toggleMapping.TryGetValue(currentMode, out var mapping) && !(Director.gameMode == (GameMode)3)) {
            Debug.Log("Got value of pre: " + Director.gameMode.ToString());
            Director.gameMode = mapping.targetMode;
            Debug.Log("Got value of: " + mapping.targetMode.ToString());
        }*/
        Director.gameMode = (GameMode)3;
        DiscordManagerPatches.LevelCodeDiscord = null;
        UIManager.instance?.menuLayer.CloseMenu();

        LevelAnimation.isLevelLoading = true;

        //Here level prep For loading, no mexican wave for now. Maybe later, when level reloads once complete ??

        //Frame.instance?.PositiveFeedback();
        //yield return new WaitWhile(() => Frame.mexicanWave);
        Director.instance.CloseLevel();
        yield return new WaitWhile(() => LevelAnimation.isLevelLoading);
        yield return new WaitForSecondsRealtime(0.1f);

        UIManager.instance?.sandboxLayer.sideButtons.Last().ResetButton();
        Director.instance?.StartCoroutine(WaitAndChangeSprite(sandboxBtn, toggleMapping[Director.gameMode].targetIcon));
        DirectorPatches.customLevelCode = inputField.text;
        Director.instance?.Init();

        Debug.Log($"[Patch] Loaded custom level code: {inputField.text}");
    }
}
