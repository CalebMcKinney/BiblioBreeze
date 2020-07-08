using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBreeze
{
    public class CodeGenerator
    {
        public string GenerateCode()
        {
            string code = GetRandomWord();

            Random rand = new Random();
            int frontOrBack = rand.Next(2);

            if (frontOrBack == 1)
            {
                code = code + rand.Next(100);
            }
            else
            {
                code = rand.Next(100) + code;
            }

            return code;
        }

        private string GetRandomWord()
        {
            string[] lines = File.ReadAllLines(@"..\..\MiscResources\AllWords.txt");
            Random rand = new Random();
            return lines[rand.Next(lines.Length)];
        }
    }
}
