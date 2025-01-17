using MySqlConnector;
using UnityEngine;

public static class Global
{
    //это класс дл€ переменных, которые используютс€ в нескольких окнах игры сразу
    //пока что тут только данные дл€ подключени€ к Ѕƒ

    public static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "sql7.freesqldatabase.com",
        UserID = "sql7758157",
        Password = "W8e5PQgmU4",
        Database = "sql7758157",
    };
}
