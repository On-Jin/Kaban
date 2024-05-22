using Kaban.Data;
using Kaban.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaban.GraphQL.Boards;

public class BoardType : ObjectType<Board>
{
    protected override void Configure(IObjectTypeDescriptor<Board> descriptor)
    {
        descriptor.Description("G Description");

        descriptor.Field(board => board.Name).Description("A name !");
    }
}