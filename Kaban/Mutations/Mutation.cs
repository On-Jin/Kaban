using Kaban.Data;
using Kaban.GraphQL;
using Kaban.GraphQL.Boards;
using Kaban.GraphQL.Columns;
using Kaban.GraphQL.MainTasks;
using Kaban.GraphQL.Payloads;
using Kaban.GraphQL.SubTasks;
using Kaban.Models;
using Kaban.Models.Dto;
using Kaban.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kaban.Mutations;

public class Mutation
{
    private void ProcessOrderInput<T>(int orderInput, T activeObject, List<T> listObjects) where T : class, IOrdered
    {
        if (orderInput < 0 || orderInput >= listObjects.Count)
        {
            throw new GraphQLException(
                new Error($"Index {orderInput} out of scope.", ErrorCode.WrongInput));
        }

        if (orderInput != activeObject.Order)
        {
            listObjects.Sort((a, b) => a.Order - b.Order);
            listObjects.Remove(activeObject);
            listObjects.Insert(orderInput, activeObject);

            for (var i = 0; i < listObjects.Count; i++)
            {
                listObjects[i].Order = i;
            }
        }
    }

    #region Board

    [Authorize]
    public async Task<BoardsPayload> PopulateMe(
        AddBoardInput input,
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var boards = await GenerateHelper.GenerateDefaultKabanBoards();
        user.Boards.AddRange(boards);

        await context.SaveChangesAsync(cancellationToken);

        return new BoardsPayload(boards);
    }

