using MySqlConnector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Data;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;

public class Quiz : MonoBehaviour
{
    //объявление элементов интерфейса unity
    public TMP_Text questionNumber;
    public TMP_Text questionText;
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;
    public Button playAgainButton;

    //объявление переменных
    int correctAnswer = 0;
    int questionCounter = 0;
    int answerCounter = 0;
    DataTable questionsTable = new DataTable();

    void Start()
    {
        GetQuestions();
        answerButton1.onClick.AddListener(() => CheckAnswer(answerButton1, 1));
        answerButton2.onClick.AddListener(() => CheckAnswer(answerButton2, 2));
        answerButton3.onClick.AddListener(() => CheckAnswer(answerButton3, 3));
        answerButton4.onClick.AddListener(() => CheckAnswer(answerButton4, 4));
    }

    public async void GetQuestions()
    {
        DataHolder dataHolder = FindAnyObjectByType<DataHolder>();
        if (dataHolder == null)
        {
            GameObject go = new GameObject("DataHolder");
            dataHolder = go.AddComponent<DataHolder>();
        }
        List<string> categories = dataHolder.listQuizCategories;
        List<int> levels = dataHolder.listQuizLevels;
        try 
        { 
            //подключение к БД
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            var sqlCategories = new List<string>();
            for (int i = 0; i < categories.Count; i++)
            {
                sqlCategories.Add($"@c{i}");
            }

            var sqlLevels = new List<string>();
            for (int i = 0; i < levels.Count; i++)
            {
                sqlLevels.Add($"@l{i}");
            }

            //запрос на получение 5 случайных вопросов 
            using var command = new MySqlCommand(
                $"SELECT question, answer1, answer2, answer3, answer4 FROM quiz WHERE category IN ({string.Join(", ", sqlCategories)}) AND level IN ({string.Join(", ", sqlLevels)}) ORDER BY RAND() LIMIT 5;",
                connection);

            // Добавляем параметры
            for (int i = 0; i < categories.Count; i++)
            {
                command.Parameters.AddWithValue($"@c{i}", categories[i]);
            }

            for (int i = 0; i < levels.Count; i++)
            {
                command.Parameters.AddWithValue($"@l{i}", levels[i]);
            }

            MySqlDataReader reader = await command.ExecuteReaderAsync();
            questionsTable.Load(reader);
            DisplayQuestion();
        }
        catch (Exception ex)
        {
            Debug.Log("Не удалось получить вопросы");
            Debug.Log(ex.Message);
        }
    }

    //выводит на экран вопрос и варианты ответа (меняет вопрос на следующий)
    public void DisplayQuestion()
    {
        try
        {
            if (questionCounter < questionsTable.Rows.Count)
            {
                DataRow row = questionsTable.Rows[questionCounter];
                questionText.text = (string)row[0];

                System.Random random = new System.Random();
                int randomNumber = random.Next(4);
                correctAnswer = randomNumber + 1;

                answerButton1.GetComponentInChildren<TMP_Text>().text = (string)row[randomNumber + 1];
                answerButton2.GetComponentInChildren<TMP_Text>().text = (string)row[(1 + randomNumber) % 4 + 1];
                answerButton3.GetComponentInChildren<TMP_Text>().text = (string)row[(2 + randomNumber) % 4 + 1];
                answerButton4.GetComponentInChildren<TMP_Text>().text = (string)row[(3 + randomNumber) % 4 + 1];

                questionCounter++;
                questionNumber.text = questionCounter.ToString();
            }
            else
            {
                EndQuiz();
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //проверяет правильность выбранного варианта ответа
    public async void CheckAnswer(Button button, int buttonNumber)
    {
        // Отключение анимации кнопки
        Animator animator = button.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false; 
        }

<<<<<<< Updated upstream
        // Смена цвета
=======
        // Смена цвета кнопки
>>>>>>> Stashed changes
        Color defaultColor = button.image.color;
        if (buttonNumber == correctAnswer)
        {
            button.image.color = Color.green;
            answerCounter += 1;
        }
        else
        {
            button.image.color = Color.red;
        }

        await Task.Delay(2000);

        // Возврат стандартного цвета
        button.image.color = defaultColor;
<<<<<<< Updated upstream

=======
>>>>>>> Stashed changes
        // Включение анимации кнопки
        if (animator != null)
        {
            animator.enabled = true; 
<<<<<<< Updated upstream
            animator.Rebind();
=======
            animator.Rebind(); 
>>>>>>> Stashed changes
            animator.Update(0f); 
        }

        DisplayQuestion();
    }

    //конец викторины, меняются кнопки на экране
    public void EndQuiz()
    {   
        questionNumber.gameObject.SetActive(false);
        answerButton1.gameObject.SetActive(false);
        answerButton2.gameObject.SetActive(false);
        answerButton3.gameObject.SetActive(false);
        answerButton4.gameObject.SetActive(false);

        questionText.text = string.Format("Вы правильно ответили на {0} вопросов из {1}", answerCounter, questionCounter);
        playAgainButton.gameObject.SetActive(true);
    }
}
