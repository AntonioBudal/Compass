using Compass.Domain.Entities;
using Compass.Domain.Enums;

namespace Compass.Domain.Services;

public static class TimeWindowCalculator
{
    public static int CalculateAvailableMinutes(
        DateTime nowUtc,
        string timeZoneId,
        Schedule? todaySchedule,
        IEnumerable<EventCommitment> events)
    {
        // 1. Resolver fuso horário do usuário (Fallback seguro para UTC se inválido)
        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException)
        {
            timeZone = TimeZoneInfo.Utc;
        }

        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timeZone);

        // Filtra apenas eventos ativos (não arquivados)
        var activeEvents = events
            .Where(e => e.Status != CommitmentStatus.Archived)
            .ToList();

        // 2. Verificar se o usuário está ATUALMENTE dentro de um Hard Blocker (Reunião em andamento)
        bool isCurrentlyBlocked = activeEvents.Any(e => e.StartTime <= nowUtc && e.EndTime > nowUtc);
        if (isCurrentlyBlocked)
        {
            return 0; // O usuário está ocupado neste milissegundo
        }

        // 3. Determinar o limite máximo de tempo contínuo (Cutoff)
        DateTime maxWindowEndUtc;

        if (todaySchedule != null && todaySchedule.IsActive)
        {
            // Constrói o DateTime local do fim do turno no dia atual
            var workEndLocal = new DateTime(
                nowLocal.Year, nowLocal.Month, nowLocal.Day,
                todaySchedule.WorkEnd.Hour, todaySchedule.WorkEnd.Minute, todaySchedule.WorkEnd.Second,
                DateTimeKind.Unspecified);

            var workEndUtc = TimeZoneInfo.ConvertTimeToUtc(workEndLocal, timeZone);

            // Se ainda está dentro do horário de trabalho, o limite é o fim do turno (UC-34)
            if (nowUtc < workEndUtc)
            {
                maxWindowEndUtc = workEndUtc;
            }
            else
            {
                // Se já passou do turno útil, o limite natural é o fim do dia local (23:59:59)
                var endOfDayLocal = nowLocal.Date.AddDays(1).AddTicks(-1);
                maxWindowEndUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDayLocal, timeZone);
            }
        }
        else
        {
            // Sem agenda de turno configurada para hoje, o limite é o fim do dia local
            var endOfDayLocal = nowLocal.Date.AddDays(1).AddTicks(-1);
            maxWindowEndUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDayLocal, timeZone);
        }

        // 4. Buscar o PRÓXIMO evento que começa no futuro (UC-31)
        var nextEvent = activeEvents
            .Where(e => e.StartTime > nowUtc)
            .OrderBy(e => e.StartTime)
            .FirstOrDefault();

        // Se houver um evento antes do fim do turno/dia, o evento limita a nossa janela livre
        if (nextEvent != null && nextEvent.StartTime < maxWindowEndUtc)
        {
            maxWindowEndUtc = nextEvent.StartTime;
        }

        // 5. Calcular a diferença em minutos inteiros
        var remainingMinutes = (maxWindowEndUtc - nowUtc).TotalMinutes;

        return remainingMinutes <= 0 ? 0 : (int)Math.Floor(remainingMinutes);
    }
}