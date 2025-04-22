using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApplication1;

namespace WebApplication1IntegrationTest
{
    public class UserControllerFixture
    {
        [Fact]
        public async Task LoginPageTest()
        {
            // arrange
            var factory = new WebApplicationFactory<TestEntryPointClass>();
            var client = factory.CreateClient();

            // act
            var response = await client.GetAsync("/");

            // assert
            Assert.NotNull(response);

            var content = await response.Content.ReadAsStringAsync();
            ArrayList contentKeywords = new ArrayList() { "Username", "Password", "Register", "Login", "form", "input", "label"};
            foreach(string s in contentKeywords)
            {
                Assert.Contains(s, content);
            }

            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
