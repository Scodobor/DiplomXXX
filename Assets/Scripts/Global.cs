using MySqlConnector;
using UnityEngine;

public static class Global
{
    //это класс дл€ переменных, которые используютс€ в нескольких окнах игры сразу
    //пока что тут только данные дл€ подключени€ к Ѕƒ

    public static MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
    {
        Server = "sql7.freesqldatabase.com",
        UserID = "sql7747973",
        Password = "DGN3SejZT9",
        Database = "sql7747973",
    };
}
