@echo off
chcp 65001 >nul
title FitApp Baslatici

echo ============================================
echo            FitApp baslatiliyor
echo ============================================
echo.
echo  1) Backend (API) aciliyor...
start "FitApp Backend" /D "%~dp0backend" cmd /k "dotnet run --project src/FitApp.Api"

echo  2) Web uygulamasi aciliyor (ilk derleme biraz surer)...
start "FitApp Web" /D "%~dp0mobile\fitapp" cmd /k "flutter run -d web-server --web-port 8080"

echo.
echo  Tarayici derleme bitince acilacak.
echo  Acilmazsa / bos gorunurse sayfayi yenile:  http://localhost:8080
echo.
echo  Durdurmak icin acilan iki pencereyi kapat.
echo ============================================

rem Web derlemesi bitene kadar bekleyip tarayiciyi ac
timeout /t 55 /nobreak >nul
start http://localhost:8080

exit
