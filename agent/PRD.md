# PRD: Legaly — AI Agent Context for Code Generation

> **Instruksi untuk AI Agent:**
> Dokumen ini adalah spesifikasi lengkap sistem Legaly. Baca seluruh dokumen sebelum menulis kode apapun. Ikuti konvensi, struktur, dan behavior yang didefinisikan di sini secara konsisten. Jika ada ambiguitas, tanyakan — jangan asumsikan.

---

## 1. Project Identity

| Key | Value |
|-----|-------|
| Project Name | Legaly |
| Type | Web Application |
| Framework | ASP.NET Core 8.0 MVC |
| Language | C# |
| Database | SQL Server |
| ORM | Entity Framework Core 8 |
| PDF Engine | QuestPDF |
| Auth | Cookie Authentication (ASP.NET Core Identity-free, manual) |
| UI | Razor Views + Bootstrap 5 |
| IDE Target | Visual Studio 2022 / VS Code |

---

## 2. Project Structure

```
Legaly/
├── Controllers/
│   ├── HomeController.cs
│   ├── AuthController.cs
│   ├── TemplateController.cs
│   └── DocumentController.cs
├── Models/
│   ├── Entities/
│   │   ├── Admin.cs
│   │   ├── Template.cs
│   │   └── Document.cs
│   ├── ViewModels/
│   │   ├── LoginViewModel.cs
│   │   ├── TemplateFormViewModel.cs
│   │   ├── DocumentFormViewModel.cs
│   │   └── PreviewViewModel.cs
├── Services/
│   ├── ITemplateService.cs
│   ├── TemplateService.cs
│   ├── IDocumentService.cs
│   ├── DocumentService.cs
│   ├── IPdfService.cs
│   └── PdfService.cs
├── Data/
│   └── AppDbContext.cs
├── Views/
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Auth/
│   │   └── Login.cshtml
│   ├── Template/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   └── Edit.cshtml
│   ├── Document/
│   │   ├── Create.cshtml
│   │   └── Preview.cshtml
│   └── Shared/
│       ├── _Layout.cshtml
│       └── _AdminLayout.cshtml
├── Migrations/
├── appsettings.json
└── Program.cs
```

---

## 3. Database Schema

### Tabel: `Admins`

