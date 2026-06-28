# Design Document: Legaly Web App

## Overview

Legaly adalah aplikasi web ASP.NET Core MVC (net10.0) untuk menghasilkan dokumen Surat Perjanjian Jual Beli Kendaraan Bermotor. Sistem ini memungkinkan admin mengelola template surat, dan visitor (pengguna umum) mengisi form data transaksi untuk menghasilkan, mempratinjau, dan mengunduh surat perjanjian dalam format PDF.

Arsitektur yang dipilih adalah MVC berlapis standar dengan pemisahan antara Controllers, Services (business logic), Models (Entities + ViewModels), Views (Razor), dan Data (EF Core DbContext). Pilihan ini sesuai dengan kompleksitas aplikasi yang sederhana-sedang dan memudahkan pengujian tiap lapisan secara terpisah.

## Architecture

### Layered MVC Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                       HTTP Request/Response                   │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│                      Controllers Layer                        │
│   AuthController │ TemplateController │ DocumentController   │
└──────────────────────────┬───────────────────────────────────┘
                           │ calls
┌──────────────────────────▼───────────────────────────────────┐
│                       Services Layer                          │
│    ITemplateService │ IDocumentService │ IPdfService         │
└──────────────────────────┬───────────────────────────────────┘
                           │ queries/commands
┌──────────────────────────▼───────────────────────────────────┐
│                        Data Layer                             │
│              ApplicationDbContext (EF Core)                  │
└──────────────────────────┬───────────────────────────────────┘
                           │
┌──────────────────────────▼───────────────────────────────────┐
│                     SQL Server Database                       │
│            Admins │ Templates │ Documents                    │
└──────────────────────────────────────────────────────────────┘
```

### Authentication Flow

Cookie-based authentication tanpa ASP.NET Identity. Admin diverifikasi manual dengan BCrypt. Cookie di-set dengan `HttpOnly = true`, `Secure = true`, dan `ExpireTimeSpan = 8 jam`.

```
POST /auth/login
  │
  ├─ Cari Admin berdasarkan Email di tabel Admins
  ├─ BCrypt.Verify(inputPassword, admin.PasswordHash)
  ├─ [valid]   → SignInAsync → set cookie → redirect /template
  └─ [invalid] → ViewData["Error"] = "Email atau password salah."
```

### Request Pipeline (Program.cs)

```
UseHttpsRedirection
UseStaticFiles
UseRouting
UseAuthentication   ← harus sebelum UseAuthorization
UseAuthorization
MapControllerRoute
```

## Components and Interfaces

### Controllers

#### `AuthController`
- `GET  /auth/login`  → tampilkan form login (redirect ke /template jika sudah auth)
- `POST /auth/login`  → proses login, set cookie, redirect ke /template atau kembali dengan error
- `POST /auth/logout` → sign out, redirect ke /auth/login

#### `TemplateController` (`[Authorize]`)
- `GET  /template`             → daftar semua template
- `GET  /template/create`      → form buat template baru
- `POST /template/create`      → simpan template baru
- `GET  /template/edit/{id}`   → form edit template
- `POST /template/edit/{id}`   → update template
- `POST /template/delete/{id}` → hapus template

#### `DocumentController`
- `GET  /document/create`        → form input data perjanjian
- `POST /document/create`        → validasi, generate, simpan, redirect ke preview
- `GET  /document/preview/{id}`  → halaman pratinjau dokumen
- `GET  /document/download/{id}` → generate dan download PDF

### Service Interfaces

```csharp
public interface ITemplateService
{
    Task<List<Template>> GetAllAsync();
    Task<Template?> GetByIdAsync(int id);
    Task<Template> CreateAsync(TemplateFormViewModel vm);
    Task<Template> UpdateAsync(int id, TemplateFormViewModel vm);
    Task<bool> DeleteAsync(int id); // false if referenced by documents
}

public interface IDocumentService
{
    Task<Document> CreateAsync(DocumentFormViewModel vm);
    Task<Document?> GetByIdAsync(int id);
    string AutoFill(string templateContent, DocumentFormViewModel form, string nomorSurat);
    Task<string> GenerateNomorSuratAsync(DateOnly tanggal);
}

