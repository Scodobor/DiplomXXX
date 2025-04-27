using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FormQuestion : MonoBehaviour
{
    DataRow row;
    public TMP_InputField question;
    public TMP_InputField answer1;
    public TMP_InputField answer2;
    public TMP_InputField answer3;
    public TMP_InputField answer4;
    public TMP_Dropdown category;
    public TMP_Dropdown level;

    void Start()
    {
        row = FindAnyObjectByType<DataHolder>().dataRowQuestion;

        question.text = row["question"].ToString();
        answer1.text = row["answer1"].ToString();
        answer2.text = row["answer2"].ToString();
        answer3.text = row["answer3"].ToString();
        answer4.text = row["answer4"].ToString();

        category.AddOptions(FindAnyObjectByType<DataHolder>().listCategories);
        category.value = category.options.FindIndex(option => option.text == row["category"].ToString());

        List<string> levelOptions = new List<string> { "Легко", "Средне", "Тяжело" };
        level.AddOptions(levelOptions);
        level.value = Convert.ToInt32(row["level"]) - 1;
    }

}
