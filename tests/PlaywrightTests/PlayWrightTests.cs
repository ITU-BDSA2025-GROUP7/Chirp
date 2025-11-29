using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Xunit;

namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlayWrightTests : PageTest, IClassFixture<EndToEndWebApplicationFactory> {
    private string _serverUrl;

    public PlayWrightTests() {
        var server = new EndToEndWebApplicationFactory();
        _serverUrl = server.ServerAddress;
    }

    #region LoginTests

    /**
    * Tests that it's not possible to log in with a user that does not exist
    */
    [Test]
    public async Task LoginToAUserThatDoesNotExist() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.Locator("body")).ToContainTextAsync("Public Timeline");
        await Expect(Page.Locator("span")).ToContainTextAsync("Register");
        await Expect(Page.Locator("span")).ToContainTextAsync("Login");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.Locator("#account")).ToContainTextAsync("Email");
        await Expect(Page.Locator("#account")).ToContainTextAsync("Password");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" })
                  .FillAsync("Nikki@NotAMail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("test");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem))
           .ToContainTextAsync("Invalid login attempt.");
    }


    /**
     * Tests that it's not possible to login using username instead of email
     */
    [Test]
    public async Task LoginNotPossibleWithUsername() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("Helge");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        // is on the right page
        await Expect(Page).ToHaveURLAsync(_serverUrl + "Identity/Account/Login");
    }

    /**
     * Tests that the navigation bar changes when logged in
     */
    [Test]
    public async Task NavigationBarChangesWhenLoggedIn() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.Locator("body")).ToContainTextAsync("Public Timeline");
        await Expect(Page.Locator("span")).ToContainTextAsync("Register");
        await Expect(Page.Locator("span")).ToContainTextAsync("Login");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.Locator("body").ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.Locator("body").ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.Locator("body")).ToBeVisibleAsync();
        await Expect(Page.Locator("h2")).ToContainTextAsync("Public Timeline");
        await Expect(Page.Locator("body"))
           .ToContainTextAsync("My Timeline | Public Timeline | About me | Logout");
        await Expect(Page.Locator("body")).ToContainTextAsync("What's on your mind, Helge? Share");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Chirp!");
    }

    /**
     * The user should be able to log out
     */
    [Test]
    public async Task LogOut() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.Locator("h3")).ToContainTextAsync("What's on your mind, Helge?");
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Paragraph))
           .ToContainTextAsync("You have successfully logged out of the application.");
        await Expect(Page.Locator("span")).ToContainTextAsync("Login");
    }

    #endregion

    #region NavigationTests

    /**
     * users cant follow or unfollow when not logged in
     */
    [Test]
    public async Task FollowAndUnfollowNotVisableWhenNotLoggedIn() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Listitem)
                         .Filter(new() {
                             HasText = "— 2023-08-01 13:17:39 Starbuck now is what we hear"
                         })
                         .GetByRole(AriaRole.Button))
             .Not.ToBeVisibleAsync();
    }

    /**
     * Users can follow someone and unfollow when logged in
     */
    [Test]
    public async Task FollowAndUnfollow() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.Locator("#account div").Nth(1).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Listitem)
                  .Filter(new() { HasText = "Mellie Yost Follow — 2023-08-" })
                  .GetByRole(AriaRole.Button)
                  .ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Listitem)
                  .Filter(new() { HasText = "Mellie Yost Unfollow — 2023-" })
                  .GetByRole(AriaRole.Button)
                  .ClickAsync();
    }

    /**
     * There is no button to follow/unfollow yourself
     */
    [Test]
    public async Task NoFollowButtonForOwnCheep() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }).ClickAsync();
        await Expect(Page.GetByText("Jacqualine Gilcoine Unfollow — 2023-08-01 13:17:39"))
           .ToBeVisibleAsync();
        await Expect(Page.GetByText("Helge — 2023-08-01 13:17:37")).ToBeVisibleAsync();
    }


    /**
     * Test that my Page shows my display name
     */
    [Test]
    public async Task MyPageShowsDisplayName() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }))
           .ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" })
                  .ScrollIntoViewIfNeededAsync();


        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }).ClickAsync();
        await Expect(Page.Locator("h2")).ToContainTextAsync("Helge's Timeline");
    }

    [Test]
    public async Task PostAreShown() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine");
        await Expect(Page.Locator("#messagelist"))
           .ToContainTextAsync("Starbuck now is what we hear the worst.");
    }

    [Test]
    public async Task AuthorTimelineNotLoggedIn() {
        await Page.GotoAsync(_serverUrl + "Helge");
        await Expect(Page.Locator("#messagelist")).Not.ToContainTextAsync("Jacqualine Gilcoine");
        await Expect(Page.Locator("#messagelist"))
           .ToContainTextAsync("Helge — 2023-08-01 13:17:37");
    }

    #endregion

    #region RegistrationTests

    /**
     * Delete registered user
     */
    [Test]
    public async Task DeleteARegisteredUser() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Test@emailsyayaya.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("Tester");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" })
                  .ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" })
                  .FillAsync("Test@EmailsYayaya.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Personal data" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Delete data and close my" })
                  .ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" })
                  .FillAsync("Test@EmailsYayaya.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem))
           .ToContainTextAsync("Invalid login attempt.");
    }

    /**
     * Tests that a profile cannot have a blank email
     */
    [Test]
    public async Task BlankEmailRegistrationTest() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#registerForm"))
           .ToContainTextAsync("The Email field is required.");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

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
    public async Task CaseSensitivePasswordRegistrationTest() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("TestEmail@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("LilleK4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }))
           .ToBeVisibleAsync();
    }

    /**
     * Test to ensure an email without a @ is not valid
     */
    [Test]
    public async Task EmailFormatTest() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("TestEmail");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }))
           .ToBeVisibleAsync();
    }

    /**
     * Tests that a blank password is invalid
     */
    [Test]
    public async Task BlankPasswordRegistrationTest() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Email@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).PressAsync("Tab");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }))
           .ToBeVisibleAsync();
    }

    /**
     * Confrmation email neades to be exact match
     */
    [Test]
    public async Task ConfirmationPasswordNeedsToMatch() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("tester@Testmail.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("IAmAUser");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("DisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("lillekat!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#registerForm"))
           .ToContainTextAsync("The password and confirmation password do not match.");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#registerForm"))
           .ToContainTextAsync("The Confirm password field is required.");
    }

    /**
     * Test that if you make an identical copy of a user,
     * the text "Username '<name>' is already taken." is shown.
     */
    [Test]
    public async Task RegisteringWithSameUsernameTest() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Test@testemail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#confirm-link"))
           .ToContainTextAsync("Click here to confirm your account");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" })
                  .ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Alert))
           .ToContainTextAsync("Thank you for confirming your email.");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Test@testemail.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("TestName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TestDisplayName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem))
           .ToContainTextAsync("Username 'TestName' is already taken.");
    }

    /**
     * Username cannot be blank
     */
    [Test]
    public async Task BlankUsernameNotAllowed() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Tester@SuperTester.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("TesterDisplayname");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.Locator("#registerForm"))
           .ToContainTextAsync("The Username field is required.");
    }

    /**
     * Username must be unique
     */
    [Test]
    public async Task UsernameMustBeUnique() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Tester@email1.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("Username1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("displayname");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" })
                  .ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Tester@email2.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("Username1");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("displayname2");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("displayname");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem))
           .ToContainTextAsync("Username 'Username1' is already taken.");
    }

    /**
     * Tests that if you register using an invalid email, the waring "Email is invalid" is displayed
     */
    [Test]
    public async Task InvalidEmail() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" })).ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).FillAsync("Invalid@Emails");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("Nikki");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).FillAsync("NikkiTester");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByText("is invalid")).ToBeVisibleAsync();
        await Page.GetByText("*Email").ClickAsync();
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
     * Checks if it is possible to perform a xss attack when sending cheeps.
     * xss attempt is expetected to simply write a new cheep and not a popup
     */
    [Test]
    public async Task tryXSSAttackOnSendCheep() {
        await Page.GotoAsync(_serverUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("TestEmail@testemails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" })
                  .FillAsync("TesterName");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Display Name" })
                  .FillAsync("NameCool");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" })
                  .ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" })
                  .FillAsync("TestEmail@TestEmails.com");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text")
                  .FillAsync(
                       "Hello, I am feeling good!<script>alert('If you see this in a popup, you are in trouble!');</script>");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist"))
           .ToContainTextAsync(
                "<script>alert('If you see this in a popup, you are in trouble!');</script>");
    }

    /**
     * Tests that when sending a cheep, the user will still be able to se the timeline whey were on
     * So if they post while on the private timeline, then they will se the private one
     * If the post while on the public timeline, they will se the public one
     */
    [Test]
    public async Task SendingCheepOnPublicVsPrivate() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.Locator("h3")).ToContainTextAsync("What's on your mind, Helge?");
        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Cheep from public timeline");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem)
                         .Filter(new() { HasText = "Cheep from public" }))
           .ToBeVisibleAsync();

        // is on the right page
        await Expect(Page).ToHaveURLAsync(_serverUrl);

        await Page.GetByRole(AriaRole.Link, new() { Name = "My Timeline" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Cheep from private Timeline");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem)
                         .Filter(new() { HasText = "Cheep from private" }))
           .ToBeVisibleAsync();

        // is on the right page
        await Expect(Page).ToHaveURLAsync(_serverUrl + "Helge");

        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine ");
    }

    /**
     * Sending a cheep with 0 carecters is not posible
     */
    [Test]
    public async Task EmptyCheep() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine");
    }

    /**
     * Snding a message that is too long is not posible
     */
    [Test]
    public async Task TooLongMessage() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text")
                  .FillAsync(
                       "This Messages Last Carekter should be a A. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist"))
           .ToContainTextAsync(
                "This Messages Last Carekter should be a A. AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }

    /**
     * Sending a cheep with a emoji is posible
     */
    [Test]
    public async Task EmojiTest() {
        await Page.GotoAsync(_serverUrl);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();

        await Page.Locator("#Text").ClickAsync();
        await Page.Locator("#Text").FillAsync("Look at this emoji 😁. It's cool");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.Locator("#messagelist"))
           .ToContainTextAsync("Look at this emoji 😁. It's cool");
    }

    #endregion

    #region AccountManagementPage

    /** Tests that the user's cheeps are displayed when have cheeps. */
    [Test]
    public async Task ListOfOwnCheeps() {
        await Page.GotoAsync(_serverUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("ropf@itu.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "My cheeps" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();
        await Expect(Page.Locator("body")).ToBeVisibleAsync();
        await Expect(Page.Locator("h3")).ToContainTextAsync("My cheeps");

        // Ensure that own cheeps are shown
        await Expect(Page.GetByText("Helge — 2023-08-01 13:17:37")).ToBeVisibleAsync();

        // Ensure that cheeps from followed authors are not shown
        await Expect(Page.GetByText("Jacqualine Gilcoine")).Not.ToBeVisibleAsync();
    }

    /** Tests that the no cheeps are displayed on the list if the user has no cheeps. */
    [Test]
    public async Task ListOfOwnCheepsNoCheeps() {
        await Page.GotoAsync(_serverUrl);

        // Register a new user
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Email" })
                  .FillAsync("Test@emailsyayaya.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Username" }).FillAsync("Tester");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "*Confirm Password" })
                  .FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" })
                  .ClickAsync();

        // Login
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" })
                  .FillAsync("Test@EmailsYayaya.dk");
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync("Lillek4t!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        // Go to account page
        await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();

        // Go to list of personal cheeps
        await Page.GetByRole(AriaRole.Link, new() { Name = "My cheeps" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Icon1Chirp!" }))
           .ToBeVisibleAsync();
        await Expect(Page.Locator("body")).ToBeVisibleAsync();
        await Expect(Page.Locator("h3")).ToContainTextAsync("My cheeps");

        // Ensure that no cheeps from self are shown (since this user has no cheeps)
        await Expect(Page.GetByText("Tester")).Not.ToBeVisibleAsync();

        // Ensure that cheeps from followed authors are not shown
        await Expect(Page.GetByText("Jacqualine Gilcoine")).Not.ToBeVisibleAsync();
    }

    #endregion
}