using MySqlConnector;
using UnityEngine;

public static class Global
{
    //��� ����� ��� ����������, ������� ������������ � ���������� ����� ���� �����
    //���� ��� ��� ������ ������ ��� ����������� � ��

    public static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "sql.freedb.tech",
        UserID = "freedb_antonov",
        Password = "P*zwcAWSc7&h@5E",
        Database = "freedb_mgkeit",
    };
}
