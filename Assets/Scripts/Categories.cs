using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.DebugUI.Table;

public class Categories : MonoBehaviour
{
    public GameObject buttonPrefab; // Prefab for your button
    public Transform buttonsParent; // Parent object for buttons (should have ScrollRect)
    public ScrollRect scrollRect; // ScrollRect component for scrolling
    public string columnName = "category"; // Column to get values from
    DataTable dataTable = new DataTable();

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

        DataHolder dataHolder = FindAnyObjectByType<DataHolder>();
        if (dataHolder == null)
        {
            GameObject go = new GameObject("DataHolder");
            dataHolder = go.AddComponent<DataHolder>();
        }
        dataHolder.listCategories = values;

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
            button.onClick.AddListener(() => OnButtonClick(value));
        }

    }

    void OnButtonClick(string category)
    {
        PlayerPrefs.SetString("Category", category);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Questions");
    }

    public void ButtonNewCategory()
    {
        PlayerPrefs.SetString("Category", "AddNewCategory");
        PlayerPrefs.Save();
        FindAnyObjectByType<DataHolder>().dataRowQuestion = null;
        SceneManager.LoadScene("FormQuestion");
    }
}