using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Threading.Tasks;

namespace PlaywrightTests;
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    private const string ServerUrl = "http://localhost:5273";
    private Process _serverProcess;
    
    [SetUp]
    public async Task SetUp()
    {
        //_factory = new RealServerFactory<Program>();
        _serverProcess = await EndToEndUtil.StartServer();
    }
    
    [TearDown]
    public void TearDown()
    {
        _serverProcess.Kill();
        _serverProcess.Dispose();
    }
    
    
    
    [Test]
    public async Task MyTest()
    {
        await Page.GotoAsync(ServerUrl);
        await Expect(Page.Locator("body")).ToContainTextAsync("Public Timeline");
        await Expect(Page.Locator("span")).ToContainTextAsync("Register");
        await Expect(Page.Locator("span")).ToContainTextAsync("Login");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.Locator("#account")).ToContainTextAsync("Email");
        await Expect(Page.Locator("#account")).ToContainTextAsync("Password");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("Nikki@NotAMail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("test");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem)).ToContainTextAsync("Invalid login attempt.");
    }
}