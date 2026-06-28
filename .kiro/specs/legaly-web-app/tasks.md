# Implementation Plan: Legaly Web App

## Overview

Implementasi ASP.NET Core MVC (net10.0) untuk aplikasi Legaly — generator Surat Perjanjian Jual Beli Kendaraan Bermotor. Pendekatan incremental: fondasi (NuGet packages, entities, DbContext, migrations) → autentikasi → manajemen template → pembuatan dokumen → ekspor PDF → seed data → test project dengan property-based tests.

## Tasks

- [x] 1. Siapkan NuGet packages dan konfigurasi proyek
  - Tambahkan `BCrypt.Net-Next` versi `4.0.3` ke `Legaly.Web.csproj`
  - Tambahkan `QuestPDF` versi `2024.10.4` ke `Legaly.Web.csproj`
  - Perbarui `appsettings.json` dengan connection string `DefaultConnection` ke SQL Server (`LegalyDb`)
  - _Requirements: 10.1, 13.3_

- [x] 2. Buat entity models dan perbarui ApplicationDbContext
  - [x] 2.1 Buat `Models/Entities/Admin.cs`, `Models/Entities/Template.cs`, `Models/Entities/Document.cs`
    - Definisikan properties sesuai skema database (Id, Email, PasswordHash, CreatedAt, UpdatedAt untuk Admin; Id, Name, Content, CreatedAt, UpdatedAt untuk Template; semua fields untuk Document termasuk `DateOnly TanggalPerjanjian`)
    - _Requirements: 1.6, 3.2, 4.2, 7.4_
  - [x] 2.2 Perbarui `Data/ApplicationDbContext.cs`
    - Ganti `DbSet<User>` dengan `DbSet<Admin>`, `DbSet<Template>`, `DbSet<Document>`
    - Tambahkan `OnModelCreating`: unique index pada `Admin.Email`, unique index pada `Document.NomorSurat`, kolom tipe `decimal(18,2)` untuk `Document.Harga`, relasi `Document → Template` dengan `DeleteBehavior.Restrict`
    - _Requirements: 5.3, 7.3, 7.8_
  - [x] 2.3 Hapus `Models/Users.cs` dan buat EF Core migration baru untuk skema Admins, Templates, Documents
    - Jalankan `dotnet ef migrations add AddLegalySchema` untuk menghasilkan migration baru
    - Verifikasi migration menghasilkan tabel `Admins`, `Templates`, `Documents` dengan semua constraint yang benar
    - _Requirements: 10.1, 10.2_

- [x] 3. Buat ViewModels
  - [x] 3.1 Buat `Models/ViewModels/LoginViewModel.cs`
    - Fields: `Email` (`[Required][EmailAddress]`) dan `Password` (`[Required]`)
    - _Requirements: 1.1, 1.2_
  - [x] 3.2 Buat `Models/ViewModels/TemplateFormViewModel.cs`
    - Fields: `Name` (`[Required][StringLength(255)]`) dan `Content` (`[Required]`)
    - _Requirements: 3.3, 3.4, 4.3, 4.4_
  - [x] 3.3 Buat `Models/ViewModels/DocumentFormViewModel.cs`
    - Semua fields form dokumen dengan Data Annotations: `NamaPenjual` (`[Required][StringLength(255)]`), `NikPenjual` (`[Required][StringLength(20)][RegularExpression(@"^\d{16}$", ErrorMessage="NIK harus 16 digit angka")]`), dst.
    - `TahunMotor` (`[Required][Range(1900, 9999)]`), `Harga` (`[Required][Range(1, 999999999999.99)]`), `TanggalPerjanjian` (`[Required]` sebagai `DateOnly`)
    - _Requirements: 6.2–6.18_
  - [x] 3.4 Buat `Models/ViewModels/PreviewViewModel.cs`
    - Fields: `Id`, `NomorSurat`, `GeneratedContent`, `NomorPolisi`
    - _Requirements: 8.1, 8.2, 8.3_

- [x] 4. Checkpoint — Verifikasi kompilasi fondasi
  - Pastikan proyek berhasil di-build (`dotnet build`) tanpa error setelah entity models, DbContext, dan ViewModels dibuat.

- [x] 5. Perbarui `Program.cs` dan daftarkan services
  - Tulis ulang `Program.cs` sesuai desain: tambahkan `UseAuthentication`, `UseAuthorization`, daftarkan cookie authentication (LoginPath `/auth/login`, ExpireTimeSpan 8 jam, `HttpOnly = true`, `SecurePolicy = Always`), daftarkan `ITemplateService`, `IDocumentService`, `IPdfService` sebagai scoped services
  - Tambahkan blok seed data setelah `app.Build()`: jalankan `db.Database.Migrate()`, seed Admin default jika tabel kosong, seed Template default jika tabel kosong
  - _Requirements: 1.1, 1.3, 1.5, 10.1, 10.2, 10.3, 13.4_

