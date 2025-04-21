using MySqlConnector;
using UnityEngine;

public static class Global
{
    //это класс дл€ переменных, которые используютс€ в нескольких окнах игры сразу
    //пока что тут только данные дл€ подключени€ к Ѕƒ

    public static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "sql.freedb.tech",
        UserID = "freedb_antonov",
        Password = "P*zwcAWSc7&h@5E",
        Database = "freedb_mgkeit",
    };
}
