Write-Host "Iniciando scaffolding da solução Compass (Clean Architecture)..." -ForegroundColor Cyan

# 1. Criar pasta raiz e entrar nela
New-Item -ItemType Directory -Force -Path "Compass.Backend\src" | Out-Null
Set-Location "Compass.Backend"

# 2. Criar a Solution
dotnet new sln -n Compass

# 3. Criar os Projetos
Write-Host "Criando projetos .NET..." -ForegroundColor Yellow
dotnet new classlib -n Compass.Domain -o src/Compass.Domain
dotnet new classlib -n Compass.Application -o src/Compass.Application
dotnet new classlib -n Compass.Infrastructure -o src/Compass.Infrastructure
dotnet new webapi -n Compass.Api -o src/Compass.Api

# 4. Adicionar projetos à Solution
Write-Host "Adicionando projetos na solução..." -ForegroundColor Yellow
dotnet sln add src/Compass.Domain/Compass.Domain.csproj
dotnet sln add src/Compass.Application/Compass.Application.csproj
dotnet sln add src/Compass.Infrastructure/Compass.Infrastructure.csproj
dotnet sln add src/Compass.Api/Compass.Api.csproj

# 5. Configurar referências (Clean Architecture)
Write-Host "Configurando referências e blindando o Domínio..." -ForegroundColor Yellow
dotnet add src/Compass.Application/Compass.Application.csproj reference src/Compass.Domain/Compass.Domain.csproj
dotnet add src/Compass.Infrastructure/Compass.Infrastructure.csproj reference src/Compass.Application/Compass.Application.csproj
dotnet add src/Compass.Infrastructure/Compass.Infrastructure.csproj reference src/Compass.Domain/Compass.Domain.csproj
dotnet add src/Compass.Api/Compass.Api.csproj reference src/Compass.Application/Compass.Application.csproj
dotnet add src/Compass.Api/Compass.Api.csproj reference src/Compass.Infrastructure/Compass.Infrastructure.csproj

# 6. Limpar arquivos padrão do template
Write-Host "Limpando arquivos padrão do template..." -ForegroundColor Yellow
Remove-Item -Path "src/Compass.Domain/Class1.cs" -ErrorAction SilentlyContinue
Remove-Item -Path "src/Compass.Application/Class1.cs" -ErrorAction SilentlyContinue
Remove-Item -Path "src/Compass.Infrastructure/Class1.cs" -ErrorAction SilentlyContinue
Remove-Item -Path "src/Compass.Api/WeatherForecast.cs" -ErrorAction SilentlyContinue
Remove-Item -Path "src/Compass.Api/Controllers/WeatherForecastController.cs" -ErrorAction SilentlyContinue

# Função auxiliar para criar arquivos em branco
function New-EmptyFile ($path) {
    New-Item -ItemType File -Path $path -Force | Out-Null
}

# 7. Criar estrutura de pastas e arquivos em branco do Domínio
Write-Host "Criando arquivos da Etapa 01 em Compass.Domain..." -ForegroundColor Yellow
$domainDirs = @("Enums", "Exceptions", "Entities", "Interfaces")
foreach ($dir in $domainDirs) {
    New-Item -ItemType Directory -Force -Path "src/Compass.Domain/$dir" | Out-Null
}

@("CommitmentType.cs", "CommitmentStatus.cs", "GoalStatus.cs") | ForEach-Object { New-EmptyFile "src/Compass.Domain/Enums/$_" }
@("DomainException.cs", "EventOverlapException.cs", "InvalidTimeRangeException.cs") | ForEach-Object { New-EmptyFile "src/Compass.Domain/Exceptions/$_" }
@("User.cs", "Setting.cs", "Goal.cs", "Project.cs", "Commitment.cs", "TaskCommitment.cs", "EventCommitment.cs", "HabitCommitment.cs", "NoteCommitment.cs", "Dependency.cs", "Schedule.cs", "Tag.cs", "Reminder.cs", "FocusSession.cs") | ForEach-Object { New-EmptyFile "src/Compass.Domain/Entities/$_" }
@("IDomainEvent.cs", "ICommitmentRepository.cs", "IProjectRepository.cs") | ForEach-Object { New-EmptyFile "src/Compass.Domain/Interfaces/$_" }

# 8. Criar estrutura de pastas e arquivos em branco da Infraestrutura
Write-Host "Criando arquivos da Etapa 01 em Compass.Infrastructure..." -ForegroundColor Yellow
$infraDirs = @("Persistence\Configurations", "Repositories", "Migrations")
foreach ($dir in $infraDirs) {
    New-Item -ItemType Directory -Force -Path "src/Compass.Infrastructure/$dir" | Out-Null
}

New-EmptyFile "src/Compass.Infrastructure/Persistence/CompassDbContext.cs"
@("UserConfiguration.cs", "SettingConfiguration.cs", "GoalConfiguration.cs", "ProjectConfiguration.cs", "CommitmentConfiguration.cs", "DependencyConfiguration.cs", "ScheduleConfiguration.cs", "TagConfiguration.cs", "ReminderConfiguration.cs", "FocusSessionConfiguration.cs") | ForEach-Object { New-EmptyFile "src/Compass.Infrastructure/Persistence/Configurations/$_" }

Write-Host "Scaffolding concluído com sucesso! A estrutura da Etapa 01 está pronta." -ForegroundColor Green