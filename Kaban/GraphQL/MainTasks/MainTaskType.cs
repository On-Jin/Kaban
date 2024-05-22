using Kaban.Models;

namespace Kaban.GraphQL.MainTasks;

public class MainTaskType : ObjectType<MainTask>
{
    protected override void Configure(IObjectTypeDescriptor<MainTask> descriptor)
    {
    }
}