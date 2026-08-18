using System;

namespace Compass.Modules.Calendar.Contracts.Queries;

public record AvailabilityWindow(DateTimeOffset Start, DateTimeOffset End);
