param(
    [string]$OldName = "AxleLoadSystem",
    [string]$NewName = "DSRSystem",
    [string]$SolutionDir = "."
)

Write-Host "🔄 Starting Blazor project rename..." -ForegroundColor Cyan

# Paths
$OldProjectPath = Join-Path $SolutionDir $OldName
$NewProjectPath = Join-Path $SolutionDir $NewName
$OldCsproj = Join-Path $OldProjectPath "$OldName.csproj"
$NewCsproj = Join-Path $OldProjectPath "$NewName.csproj"
$SolutionFile = Get-ChildItem $SolutionDir -Filter *.sln | Select-Object -First 1

if (-not $SolutionFile) {
    Write-Host "❌ No solution (.sln) file found!" -ForegroundColor Red
    exit
}

Write-Host "✔ Solution file: $($SolutionFile.Name)"

# STEP 1: Rename folder
Write-Host "📁 Renaming project folder..."
Rename-Item -Path $OldProjectPath -NewName $NewName -Force

# STEP 2: Rename csproj
$FinalCsproj = Join-Path $NewProjectPath "$NewName.csproj"

Write-Host "📄 Renaming csproj file..."
Rename-Item -Path $OldCsproj -NewName "$NewName.csproj" -Force

# STEP 3: Fix solution file (.sln)
Write-Host "🛠 Updating .sln references..."
(Get-Content $SolutionFile.FullName) `
    -replace $OldName, $NewName `
    -replace "$OldName\\$OldName.csproj", "$NewName\\$NewName.csproj" |
    Set-Content $SolutionFile.FullName

# STEP 4: Update AssemblyName & RootNamespace in csproj
Write-Host "🛠 Updating AssemblyName & RootNamespace in csproj..."
$csprojContent = Get-Content $FinalCsproj

if ($csprojContent -notmatch "<AssemblyName>") {
    $csprojContent = $csprojContent -replace "</PropertyGroup>", "  <AssemblyName>$NewName</AssemblyName>`n  <RootNamespace>$NewName</RootNamespace>`n</PropertyGroup>"
} else {
    $csprojContent = $csprojContent -replace "<AssemblyName>.*?</AssemblyName>", "<AssemblyName>$NewName</AssemblyName>"
    $csprojContent = $csprojContent -replace "<RootNamespace>.*?</RootNamespace>", "<RootNamespace>$NewName</RootNamespace>"
}

$csprojContent | Set-Content $FinalCsproj

# STEP 5: Update launchSettings.json
$launchSettings = "$NewProjectPath\Properties\launchSettings.json"
if (Test-Path $launchSettings) {
    Write-Host "⚙ Updating launchSettings.json..."
    (Get-Content $launchSettings) `
        -replace $OldName, $NewName |
        Set-Content $launchSettings
}

# STEP 6: Replace namespace in ALL .cs, .razor, .cshtml, .razor.cs files
Write-Host "🔍 Replacing namespaces in code files..."
$extensions = "*.cs","*.razor","*.cshtml"
$files = Get-ChildItem -Path $NewProjectPath -Recurse -Include $extensions

foreach ($file in $files) {
    (Get-Content $file.FullName) `
        -replace $OldName, $NewName |
        Set-Content $file.FullName
}

Write-Host "🎉 Project rename completed successfully!" -ForegroundColor Green
Write-Host "➡ New Project: $NewName"