public interface IPdfService
{
    byte[] GeneratePdf(Document document);
    string BuildFileName(string nomorPolisi, int id);
}
```

### Views Structure

```
Views/
├── Shared/
│   ├── _Layout.cshtml          ← layout publik (visitor)
│   ├── _AdminLayout.cshtml     ← layout admin (navbar, sidebar)
│   └── Error.cshtml
├── Auth/
│   └── Login.cshtml
├── Template/
│   ├── Index.cshtml            ← daftar template + alert TempData
│   ├── Create.cshtml
│   └── Edit.cshtml
├── Document/
│   ├── Create.cshtml           ← form 3-section (Penjual, Pembeli, Kendaraan)
│   └── Preview.cshtml          ← NomorSurat + GeneratedContent + tombol unduh PDF
└── Home/
    └── Index.cshtml            ← landing page publik
```

## Data Models

### Entity Models

#### `Admin.cs` (`Models/Entities/`)
```csharp
public class Admin
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;        // NVARCHAR(255) UNIQUE
    public string PasswordHash { get; set; } = string.Empty; // BCrypt hash, cost 12
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### `Template.cs` (`Models/Entities/`)
```csharp
public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;    // NVARCHAR(255)
    public string Content { get; set; } = string.Empty; // NVARCHAR(MAX), berisi {{placeholder}}
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### `Document.cs` (`Models/Entities/`)
```csharp
public class Document
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public Template Template { get; set; } = null!;
    public string NomorSurat { get; set; } = string.Empty;       // UNIQUE
    public string NamaPenjual { get; set; } = string.Empty;
    public string NikPenjual { get; set; } = string.Empty;
    public string AlamatPenjual { get; set; } = string.Empty;
    public string NamaPembeli { get; set; } = string.Empty;
    public string NikPembeli { get; set; } = string.Empty;
    public string AlamatPembeli { get; set; } = string.Empty;
    public string MerkMotor { get; set; } = string.Empty;
    public string TipeMotor { get; set; } = string.Empty;
    public int TahunMotor { get; set; }
    public string WarnaMotor { get; set; } = string.Empty;
    public string NomorPolisi { get; set; } = string.Empty;
    public string NomorBPKB { get; set; } = string.Empty;
    public string NomorSTNK { get; set; } = string.Empty;
    public decimal Harga { get; set; }                           // DECIMAL(18,2)
    public DateOnly TanggalPerjanjian { get; set; }
    public string GeneratedContent { get; set; } = string.Empty; // NVARCHAR(MAX)
    public DateTime CreatedAt { get; set; }
}
```

### ViewModels

#### `LoginViewModel.cs`
```csharp
public class LoginViewModel
{
    [Required] [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
```

#### `TemplateFormViewModel.cs`
```csharp
public class TemplateFormViewModel
{
    [Required] [StringLength(255)]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Content { get; set; } = string.Empty;
}
```

#### `DocumentFormViewModel.cs`
```csharp
public class DocumentFormViewModel
{
    [Required][StringLength(255)]   public string NamaPenjual { get; set; } = string.Empty;
    [Required][StringLength(20)][RegularExpression(@"^\d{16}$", ErrorMessage="NIK harus 16 digit angka")]
                                    public string NikPenjual { get; set; } = string.Empty;
    [Required][StringLength(500)]   public string AlamatPenjual { get; set; } = string.Empty;
    [Required][StringLength(255)]   public string NamaPembeli { get; set; } = string.Empty;
    [Required][StringLength(20)][RegularExpression(@"^\d{16}$", ErrorMessage="NIK harus 16 digit angka")]
                                    public string NikPembeli { get; set; } = string.Empty;
    [Required][StringLength(500)]   public string AlamatPembeli { get; set; } = string.Empty;
    [Required][StringLength(100)]   public string MerkMotor { get; set; } = string.Empty;
    [Required][StringLength(100)]   public string TipeMotor { get; set; } = string.Empty;
    [Required][Range(1900, 9999)]   public int TahunMotor { get; set; }
    [Required][StringLength(50)]    public string WarnaMotor { get; set; } = string.Empty;
    [Required][StringLength(20)]    public string NomorPolisi { get; set; } = string.Empty;
    [Required][StringLength(50)]    public string NomorBPKB { get; set; } = string.Empty;
    [Required][StringLength(50)]    public string NomorSTNK { get; set; } = string.Empty;
    [Required][Range(1, 999999999999.99)] public decimal Harga { get; set; }
    [Required]                      public DateOnly TanggalPerjanjian { get; set; }
}
```

#### `PreviewViewModel.cs`
```csharp
public class PreviewViewModel
{
    public int Id { get; set; }
    public string NomorSurat { get; set; } = string.Empty;
    public string GeneratedContent { get; set; } = string.Empty;
    public string NomorPolisi { get; set; } = string.Empty;
}
```

### Database Schema

```sql
CREATE TABLE Admins (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    Email        NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt    DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Templates (
    Id        INT PRIMARY KEY IDENTITY(1,1),
    Name      NVARCHAR(255) NOT NULL,
    Content   NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Documents (
    Id                INT PRIMARY KEY IDENTITY(1,1),
    TemplateId        INT NOT NULL REFERENCES Templates(Id),
    NomorSurat        NVARCHAR(100) NOT NULL UNIQUE,
    NamaPenjual       NVARCHAR(255) NOT NULL,
    NikPenjual        NVARCHAR(20) NOT NULL,
    AlamatPenjual     NVARCHAR(500) NOT NULL,
    NamaPembeli       NVARCHAR(255) NOT NULL,
    NikPembeli        NVARCHAR(20) NOT NULL,
    AlamatPembeli     NVARCHAR(500) NOT NULL,
    MerkMotor         NVARCHAR(100) NOT NULL,
    TipeMotor         NVARCHAR(100) NOT NULL,
    TahunMotor        INT NOT NULL,
    WarnaMotor        NVARCHAR(50) NOT NULL,
    NomorPolisi       NVARCHAR(20) NOT NULL,
    NomorBPKB         NVARCHAR(50) NOT NULL,
    NomorSTNK         NVARCHAR(50) NOT NULL,
    Harga             DECIMAL(18,2) NOT NULL,
    TanggalPerjanjian DATE NOT NULL,
    GeneratedContent  NVARCHAR(MAX) NOT NULL,
    CreatedAt         DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

### `ApplicationDbContext` (Updated)

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Document> Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>()
            .HasIndex(a => a.Email).IsUnique();

        modelBuilder.Entity<Document>()
            .HasIndex(d => d.NomorSurat).IsUnique();

        modelBuilder.Entity<Document>()
            .Property(d => d.Harga)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Document>()
            .HasOne(d => d.Template)
            .WithMany()
            .HasForeignKey(d => d.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

## Key Algorithms

### Auto-Fill Algorithm

Auto-Fill adalah fungsi murni (`pure function`) yang menerima konten template (string dengan placeholder `{{NamaField}}`) dan `DocumentFormViewModel`, lalu mengembalikan string baru di mana setiap placeholder diganti dengan nilai aktual yang sudah diformat.

```csharp
// DocumentService.cs
public string AutoFill(string templateContent, DocumentFormViewModel form, string nomorSurat)
{
    var culture = new CultureInfo("id-ID");
    return templateContent
        .Replace("{{NamaPenjual}}",      form.NamaPenjual)
        .Replace("{{NikPenjual}}",       form.NikPenjual)
        .Replace("{{AlamatPenjual}}",    form.AlamatPenjual)
        .Replace("{{NamaPembeli}}",      form.NamaPembeli)
        .Replace("{{NikPembeli}}",       form.NikPembeli)
        .Replace("{{AlamatPembeli}}",    form.AlamatPembeli)
        .Replace("{{MerkMotor}}",        form.MerkMotor)
        .Replace("{{TipeMotor}}",        form.TipeMotor)
        .Replace("{{TahunMotor}}",       form.TahunMotor.ToString())
        .Replace("{{WarnaMotor}}",       form.WarnaMotor)
        .Replace("{{NomorPolisi}}",      form.NomorPolisi)
        .Replace("{{NomorBPKB}}",        form.NomorBPKB)
        .Replace("{{NomorSTNK}}",        form.NomorSTNK)
        .Replace("{{Harga}}",            form.Harga.ToString("C0", culture))
        .Replace("{{TanggalPerjanjian}}", form.TanggalPerjanjian.ToString("dd MMMM yyyy", culture))
        .Replace("{{NomorSurat}}",       nomorSurat);
}
```

Format nilai khusus:
- `Harga`: `C0` dengan locale `id-ID` → `Rp 15.000.000` (tanpa desimal, pemisah ribuan titik)
- `TanggalPerjanjian`: `dd MMMM yyyy` dengan locale `id-ID` → `01 Juli 2026`
- `NomorSurat`: digenerate sebelum Auto-Fill, disisipkan melalui parameter terpisah

### NomorSurat Generation Algorithm

NomorSurat adalah penomoran dokumen per-bulan dengan format `{NoUrut:D3}/LEGALY/JBM/{MM}/{YYYY}`.

```csharp
// DocumentService.cs
public async Task<string> GenerateNomorSuratAsync(DateOnly tanggal)
{
    // Hitung dokumen pada bulan dan tahun yang sama
    var count = await _context.Documents
        .Where(d => d.TanggalPerjanjian.Month == tanggal.Month
                 && d.TanggalPerjanjian.Year  == tanggal.Year)
        .CountAsync();

    return $"{(count + 1):D3}/LEGALY/JBM/{tanggal.Month:D2}/{tanggal.Year}";
}
```

Contoh output: `001/LEGALY/JBM/07/2026`, `012/LEGALY/JBM/12/2025`.

**Keunikan dalam kondisi bersamaan (concurrent):** Karena `NomorSurat` memiliki constraint `UNIQUE` di database, jika dua request bersamaan menghasilkan nomor yang sama, salah satunya akan menerima `DbUpdateException`. Controller harus menangkap exception ini dan melakukan retry sekali atau menampilkan halaman error yang informatif. Alternatif yang lebih kuat adalah menggunakan transaction dengan isolation level `Serializable` pada operasi count + insert.

### BCrypt Authentication Algorithm

```csharp
// Di AuthController — Login POST
var admin = await _context.Admins
    .FirstOrDefaultAsync(a => a.Email == vm.Email);

if (admin == null || !BCrypt.Net.BCrypt.Verify(vm.Password, admin.PasswordHash))
{
    ViewData["Error"] = "Email atau password salah.";
    return View(vm);
}

var claims = new List<Claim> { new(ClaimTypes.Name, admin.Email) };
var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
await HttpContext.SignInAsync(
    CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(identity));
return RedirectToAction("Index", "Template");
```

Seed password menggunakan: `BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12)`.

### PDF Generation Algorithm

QuestPDF digunakan untuk menghasilkan PDF dari `GeneratedContent` dokumen.

```csharp
// PdfService.cs
public byte[] GeneratePdf(Document document)
{
    return QuestPDF.Fluent.Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

            page.Content().Column(col =>
            {
                col.Item().Text(document.NomorSurat).Bold().AlignCenter();
                col.Item().PaddingTop(10)
                   .Text(document.GeneratedContent)
                   .PreserveNewlines();
            });
        });
    }).GenerateBytes();
}
```

### PDF Filename Generation Algorithm

```csharp
// PdfService.cs
public string BuildFileName(string nomorPolisi, int id)
{
    // Ganti spasi dengan tanda hubung, hapus karakter non-alfanumerik kecuali tanda hubung
    var sanitized = Regex.Replace(nomorPolisi.Replace(" ", "-"), @"[^a-zA-Z0-9\-]", "");
    return $"Surat-Perjanjian-Jual-Beli-{sanitized}-{id}.pdf";
}
```

### Seed Data Algorithm

```csharp
// Program.cs — setelah app.Build()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Admins.Any())
    {
        db.Admins.Add(new Admin
        {
            Email        = "admin@legaly.id",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow
        });
    }

    if (!db.Templates.Any())
    {
        db.Templates.Add(new Template
        {
            Name      = "Surat Perjanjian Jual Beli Kendaraan Bermotor",
            Content   = /* teks surat lengkap dengan placeholder standar */,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    db.SaveChanges();
}
```

Seed hanya dijalankan jika tabel kosong (idempotent). Jika tabel tidak kosong, seed dilewati.

## Service Interfaces and Implementations

### `TemplateService`

| Method | Behavior |
|--------|----------|
| `GetAllAsync()` | Query semua Template, urut by Id ascending |
| `GetByIdAsync(id)` | Return Template atau null |
| `CreateAsync(vm)` | Set CreatedAt = UpdatedAt = UtcNow, simpan ke DB |
| `UpdateAsync(id, vm)` | Update Name, Content, UpdatedAt = UtcNow |
| `DeleteAsync(id)` | Cek `Documents.Any(d => d.TemplateId == id)`. Jika ada, return false. Jika tidak, hapus dan return true. |

### `DocumentService`

| Method | Behavior |
|--------|----------|
| `CreateAsync(vm)` | Ambil template pertama (OrderBy Id), generate NomorSurat, jalankan AutoFill, simpan Document baru |
| `GetByIdAsync(id)` | Return Document dengan Include(d => d.Template) atau null |
| `AutoFill(content, form, nomorSurat)` | Pure function — replace semua placeholder dengan nilai terformat |
| `GenerateNomorSuratAsync(tanggal)` | Hitung count dokumen bulan/tahun sama, return formatted string |

### `PdfService`

| Method | Behavior |
|--------|----------|
| `GeneratePdf(document)` | Hasilkan byte[] PDF via QuestPDF dengan layout A4 |
| `BuildFileName(nomorPolisi, id)` | Sanitasi NomorPolisi, return nama file PDF |

### Program.cs — Service Registration (updated)

```csharp
// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath    = "/auth/login";
        options.LogoutPath   = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly     = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Application Services
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfService, PdfService>();

builder.Services.AddControllersWithViews();
```

## Routing Table

| Method | Route | Controller | Action | Auth Required |
|--------|-------|------------|--------|---------------|
| GET | `/` | Home | Index | No |
| GET | `/auth/login` | Auth | Login | No (redirect if authenticated) |
| POST | `/auth/login` | Auth | Login | No |
| POST | `/auth/logout` | Auth | Logout | No |
| GET | `/template` | Template | Index | Yes |
| GET | `/template/create` | Template | Create | Yes |
| POST | `/template/create` | Template | Create | Yes |
| GET | `/template/edit/{id}` | Template | Edit | Yes |
| POST | `/template/edit/{id}` | Template | Edit | Yes |
| POST | `/template/delete/{id}` | Template | Delete | Yes |
| GET | `/document/create` | Document | Create | No |
| POST | `/document/create` | Document | Create | No |
| GET | `/document/preview/{id}` | Document | Preview | No |
| GET | `/document/download/{id}` | Document | Download | No |

Route mapping di `Program.cs` menggunakan pattern atribut di controller:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

`TemplateController` menggunakan `[Authorize]` di class level. `AuthController` dan `DocumentController` tidak memerlukan atribut auth di class level.

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Auto-Fill mengganti semua placeholder yang dikenal

*For any* template content string yang mengandung subset dari placeholder standar (`{{NamaPenjual}}`, `{{NikPenjual}}`, dst.) dan *for any* valid `DocumentFormViewModel` dengan `NomorSurat` yang valid, hasil `AutoFill` tidak boleh mengandung token `{{...}}` yang tersisa.

**Validates: Requirements 7.2, 8.2**

### Property 2: Auto-Fill mempertahankan nilai field secara akurat

*For any* valid `DocumentFormViewModel`, hasil `AutoFill` harus mengandung nilai `Harga` yang diformat sebagai mata uang Indonesia tanpa desimal, nilai `TanggalPerjanjian` yang diformat sebagai `dd MMMM yyyy` dengan locale `id-ID`, dan nilai `NomorSurat` yang disisipkan dengan tepat di posisi placeholder `{{NomorSurat}}`.

**Validates: Requirements 7.2**

### Property 3: Format NomorSurat selalu konsisten

*For any* kombinasi (count, month, year) yang valid, `GenerateNomorSurat` harus menghasilkan string yang sesuai dengan format `{count+1:D3}/LEGALY/JBM/{MM:D2}/{YYYY}`, di mana `count+1` selalu 3 digit dengan leading zero dan month selalu 2 digit.

**Validates: Requirements 7.3**

### Property 4: Validasi NIK menolak semua string yang bukan tepat 16 digit

*For any* string yang tidak terdiri dari tepat 16 karakter digit (misalnya string dengan panjang ≠ 16, atau mengandung karakter non-digit), validasi `NikPenjual` dan `NikPembeli` harus gagal dengan pesan `"NIK harus 16 digit angka"` dan tidak ada dokumen yang disimpan ke database.

**Validates: Requirements 6.3, 6.6, 6.18**

### Property 5: Form dengan field invalid tidak menyimpan data

*For any* `DocumentFormViewModel` di mana setidaknya satu field tidak memenuhi aturan validasi (panjang berlebih, wajib kosong, rentang dilanggar), tidak ada baris `Document` baru yang dibuat di database, dan seluruh nilai yang telah dimasukkan sebelumnya dikembalikan ke form.

**Validates: Requirements 6.17**

### Property 6: Nama file PDF mengandung NomorPolisi yang telah disanitasi

*For any* string `NomorPolisi` dan document `id`, `BuildFileName` harus menghasilkan nama file yang dimulai dengan `"Surat-Perjanjian-Jual-Beli-"`, diikuti `NomorPolisi` di mana spasi diganti tanda hubung dan semua karakter non-alfanumerik (selain tanda hubung) dihapus, diikuti `-{id}.pdf`.

**Validates: Requirements 9.5**

### Property 7: Validasi Name template menolak nilai kosong, whitespace, dan terlalu panjang

*For any* string `Name` yang kosong, terdiri dari whitespace saja, atau melebihi 255 karakter, pengiriman form create/edit template harus ditolak dengan pesan validasi dan tidak ada template yang disimpan atau diperbarui di database.

**Validates: Requirements 3.3, 4.3**

### Property 8: Proteksi route — setiap route yang dilindungi mengarahkan ke login jika belum auth

*For any* route di dalam `TemplateController` (`/template`, `/template/create`, `/template/edit/{id}`, `/template/delete/{id}`), setiap HTTP request tanpa cookie autentikasi yang valid harus menerima redirect ke `/auth/login`.

**Validates: Requirements 1.4**

## Error Handling

### Strategy

Semua error ditangani di dua lapisan: lapisan controller dan global exception handler. Tidak ada stack trace atau nama exception yang pernah dikembalikan ke klien.

### Lapisan Controller

| Skenario | Penanganan |
|----------|------------|
| Template tidak ditemukan (edit/delete/preview) | Return `NotFound()` → HTTP 404 |
| Tidak ada template saat generate dokumen | Return View dengan pesan `"Template tidak tersedia. Hubungi administrator."` |
| Template masih direferensikan dokumen saat hapus | `TempData["Error"]` + redirect ke `/template` |
| NIK tidak valid | `ModelState.AddModelError` + return form |
| `DbUpdateException` saat simpan (misal NomorSurat duplikat) | Catch, log, return error page |
| Error generate PDF | Catch, log, return View error tanpa detail teknis |

### Global Error Handler

```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```

`HomeController.Error` memanggil `HttpContext.Features.Get<IExceptionHandlerPathFeature>()` hanya untuk logging (via `ILogger`), bukan untuk diteruskan ke view. View hanya menampilkan pesan generik.

### Seed Data Error

Jika database tidak tersedia saat startup (seed gagal), exception akan menyebar ke `Program.cs` dan menghentikan startup dengan log yang informatif. Tidak ada partial state yang ditinggalkan karena seed menggunakan `SaveChanges()` dalam satu operasi.

### Validation Error Display

Validasi ditampilkan di view menggunakan ASP.NET Core Tag Helpers:
```html
<span asp-validation-for="NikPenjual" class="text-danger"></span>
```
`_ValidationScriptsPartial.cshtml` disertakan di setiap form view untuk validasi client-side (opsional, server-side tetap wajib).

## Testing Strategy

### Framework dan Library

- **Unit/Property Tests**: xUnit + [FsCheck](https://fscheck.github.io/FsCheck/) (property-based testing untuk C#/.NET)
- **PBT minimum iterations**: 100 per properti (default FsCheck)
- **Mocking**: Moq untuk mock `ApplicationDbContext` dan service dependencies

FsCheck dipilih karena merupakan library PBT paling matang untuk ekosistem .NET, dengan integrasi xUnit yang langsung tersedia via `FsCheck.Xunit`.

### Unit Tests

Unit test difokuskan pada skenario spesifik dan edge case:

- `AuthController`: login valid, login invalid (email salah, password salah, field kosong), logout, redirect untuk authenticated user di GET /auth/login
- `TemplateService.DeleteAsync`: template dengan dokumen → return false; template tanpa dokumen → return true dan hapus dari DB
- `DocumentController`: form dengan satu field kosong → tidak simpan, re-populate form
- `PdfService.BuildFileName`: NomorPolisi dengan spasi, karakter khusus, dan kombinasi keduanya
- Seed data: database kosong → data seed tersimpan; database tidak kosong → seed dilewati

### Property-Based Tests

Setiap properti diimplementasikan dengan satu property-based test menggunakan FsCheck:

**Feature: legaly-web-app, Property 1: Auto-Fill mengganti semua placeholder yang dikenal**
```csharp
[Property]
public Property AutoFill_NoRemainingPlaceholders(
    NonEmptyString namaPenjual, NonEmptyString nikPenjual, /* ... */)
{
    var form = /* build valid DocumentFormViewModel */;
    var templateContent = "{{NamaPenjual}} {{NikPenjual}} ... {{NomorSurat}}";
    var result = _documentService.AutoFill(templateContent, form, "001/LEGALY/JBM/07/2026");
    return (!result.Contains("{{")).Label("No placeholders remain");
}
```

**Feature: legaly-web-app, Property 2: Auto-Fill mempertahankan nilai field secara akurat**
```csharp
[Property]
public Property AutoFill_FormatsHargaCorrectly(PositiveInt rawHarga)
{
    var form = new DocumentFormViewModel { Harga = rawHarga.Get, /* ... */ };
    var expected = rawHarga.Get.ToString("C0", new CultureInfo("id-ID"));
    var result = _documentService.AutoFill("{{Harga}}", form, "");
    return result.Contains(expected).Label("Harga formatted correctly");
}
```

**Feature: legaly-web-app, Property 3: Format NomorSurat selalu konsisten**
```csharp
[Property]
public Property NomorSurat_MatchesFormat(
    NonNegativeInt count, PositiveInt month12, PositiveInt year)
{
    var m = (month12.Get % 12) + 1;
    var y = 2000 + (year.Get % 100);
    var result = DocumentService.FormatNomorSurat(count.Get, m, y);
    var expected = $"{(count.Get + 1):D3}/LEGALY/JBM/{m:D2}/{y}";
    return (result == expected).Label("NomorSurat format matches");
}
```

**Feature: legaly-web-app, Property 4: Validasi NIK menolak semua string yang bukan tepat 16 digit**
```csharp
[Property]
public Property NikValidation_RejectsInvalidNik(string input)
{
    var isValid = input != null && System.Text.RegularExpressions.Regex.IsMatch(input, @"^\d{16}$");
    var validationResult = ValidateNik(input);
    return (!isValid == (validationResult != null)).Label("NIK validation correct");
}
```

**Feature: legaly-web-app, Property 5: Form dengan field invalid tidak menyimpan data**
Menggunakan generator yang menghasilkan `DocumentFormViewModel` dengan setidaknya satu field tidak valid, kemudian verifikasi `Documents.Count()` tidak bertambah.

**Feature: legaly-web-app, Property 6: Nama file PDF mengandung NomorPolisi yang telah disanitasi**
```csharp
[Property]
public Property BuildFileName_SanitizesNomorPolisi(string nomorPolisi, PositiveInt id)
{
    var filename = _pdfService.BuildFileName(nomorPolisi ?? "", id.Get);
    var noSpaces = !filename.Contains(" ");
    var startsCorrectly = filename.StartsWith("Surat-Perjanjian-Jual-Beli-");
    var endsCorrectly = filename.EndsWith($"-{id.Get}.pdf");
    return (noSpaces && startsCorrectly && endsCorrectly).Label("Filename format valid");
}
```

**Feature: legaly-web-app, Property 7: Validasi Name template menolak nilai kosong, whitespace, dan terlalu panjang**
Generator menghasilkan string yang kosong, hanya whitespace, atau berisi lebih dari 255 karakter. Verifikasi bahwa `ModelState` tidak valid dan tidak ada template yang ditambahkan ke DB.

**Feature: legaly-web-app, Property 8: Proteksi route — redirect ke login jika belum auth**
Loop atas semua protected routes menggunakan `TestServer` tanpa cookie auth. Verifikasi setiap response adalah redirect ke `/auth/login`.

### Integration Tests

- Login flow end-to-end dengan test SQL Server database
- Concurrency: dua request bersamaan pada bulan/tahun yang sama tidak menghasilkan NomorSurat duplikat
- PDF: `PdfService.GeneratePdf` menghasilkan byte array non-kosong yang valid dalam waktu < 5 detik untuk dokumen standar

### Test Configuration

```csharp
// Contoh konfigurasi FsCheck di xUnit
[assembly: Properties(MaxTest = 100, StartSize = 1, EndSize = 100)]
```
