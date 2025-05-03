using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI;
using UnityEditorInternal;
using Button = UnityEngine.UI.Button;

public class QuizSettings : MonoBehaviour
{
    public Button buttonPlay;
    public Toggle toggleLevel1;
    public Toggle toggleLevel2;
    public Toggle toggleLevel3;

    public GameObject buttonPrefab; // Prefab for your button
    public Transform buttonsParent; // Parent object for buttons (should have ScrollRect)
    public ScrollRect scrollRect; // ScrollRect component for scrolling
    public string columnName = "category"; // Column to get values from
    DataTable dataTable = new DataTable();
    List<string> categories = new List<string>();
    List<int> levels = new List<int>();
    private GridLayoutGroup gridLayout;

    void Start()
    {
        // Ensure we have all required references
        if (buttonPrefab == null || buttonsParent == null || scrollRect == null)
        {
            Debug.LogError("Не указаны элементы интерфейса для раздела категорий!");
            return;
        }

        // Add GridLayoutGroup if not present
        gridLayout = buttonsParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = buttonsParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        GetCategories();
    }

    public async void GetCategories()
    {
        try
        {
            //подключение к БД
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT DISTINCT category FROM quiz ORDER BY category;";
            MySqlDataReader reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);
        }
        catch (Exception ex)
        {
            Debug.Log("Не удалось получить категории");
            Debug.Log(ex.Message);
        }
        finally
        {
            CreateButtonsFromData();
        }
    }

    void CreateButtonsFromData()
    {
        // Get values from DataTable column
        List<string> values = new List<string>();

        // Assuming DataTable has rows and you can access column by name
        // This part depends on your specific DataTable implementation
        foreach (DataRow row in dataTable.Rows)
        {
            if (row[columnName] != null)
            {
                values.Add(row[columnName].ToString());
            }
        }

        // Sort values alphabetically
        values = values.OrderBy(v => v).ToList();

        // Create buttons
        foreach (string value in values)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonsParent);
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();

            if (buttonText != null)
            {
                buttonText.text = value;
            }

            // Add click handler
            Color defaultColor = button.image.color;
            button.onClick.AddListener(() => OnButtonClick(button, value, defaultColor));
        }
    }

    void OnButtonClick(Button button, string category, Color defaultColor)
    {
        // Отключение анимации кнопки
        Animator animator = button.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Смена цвета кнопки
        if (button.image.color.Equals(defaultColor))
        {
            button.image.color = Color.green;
            if (!categories.Contains(category))
            {
                categories.Add(category);
            }
        }
        else
        {
            button.image.color = defaultColor;
            categories.Remove(category);
        }

        // Включение анимации кнопки
        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void StartQuiz()
    {
        if (categories.Count > 0)
        {
            if (toggleLevel1.isOn)
            {
                levels.Add(1);
            }
            if (toggleLevel2.isOn)
            {
                levels.Add(2);
            }
            if (toggleLevel3.isOn)
            {
                levels.Add(3);
            }
            if (levels.Count > 0)
            {
                DataHolder dataHolder = FindAnyObjectByType<DataHolder>();
                if (dataHolder == null)
                {
                    GameObject go = new GameObject("DataHolder");
                    dataHolder = go.AddComponent<DataHolder>();
                }
                dataHolder.listQuizCategories = categories;
                dataHolder.listQuizLevels = levels;
                SceneManager.LoadScene("Quiz");
            }
        }
    }
}
