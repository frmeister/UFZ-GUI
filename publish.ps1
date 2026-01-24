# publish.ps1 - Скрипт сборки и публикации
param(
    [string]$Version = "1.0.0"
)

# 1. Очистка предыдущих сборок
Remove-Item -Path ".\publish" -Recurse -Force -ErrorAction SilentlyContinue

# 2. Публикация проекта
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o ".\publish\UFZapret"

# 3. Копирование дополнительных файлов
Copy-Item "Config.cfg" -Destination ".\publish\UFZapret\" -Force

# 4. Создание версионного файла
$versionContent = $Version
Set-Content -Path ".\publish\UFZapret\.service\version.txt" -Value $versionContent

# 5. Создание ZIP архива
Compress-Archive -Path ".\publish\UFZapret\*" -DestinationPath ".\publish\UFZapret-v$Version.zip" -Force

Write-Host "✅ Сборка завершена: .\publish\UFZapret-v$Version.zip"
Write-Host "📦 Размер: $(Get-Item ".\publish\UFZapret-v$Version.zip").Length байт"