- [x] 6. Implementasi autentikasi
  - [x] 6.1 Buat `Services/ITemplateService.cs` dan `Services/TemplateService.cs` (stub awal — `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`)
    - `DeleteAsync` harus memeriksa apakah template masih direferensikan oleh dokumen sebelum menghapus; return `false` jika masih ada, hapus dan return `true` jika tidak
    - _Requirements: 2.1, 3.2, 4.2, 5.1, 5.2, 5.3_
  - [x] 6.2 Buat `Controllers/AuthController.cs`
    - `GET /auth/login`: tampilkan form; redirect ke `/template` jika sudah terautentikasi
    - `POST /auth/login`: cari admin by email, verifikasi BCrypt, buat cookie auth, redirect ke `/template`; tampilkan `ViewData["Error"] = "Email atau password salah."` jika gagal
    - `POST /auth/logout`: `HttpContext.SignOutAsync`, redirect ke `/auth/login`
    - _Requirements: 1.1, 1.2, 1.3, 1.5, 1.6, 1.7_
  - [x] 6.3 Buat `Views/Auth/Login.cshtml`
    - Form Bootstrap 5 dengan fields email dan password, tampilkan `ViewData["Error"]` sebagai alert merah jika ada, gunakan `_Layout.cshtml`
    - _Requirements: 1.1, 1.2, 12.1, 12.2, 12.3_

- [x] 7. Implementasi manajemen template
  - [x] 7.1 Buat `Controllers/TemplateController.cs` dengan atribut `[Authorize]` di class level
    - Actions: `Index` (GET), `Create` (GET/POST), `Edit` (GET/POST, return 404 jika tidak ditemukan), `Delete` (POST, return 404 jika tidak ditemukan, tampilkan error jika template masih direferensikan)
    - Gunakan `TempData["Success"]` dan `TempData["Error"]` untuk notifikasi
    - _Requirements: 1.4, 2.1–2.4, 3.1–3.4, 4.1–4.5, 5.1–5.3, 11.1–11.3_
  - [x] 7.2 Perbarui `Views/Shared/_Layout.cshtml` dan buat `Views/Shared/_AdminLayout.cshtml`
    - `_Layout.cshtml` (publik): navbar Bootstrap 5 dengan link ke home dan `/document/create`, tampilkan `TempData["Success"]`/`TempData["Error"]` sebagai Bootstrap alert dismissible
    - `_AdminLayout.cshtml`: extends layout admin dengan link ke `/template` dan tombol logout
    - _Requirements: 11.1–11.3, 12.1–12.3_
  - [x] 7.3 Buat `Views/Template/Index.cshtml`, `Views/Template/Create.cshtml`, `Views/Template/Edit.cshtml`
    - `Index.cshtml`: tabel daftar template dengan tombol Edit dan Hapus per baris; tampilkan pesan jika kosong
    - `Create.cshtml` dan `Edit.cshtml`: form Bootstrap 5 dengan fields `Name` dan `Content` (textarea), validasi per field via `asp-validation-for`, gunakan `_AdminLayout.cshtml`
    - _Requirements: 2.1–2.4, 3.1–3.4, 4.1–4.5, 11.4, 12.1–12.3_

- [x] 8. Checkpoint — Verifikasi alur autentikasi dan manajemen template
  - Pastikan proyek berhasil di-build. Semua tests yang ada harus pass. Tanyakan kepada user jika ada pertanyaan.

