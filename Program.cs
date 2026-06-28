using Legaly.Web.Data;
using Legaly.Web.Models.Entities;
using Legaly.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

// Set QuestPDF community license
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/auth/login";
        options.LogoutPath       = "/auth/logout";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Application Services
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Admins.Any())
    {
        db.Admins.Add(new Admin
        {
            Email        = "admin@legaly.id",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 12),
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        });
    }

    if (!db.Templates.Any())
    {
        db.Templates.Add(new Template
        {
            Name    = "Surat Perjanjian Jual Beli Kendaraan Bermotor",
            Content = """
                SURAT PERJANJIAN JUAL BELI KENDARAAN BERMOTOR
                Nomor: {{NomorSurat}}

                Pada hari ini, tanggal {{TanggalPerjanjian}}, yang bertanda tangan di bawah ini:

                PIHAK PERTAMA (PENJUAL)
                Nama          : {{NamaPenjual}}
                NIK           : {{NikPenjual}}
                Alamat        : {{AlamatPenjual}}

                PIHAK KEDUA (PEMBELI)
                Nama          : {{NamaPembeli}}
                NIK           : {{NikPembeli}}
                Alamat        : {{AlamatPembeli}}

                Selanjutnya disebut sebagai PARA PIHAK, dengan ini menyatakan telah sepakat untuk mengadakan perjanjian jual beli kendaraan bermotor dengan ketentuan-ketentuan sebagai berikut:

                Pasal 1 - DATA KENDARAAN
                Merk/Tipe     : {{MerkMotor}} {{TypeMotor}}
                Tahun         : {{TahunMotor}}
                Nomor Polisi  : {{NomorPolisi}}
                Nomor Rangka  : {{NomorRangka}}
                Nomor Mesin   : {{NomorMesin}}
                Warna         : {{WarnaMOtor}}

                Pasal 2 - HARGA JUAL BELI
                Harga jual beli kendaraan bermotor tersebut di atas disepakati sebesar {{Harga}} (terbilang sesuai kesepakatan para pihak).

                Pasal 3 - PENYERAHAN
                Pihak Pertama menyerahkan kendaraan beserta dokumen-dokumen kepemilikan kepada Pihak Kedua pada saat penandatanganan surat perjanjian ini.

                Pasal 4 - PERNYATAAN PENJUAL
                Pihak Pertama menyatakan bahwa kendaraan yang dijual adalah benar miliknya, bebas dari sengketa, tidak sedang dijaminkan, dan tidak sedang dalam proses hukum apapun.

                Pasal 5 - KETENTUAN LAIN
                Hal-hal yang belum diatur dalam perjanjian ini akan diselesaikan secara musyawarah dan mufakat oleh PARA PIHAK.

                Demikian surat perjanjian ini dibuat dan ditandatangani oleh PARA PIHAK dalam keadaan sadar dan tanpa paksaan dari pihak manapun.

                Pihak Pertama (Penjual)                    Pihak Kedua (Pembeli)


                ({{NamaPenjual}})                          ({{NamaPembeli}})
                """,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    db.SaveChanges();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
