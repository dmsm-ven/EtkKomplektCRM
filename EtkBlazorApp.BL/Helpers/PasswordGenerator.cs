using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.BL
{
    public static class PasswordGenerator
    {      
        readonly static char[] Numbers = "0123456789".ToArray();
        readonly static char[] Letters = "abcdefghijklmnopqrstuvwxyz".ToArray();
        readonly static char[] SpecialCharacters = "!#$%&'()*+,-./:;<=>?@[\\]^_`{|}~".ToArray();


        public static async Task<string> GeneratePassword(int length = 8, bool allowUpperCase = true, bool allowSpecialCharacters = false)
        {
            if(length <= 0) { return string.Empty; }

            var sb = new StringBuilder();

            for(int i = 0; i < length; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25));
                var randomGroupIndex = 1 + new Random().Next(allowSpecialCharacters ? 3 : 2);
                char[] group = null;
                switch (randomGroupIndex)
                {
                    case 1: group = Numbers; break;
                    case 2: group = Letters; break;
                    case 3: group = SpecialCharacters; break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(25));
                int randomIndex = new Random().Next(group.Length);
                char randomCharacter = group[randomIndex];

                if (group == Letters && allowUpperCase)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(25));
                    var randomCase = new Random().Next(2);
                    randomCharacter = randomCase == 0 ? randomCharacter : char.ToUpper(randomCharacter);
                }

                sb.Append(randomCharacter);
            }

            var password = sb.ToString();
            return password;
        }
    }
}
