using System;
using ProjectStatus = Compass.Modules.Planning.Domain.Projects.ProjectStatus;

namespace Compass.Modules.Planning.Application.Projects.CreateProject;

public record CreateProjectResult(Guid ProjectId, ProjectStatus Status);
