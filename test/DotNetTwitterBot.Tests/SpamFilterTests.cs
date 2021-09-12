using System.Linq;
using Xunit;

namespace DotNetTwitterBot.Tests
{
    public class SpamFilterTests
    {
        [Fact]
        public void SmokeTest()
        {
            // Arrange
            var spamFilter = new SpamFilter("spam_filter.onnx");
            var spamText =
"#include<stdio.h>" +
"#include<conio.h>" +
"void main()" +
"{" +
"printf{\"#GaneshChaturthi #DBoss};" +
@"getch()" +
@"#forex #bot #AI #Website #CodeNewbie #women #hacker #flutter #python #100DaysOfCode #ad #WomenWhoCode #tech #css  #cybersecurity #Blockchain #DataScience #infosec #dotnet #ClimateAction";

            var input = new[]
            {
                spamText,
                ".NET is a great language!",
                spamText
            };


            // Act
            var isSpam = spamFilter.Run(input).ToArray();

            // Assert
            Assert.True(isSpam[0]);
            Assert.False(isSpam[1]);
            Assert.True(isSpam[2]);
        }

        [Fact]
        public void VolumeTest()
        {
            // Arrange
            var spamFilter = new SpamFilter("spam_filter.onnx");
            var spamText =
"#include<stdio.h>" +
"#include<conio.h>" +
"void main()" +
"{" +
"printf{\"#GaneshChaturthi #DBoss};" +
@"getch()" +
@"#forex #bot #AI #Website #CodeNewbie #women #hacker #flutter #python #100DaysOfCode #ad #WomenWhoCode #tech #css  #cybersecurity #Blockchain #DataScience #infosec #dotnet #ClimateAction";

            var volume = 1000;
            var input = new string[volume];

            for (int i = 0; i < input.Length; i++)
            {
                input[i] = spamText;
            }

            // Act
            var isSpam = spamFilter.Run(input).ToArray();

            // Assert
            for (int i = 0; i < isSpam.Length; i++)
            {
                Assert.True(isSpam[i]);
            }
        }
    }
}
