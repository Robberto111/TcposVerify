using TcposVerify.Data;
using TcposVerify.Services;

var builder = WebApplication.CreateBuilder(args);


// ── MVC con Views ────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Repository: in-memory in Development, SQL Server in Production ──────────
//if (builder.Environment.IsDevelopment())
	{
		// Nessun DB necessario — tutto in RAM, si azzera al riavvio
//		builder.Services.AddSingleton<IVerifyRepository, InMemoryVerifyRepository>();
//	}
//	else
//	{
		string connStr = builder.Configuration.GetConnectionString("DefaultConnection")
			?? throw new InvalidOperationException(
				   "Connection string 'DefaultConnection' non trovata in appsettings.json.");

		builder.Services.AddSingleton<IVerifyRepository>(new VerifyRepository(connStr));
	}

// ── Servizi di business ──────────────────────────────────────────────────────
builder.Services.AddSingleton<ChecksumService>();
builder.Services.AddSingleton<MondrianService>();
builder.Services.AddScoped<IVerifyService, VerifyService>();

// ── Sicurezza HTTP ───────────────────────────────────────────────────────────
builder.Services.AddHsts(o =>
{
	o.MaxAge            = TimeSpan.FromDays(365);
	o.IncludeSubDomains = true;
});

// ────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (ctx, next) =>
{
	ctx.Response.Headers.XContentTypeOptions = "nosniff";
	ctx.Response.Headers.XFrameOptions = "DENY";
	ctx.Response.Headers.XXSSProtection = "1; mode=block";
	ctx.Response.Headers["Referrer-Policy"]          = "no-referrer";
	ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
	await next();
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Route principale: /Verify?neg=...
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Verify}/{action=Index}/{id?}");

app.Run();
