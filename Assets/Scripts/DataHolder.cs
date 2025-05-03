using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class DataHolder : MonoBehaviour
{
    public DataRow dataRowQuestion;
    public List<string> listCategories;
    public List<string> listQuizCategories;
    public List<int> listQuizLevels;

    private static DataHolder instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
