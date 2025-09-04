using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests.Utils
{
    public static class Faker
    {
        public static string RandomEmail()
        {
            var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            return $"heba.qa+{ts}@example.com";
        }
    }
}

