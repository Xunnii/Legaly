# Requirements Document

## Introduction

Legaly adalah aplikasi web ASP.NET Core MVC (net10.0) untuk menghasilkan dokumen Surat Perjanjian Jual Beli Kendaraan Bermotor. Sistem ini memungkinkan admin untuk mengelola template surat dan pengguna umum untuk mengisi data transaksi kendaraan, menghasilkan dokumen dengan placeholder yang terisi otomatis, melihat pratinjau dokumen, dan mengunduh dokumen dalam format PDF.

Aplikasi ini menggunakan Entity Framework Core dengan SQL Server sebagai database, autentikasi berbasis cookie tanpa ASP.NET Identity, BCrypt untuk hashing password, QuestPDF untuk ekspor PDF, dan Bootstrap 5 untuk antarmuka pengguna.

---

## Glossary

- **System**: Aplikasi web Legaly secara keseluruhan
- **Auth_Module**: Modul autentikasi yang menangani login dan logout admin
- **Template_Manager**: Modul pengelolaan template dokumen
- **Document_Generator**: Modul pembuatan dokumen dari template dan data form
- **PDF_Exporter**: Modul yang menghasilkan file PDF dari konten dokumen
- **Admin**: Pengguna dengan hak akses penuh ke fitur manajemen template
- **Visitor**: Pengguna umum yang mengakses form pembuatan dokumen tanpa autentikasi
- **Template**: Teks surat dengan placeholder dalam format `{{NamaField}}`
- **Document**: Rekaman surat perjanjian yang sudah diisi dan disimpan di database
- **NomorSurat**: Nomor unik dokumen dengan format `{NoUrut:D3}/LEGALY/JBM/{MM}/{YYYY}`
- **Placeholder**: Token dalam format `{{NamaField}}` di dalam konten template yang akan diganti dengan data aktual
- **GeneratedContent**: Konten template setelah semua placeholder diganti dengan data dokumen
- **Auto-Fill**: Proses penggantian semua placeholder dalam template dengan data yang diisi pada form
- **BCrypt**: Algoritma hashing password dengan cost factor 12
- **TanggalPerjanjian**: Tanggal perjanjian jual beli dalam format `dd MMMM yyyy` dengan locale `id-ID`
- **NIK**: Nomor Induk Kependudukan, terdiri dari tepat 16 digit angka

---

## Requirements

### Requirement 1: Autentikasi Admin

**User Story:** Sebagai admin, saya ingin dapat login dan logout dengan aman, agar hanya saya yang dapat mengakses fitur manajemen template.

#### Acceptance Criteria

1. WHEN admin mengirimkan email yang terdaftar di tabel `Admins` dan password yang lolos verifikasi BCrypt melalui form login, THE Auth_Module SHALL membuat cookie autentikasi dengan masa berlaku 8 jam, kemudian mengarahkan admin ke `/template`.
2. WHEN admin mengirimkan email yang tidak terdaftar, password yang salah, atau field email/password yang kosong, THE Auth_Module SHALL menampilkan pesan `"Email atau password salah."` pada halaman login tanpa mengarahkan ke halaman lain.
3. WHEN admin mengirimkan permintaan logout, THE Auth_Module SHALL menghapus cookie autentikasi dan mengarahkan pengguna ke `/auth/login`.
4. WHILE admin belum terautentikasi, THE System SHALL mengarahkan setiap permintaan ke `/template`, `/template/create`, `/template/edit/{id}`, dan `/template/delete/{id}` ke halaman `/auth/login`.
5. THE Auth_Module SHALL menyimpan cookie autentikasi dengan atribut `HttpOnly = true` dan `Secure = true`.
6. THE Auth_Module SHALL menyimpan password admin sebagai BCrypt hash dengan cost factor 12; tidak ada plain-text password yang disimpan di database.
7. WHEN admin yang sudah terautentikasi mengakses `GET /auth/login`, THE Auth_Module SHALL mengarahkan admin ke `/template` tanpa menampilkan form login.

---

### Requirement 2: Manajemen Template — Menampilkan Daftar

**User Story:** Sebagai admin, saya ingin melihat semua template yang tersedia, agar saya dapat mengelola template dengan mudah.

#### Acceptance Criteria

