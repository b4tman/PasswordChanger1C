using PasswordChanger1C;
using Xunit;

namespace PasswordChanger1C.Tests
{
    using static CommonModule;

    public class CommonModule_Tests
    {
        [Theory()]
        [InlineData("", "2jmj7l5rSw0yVb/vlWAYkK/YBwk=")]
        [InlineData("1", "NWoZK3kTsExUV00Ywo1G5jlUKKs=")]
        [InlineData("test1", "tESsBmE/yNY3lb6a0L6vVQEZNqw=")]
        [InlineData("TEST1", "/+tKGP8cN+WSkMhrkt8o9l21hNk=")]
        [InlineData("TeSt1", "X5pltlMitHnQjMO9ZteC3800pf8=")]
        [InlineData("234234234SDFsrew$%2", "oIWJb4uuvo46fYk0GLHAhcEM1is=")]
        public void EncryptStringSHA1_Test(string input, string hash)
        {
            Assert.Equal(hash, EncryptStringSHA1(input));
        }

        [Theory()]
        [InlineData("2jmj7l5rSw0yVb/vlWAYkK/YBwk=", true)]
        [InlineData("/+tKGP8cN+WSkMhrkt8o9l21hNk=", true)]
        [InlineData("2jmj7l5r'wyVb/vYkK/YBwk=", false)]
        [InlineData("2jmj7l5r'wyVb/vlWAYksdfsdf345sfdK/YBwk=", false)]
        [InlineData("2jm j7l5r'wyVb /vl WAYkK/YBwk=", false)]
        [InlineData("2jmj7l5r'w\tyVb/vlWAYkK/YBwk=", false)]
        [InlineData("2jmj7l5r'wyVb/vlWAYk\rK/YBwk=", false)]
        [InlineData("2jmj7l\n5r'wyVb/vWAYk4K/YBwk=", false)]
        [InlineData("2jmj,l5rSw0yVb/vlWAY,K/YBwk=", false)]
        [InlineData("2jmj7l5'Sw0yVb/vlWAYkK/YBwk=", false)]
        [InlineData("", false)]
        [InlineData("X5pltlMitHnQjMO9ZteC3800pf8=", true)]
        [InlineData("oIWJb4uuvo46fYk0GLHAhcEM1is=", true)]
        public void IsPasswordHash_Test(string input, bool is_valid)
        {
            Assert.Equal(is_valid, IsPasswordHash(input));
        }
    }
}