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
using System.Collections;

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

    // ������ �� ������ ��� ����������� ���������� �������������
    public TMP_Text hintRemoveOneCountText;
    public TMP_Text hintTwoOptionsCountText;
    public TMP_Text hintShowCorrectCountText;
    public TMP_Text hintReplaceQuestionCountText;

    // ������ �������
    public Button hintRemoveOneButton;
    public Button hintTwoOptionsButton;
    public Button hintShowCorrectButton;
    public Button hintReplaceQuestionButton;

    // �������� ������������� �������
    private int usesHintRemoveOne = 1;
    private int usesHintTwoOptions = 1;
    private int usesHintShowCorrect = 1;
    private int usesHintReplaceQuestion = 1;

    // ����, ����� �� ������������ ��������� ������� �� ���
    private bool isUsingHint = false;

    //���������� ����������
    int correctAnswer = 0;
    int questionCounter = 0;
    int answerCounter = 0;
    DataTable questionsTable = new DataTable();
    private bool isCheckingAnswer = false;

    void Start()
    {
        GetQuestions();
        answerButton1.onClick.AddListener(() => CheckAnswer(answerButton1, 1));
        answerButton2.onClick.AddListener(() => CheckAnswer(answerButton2, 2));
        answerButton3.onClick.AddListener(() => CheckAnswer(answerButton3, 3));
        answerButton4.onClick.AddListener(() => CheckAnswer(answerButton4, 4));

        // ������
        hintRemoveOneButton.onClick.AddListener(UseHint_RemoveOne);
        hintTwoOptionsButton.onClick.AddListener(UseHint_TwoOptions);
        hintShowCorrectButton.onClick.AddListener(UseHint_ShowCorrect);
        hintReplaceQuestionButton.onClick.AddListener(UseHint_ReplaceQuestion);
        UpdateHintUsageDisplays();
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
            //����������� � ��
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

            //������ �� ��������� 5 ��������� �������� 
            using var command = new MySqlCommand(
                $"SELECT question, answer1, answer2, answer3, answer4 FROM quiz WHERE category IN ({string.Join(", ", sqlCategories)}) AND level IN ({string.Join(", ", sqlLevels)}) ORDER BY RAND() LIMIT 5;",
                connection);

            // ��������� ���������
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

                // ������� ������ ���� ��������� �������
                List<string> answers = new List<string>
            {
                (string)row[1], // ����������
                (string)row[2],
                (string)row[3],
                (string)row[4]
            };

                // ������������ ��
                Shuffle(answers);

                // ������������� ����� �� ������
                answerButton1.GetComponentInChildren<TMP_Text>().text = answers[0];
                answerButton2.GetComponentInChildren<TMP_Text>().text = answers[1];
                answerButton3.GetComponentInChildren<TMP_Text>().text = answers[2];
                answerButton4.GetComponentInChildren<TMP_Text>().text = answers[3];

                // ����������, ����� �� ������������ ������ �������� ���������� �����
                correctAnswer = answers.FindIndex(a => a == (string)row[1]) + 1;
                // ���� �� ������ � ����� ������� fallback ��� ��������� ������

                questionCounter++;
                questionNumber.text = questionCounter.ToString();
                EnableAllHints();
                answerButton1.interactable = true;
                answerButton2.interactable = true;
                answerButton3.interactable = true;
                answerButton4.interactable = true;
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

    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1); 
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    //��������� ������������ ���������� �������� ������
    public async void CheckAnswer(Button button, int buttonNumber)
    {
        // ���� ������� ��� ����������� - �������
        if (isCheckingAnswer)
        {
            return;
        }
        isCheckingAnswer = true;

        // ���������� �������� ������
        Animator animator = button.GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = false; 
        }

        // ����� ����� ������
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

        // ������� ������������ �����
        button.image.color = defaultColor;

        // ��������� �������� ������
        if (animator != null)
        {
            animator.enabled = true; 
            animator.Rebind();
            animator.Update(0f); 
        }

        DisplayQuestion();
        isCheckingAnswer = false;
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

    private void UseHint_RemoveOne()
    {
        if (isCheckingAnswer || isUsingHint || usesHintRemoveOne <= 0) return;

        usesHintRemoveOne--;
        UpdateHintUsageDisplays();
        List<Button> buttons = new List<Button> { answerButton1, answerButton2, answerButton3, answerButton4 };
        List<string> answers = new List<string>
    {
        answerButton1.GetComponentInChildren<TMP_Text>().text,
        answerButton2.GetComponentInChildren<TMP_Text>().text,
        answerButton3.GetComponentInChildren<TMP_Text>().text,
        answerButton4.GetComponentInChildren<TMP_Text>().text
    };

        for (int i = 0; i < answers.Count; i++)
        {
            if (i + 1 != correctAnswer)
            {
                buttons[i].interactable = false;
                buttons[i].GetComponentInChildren<TMP_Text>().text = "";
                break;
            }
        }

        DisableAllHints();
    }

    private void UseHint_TwoOptions()
    {
        if (isCheckingAnswer || isUsingHint || usesHintTwoOptions <= 0) return;

        usesHintTwoOptions--;
        UpdateHintUsageDisplays();
        List<Button> buttons = new List<Button> { answerButton1, answerButton2, answerButton3, answerButton4 };
        List<int> indexes = new List<int> { 0, 1, 2, 3 };
        indexes.Remove(correctAnswer - 1); // ������� ������ ����������� ������

        // �������� ��������� �� ������������
        int wrongIndex = indexes[UnityEngine.Random.Range(0, indexes.Count)];

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i != correctAnswer - 1 && i != wrongIndex)
            {
                buttons[i].interactable = false;
                buttons[i].GetComponentInChildren<TMP_Text>().text = "";
            }
        }

        DisableAllHints();
    }

    private async void UseHint_ShowCorrect()
    {
        if (isCheckingAnswer || isUsingHint || usesHintShowCorrect <= 0) return;

        usesHintShowCorrect--;
        UpdateHintUsageDisplays();
        isUsingHint = true;

        // �������� ��� ������
        List<Button> buttons = new List<Button>
    {
        answerButton1,
        answerButton2,
        answerButton3,
        answerButton4
    };

        // ������� ���������� ������
        Button correctButton = buttons[correctAnswer - 1];

        // ��������� ������������ ����� ���� ������
        Dictionary<Button, Color> originalColors = new Dictionary<Button, Color>();
        foreach (var btn in buttons)
        {
            originalColors[btn] = btn.image.color;
        }

        // ������ ����� ���� ������������ ������ �� ��������� (�� ������)
        foreach (var btn in buttons)
        {
            if (btn != correctButton)
            {
                btn.image.color = GetRandomColor();
            }
        }

        // ������������ ���������� ����� ������
        correctButton.image.color = Color.green;

        // ��� 1.5 �������
        await Task.Delay(100);

        // ������ ��������������� ������������ �����
        StartCoroutine(AnimateColorRestore(buttons, originalColors));

        isUsingHint = false;
    }

    private IEnumerator AnimateColorRestore(List<Button> buttons, Dictionary<Button, Color> originalColors)
    {
        float duration = 0.5f; // ����� ��������
        float elapsed = 0f;

        // ��������� ��������� �����
        Dictionary<Button, Color> startColors = new Dictionary<Button, Color>();
        foreach (var btn in buttons)
        {
            startColors[btn] = btn.image.color;
        }

        while (elapsed < duration)
        {
            foreach (var btn in buttons)
            {
                btn.image.color = Color.Lerp(startColors[btn], originalColors[btn], elapsed / duration);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // � ����� ������������� ������ ������������ ��������
        foreach (var btn in buttons)
        {
            btn.image.color = originalColors[btn];
        }
    }

    private Color GetRandomColor()
    {
        Color randomColor;
        float hue;

        // ���������� ����, �������� ������ ��������
        do
        {
            // ���������� ��������� HSV-����
            hue = UnityEngine.Random.Range(0f, 1f); // Hue: 0.0 - 1.0
            float saturation = UnityEngine.Random.Range(0.5f, 1f);
            float value = UnityEngine.Random.Range(0.8f, 1f); // �������

            randomColor = Color.HSVToRGB(hue, saturation, value);

        } while (IsGreenish(hue)); // ���������, ���� �� ������� ��-������

        return randomColor;
    }

    private bool IsGreenish(float hue)
    {
        // Hue � Unity: 
        // 0.0 = �������, 
        // ~0.33 = ������, 
        // ~0.66 = �����

        // �������� �������: �������� �� 0.28 �� 0.40
        return hue >= 0.21f && hue <= 0.43f;
    }

    private void UseHint_ReplaceQuestion()
    {
        if (isCheckingAnswer || isUsingHint || usesHintReplaceQuestion <= 0) return;

        usesHintReplaceQuestion--;
        UpdateHintUsageDisplays();

        DisplayQuestion();

        DisableAllHints();
    }

    private void DisableAllHints()
    {
        isUsingHint = true;
        hintRemoveOneButton.interactable = false;
        hintTwoOptionsButton.interactable = false;
        hintShowCorrectButton.interactable = false;
        hintReplaceQuestionButton.interactable = false;

    }

    private void EnableAllHints()
    {
        isUsingHint = false;
        hintRemoveOneButton.interactable = true;
        hintTwoOptionsButton.interactable = true;
        hintShowCorrectButton.interactable = true;
        hintReplaceQuestionButton.interactable = true;
    }

    private void UpdateHintUsageDisplays()
    {
        hintRemoveOneCountText.text = usesHintRemoveOne.ToString();
        hintTwoOptionsCountText.text = usesHintTwoOptions.ToString();
        hintShowCorrectCountText.text = usesHintShowCorrect.ToString();
        hintReplaceQuestionCountText.text = usesHintReplaceQuestion.ToString();
    }
}
