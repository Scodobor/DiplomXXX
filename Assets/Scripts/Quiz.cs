using MySqlConnector;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Data;

public class Quiz : MonoBehaviour
{
    //���������� ��������� ���������� unity
    public TMP_Text questionNumber;
    public TMP_Text questionText;
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button answerButton4;
    public Button playAgainButton;

    //���������� ����������
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
        try 
        { 
            //����������� � ��
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            //������ �� ��������� 5 ��������� �������� 
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT question, answer1, answer2, answer3, answer4" +
                " FROM quiz ORDER BY RAND() LIMIT 5;";
            MySqlDataReader reader = await command.ExecuteReaderAsync();
            questionsTable.Load(reader);
            DisplayQuestion();
        }
        catch (Exception ex)
        {
            Debug.Log("�� ������� �������� �������");
            Debug.Log(ex.Message);
        }
    }

    //������� �� ����� ������ � �������� ������ (������ ������ �� ���������)
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

    //��������� ������������ ���������� �������� ������
    public async void CheckAnswer(Button button, int buttonNumber)
    {
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
        button.image.color = defaultColor;
        DisplayQuestion();
    }

    //����� ���������, �������� ������ �� ������
    public void EndQuiz()
    {   
        questionNumber.gameObject.SetActive(false);
        answerButton1.gameObject.SetActive(false);
        answerButton2.gameObject.SetActive(false);
        answerButton3.gameObject.SetActive(false);
        answerButton4.gameObject.SetActive(false);

        questionText.text = string.Format("�� ��������� �������� �� {0} �������� �� {1}", answerCounter, questionCounter);
        playAgainButton.gameObject.SetActive(true);
    }
}
