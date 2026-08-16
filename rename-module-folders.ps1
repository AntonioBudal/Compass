# rename-module-folders.ps1
$ErrorActionPreference = "Stop"

Write-Host "Iniciando Padronizacao Estrutural dos Modulos do Compass..." -ForegroundColor Cyan

# 1. Validacoes Iniciais
if (-not ((Test-Path "src/Compass.slnx") -or (Test-Path "Compass.slnx"))) {
    Write-Error "Este script deve ser executado na raiz do repositorio Compass (onde fica a pasta src ou o arquivo .slnx)."
}

# 2. Mapeamento de Renames
$renames = @(
    # Planning
    @{ Old = "src/Modules/Planning/Compass.Modules.Planning"; New = "src/Modules/Planning/Planning" },
    @{ Old = "src/Modules/Planning/Compass.Modules.Planning.Contracts"; New = "src/Modules/Planning/Planning.Contracts" },
    @{ Old = "src/Modules/Planning/Compass.Modules.Planning.Infrastructure"; New = "src/Modules/Planning/Planning.Infrastructure" },
    @{ Old = "src/Modules/Planning/Compass.Modules.Planning.Tests"; New = "src/Modules/Planning/Planning.Tests" },
    @{ Old = "src/Modules/Planning/Compass.Modules.Planning.IntegrationTests"; New = "src/Modules/Planning/Planning.IntegrationTests" },
    
    # Calendar
    @{ Old = "src/Modules/Calendar/Compass.Modules.Calendar"; New = "src/Modules/Calendar/Calendar" },
    @{ Old = "src/Modules/Calendar/Compass.Modules.Calendar.Contracts"; New = "src/Modules/Calendar/Calendar.Contracts" },
    @{ Old = "src/Modules/Calendar/Compass.Modules.Calendar.Infrastructure"; New = "src/Modules/Calendar/Calendar.Infrastructure" },
    @{ Old = "src/Modules/Calendar/Compass.Modules.Calendar.Tests"; New = "src/Modules/Calendar/Calendar.Tests" },
    @{ Old = "src/Modules/Calendar/Compass.Modules.Calendar.IntegrationTests"; New = "src/Modules/Calendar/Calendar.IntegrationTests" },
    
    # Execution
    @{ Old = "src/Modules/Execution/Compass.Modules.Execution"; New = "src/Modules/Execution/Execution" },
    @{ Old = "src/Modules/Execution/Compass.Modules.Execution.Contracts"; New = "src/Modules/Execution/Execution.Contracts" },
    @{ Old = "src/Modules/Execution/Compass.Modules.Execution.Tests"; New = "src/Modules/Execution/Execution.Tests" }
)

# 3. Validacao Pre-Execucao (Impede execucao parcial)
$hasErrors = $false
foreach ($map in $renames) {
    if (-not (Test-Path $map.Old)) {
        Write-Warning "Pasta origem nao encontrada (pode ja ter sido renomeada): $($map.Old)"
    }
    if (Test-Path $map.New) {
        Write-Error "CONFLITO: A pasta destino ja existe: $($map.New)"
        $hasErrors = $true
    }
}
if ($hasErrors) { throw "Falha na validacao pre-execucao. Abortando." }

# 4. Execucao dos Renames no Filesystem
Write-Host "`nRenomeando pastas..." -ForegroundColor Yellow
foreach ($map in $renames) {
    if (Test-Path $map.Old) {
        Rename-Item -Path $map.Old -NewName (Split-Path $map.New -Leaf)
        Write-Host " [OK] $($map.Old) -> $(Split-Path $map.New -Leaf)"
    }
}

# 5. Atualizacao de Referencias (CSPROJ e SLNX)
Write-Host "`nAtualizando Referencias (.csproj e .slnx)..." -ForegroundColor Yellow

$allProjects = @(Get-ChildItem -Path "src" -Filter "*.csproj" -Recurse)
$solutionFile = Get-ChildItem -Path "." -Filter "Compass.slnx" -Recurse | Select-Object -First 1

$filesToUpdate = @($solutionFile) + $allProjects

foreach ($file in $filesToUpdate) {
    if ($null -eq $file) { continue }
    
    $content = Get-Content -Path $file.FullName -Raw
    $modified = $false

    foreach ($map in $renames) {
        $oldName = Split-Path $map.Old -Leaf
        $newName = Split-Path $map.New -Leaf
        
        # O caminho velho no csproj/slnx usa barras invertidas ou normais
        $targetStr1 = "\$oldName\$oldName.csproj"
        $replacement1 = "\$newName\$oldName.csproj"
        
        $targetStr2 = "/$oldName/$oldName.csproj"
        $replacement2 = "/$newName/$oldName.csproj"

        if ($content.Contains($targetStr1)) {
            $content = $content.Replace($targetStr1, $replacement1)
            $modified = $true
        }
        if ($content.Contains($targetStr2)) {
            $content = $content.Replace($targetStr2, $replacement2)
            $modified = $true
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
        Write-Host " [ATUALIZADO] $($file.Name)"
    }
}

Write-Host "`nOperacao concluida. Por favor, execute 'dotnet build Compass.slnx' e 'dotnet test Compass.slnx' para validar." -ForegroundColor Green