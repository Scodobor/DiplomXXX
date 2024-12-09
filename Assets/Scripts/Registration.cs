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
    //���������� ��������� ���������� unity
    public TMP_InputField loginField;
    public TMP_InputField passwordField;
    public TMP_InputField passwordRepeatField;
    public Button submitButton;

    //����������� ������
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
            //����������� � ��
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            //������, �����������, �������� �� ��� ������������
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT login FROM user WHERE login = @login;";
            command.Parameters.AddWithValue("@login", login);

            if (await command.ExecuteScalarAsync() == null)
            {
                //��������� ���� ��� �����������
                System.Random random = new System.Random();
                string salt = "";
                for (int i = 0; i < 10; i++)
                {
                    salt += Convert.ToChar(random.Next(0, 26) + 65);
                }
                
                //��������� ����
                byte[] hash = CalculateSHA256(password + salt);

                //������ �� ����������� ������������
                command.CommandText = @"INSERT INTO user(login, hash, salt) VALUES (@login, @hash, @salt);";
                command.Parameters.AddWithValue("@hash", hash);
                command.Parameters.AddWithValue("@salt", salt);

                if (await command.ExecuteNonQueryAsync() == 1)
                {
                    Debug.Log("�� ������� ����������������");
                    SceneManager.LoadScene("Menu");
                }
                else
                {
                    Debug.Log("������ �����������");
                }
            }
            else
            {
                Debug.Log("������ ��� ������������ ������");
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
            //����������� � ��
            using var connection = new MySqlConnection(Global.builder.ConnectionString);
            await connection.OpenAsync();

            //������ �� ��������� ������ ������������
            using var command = connection.CreateCommand();
            command.CommandText = @"SELECT hash, salt FROM user WHERE login = @login;";
            command.Parameters.AddWithValue("@login", login);
            MySqlDataReader reader = await command.ExecuteReaderAsync();

            //������ ���������� ������
            if (reader.HasRows)
            {
                reader.Read();
                byte[] dbHash = (byte[])reader.GetValue(0);
                string salt = (string)reader.GetValue(1);
                reader.Close();

                //�������� ������
                byte[] hash = CalculateSHA256(password + salt);
                if (dbHash.SequenceEqual(hash))
                {
                    Debug.Log("�� ����� � �������");
                    SceneManager.LoadScene("Menu");
                }
                else
                {
                    Debug.Log("������������ ������");
                }
            }
            else
            {
                Debug.Log("������������ �� ������");
            }
        }
        catch (MySqlException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    //������ ����������� ����������, ����� ����� >= 6 ��������, ������ >= 8 ��������, ������ � ������ ������ ���������
    public void VerifyInputsRegister()
    {
        submitButton.interactable = (loginField.text.Length >= 6 && passwordField.text.Length >= 8 && passwordRepeatField.text == passwordField.text);
    }

    //����������� �������� ����� ������ � ������ ��� ����� � �������
    public void VerifyInputsAuthorize()
    {
        submitButton.interactable = (loginField.text.Length >= 6 && passwordField.text.Length >= 8);
    }
}
