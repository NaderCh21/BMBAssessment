using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Required for Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpClient for Gateway
builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri("https://localhost:7259");
});

// Cookie Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CookieAuth";
    options.DefaultChallengeScheme = "CookieAuth";
    options.DefaultSignInScheme = "CookieAuth";
})
.AddCookie("CookieAuth", options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/Login";

    // DEVELOPMENT VALUES
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;

    options.Cookie.HttpOnly = true;
    options.ReturnUrlParameter = "ReturnUrl";

    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Auth/Login?ReturnUrl={returnUrl}");
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization();

// ? MUST COME AFTER Authentication + Authorization
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Auth/Login");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