1. WHEN admin yang terautentikasi mengakses `GET /template`, THE Template_Manager SHALL mengambil semua template dari database dan menampilkannya dalam daftar.
2. WHEN daftar template berhasil dimuat, THE Template_Manager SHALL menampilkan nama template dan tombol aksi Edit dan Hapus untuk setiap template dalam daftar.
3. WHEN tidak ada template tersedia di database, THE Template_Manager SHALL menampilkan pesan yang menginformasikan bahwa belum ada template, beserta tautan ke `/template/create`.
4. IF terjadi error saat mengambil data dari database, THEN THE Template_Manager SHALL menampilkan halaman error yang informatif tanpa mengekspos detail teknis.

---

### Requirement 3: Manajemen Template — Membuat Template Baru

**User Story:** Sebagai admin, saya ingin membuat template surat baru, agar saya dapat mendefinisikan format surat perjanjian sesuai kebutuhan.

#### Acceptance Criteria

1. WHEN admin yang terautentikasi mengakses `GET /template/create`, THE Template_Manager SHALL menampilkan form pembuatan template dengan field `Name` dan `Content`.
2. WHEN admin mengirimkan form dengan `Name` dan `Content` yang valid, THE Template_Manager SHALL menyimpan template baru ke database, menetapkan `CreatedAt` dan `UpdatedAt` ke waktu saat ini, dan mengarahkan admin ke `/template` dengan pesan sukses `"Template berhasil dibuat."` melalui `TempData["Success"]`.
3. IF `Name` kosong, hanya terdiri dari karakter whitespace, atau melebihi 255 karakter, THEN THE Template_Manager SHALL menampilkan pesan validasi pada field `Name` tanpa menyimpan data, dan menampilkan kembali form dengan nilai yang telah dimasukkan sebelumnya.
4. IF `Content` kosong, THEN THE Template_Manager SHALL menampilkan pesan validasi pada field `Content` tanpa menyimpan data, dan menampilkan kembali form dengan nilai yang telah dimasukkan sebelumnya.

---

### Requirement 4: Manajemen Template — Mengedit Template

**User Story:** Sebagai admin, saya ingin mengedit template yang sudah ada, agar saya dapat memperbaiki atau memperbarui konten surat.

#### Acceptance Criteria

1. WHEN admin yang terautentikasi mengakses `GET /template/edit/{id}`, THE Template_Manager SHALL mengambil template dengan `id` yang sesuai dari database dan menampilkan form edit dengan field `Name` dan `Content` sudah terisi dengan data template yang ada.
2. WHEN admin mengirimkan form edit dengan `Name` dan `Content` yang valid, THE Template_Manager SHALL memperbarui template di database, memperbarui `UpdatedAt` ke waktu saat ini, dan mengarahkan admin ke `/template` dengan pesan sukses `"Template berhasil diperbarui."` melalui `TempData["Success"]`.
3. IF `Name` kosong, hanya terdiri dari karakter whitespace, atau melebihi 255 karakter pada form edit, THEN THE Template_Manager SHALL menampilkan pesan validasi pada field `Name` tanpa menyimpan perubahan, dan menampilkan kembali form edit dengan nilai yang telah dimasukkan.
4. IF `Content` kosong pada form edit, THEN THE Template_Manager SHALL menampilkan pesan validasi pada field `Content` tanpa menyimpan perubahan, dan menampilkan kembali form edit dengan nilai yang telah dimasukkan.
5. IF template dengan `id` yang diminta tidak ditemukan di database pada `GET /template/edit/{id}` maupun `POST /template/edit/{id}`, THEN THE Template_Manager SHALL mengembalikan respons HTTP 404.

---

### Requirement 5: Manajemen Template — Menghapus Template

**User Story:** Sebagai admin, saya ingin menghapus template yang sudah tidak digunakan, agar daftar template tetap bersih.

#### Acceptance Criteria

1. WHEN admin yang terautentikasi mengirimkan `POST /template/delete/{id}`, THE Template_Manager SHALL menghapus template dengan `id` yang sesuai secara permanen dari database dan mengarahkan admin ke `/template` dengan pesan `TempData["Success"]` berisi `"Template berhasil dihapus."`.
2. IF template dengan `id` yang diminta tidak ditemukan di database, THEN THE Template_Manager SHALL mengembalikan respons HTTP 404.
3. IF template yang akan dihapus masih direferensikan oleh satu atau lebih `Document` di database, THEN THE Template_Manager SHALL membatalkan penghapusan dan mengarahkan admin ke `/template` dengan pesan `TempData["Error"]` berisi `"Template tidak dapat dihapus karena masih digunakan oleh dokumen yang ada."`.