    [Authorize]
    public async Task<BoardPayload> AddBoard(
        AddBoardInput input,
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var newBoard = new Board()
        {
            Name = input.Name
        };
        user.Boards.Add(newBoard);

        await context.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(newBoard));
    }

    [Authorize]
    public async Task<BoardPayload> PatchBoard(
        PatchBoardInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var board = db.Boards.Include(b => b.User).SingleOrDefault(b => b.Id == input.Id && b.UserId == user.Id);

        if (board == null)
            throw new GraphQLException(new Error("Board not found.", ErrorCode.NotFound));

        if (input.Name != null)
            board.Name = input.Name!;

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(board));
    }

    [Authorize]
    public async Task<BoardPayload> DeleteBoard(
        DeleteBoardInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var board = db.Boards.Include(b => b.User).Single(b => b.Id == input.Id && b.UserId == user.Id);

        if (board == null)
            throw new GraphQLException(new Error("Board not found.", ErrorCode.NotFound));

        user.Boards.Remove(board);

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(board));
    }

    #endregion

    #region Column

    [Authorize]
    public async Task<ColumnPayload> AddColumn(
        AddColumnInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var board = db.Boards.Include(b => b.User).Include(board => board.Columns)
            .SingleOrDefault(b => b.Id == input.BoardId && b.UserId == user.Id);


        if (board == null)
        {
            throw new GraphQLException(new Error($"Board {input.BoardId} not found.", ErrorCode.NotFound));
        }

        if (board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }


        var newColumn = new Column()
        {
            Name = input.Name,
            Order = board.Columns.Count,
        };
        board.Columns.Add(newColumn);

        await db.SaveChangesAsync(cancellationToken);

        return new ColumnPayload(Mapper.MapToColumnDto(newColumn));
    }

    [Authorize]
    public async Task<BoardPayload> PatchColumn(
        PatchColumnInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var column = db.Columns
            .Include(column => column.Board)
            .ThenInclude(board => board.User)
            .Include(column => column.Board)
            .ThenInclude(board => board.Columns)
            .ThenInclude(column => column.MainTasks)
            .ThenInclude(mainTask => mainTask.SubTasks)
            .SingleOrDefault(c => c.Id == input.Id);

        if (column == null)
        {
            throw new GraphQLException(new Error($"Column {input.Id} not found.", ErrorCode.NotFound));
        }

        if (column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        if (input.Name != null)
        {
            column.Name = input.Name;
        }

        if (input.Order.HasValue)
        {
            ProcessOrderInput(input.Order.Value, column, column.Board.Columns);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(column.Board));
    }

    [Authorize]
    public async Task<ColumnPayload> DeleteColumn(
        DeleteColumnInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var column = db.Columns
            .Include(column => column.Board)
            .ThenInclude(board => board.User)
            .SingleOrDefault(c => c.Id == input.Id);

        if (column == null)
        {
            throw new GraphQLException(new Error($"Column {input.Id} not found.", ErrorCode.NotFound));
        }

        if (column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        db.Columns.Remove(column);

        await db.SaveChangesAsync(cancellationToken);

        return new ColumnPayload(Mapper.MapToColumnDto(column));
    }

    #endregion

    #region MainTask

    [Authorize]
    public async Task<MainTaskPayload> AddMainTask(
        AddMainTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var column = db.Columns
            .Include(column => column.Board)
            .ThenInclude(board => board.User)
            .Include(column => column.MainTasks)
            .SingleOrDefault(b => b.Id == input.ColumnId);

        if (column == null)
        {
            throw new GraphQLException(new Error($"Column {input.ColumnId} not found.", ErrorCode.NotFound));
        }

        if (column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        var newMainTask = new MainTask
        {
            Title = input.Title,
            Description = input.Description ?? "",
            Order = column.MainTasks.Count
        };
        column.MainTasks.Add(newMainTask);

        await db.SaveChangesAsync(cancellationToken);

        return new MainTaskPayload(Mapper.MapToMainTaskDto(newMainTask));
    }

    [Authorize]
    public async Task<MainTaskPayload> PatchMainTask(
        PatchMainTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var mainTask = db.MainTasks
            .Include(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.User)
            .SingleOrDefault(mainTask => mainTask.Id == input.Id);

        if (mainTask == null)
        {
            throw new GraphQLException(new Error($"MainTask {input.Id} not found.", ErrorCode.NotFound));
        }

        if (mainTask.Column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        if (input.Title != null)
            mainTask.Title = input.Title;
        if (input.Description != null)
            mainTask.Description = input.Description;

        await db.SaveChangesAsync(cancellationToken);

        return new MainTaskPayload(Mapper.MapToMainTaskDto(mainTask));
    }

    [Authorize]
    public async Task<BoardPayload> MoveMainTask(
        MoveMainTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var mainTask = db.MainTasks
            .Include(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.User)
            .Include(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.Columns)
            .ThenInclude(column => column.MainTasks)
            .ThenInclude(mainTask => mainTask.SubTasks)
            .Include(mainTask => mainTask.Column)
            .ThenInclude(column => column.MainTasks)
            .Include(mainTask => mainTask.SubTasks)
            .SingleOrDefault(mainTask => mainTask.Id == input.Id);

        if (mainTask == null)
        {
            throw new GraphQLException(new Error($"MainTask {input.Id} not found.", ErrorCode.NotFound));
        }

        if (mainTask.Column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        var fromColumn = mainTask.Column;
        if (input.Status != null &&
            !string.Equals(input.Status, mainTask.Column.Name, StringComparison.CurrentCultureIgnoreCase))
        {
            var displaceToColumn = mainTask.Column.Board.Columns.SingleOrDefault(c => c.Name == input.Status);
            if (displaceToColumn == null)
            {
                throw new GraphQLException(new Error($"Status/Column {input.Status} doesnt exist",
                    ErrorCode.NotFound));
            }

            // Remove mainTask from the current collection
            fromColumn.MainTasks.Remove(mainTask);

            // Update the column reference on the main task
            mainTask.ColumnId = displaceToColumn.Id;

            // Add mainTask to the new collection
            displaceToColumn.MainTasks.Add(mainTask);

            // Update the main task in the context
            db.Update(mainTask);

            // Save changes
            await db.SaveChangesAsync(cancellationToken);
            if (input.Order.HasValue)
            {
                ProcessOrderInput(input.Order.Value, mainTask, displaceToColumn.MainTasks);
            }
        }
        else if (input.Order.HasValue)
        {
            ProcessOrderInput(input.Order.Value, mainTask, mainTask.Column.MainTasks);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(mainTask.Column.Board));
    }

    [Authorize]
    public async Task<MainTaskPayload> DeleteMainTask(
        DeleteMainTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var mainTask = db.MainTasks
            .Include(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.User)
            .SingleOrDefault(mainTask => mainTask.Id == input.Id);

        if (mainTask == null)
        {
            throw new GraphQLException(new Error($"MainTask {input.Id} not found.", ErrorCode.NotFound));
        }

        if (mainTask.Column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        db.MainTasks.Remove(mainTask);

        await db.SaveChangesAsync(cancellationToken);

        return new MainTaskPayload(Mapper.MapToMainTaskDto(mainTask));
    }

    #endregion

    #region SubTask

    [Authorize]
    public async Task<BoardPayload> AddSubTask(
        AddSubTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var mainTask = db.MainTasks
            .Include(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.User)
            .SingleOrDefault(mainTask => mainTask.Id == input.MainTaskId);

        if (mainTask == null)
        {
            throw new GraphQLException(new Error($"MainTask {input.MainTaskId} not found.", ErrorCode.NotFound));
        }

        if (mainTask.Column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        var newSubTask = new SubTask
        {
            Title = input.Title,
            IsCompleted = false,
        };
        mainTask.SubTasks.Add(newSubTask);

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(mainTask.Column.Board));
    }

    [Authorize]
    public async Task<BoardPayload> PatchSubTask(
        PatchSubTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var subTask = db.SubTasks
            .Include(subTask => subTask.MainTask)
            .ThenInclude(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.User)
            .SingleOrDefault(mainTask => mainTask.Id == input.Id);

        if (subTask == null)
        {
            throw new GraphQLException(new Error($"SubTask {input.Id} not found.", ErrorCode.NotFound));
        }

        if (subTask.MainTask.Column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        if (input.Title != null)
            subTask.Title = input.Title;
        if (input.IsCompleted.HasValue)
            subTask.IsCompleted = input.IsCompleted.Value;

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(subTask.MainTask.Column.Board));
    }

    [Authorize]
    public async Task<BoardPayload> DeleteSubTask(
        DeleteSubTaskInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var subTask = db.SubTasks
            .Include(subTask => subTask.MainTask)
            .ThenInclude(mainTask => mainTask.Column)
            .ThenInclude(column => column.Board)
            .ThenInclude(board => board.User)
            .SingleOrDefault(mainTask => mainTask.Id == input.Id);

        if (subTask == null)
        {
            throw new GraphQLException(new Error($"SubTask {input.Id} not found.", ErrorCode.NotFound));
        }

        if (subTask.MainTask.Column.Board.User.Id != user.Id)
        {
            throw new GraphQLException(new Error("Unauthorized.", ErrorCode.Unauthorized));
        }

        db.SubTasks.Remove(subTask);

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(Mapper.MapToBoardDto(subTask.MainTask.Column.Board));
    }

    #endregion
}