```sql
CREATE TABLE Admins (
    Id          INT PRIMARY KEY IDENTITY(1,1),
    Email       NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

### Tabel: `Templates`

```sql
CREATE TABLE Templates (
    Id        INT PRIMARY KEY IDENTITY(1,1),
    Name      NVARCHAR(255) NOT NULL,
    Content   NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

> `Content` berisi teks surat lengkap dengan placeholder format `{{NamaField}}`.
> Contoh: `Pada hari ini, saya {{NamaPenjual}} telah menjual motor...`

### Tabel: `Documents`

```sql
CREATE TABLE Documents (
    Id               INT PRIMARY KEY IDENTITY(1,1),
    TemplateId       INT NOT NULL REFERENCES Templates(Id),
    NomorSurat       NVARCHAR(100) NOT NULL UNIQUE,
    NamaPenjual      NVARCHAR(255) NOT NULL,
    NikPenjual       NVARCHAR(20) NOT NULL,
    AlamatPenjual    NVARCHAR(500) NOT NULL,
    NamaPembeli      NVARCHAR(255) NOT NULL,
    NikPembeli       NVARCHAR(20) NOT NULL,
    AlamatPembeli    NVARCHAR(500) NOT NULL,
    MerkMotor        NVARCHAR(100) NOT NULL,
    TipeMotor        NVARCHAR(100) NOT NULL,
    TahunMotor       INT NOT NULL,
    WarnaMotor       NVARCHAR(50) NOT NULL,
    NomorPolisi      NVARCHAR(20) NOT NULL,
    NomorBPKB        NVARCHAR(50) NOT NULL,
    NomorSTNK        NVARCHAR(50) NOT NULL,
    Harga            DECIMAL(18,2) NOT NULL,
    TanggalPerjanjian DATE NOT NULL,
    GeneratedContent NVARCHAR(MAX) NOT NULL,
    CreatedAt        DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

---

## 4. Entity Models (C#)

### `Admin.cs`
```csharp
public class Admin
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### `Template.cs`
```csharp
public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### `Document.cs`
```csharp
public class Document
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public Template Template { get; set; } = null!;
    public string NomorSurat { get; set; } = string.Empty;
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
    public decimal Harga { get; set; }
    public DateOnly TanggalPerjanjian { get; set; }
    public string GeneratedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

---

## 5. Placeholder Convention

Format placeholder dalam template: `{{NamaField}}` (double curly braces, PascalCase, tanpa spasi).

| Placeholder | Sumber Data |
|-------------|-------------|
| `{{NamaPenjual}}` | `DocumentFormViewModel.NamaPenjual` |
| `{{NikPenjual}}` | `DocumentFormViewModel.NikPenjual` |
| `{{AlamatPenjual}}` | `DocumentFormViewModel.AlamatPenjual` |
| `{{NamaPembeli}}` | `DocumentFormViewModel.NamaPembeli` |
| `{{NikPembeli}}` | `DocumentFormViewModel.NikPembeli` |
| `{{AlamatPembeli}}` | `DocumentFormViewModel.AlamatPembeli` |
| `{{MerkMotor}}` | `DocumentFormViewModel.MerkMotor` |
| `{{TipeMotor}}` | `DocumentFormViewModel.TipeMotor` |
| `{{TahunMotor}}` | `DocumentFormViewModel.TahunMotor` |
| `{{WarnaMotor}}` | `DocumentFormViewModel.WarnaMotor` |
| `{{NomorPolisi}}` | `DocumentFormViewModel.NomorPolisi` |
| `{{NomorBPKB}}` | `DocumentFormViewModel.NomorBPKB` |
| `{{NomorSTNK}}` | `DocumentFormViewModel.NomorSTNK` |
| `{{Harga}}` | `DocumentFormViewModel.Harga` (format: Rp 15.000.000) |
| `{{TanggalPerjanjian}}` | `DocumentFormViewModel.TanggalPerjanjian` (format: dd MMMM yyyy, id-ID) |
| `{{NomorSurat}}` | Auto-generated oleh sistem |

---

## 6. Nomor Surat — Format & Logic

Format: `{NoUrut:D3}/LEGALY/JBM/{MM}/{YYYY}`

Contoh: `001/LEGALY/JBM/07/2026`

Logic generate:
1. Hitung jumlah dokumen yang ada di database pada bulan dan tahun yang sama dengan `TanggalPerjanjian`.
2. Tambahkan 1.
3. Format hasilnya sebagai 3 digit (001, 002, dst).

```csharp
// Contoh implementasi di DocumentService
private async Task<string> GenerateNomorSurat(DateOnly tanggal)
{
    var count = await _context.Documents
        .Where(d => d.TanggalPerjanjian.Month == tanggal.Month
                 && d.TanggalPerjanjian.Year == tanggal.Year)
        .CountAsync();

    return $"{(count + 1):D3}/LEGALY/JBM/{tanggal.Month:D2}/{tanggal.Year}";
}
```

---

## 7. Feature Specifications

### 7.1 Authentication

**Route:** `GET /auth/login`, `POST /auth/login`  
**Controller:** `AuthController`

**Login Flow:**
1. User POST email + password.
2. Cari admin berdasarkan email di tabel `Admins`.
3. Verifikasi password dengan `BCrypt.Verify(password, admin.PasswordHash)`.
4. Jika valid: buat cookie auth, redirect ke `/template`.
5. Jika tidak valid: tampilkan pesan `"Email atau password salah."` di halaman login.

**Logout:**
- Route: `POST /auth/logout`
- Hapus cookie auth, redirect ke `/auth/login`.

**Proteksi Halaman Admin:**
- Semua route di bawah `TemplateController` wajib menggunakan `[Authorize]`.
- Jika belum login dan akses halaman admin, redirect ke `/auth/login`.

---

### 7.2 Template Management

**Controller:** `TemplateController`  
**Base Route:** `/template`

| Action | Method | Route | Keterangan |
|--------|--------|-------|------------|
| Index | GET | `/template` | Tampilkan daftar semua template |
| Create | GET | `/template/create` | Form tambah template baru |
| Create | POST | `/template/create` | Simpan template baru |
| Edit | GET | `/template/edit/{id}` | Form edit template |
| Edit | POST | `/template/edit/{id}` | Update template |
| Delete | POST | `/template/delete/{id}` | Hapus template |

**Validasi Create/Edit:**
- `Name`: required, maxlength 255.
- `Content`: required, tidak boleh kosong.

**Behavior Delete:**
- Tidak ada soft delete. Hapus langsung dari database.
- Sebelum redirect, set `TempData["Success"] = "Template berhasil dihapus."`.

**Notifikasi:**
- Gunakan `TempData["Success"]` untuk pesan sukses.
- Tampilkan di layout sebagai Bootstrap alert dismissible.

---

### 7.3 Document Generator

**Controller:** `DocumentController`  
**Base Route:** `/document`

| Action | Method | Route | Keterangan |
|--------|--------|-------|------------|
| Create | GET | `/document/create` | Tampilkan form isi data |
| Create | POST | `/document/create` | Proses data, generate dokumen, redirect ke preview |
| Preview | GET | `/document/preview/{id}` | Tampilkan preview dokumen |
| Download | GET | `/document/download/{id}` | Generate dan download PDF |

**POST /document/create — Flow:**
1. Validasi semua field (lihat validasi di bawah).
2. Ambil template pertama yang tersedia (`Templates.OrderBy(t => t.Id).First()`).
3. Jalankan proses Auto-Fill: replace semua placeholder di `Content` dengan data form.
4. Generate `NomorSurat`.
5. Simpan record `Document` ke database.
6. Redirect ke `/document/preview/{id}`.

**Validasi Input Form:**

| Field | Rule |
|-------|------|
| NamaPenjual | Required, string, maxlength 255 |
| NikPenjual | Required, string, length 16, digits only |
| AlamatPenjual | Required, string, maxlength 500 |
| NamaPembeli | Required, string, maxlength 255 |
| NikPembeli | Required, string, length 16, digits only |
| AlamatPembeli | Required, string, maxlength 500 |
| MerkMotor | Required, string, maxlength 100 |
| TipeMotor | Required, string, maxlength 100 |
| TahunMotor | Required, int, range 1900–2026 |
| WarnaMotor | Required, string, maxlength 50 |
| NomorPolisi | Required, string, maxlength 20 |
| NomorBPKB | Required, string, maxlength 50 |
| NomorSTNK | Required, string, maxlength 50 |
| Harga | Required, decimal, min 1 |
| TanggalPerjanjian | Required, date, tidak boleh di masa depan lebih dari 1 tahun |

**Auto-Fill Logic:**
```csharp
// Di DocumentService.cs
public string AutoFill(string templateContent, DocumentFormViewModel form)
{
    return templateContent
        .Replace("{{NamaPenjual}}", form.NamaPenjual)
        .Replace("{{NikPenjual}}", form.NikPenjual)
        .Replace("{{AlamatPenjual}}", form.AlamatPenjual)
        .Replace("{{NamaPembeli}}", form.NamaPembeli)
        .Replace("{{NikPembeli}}", form.NikPembeli)
        .Replace("{{AlamatPembeli}}", form.AlamatPembeli)
        .Replace("{{MerkMotor}}", form.MerkMotor)
        .Replace("{{TipeMotor}}", form.TipeMotor)
        .Replace("{{TahunMotor}}", form.TahunMotor.ToString())
        .Replace("{{WarnaMotor}}", form.WarnaMotor)
        .Replace("{{NomorPolisi}}", form.NomorPolisi)
        .Replace("{{NomorBPKB}}", form.NomorBPKB)
        .Replace("{{NomorSTNK}}", form.NomorSTNK)
        .Replace("{{Harga}}", form.Harga.ToString("C0", new CultureInfo("id-ID")))
        .Replace("{{TanggalPerjanjian}}", form.TanggalPerjanjian.ToString("dd MMMM yyyy", new CultureInfo("id-ID")))
        .Replace("{{NomorSurat}}", form.NomorSurat ?? string.Empty);
}
```

---

### 7.4 Preview Document

**Route:** `GET /document/preview/{id}`

Tampilkan `GeneratedContent` dari database dalam tampilan yang rapi menggunakan `<pre>` atau render HTML (tergantung format template).

Halaman preview menampilkan:
- Nomor surat
- Isi dokumen lengkap (sudah ter-replace)
- Tombol "Unduh PDF" yang link ke `/document/download/{id}`

---

### 7.5 PDF Export

**Route:** `GET /document/download/{id}`

**Flow:**
1. Ambil `Document` berdasarkan `id`.
2. Generate PDF dari `GeneratedContent` menggunakan QuestPDF.
3. Return file dengan:
   - Content-Type: `application/pdf`
   - Nama file: `Surat-Perjanjian-Jual-Beli-{NomorPolisi}-{id}.pdf`

**QuestPDF Document Structure:**
```csharp
Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

        page.Content().Column(col =>
        {
            col.Item().Text(document.NomorSurat).Bold().AlignCenter();
            col.Item().PaddingTop(10).Text(document.GeneratedContent);
        });
    });
});
```

---

## 8. Non-Functional Requirements

| Parameter | Requirement |
|-----------|-------------|
| Performance | Halaman load < 3 detik. Generate PDF < 5 detik |
| Security | Password di-hash dengan BCrypt (cost factor 12). Tidak ada plain text password di database |
| Session | Cookie-based auth. Cookie expire setelah 8 jam. HttpOnly = true, Secure = true |
| Validation | Semua input divalidasi server-side menggunakan Data Annotations + ModelState |
| Error Handling | Jika template tidak ditemukan saat generate dokumen, tampilkan error page dengan pesan "Template tidak tersedia. Hubungi administrator." |
| Encoding | Semua string menggunakan UTF-8. Konten template mendukung karakter Indonesia (é, â, dst) |
| Responsive | UI menggunakan Bootstrap 5 grid. Minimal berfungsi di viewport 360px (mobile) dan 1280px (desktop) |

---

## 9. Dependencies (NuGet Packages)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.*" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.*" />
<PackageReference Include="QuestPDF" Version="2024.*" />
```

---

## 10. Program.cs — Service Registration

```csharp
// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// Services
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfService, PdfService>();
```

---

## 11. appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LegalyDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 12. Seed Data

Saat pertama kali project dijalankan, sistem harus memiliki:

**Admin default:**
- Email: `admin@legaly.id`
- Password: `Admin@123` (di-hash dengan BCrypt sebelum disimpan)

**Template default:**

```
SURAT PERJANJIAN JUAL BELI KENDARAAN BERMOTOR

Nomor: {{NomorSurat}}

Pada hari ini, {{TanggalPerjanjian}}, yang bertanda tangan di bawah ini:

PIHAK PERTAMA (PENJUAL)
Nama    : {{NamaPenjual}}
NIK     : {{NikPenjual}}
Alamat  : {{AlamatPenjual}}

PIHAK KEDUA (PEMBELI)
Nama    : {{NamaPembeli}}
NIK     : {{NikPembeli}}
Alamat  : {{AlamatPembeli}}

Pihak Pertama dengan ini menyatakan telah menjual kepada Pihak Kedua sebuah kendaraan bermotor dengan data sebagai berikut:

Merk/Type   : {{MerkMotor}} {{TipeMotor}}
Tahun       : {{TahunMotor}}
Warna       : {{WarnaMotor}}
No. Polisi  : {{NomorPolisi}}
No. BPKB    : {{NomorBPKB}}
No. STNK    : {{NomorSTNK}}
Harga       : {{Harga}}

Demikian surat perjanjian ini dibuat dengan sebenarnya dan ditandatangani oleh kedua belah pihak.

Pihak Pertama                    Pihak Kedua


({{NamaPenjual}})               ({{NamaPembeli}})
```

---

## 13. Acceptance Criteria

### Admin
- [ ] Login dengan email dan password yang valid → masuk ke dashboard template
- [ ] Login dengan kredensial salah → pesan error tampil, tidak redirect
- [ ] Logout → session terhapus, redirect ke login
- [ ] Create template → tersimpan di DB, muncul di daftar template
- [ ] Edit template → perubahan tersimpan, data lama tergantikan
- [ ] Delete template → terhapus dari DB, tidak muncul di daftar

### Pengguna Umum
- [ ] Buka halaman utama tanpa login → berhasil akses
- [ ] Isi semua field form → dokumen terbuat, redirect ke preview
- [ ] Submit form dengan field kosong → validasi error tampil per field
- [ ] NIK diisi bukan 16 digit → error "NIK harus 16 digit angka"
- [ ] Preview dokumen → semua placeholder ter-replace dengan data yang diisi
- [ ] Klik unduh PDF → file PDF terdownload, nama file sesuai format

### System
- [ ] Generate PDF tidak error untuk konten hingga 2 halaman A4
- [ ] Nomor surat unik per bulan (tidak duplikat)
- [ ] Data dokumen tersimpan di tabel Documents setelah generate