---

### Requirement 6: Pembuatan Dokumen — Form Input

**User Story:** Sebagai visitor, saya ingin mengisi form data penjual, pembeli, dan kendaraan, agar saya dapat menghasilkan surat perjanjian jual beli yang sesuai.

#### Acceptance Criteria

1. WHEN visitor mengakses `GET /document/create`, THE Document_Generator SHALL menampilkan form dengan semua field data penjual, pembeli, kendaraan, harga, dan tanggal perjanjian.
2. THE Document_Generator SHALL memvalidasi `NamaPenjual` sebagai string wajib dengan panjang maksimal 255 karakter.
3. THE Document_Generator SHALL memvalidasi `NikPenjual` sebagai string wajib dengan tepat 16 karakter digit.
4. THE Document_Generator SHALL memvalidasi `AlamatPenjual` sebagai string wajib dengan panjang maksimal 500 karakter.
5. THE Document_Generator SHALL memvalidasi `NamaPembeli` sebagai string wajib dengan panjang maksimal 255 karakter.
6. THE Document_Generator SHALL memvalidasi `NikPembeli` sebagai string wajib dengan tepat 16 karakter digit.
7. THE Document_Generator SHALL memvalidasi `AlamatPembeli` sebagai string wajib dengan panjang maksimal 500 karakter.
8. THE Document_Generator SHALL memvalidasi `MerkMotor` sebagai string wajib dengan panjang maksimal 100 karakter.
9. THE Document_Generator SHALL memvalidasi `TipeMotor` sebagai string wajib dengan panjang maksimal 100 karakter.
10. THE Document_Generator SHALL memvalidasi `TahunMotor` sebagai integer wajib dalam rentang 1900 hingga tahun kalender saat ini.
11. THE Document_Generator SHALL memvalidasi `WarnaMotor` sebagai string wajib dengan panjang maksimal 50 karakter.
12. THE Document_Generator SHALL memvalidasi `NomorPolisi` sebagai string wajib dengan panjang maksimal 20 karakter.
13. THE Document_Generator SHALL memvalidasi `NomorBPKB` sebagai string wajib dengan panjang maksimal 50 karakter.
14. THE Document_Generator SHALL memvalidasi `NomorSTNK` sebagai string wajib dengan panjang maksimal 50 karakter.
15. THE Document_Generator SHALL memvalidasi `Harga` sebagai decimal wajib dengan nilai minimal 1 dan maksimal 999.999.999.999,99.
16. THE Document_Generator SHALL memvalidasi `TanggalPerjanjian` sebagai tanggal wajib yang tidak lebih awal dari 1 Januari 1900 dan tidak melebihi satu tahun ke depan dari tanggal saat ini.
17. IF ada field yang tidak memenuhi validasi, THEN THE Document_Generator SHALL menampilkan pesan validasi per field, menampilkan kembali form dengan semua nilai yang telah dimasukkan sebelumnya, dan tidak menyimpan data apapun ke database.
18. IF `NikPenjual` atau `NikPembeli` tidak terdiri dari tepat 16 digit angka, THEN THE Document_Generator SHALL menampilkan pesan `"NIK harus 16 digit angka"` pada field yang bersangkutan.

---

### Requirement 7: Pembuatan Dokumen — Proses Generate

**User Story:** Sebagai visitor, saya ingin data yang saya isi secara otomatis menggantikan placeholder dalam template, agar dokumen yang dihasilkan berisi informasi yang benar.

#### Acceptance Criteria

