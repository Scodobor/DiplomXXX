using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class Questions : MonoBehaviour
{
    public GameObject buttonPrefab; // Prefab for your button
    public Transform buttonsParent; // Parent object for buttons (should have ScrollRect)
    public ScrollRect scrollRect; // ScrollRect component for scrolling
    DataTable dataTable = new DataTable();
    string category;
    public TMP_Text categoryTitle;

    private GridLayoutGroup gridLayout;

    void Start()
    {
        // Ensure we have all required references
        if (buttonPrefab == null || buttonsParent == null || scrollRect == null)
        {
            Debug.LogError("Не указаны элементы интерфейса для раздела вопросов!");
            return;
        }

        // Add GridLayoutGroup if not present
        gridLayout = buttonsParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = buttonsParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        category = PlayerPrefs.GetString("Category");
        categoryTitle.text = category;
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
            command.CommandText = @"SELECT id, question, answer1, answer2, answer3, answer4, category, level FROM quiz WHERE category = @category;";
            command.Parameters.AddWithValue("@category", category);
            MySqlDataReader reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);
        }
        catch (Exception ex)
        {
            Debug.Log("Не удалось получить вопросы");
            Debug.Log(ex.Message);
        }
        finally
        {
            CreateButtonsFromData();
        }
    }

    void CreateButtonsFromData()
    {
        foreach (DataRow row in dataTable.Rows)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonsParent);
            Button button = buttonObj.GetComponent<Button>();
            TMP_Text questionText = buttonObj.transform.Find("Text Question")?.GetComponent<TMP_Text>();
            TMP_Text idText = buttonObj.transform.Find("Text ID")?.GetComponent<TMP_Text>();

            idText.text = row["id"].ToString();
            questionText.text = row["question"].ToString();

            button.onClick.AddListener(() => OnButtonClick(row));
        }

    }

    void OnButtonClick(DataRow row)
    {
        FindAnyObjectByType<DataHolder>().dataRowQuestion = row;
        SceneManager.LoadScene("FormQuestion");
    }
}