TITLE PROJECT: SocialMediaSentimentApp (C# WPF Desktop App)

Tujuan:
- Aplikasi desktop modern untuk CRUD data post, analisis sentimen, chart, dan report (HTML + MD)
- Tidak perlu CLI argumen (--demo / --input). Semua lewat UI.

Cara menjalankan (Windows PowerShell):

1) Restore & build:
   dotnet restore
   dotnet build

2) Run app:
   dotnet run --project .\SocialMediaSentimentApp\SocialMediaSentimentApp.csproj

Cara pakai cepat:
- Klik "Load Demo Data"
- Klik "Run Analysis"
- Klik "Generate Report" (lihat di tab Report Preview)

Data disimpan otomatis di:
%LOCALAPPDATA%\SocialMediaSentimentApp\
- posts.json
- settings.json
- outputs\predictions.csv dan summary.json
- reports\social-media-sentiment-analysis-results.html dan .md
- reports\assets\*.png

Catatan:
- TargetFramework net8.0-windows (WPF).
