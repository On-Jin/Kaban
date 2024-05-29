using Kaban.Data;
using Kaban.GraphQL;
using Kaban.GraphQL.Boards;
using Kaban.GraphQL.Columns;
using Kaban.GraphQL.MainTasks;
using Kaban.GraphQL.SubTasks;
using Kaban.Models;
using Kaban.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Kaban.Mutations;

public class Mutation
{
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

        return new BoardPayload(newBoard);
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

        return new BoardPayload(board);
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

        return new BoardPayload(board);
    }

    #endregion

    #region Column

    [Authorize]
    public async Task<BoardPayload> AddColumn(
        AddColumnInput input,
        [Service] AppDbContext db,
        [Service] IHttpContextAccessor httpContext,
        [Service] IUserService userService,
        CancellationToken cancellationToken)
    {
        var user = (await userService.Find(httpContext.HttpContext.User.Identity.Name))!;

        var board = db.Boards.Include(b => b.User).SingleOrDefault(b => b.Id == input.BoardId && b.UserId == user.Id);


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
            Name = input.Name
        };
        board.Columns.Add(newColumn);

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(board);
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

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(column.Board);
    }

    [Authorize]
    public async Task<BoardPayload> DeleteColumn(
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

        return new BoardPayload(column.Board);
    }

    #endregion

    #region MainTask

    [Authorize]
    public async Task<BoardPayload> AddMainTask(
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
            Status = TaskState.Todo,
        };
        column.MainTasks.Add(newMainTask);

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(column.Board);
    }

    [Authorize]
    public async Task<BoardPayload> PatchMainTask(
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
        if (input.Status.HasValue)
            mainTask.Status = input.Status.Value;

        await db.SaveChangesAsync(cancellationToken);

        return new BoardPayload(mainTask.Column.Board);
    }

    [Authorize]
    public async Task<BoardPayload> DeleteMainTask(
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

        return new BoardPayload(mainTask.Column.Board);
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

        return new BoardPayload(mainTask.Column.Board);
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

        return new BoardPayload(subTask.MainTask.Column.Board);
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

        return new BoardPayload(subTask.MainTask.Column.Board);
    }

    #endregion
}