using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Xunit;

namespace PlaywrightTests;
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlayWrightTests : PageTest, IClassFixture<EndToEndWebApplicationFactory>
{
    
    private string _serverUrl;
    
    public PlayWrightTests()
    {
        var server = new EndToEndWebApplicationFactory();
        _serverUrl = server.ServerAddress;  
    }

    #region LoginTests
    
     /**
     * Tests that its not posible to log in with a user that does not exist
     */
    [Test]
    public async Task LoginToAUserThatDoesNotExist()
    {
        await Page.GotoAsync(_serverUrl);
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
    
    /**
     * Tests that the navigation bar changes when logged in
     */
    [Test]
    public async Task NavigationBarChangesWhenLogedIn()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.Locator("body")).ToContainTextAsync("Public Timeline");
        await Expect(Page.Locator("span")).ToContainTextAsync("Register");
        await Expect(Page.Locator("span")).ToContainTextAsync("Login");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.Locator("body").ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.Locator("body").ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.Locator("body")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2")).ToContainTextAsync("Public Timeline");
        await Expect(Page.Locator("body")).ToContainTextAsync("My Timeline | Public Timeline | Helge | Logout");
        await Expect(Page.Locator("body")).ToContainTextAsync("What's on your mind Helge? Share");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Chirp!");
    }
    

    #endregion

    #region NavigationTests
    /**
     * Test that my Page shows my display name
     */
    [Test]
    public async Task HelgeDisplayName()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }).ScrollIntoViewIfNeededAsync();
        
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }).ClickAsync();
        await Expect(Page.Locator("h2")).ToContainTextAsync("Helge's Timeline");
    }
    
    [Test]
    public async Task PostAreShown()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine");
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine Starbuck now is what we hear the worst. — 2023-08-01 13:17:39");
    }

    #endregion

    #region RegistrationTests
    /**
     * Tests that a profile cannot have a blank email
     */
    [Test]
    public async Task BlankEmailRegistrationTest()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#registerForm")).ToContainTextAsync("The Email field is required.");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.Locator("#account")).ToContainTextAsync("The Email field is required.");
        await Expect(Page.Locator("body")).ToContainTextAsync("Login");
    }
    
    /**
     * Tests that making passwords are case-sensitive
     */
    [Test]
    public async Task CaseSensitivePasswordRegistrationTest()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("TestEmail@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("LilleK4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" })).ToBeVisibleAsync();
    }

    /**
     * Test to ensure an email without a @ is not valid
     */
    [Test]
    public async Task EmailFormatTest()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("TestEmail");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })).ToBeVisibleAsync();
    }

    /**
     * Tests that a blank password is invalid
     */
    [Test]
    public async Task BlankPasswordRegistrationTest()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("Email@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" })).ToBeVisibleAsync();
    }

    /**
     * Test that if you make an identical copy of a user,
     * the text "Username '<name>' is already taken." is shown.
     */
    [Test]
    public async Task RegisteringWithSameUsernameTest()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("Test@TestEmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#confirm-link")).ToContainTextAsync("Click here to confirm your account");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Thank you for confirming your email.");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("Test@TestEmail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem)).ToContainTextAsync("Username 'TestName' is already taken.");
    }

    /**
     * Checks if it is possible to perform a xss attack when sending cheeps.
     * xss attempt is expetected to simply write a new cheep and not a popup
     */
    [Test]
    public async Task tryXSSAttackOnSendCheep()
    {
        await Page.GotoAsync(_serverUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("TestEmail@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("TestEmail@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Hello, I am feeling good!<script>alert('If you see this in a popup, you are in trouble!');</script>");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("TestDisplayName Hello, I am feeling good!<script>alert('If you see this in a popup, you are in trouble!');</script>");
    }
    
    /**
     * It should not be possible to create two otherwise identical users with different usernames.
     * This test is a regression test to make sure a specific bug encountered is fixed.
     */
    /*
    [Test]
    public async Task SameEmailDifferentUserNameNotPossible()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("ControlOrMeta+c");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("Test@TestEmailTwo.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestNameTwo");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayNameTwo");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!Two");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!Two");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#confirm-link")).ToContainTextAsync("Click here to confirm your account");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("Test@TestEmailTwo.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestDisplayNameTwoDifferent");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("TestDisplayNameTwo");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!Two");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!Two");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("h2")).Not.ToContainTextAsync("Error's Timeline");
    }*/

    #endregion

    #region Sending Cheeps

    /**
     * Tests that when sending a cheep, the user will still be able to se the timeline whey were on
     * So if they post while on the private timeline, then they will se the private one
     * If the post while on the public timeline, they will se the public one
     */
    [Test]
    public async Task SnedingCheepOnPublicVsPrivate()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.Locator("h3")).ToContainTextAsync("What's on your mind Helge?");
        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Cheep from public timeline");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Helge Cheep from public" })).ToBeVisibleAsync();
        
        // is on the right page
        await Expect(Page).ToHaveURLAsync(_serverUrl);
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Cheep from private Timeline");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Helge Cheep from private" })).ToBeVisibleAsync();
        
        // is on the right page
        await Expect(Page).ToHaveURLAsync(_serverUrl+"Helge");
    }

    
    /**
     * Sending a cheep with 0 carecters is not posible
     */
    [Test]
    public async Task emptyCheep()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine");
    }
    
    /**
     * Snding a message that is too long is not posible
     */
    [Test]
    public async Task TooLongMessage()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("This Messages Last Carekter should be a A. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Helge This Messages Last Carekter should be a A. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
    /**
     * Sending a cheep with a emoji is posible
     */
    [Test]
    public async Task EmojiTest()
    {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Look at this emoji 😁. It's cool");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Helge Look at this emoji 😁. It's cool");
    }
    #endregion
}