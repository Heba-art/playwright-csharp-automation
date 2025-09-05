using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using NUnit.Framework;

namespace PlaywrightTests.Utils
{
    public record Creds(string Email, string Password);

    public static class CredentialStore
    {
        // We use a timestamp to uniquely name the file.
        private static readonly string FilePath =
            Path.Combine(TestContext.CurrentContext.WorkDirectory,
                         $"lastUser_{DateTime.Now:yyyyMMddHHmmss}.json");

        // Save email and password in a JSON file
        public static async Task SaveAsync(string email, string password)
        {
            var json = JsonSerializer.Serialize(new Creds(email, password));
            await File.WriteAllTextAsync(FilePath, json);
            TestContext.Out.WriteLine($"[INFO] Saved creds in: {Path.GetFileName(FilePath)}");
        }

        // Read data (last created file)
        public static async Task<Creds?> LoadAsync()
        {
            var dir = TestContext.CurrentContext.WorkDirectory;
            var files = Directory.GetFiles(dir, "lastUser_*.json");
            if (files.Length == 0) return null;

        // bring latest file
            var latest = files.OrderByDescending(f => f).First();
            var json = await File.ReadAllTextAsync(latest);
            return JsonSerializer.Deserialize<Creds>(json);
        }
    }
}

