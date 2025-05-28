using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatchFontChanger : EditorWindow
{
    private Font newFont;
    private TMP_FontAsset newTMPFont;

    // ���� ��� �������� ����
    [MenuItem("Tools/Change Fonts")]
    public static void ShowWindow()
    {
        GetWindow<BatchFontChanger>("Change Fonts");
    }

    // ��������� ����
    private void OnGUI()
    {
        GUILayout.Label("Change Fonts in Scene", EditorStyles.boldLabel);

        newFont = EditorGUILayout.ObjectField("New Font (Legacy)", newFont, typeof(Font), false) as Font;
        newTMPFont = EditorGUILayout.ObjectField("New TMP Font", newTMPFont, typeof(TMP_FontAsset), false) as TMP_FontAsset;

        if (GUILayout.Button("Apply to All Texts"))
        {
            ApplyFonts();
        }
    }

    // �������� ������ ��������� �������
    private static void ApplyFonts()
    {
        BatchFontChanger window = GetWindow<BatchFontChanger>();

        // Legacy UI Text (UnityEngine.UI.Text)
        Text[] allTexts = Object.FindObjectsOfType<Text>();
        foreach (Text text in allTexts)
        {
            if (window.newFont != null)
            {
                text.font = window.newFont;
                EditorUtility.SetDirty(text); // ��������� ���������
            }
        }

        // TextMeshProUGUI
        TextMeshProUGUI[] allTMPTexts = Object.FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI tmpText in allTMPTexts)
        {
            if (window.newTMPFont != null)
            {
                tmpText.font = window.newTMPFont;
                EditorUtility.SetDirty(tmpText);
            }
        }

        Debug.Log("Fonts changed successfully!");
    }
}