1. WHEN visitor mengirimkan form dengan semua data valid, THE Document_Generator SHALL mengambil template pertama yang tersedia dari database (diurutkan berdasarkan `Id` ascending).
2. WHEN template berhasil diambil, THE Document_Generator SHALL menjalankan proses Auto-Fill dengan mengganti semua placeholder `{{NamaField}}` dalam `Content` template dengan nilai yang sesuai dari data form, memformat `Harga` sebagai `Rp X.XXX.XXX` menggunakan format mata uang `id-ID` tanpa desimal, memformat `TanggalPerjanjian` sebagai `dd MMMM yyyy` menggunakan locale `id-ID`, dan menghasilkan `NomorSurat` dengan format `{NoUrut:D3}/LEGALY/JBM/{MM}/{YYYY}`.
3. THE Document_Generator SHALL menghasilkan `NomorSurat` berdasarkan jumlah dokumen yang sudah ada pada bulan dan tahun yang sama dengan `TanggalPerjanjian`, ditambah 1, diformat sebagai 3 digit dengan leading zero.
4. THE Document_Generator SHALL menyimpan rekaman `Document` baru ke database dengan seluruh data form, `NomorSurat` yang dihasilkan, dan `GeneratedContent` hasil Auto-Fill.
5. WHEN dokumen berhasil disimpan, THE Document_Generator SHALL mengarahkan pengguna ke `/document/preview/{id}` dokumen yang baru dibuat.
6. IF tidak ada template tersedia di database saat proses generate, THEN THE Document_Generator SHALL menampilkan halaman error dengan pesan `"Template tidak tersedia. Hubungi administrator."`.
7. IF terjadi error saat menyimpan dokumen ke database, THEN THE Document_Generator SHALL menampilkan halaman error yang informatif dan tidak menyimpan data parsial.
8. IF dua permintaan generate dokumen terjadi secara bersamaan pada bulan dan tahun yang sama, THEN THE Document_Generator SHALL memastikan `NomorSurat` yang dihasilkan tetap unik tanpa duplikasi.

---

### Requirement 8: Pratinjau Dokumen

**User Story:** Sebagai visitor, saya ingin melihat pratinjau dokumen yang sudah dihasilkan, agar saya dapat memverifikasi konten sebelum mengunduh PDF.

#### Acceptance Criteria

1. WHEN visitor mengakses `GET /document/preview/{id}`, THE Document_Generator SHALL mengambil dokumen dengan `id` yang sesuai dari database dan menampilkan halaman pratinjau.
2. WHEN halaman pratinjau berhasil dimuat, THE Document_Generator SHALL menampilkan `NomorSurat` dan `GeneratedContent` lengkap, di mana tidak ada token `{{...}}` yang tersisa dalam konten yang ditampilkan.
3. WHEN halaman pratinjau berhasil dimuat, THE Document_Generator SHALL menampilkan tombol "Unduh PDF" yang tertaut ke `/document/download/{id}`.
4. IF dokumen dengan `id` yang diminta tidak ditemukan di database, THEN THE Document_Generator SHALL mengembalikan respons HTTP 404.

---

### Requirement 9: Ekspor PDF

**User Story:** Sebagai visitor, saya ingin mengunduh dokumen dalam format PDF, agar saya dapat mencetak atau menyimpan surat perjanjian secara resmi.

#### Acceptance Criteria

1. WHEN visitor mengakses `GET /document/download/{id}`, THE PDF_Exporter SHALL mengambil dokumen dari database dan menghasilkan file PDF menggunakan QuestPDF.
2. THE PDF_Exporter SHALL menghasilkan PDF dengan ukuran halaman A4, margin 2 cm di semua sisi, dan ukuran font default 12pt dengan font family Arial.
3. THE PDF_Exporter SHALL menyertakan `NomorSurat` yang dicetak tebal dan rata tengah di bagian atas dokumen PDF.
4. THE PDF_Exporter SHALL menyertakan `GeneratedContent` sebagai isi utama dokumen PDF dengan whitespace dan line break yang dipertahankan sesuai konten aslinya.
5. THE PDF_Exporter SHALL mengembalikan file PDF dengan header `Content-Type: application/pdf`, header `Content-Disposition: attachment`, dan nama file `Surat-Perjanjian-Jual-Beli-{NomorPolisi}-{id}.pdf` di mana spasi pada `NomorPolisi` diganti tanda hubung dan karakter non-alfanumerik dihapus.
6. IF dokumen dengan `id` yang diminta tidak ditemukan di database, THEN THE PDF_Exporter SHALL mengembalikan respons HTTP 404.
7. THE PDF_Exporter SHALL menghasilkan PDF dalam waktu kurang dari 5 detik untuk konten hingga 2 halaman A4.
8. IF terjadi error saat menghasilkan PDF, THEN THE PDF_Exporter SHALL menampilkan halaman error yang informatif tanpa mengekspos detail teknis kepada pengguna.

---

### Requirement 10: Seed Data dan Inisialisasi Sistem

**User Story:** Sebagai admin, saya ingin sistem sudah memiliki akun admin default dan template default saat pertama kali dijalankan, agar aplikasi langsung dapat digunakan tanpa konfigurasi manual.

#### Acceptance Criteria

