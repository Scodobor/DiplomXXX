using MySqlConnector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;


public class Registration : MonoBehaviour
{
    //объявление элементов интерфейса unity
    public TMP_InputField loginField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordRepeatField;
    public Button submitButton;

    //хэширование пароля
    private byte[] CalculateSHA256(string passwordSalted)
    {
        SHA256 sha256 = SHA256Managed.Create();
        byte[] hash;
        UTF8Encoding objectUTF8 = new UTF8Encoding();
        hash = sha256.ComputeHash(objectUTF8.GetBytes(passwordSalted));

        return hash;
    }

    public async void Register()
    {
        string login = loginField.text;
        string password = passwordField.text;

        try
        {
            //подключение к БД
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            //запрос, проверяющий, свободно ли имя пользователя
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT login FROM user WHERE login = @login;";
            command.Parameters.AddWithValue("@login", login);

            if (await command.ExecuteScalarAsync() == null)
            {
                //генерация соли для хэширования
                System.Random random = new System.Random();
                string salt = "";
                for (int i = 0; i < 10; i++)
                {
                    salt += Convert.ToChar(random.Next(0, 26) + 65);
                }
                
                //генерация хэша
                byte[] hash = CalculateSHA256(password + salt);

                //запрос на регистрацию пользователя
                command.CommandText = @"INSERT INTO user(login, hash, salt) VALUES (@login, @hash, @salt);";
                command.Parameters.AddWithValue("@hash", hash);
                command.Parameters.AddWithValue("@salt", salt);

                if (await command.ExecuteNonQueryAsync() == 1)
                {
                    Debug.Log("Вы успешно зарегистрированы");
                    SceneManager.LoadScene("Menu");
                }
                else
                {
                    Debug.Log("Ошибка регистрации");
                }
            }
            else
            {
                Debug.Log("Данное имя пользователя занято");
            }
        } catch (MySqlException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public async void Authorize()
    {
        string login = loginField.text;
        string password = passwordField.text;

        try
        {
            //подключение к БД
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            //запрос на получение данных пользователя
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT hash, salt FROM user WHERE login = @login;";
            command.Parameters.AddWithValue("@login", login);
            MySqlDataReader reader = await command.ExecuteReaderAsync();

            //чтение полученных данных
            if (reader.HasRows)
            {
                reader.Read();
                byte[] dbHash = (byte[])reader.GetValue(0);
                string salt = (string)reader.GetValue(1);
                reader.Close();

                //проверка пароля
                byte[] hash = CalculateSHA256(password + salt);
                if (dbHash.SequenceEqual(hash))
                {
                    Debug.Log("Вы вошли в аккаунт");
                    SceneManager.LoadScene("Menu");
                }
                else
                {
                    Debug.Log("Неправильный пароль");
                }
            }
            else
            {
                Debug.Log("Пользователь не найден");
            }
        }
        catch (MySqlException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //кнопка регистрации включается, когда логин >= 6 символов, пароль >= 8 символов, пароль и повтор пароля совпадают
    public void VerifyInputsRegister()
    {
        submitButton.interactable = (loginField.text.Length >= 6 && passwordField.text.Length >= 8 && passwordRepeatField.text == passwordField.text);
    }

    //аналогичная проверка длины логина и пароля для входа в аккаунт
    public void VerifyInputsAuthorize()
    {
        submitButton.interactable = (loginField.text.Length >= 6 && passwordField.text.Length >= 8);
    }
}