- [x] 9. Implementasi pembuatan dokumen
  - [x] 9.1 Buat `Services/IDocumentService.cs` dan `Services/DocumentService.cs`
    - `AutoFill(string templateContent, DocumentFormViewModel form, string nomorSurat)`: pure function yang mereplace semua 16 placeholder; format `Harga` sebagai `C0` dengan locale `id-ID`, `TanggalPerjanjian` sebagai `dd MMMM yyyy` dengan locale `id-ID`
    - `GenerateNomorSuratAsync(DateOnly tanggal)`: hitung count dokumen pada bulan/tahun yang sama, return `{count+1:D3}/LEGALY/JBM/{MM:D2}/{YYYY}`
    - `CreateAsync(DocumentFormViewModel vm)`: ambil template pertama (`OrderBy Id`), generate NomorSurat, jalankan AutoFill, simpan `Document`, return entity
    - `GetByIdAsync(int id)`: return `Document` dengan Include Template atau null
    - _Requirements: 7.1–7.8_
  - [x] 9.2 Buat `Controllers/DocumentController.cs`
    - `GET /document/create`: tampilkan form kosong
    - `POST /document/create`: validasi ModelState, jika invalid re-populate form; jika valid panggil `IDocumentService.CreateAsync`, tangkap `DbUpdateException` (NomorSurat duplikat), redirect ke `/document/preview/{id}`; jika tidak ada template tampilkan error page
    - `GET /document/preview/{id}`: ambil dokumen, return 404 jika tidak ditemukan, return `PreviewViewModel`
    - `GET /document/download/{id}`: ambil dokumen, return 404 jika tidak ditemukan, panggil `IPdfService.GeneratePdf`, return `File()` dengan header yang tepat
    - _Requirements: 6.1, 6.17, 7.1–7.8, 8.1–8.4, 9.1, 9.6, 9.8_
  - [x] 9.3 Buat `Views/Document/Create.cshtml`
    - Form Bootstrap 5 tiga-section (Penjual, Pembeli, Kendaraan + Harga + Tanggal), validasi per field via `asp-validation-for`, sertakan `_ValidationScriptsPartial.cshtml`, gunakan `_Layout.cshtml`
    - _Requirements: 6.1–6.18, 11.4, 12.1–12.3_
  - [x] 9.4 Buat `Views/Document/Preview.cshtml`
    - Tampilkan `NomorSurat` (bold/heading), `GeneratedContent` dalam `<pre>` untuk mempertahankan whitespace, tombol "Unduh PDF" yang tertaut ke `/document/download/{id}`, gunakan `_Layout.cshtml`
    - _Requirements: 8.1–8.4, 12.1–12.3_

- [ ] 10. Implementasi ekspor PDF
  - [ ] 10.1 Buat `Services/IPdfService.cs` dan `Services/PdfService.cs`
    - `GeneratePdf(Document document)`: generate PDF via QuestPDF dengan ukuran A4, margin 2cm semua sisi, font Arial 12pt, `NomorSurat` bold + rata tengah di atas, `GeneratedContent` dengan `PreserveNewlines()`
    - `BuildFileName(string nomorPolisi, int id)`: ganti spasi dengan `-`, hapus semua karakter non-alfanumerik kecuali `-`, return `Surat-Perjanjian-Jual-Beli-{sanitized}-{id}.pdf`
    - _Requirements: 9.1–9.8, 13.2_
  - [ ]* 10.2 Tulis unit test untuk `PdfService.BuildFileName` (edge cases: spasi, karakter khusus, kosong)
    - Test: `NomorPolisi` = `"AB 1234 CD"` → `"AB-1234-CD"`, karakter khusus seperti `/` dihapus, string kosong → nama file tetap valid
    - _Requirements: 9.5_

- [ ] 11. Buat test project dan konfigurasi FsCheck
  - Buat solution file dan project xUnit baru: `Legaly.Tests/Legaly.Tests.csproj` dengan referensi ke project utama
  - Tambahkan packages: `xunit` `2.9.3`, `xunit.runner.visualstudio` `2.8.2`, `FsCheck.Xunit` `3.2.0`, `Moq` `4.20.72`, `Microsoft.AspNetCore.Mvc.Testing` `10.0.9`, `Microsoft.EntityFrameworkCore.InMemory` `10.0.9`
  - Tambahkan konfigurasi FsCheck: `[assembly: Properties(MaxTest = 100)]` di file `AssemblyConfiguration.cs`
  - _Requirements: semua property tests_

