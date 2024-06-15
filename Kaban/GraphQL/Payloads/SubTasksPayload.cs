using Kaban.Models.Dto;

namespace Kaban.GraphQL.Payloads;

public record SubTasksPayload(List<SubTaskDto> SubTask);