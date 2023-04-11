using System.Text;

namespace UPDBroadcast;

internal class NameGenerator
{
    private char[] vowels;
    private char[] consonants;

    private Random random;

    public NameGenerator()
    {
        random = new Random(DateTime.Now.Second);

        vowels = new[] { 'a', 'e', 'i', 'o', 'u', 'y' };

        consonants = new[] { 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k',
            'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z' };
    }

    public string Generate(int length)
    {
        StringBuilder sb = new StringBuilder();
        //initialize our vowel/consonant flag
        bool flag = (random.Next(2) == 0);
        for (int i = 0; i < length; i++)
        {
            sb.Append(GetChar(flag));
            flag = !flag; //invert the vowel/consonant flag
        }

        sb[0] = char.ToUpper(sb[0]);

        return sb.ToString();
    }

    private char GetChar(bool vowel)
    {
        if (vowel)
            return vowels[random.Next(vowels.Length)];

        return consonants[random.Next(consonants.Length)];
    }

}