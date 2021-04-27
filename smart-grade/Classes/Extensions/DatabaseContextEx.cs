using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MySqlConnector;

namespace FirestormSW.SmartGrade.Extensions
{
    public static class DatabaseContextEx
    {
        public static DbContextOptionsBuilder UseMySql([NotNull] this DbContextOptionsBuilder optionsBuilder, [NotNull] string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            var serverVersion = connection.ServerVersion.ToLower();
            bool mariaDb = serverVersion.Contains("mariadb");
            var versionMatch = Regex.Match(serverVersion, mariaDb ? @"(\d+)\.(\d+)\.(\d+)(?=-[Mm]aria)" : @"(\d+)\.(\d+)\.(\d+)");
            var version = new Version(
                int.Parse(versionMatch.Groups[1].Value),
                int.Parse(versionMatch.Groups[2].Value),
                int.Parse(versionMatch.Groups[3].Value));


            connection.Close();
            return optionsBuilder.UseMySql(connectionString, mariaDb ? new MariaDbServerVersion(version) : new MySqlServerVersion(version));
        }
    }
}