- [ ] 12. Implementasi property-based tests
  - [ ]* 12.1 Tulis property test untuk Property 1: Auto-Fill tidak meninggalkan placeholder
    - Gunakan FsCheck generator untuk `NonEmptyString` fields, bangun template dengan semua 16 placeholder standar, verifikasi hasil `AutoFill` tidak mengandung `{{`
    - **Property 1: Auto-Fill mengganti semua placeholder yang dikenal**
    - **Validates: Requirements 7.2, 8.2**
  - [ ]* 12.2 Tulis property test untuk Property 2: Auto-Fill mempertahankan nilai field secara akurat
    - Generator `PositiveInt` untuk `Harga`, verifikasi output mengandung `Harga` terformat sebagai `C0` locale `id-ID`; verifikasi `TanggalPerjanjian` diformat `dd MMMM yyyy` locale `id-ID`; verifikasi `NomorSurat` disisipkan tepat
    - **Property 2: Auto-Fill mempertahankan nilai field secara akurat**
    - **Validates: Requirements 7.2**
  - [ ]* 12.3 Tulis property test untuk Property 3: Format NomorSurat selalu konsisten
    - Ekstrak metode statis `DocumentService.FormatNomorSurat(int count, int month, int year)`, test dengan generator `NonNegativeInt` count dan month/year valid; verifikasi format `{count+1:D3}/LEGALY/JBM/{MM:D2}/{YYYY}`
    - **Property 3: Format NomorSurat selalu konsisten**
    - **Validates: Requirements 7.3**
  - [ ]* 12.4 Tulis property test untuk Property 4: Validasi NIK menolak semua string bukan 16 digit
    - Generator menghasilkan string acak; verifikasi `Regex.IsMatch(input, @"^\d{16}$")` selalu berkorespondensi dengan hasil validasi `DocumentFormViewModel`
    - **Property 4: Validasi NIK menolak semua string yang bukan tepat 16 digit**
    - **Validates: Requirements 6.3, 6.6, 6.18**
  - [ ]* 12.5 Tulis property test untuk Property 5: Form invalid tidak menyimpan data
    - Generator menghasilkan `DocumentFormViewModel` dengan setidaknya satu field tidak valid (string kosong / melebihi batas / NIK tidak valid); mock `ApplicationDbContext` (InMemory), verifikasi `Documents.Count()` tidak bertambah setelah controller action
    - **Property 5: Form dengan field invalid tidak menyimpan data**
    - **Validates: Requirements 6.17**
  - [ ]* 12.6 Tulis property test untuk Property 6: Nama file PDF mengandung NomorPolisi yang tersanitasi
    - Generator string acak untuk `nomorPolisi` dan `PositiveInt` untuk `id`; verifikasi file dimulai dengan `"Surat-Perjanjian-Jual-Beli-"`, tidak mengandung spasi, tidak mengandung karakter non-alfanumerik selain `-`, diakhiri `-{id}.pdf`
    - **Property 6: Nama file PDF mengandung NomorPolisi yang telah disanitasi**
    - **Validates: Requirements 9.5**
  - [ ]* 12.7 Tulis property test untuk Property 7: Validasi Name template menolak nilai kosong, whitespace, dan terlalu panjang
    - Generator menghasilkan string kosong, string yang hanya whitespace, atau string > 255 karakter; validasi `TemplateFormViewModel` menggunakan `Validator.TryValidateObject`; verifikasi `ModelState` tidak valid dan field `Name` ada dalam errors
    - **Property 7: Validasi Name template menolak nilai kosong, whitespace, dan terlalu panjang**
    - **Validates: Requirements 3.3, 4.3**
  - [ ]* 12.8 Tulis property test untuk Property 8: Proteksi route mengarahkan ke login jika belum auth
    - Gunakan `WebApplicationFactory` tanpa cookie auth; loop atas semua protected routes (`/template`, `/template/create`, `/template/edit/1`, `/template/delete/1`); verifikasi setiap response adalah redirect (302) ke `/auth/login`
    - **Property 8: Proteksi route — setiap route yang dilindungi mengarahkan ke login jika belum auth**
    - **Validates: Requirements 1.4**

- [ ] 13. Checkpoint Final — Pastikan semua tests pass
  - Pastikan proyek berhasil di-build. Jalankan `dotnet test` dan pastikan semua tests pass. Tanyakan kepada user jika ada pertanyaan.

## Notes

- Tasks bertanda `*` bersifat opsional dan dapat dilewati untuk MVP yang lebih cepat
- Setiap task mereferensikan requirement spesifik untuk traceability
- Checkpoint memastikan validasi inkremental di setiap fase
- Property tests memvalidasi correctness properties universal dari design.md
- Unit tests memvalidasi skenario dan edge case spesifik
- Karena `NomorSurat` memiliki constraint `UNIQUE` di database, `DocumentController` harus menangkap `DbUpdateException` dan melakukan retry atau menampilkan error informatif (lihat design.md — Concurrency section)
- `QuestPDF` memerlukan pengaturan lisensi `QuestPDF.Settings.License = LicenseType.Community;` di `Program.cs` sebelum penggunaan pertama
- Migration `InitialCreate` yang sudah ada harus di-drop atau digabungkan — paling aman: hapus folder `Migrations/`, jalankan `dotnet ef migrations add InitialCreate` yang baru setelah entity models dan DbContext diperbarui

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["2.1"] },
    { "id": 1, "tasks": ["2.2"] },
    { "id": 2, "tasks": ["2.3", "3.1", "3.2", "3.3", "3.4"] },
    { "id": 3, "tasks": ["6.1", "6.2", "6.3"] },
    { "id": 4, "tasks": ["7.1", "7.2", "7.3"] },
    { "id": 5, "tasks": ["9.1"] },
    { "id": 6, "tasks": ["9.2", "9.3", "9.4"] },
    { "id": 7, "tasks": ["10.1"] },
    { "id": 8, "tasks": ["10.2", "12.1", "12.2", "12.3", "12.4", "12.5", "12.6", "12.7", "12.8"] }
  ]
}
```
