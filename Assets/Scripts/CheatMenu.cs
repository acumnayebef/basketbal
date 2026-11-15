using UnityEngine;
using System;

public class CheatMenu : MonoBehaviour
{
    private bool isOpen = false;           // Меню по умолчанию закрыто, видна только плавающая кнопка
    private int currentTab = 0;            // 0: Creators, 1: Player, 2: Money

    private bool alwaysGreenShot = false;  
    private bool bypassDefender = false;   
    private bool infiniteMoney = false;    
    private bool freePurchases = false;    

    private Rect windowRect = new Rect(50, 80, 400, 320);
    
    // Автоматическая инициализация при запуске игры (чтобы не вешать скрипт вручную)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (FindObjectOfType<CheatMenu>() == null)
        {
            GameObject go = new GameObject("XantaPers_Menu_Manager");
            go.AddComponent<CheatMenu>();
            DontDestroyOnLoad(go);
        }
    }

    private void Update()
    {
        ApplyCheats();
    }

    private void OnGUI()
    {
        // Динамическое вычисление позиции кнопки по центру сверху экрана (чтобы не перекрывать меню игры)
        Rect toggleButtonRect = new Rect(Screen.width / 2 - 75, 10, 150, 45);

        // Кнопка открытия/закрытия меню
        GUI.skin.button.fontSize = 15;
        string btnText = isOpen ? "Закрыть меню" : "XantaPers Menu";
        
        if (GUI.Button(toggleButtonRect, btnText))
        {
            isOpen = !isOpen;
        }

        // Окно чит-меню
        if (isOpen)
        {
            windowRect = GUI.Window(0, windowRect, DrawWindowContent, "XantaPers Cheat Menu (iOS)");
        }
    }

    private void DrawWindowContent(int windowID)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Creators", GUILayout.Height(35))) currentTab = 0;
        if (GUILayout.Button("Player", GUILayout.Height(35))) currentTab = 1;
        if (GUILayout.Button("Money", GUILayout.Height(35))) currentTab = 2;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (currentTab == 0)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("<b>Создатель:</b> XantaPers");
            GUILayout.Label("<b>Telegram:</b> @xantapers");
            
            if (GUILayout.Button("Открыть Telegram (@xantapers)", GUILayout.Height(35)))
            {
                Application.OpenURL("https://t.me/xantapers");
            }
            GUILayout.EndVertical();
        }
        else if (currentTab == 1)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("<b>Настройки игрока</b>");
            alwaysGreenShot = GUILayout.Toggle(alwaysGreenShot, " Без промахов (Green Release)");
            bypassDefender = GUILayout.Toggle(bypassDefender, " Всегда обвод игрока");
            GUILayout.EndVertical();
        }
        else if (currentTab == 2)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("<b>Экономика</b>");
            infiniteMoney = GUILayout.Toggle(infiniteMoney, " Бесконечные монеты");
            freePurchases = GUILayout.Toggle(freePurchases, " Бесплатные покупки (In-App)");
            
            GUILayout.Space(5);
            if (GUILayout.Button("Выдать +10,000 монет", GUILayout.Height(35)))
            {
                ForceAddMoney(10000);
            }
            GUILayout.EndVertical();
        }

        GUI.DragWindow(new Rect(0, 0, 10000, 30));
    }

    private void ApplyCheats()
    {
        try
        {
            if (alwaysGreenShot && BallShoot.Instance != null) { /* Логика */ }
            if (infiniteMoney && AllDayRun.Instance != null) { /* Логика */ }
        }
        catch (Exception) { }
    }

    private void ForceAddMoney(int amount)
    {
        Debug.Log($"[XantaPers iOS] Выдано монет: {amount}");
    }
}
