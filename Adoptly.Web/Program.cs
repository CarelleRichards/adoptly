using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Azure;
using Adoptly.Web.Data;
using Adoptly.Web.Services;
using Adoptly.Web.Managers;
using Adoptly.Web.Filters;
using Adoptly.Web.BackgroundServices;
using Adoptly.Web.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add database.
// If you want to use the in-memory database,
// uncomment it and comment out the SQL server. 

builder.Services.AddDbContext<DataContext>(options =>
{
    // options.UseInMemoryDatabase(databaseName: "DataContext");
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext"));
    options.UseLazyLoadingProxies();
});

// Store session and make cookie essential.

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true;
});

// Add the HttpContextAccessor.

builder.Services.AddHttpContextAccessor();

// Add blob storage.

builder.Services.AddSingleton(x => new BlobServiceClient(builder.Configuration.GetConnectionString("BlobStorage")));
builder.Services.AddSingleton<BlobManager>();

// Add SMTP configuration.

builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("SmtpConfig"));

// Configure the default client.

builder.Services.AddHttpClient(Options.DefaultName, client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetSection("BlobConfig").Get<BlobConfig>().Base);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration.GetSection("BlobConfig").Get<BlobConfig>().Key);
});

// Add Azure Cognitive Search client.

builder.Services.AddAzureClients(client =>
{
    client.AddSearchClient(builder.Configuration.GetSection("SearchClient"));
});

// Add services.

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<SmtpService>();
builder.Services.AddScoped<MatchmakerService>();
builder.Services.AddScoped<FileService>();

// Add managers.

builder.Services.AddScoped<LoginManager>();
builder.Services.AddScoped<AdopterManager>();
builder.Services.AddScoped<AdminManager>();
builder.Services.AddScoped<ShelterManager>();
builder.Services.AddScoped<PetManager>();
builder.Services.AddScoped<QuizManager>();
builder.Services.AddScoped<MatchManager>();
builder.Services.AddScoped<ApplicationManager>();
builder.Services.AddScoped<AddressManager>();

builder.Services.AddControllersWithViews();

builder.Services.AddHostedService<AccountManagementBackgroundService>();

// Ignore JSON reference cycles during serialisation.

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Disable client-side validation.

builder.Services.AddMvc().AddViewOptions(options => options.HtmlHelperOptions.ClientValidationEnabled = false);

builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

var app = builder.Build();

// Seed data.

// To seed test data to the database, uncomment the code below.
// Within this code, comment out Initialise() or AddAdminOnly(), depending on your needs.

// using (var scope = app.Services.CreateScope())
// {
//    var services = scope.ServiceProvider;
//    try
//    {
//        // This function will populate the database with many users and pets.

//        SeedData.Initialise(services);

//        // This function will populate the database with an admin user.

//        SeedData.AddAdminOnly(services);
//    }
//    catch (Exception e)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(e, "An error occurred seeding the database.");
//    }
// }

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configure 404 page.

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = "/Error/PageNotFound";
        await next();
    }
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseResponseCompression();
app.UseSession();
app.MapDefaultControllerRoute();
app.Run();