1. WHEN aplikasi dijalankan dan tabel `Admins` kosong, THE System SHALL menyisipkan satu admin default dengan email `admin@legaly.id` dan password `Admin@123` yang di-hash menggunakan BCrypt dengan cost factor 12. Pada jalankan berikutnya ketika tabel tidak kosong, THE System SHALL melewati proses seed ini.
2. WHEN aplikasi dijalankan dan tabel `Templates` kosong, THE System SHALL menyisipkan satu template default dengan `Name` = `"Surat Perjanjian Jual Beli Kendaraan Bermotor"` yang berisi teks surat lengkap dengan seluruh placeholder standar: `{{NamaPenjual}}`, `{{NikPenjual}}`, `{{AlamatPenjual}}`, `{{NamaPembeli}}`, `{{NikPembeli}}`, `{{AlamatPembeli}}`, `{{MerkMotor}}`, `{{TipeMotor}}`, `{{TahunMotor}}`, `{{WarnaMotor}}`, `{{NomorPolisi}}`, `{{NomorBPKB}}`, `{{NomorSTNK}}`, `{{Harga}}`, `{{TanggalPerjanjian}}`, dan `{{NomorSurat}}`. Pada jalankan berikutnya ketika tabel tidak kosong, THE System SHALL melewati proses seed ini.
3. IF terjadi error saat proses seed data (misalnya database tidak tersedia), THEN THE System SHALL menghentikan startup dan menampilkan pesan error yang informatif di log aplikasi.

---

### Requirement 11: Notifikasi dan Umpan Balik Pengguna

**User Story:** Sebagai admin, saya ingin melihat pesan konfirmasi setelah berhasil melakukan operasi, agar saya dapat mengetahui bahwa aksi saya berhasil dilakukan.

#### Acceptance Criteria

1. WHEN operasi create template berhasil, THE System SHALL menampilkan pesan `"Template berhasil dibuat."` sebagai Bootstrap alert dismissible di halaman `/template`.
2. WHEN operasi edit template berhasil, THE System SHALL menampilkan pesan `"Template berhasil diperbarui."` sebagai Bootstrap alert dismissible di halaman `/template`.
3. WHEN operasi delete template berhasil, THE System SHALL menampilkan pesan `"Template berhasil dihapus."` sebagai Bootstrap alert dismissible di halaman `/template`.
4. IF ada field pada form template atau form dokumen yang tidak memenuhi validasi, THEN THE System SHALL menampilkan pesan validasi per field secara langsung di bawah field yang bermasalah dan menampilkan kembali form yang sama tanpa menyimpan data.

---

### Requirement 12: Antarmuka Pengguna Responsif

**User Story:** Sebagai pengguna, saya ingin antarmuka aplikasi dapat diakses dengan baik di perangkat mobile maupun desktop, agar saya dapat menggunakan aplikasi dari berbagai perangkat.

#### Acceptance Criteria

1. THE System SHALL menampilkan antarmuka tanpa horizontal overflow dan dengan semua konten serta elemen interaktif dapat dioperasikan pada viewport dengan lebar minimal 360px.
2. THE System SHALL menampilkan antarmuka tanpa horizontal overflow dan dengan semua konten serta elemen interaktif dapat dioperasikan pada viewport dengan lebar 1280px.
3. THE System SHALL menggunakan Bootstrap 5 grid system sehingga tata letak menyesuaikan secara fluid pada semua lebar viewport antara 360px dan 1280px tanpa horizontal overflow.

---

### Requirement 13: Performa dan Keandalan Sistem

**User Story:** Sebagai pengguna, saya ingin aplikasi merespons dengan cepat, agar saya tidak perlu menunggu lama untuk mengakses atau menghasilkan dokumen.

#### Acceptance Criteria

1. THE System SHALL memuat setiap halaman (Time to First Byte + full HTML response) dalam waktu kurang dari 3 detik pada kondisi operasi normal dengan koneksi lokal atau LAN.
2. THE PDF_Exporter SHALL menyelesaikan pembuatan dan pengiriman file PDF dalam waktu kurang dari 5 detik diukur dari penerimaan request hingga respons lengkap diterima klien.
3. THE System SHALL menggunakan encoding UTF-8 untuk semua response HTTP dan konten database, sehingga karakter Indonesia (seperti é, â, ñ) tampil dengan benar di browser dan PDF.
4. WHEN terjadi exception yang tidak tertangani (unhandled exception) di lapisan controller atau service, THE System SHALL menampilkan halaman error HTTP yang informatif tanpa mengekspos stack trace, nama exception, atau detail teknis lainnya kepada pengguna.
