using MySqlConnector;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FormQuestion : MonoBehaviour
{
    DataRow row;
    public TMP_InputField question;
    public TMP_InputField answer1;
    public TMP_InputField answer2;
    public TMP_InputField answer3;
    public TMP_InputField answer4;
    public TMP_Dropdown dropdownCategory;
    public TMP_Dropdown dropdownLevel;
    public Button buttonExit;
    public Button buttonSave;
    public Button buttonDelete; 
    public TMP_InputField inputCategory;

    void Start()
    {
        dropdownCategory.AddOptions(FindAnyObjectByType<DataHolder>().listCategories);

        List<string> levelOptions = new List<string> { "Легко", "Средне", "Тяжело" };
        dropdownLevel.AddOptions(levelOptions);

        row = FindAnyObjectByType<DataHolder>().dataRowQuestion;

        if (row != null)
        {
            question.text = row["question"].ToString();
            answer1.text = row["answer1"].ToString();
            answer2.text = row["answer2"].ToString();
            answer3.text = row["answer3"].ToString();
            answer4.text = row["answer4"].ToString();
            
            dropdownCategory.value = dropdownCategory.options.FindIndex(option => option.text == row["category"].ToString());
            dropdownLevel.value = Convert.ToInt32(row["level"]) - 1;

            buttonDelete.gameObject.SetActive(true);
            buttonSave.onClick.AddListener(() => SaveQuestion("update"));

            inputCategory.gameObject.SetActive(false);
            dropdownCategory.gameObject.SetActive(true);
        }
        else
        {
            if (PlayerPrefs.GetString("Category").Equals("AddNewCategory"))
            {
                inputCategory.gameObject.SetActive(true);
                dropdownCategory.gameObject.SetActive(false);
                buttonExit.onClick.RemoveAllListeners();
                buttonExit.onClick.AddListener(() => SceneManager.LoadScene("Categories"));
            }
            else
            {
                inputCategory.gameObject.SetActive(false);
                dropdownCategory.gameObject.SetActive(true);
                dropdownCategory.value = dropdownCategory.options.FindIndex(option => option.text == PlayerPrefs.GetString("Category"));
            }

            buttonSave.onClick.AddListener(() => SaveQuestion("insert"));
        }

        
    }

    public async void DeleteQuestion()
    {
        //подключение к БД
        using var connection = new MySqlConnection(Global.builder.ConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = @"DELETE FROM quiz WHERE id = @id;";
        command.Parameters.AddWithValue("@id", Convert.ToInt32(row["id"]));

        if (await command.ExecuteNonQueryAsync() == 1)
        {
            Debug.Log("Вопрос успешно удален");
            SceneManager.LoadScene("Questions");
        }
        else
        {
            Debug.Log("Ошибка удаления вопроса");
        }
    }

    async void SaveQuestion(string mode)
    {
        //подключение к БД
        using var connection = new MySqlConnection(Global.builder.ConnectionString);
        await connection.OpenAsync();
        using var command = connection.CreateCommand();

        if(mode.Equals("update"))
        {
            command.CommandText = @"UPDATE quiz SET category = @category, level = @level, question = @question," +
            " answer1 = @answer1, answer2 = @answer2, answer3 = @answer3, answer4 = @answer4 WHERE id = @id;";
            command.Parameters.AddWithValue("@id", Convert.ToInt32(row["id"]));
        }
        else
        {
            command.CommandText = @"INSERT INTO quiz(category, level, question, answer1, answer2, answer3, answer4)" +
            " VALUES (@category, @level, @question, @answer1, @answer2, @answer3, @answer4);";
        }
        
        if (inputCategory.gameObject.activeSelf)
        {
            command.Parameters.AddWithValue("@category", inputCategory.text);
            PlayerPrefs.SetString("Category", inputCategory.text);
            PlayerPrefs.Save();
        }
        else
        {
            command.Parameters.AddWithValue("@category", dropdownCategory.options[dropdownCategory.value].text);
        }
        command.Parameters.AddWithValue("@level", Convert.ToInt32(dropdownLevel.value) + 1);
        command.Parameters.AddWithValue("@question", question.text);
        command.Parameters.AddWithValue("@answer1", answer1.text);
        command.Parameters.AddWithValue("@answer2", answer2.text);
        command.Parameters.AddWithValue("@answer3", answer3.text);
        command.Parameters.AddWithValue("@answer4", answer4.text);

        if (await command.ExecuteNonQueryAsync() == 1)
        {
            Debug.Log("Вопрос успешно сохранен");
        }
        else
        {
            Debug.Log("Ошибка сохранения вопроса");
        }
    }
}
