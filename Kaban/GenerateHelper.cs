using System.Text.Json.Nodes;
using Kaban.Models;
using Path = System.IO.Path;

namespace Kaban;

public static class GenerateHelper
{
    public static async Task<List<Board>> GenerateDefaultKabanBoards()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default.json");
        var jsonString = await File.ReadAllTextAsync(filePath);
        var node = JsonNode.Parse(jsonString)!;

        var boards = new List<Board>();
        foreach (var boardNode in node["boards"]!.AsArray())
        {
            var board = new Board()
            {
                Name = boardNode["name"].ToString()
            };
            foreach (var columnNode in boardNode["columns"].AsArray())
            {
                var column = new Column()
                {
                    Name = columnNode["name"].ToString()
                };
                foreach (var taskNode in columnNode["tasks"].AsArray())
                {
                    var mainTask = new MainTask()
                    {
                        Title = taskNode["title"].ToString(),
                        Description = taskNode["description"].ToString(),
                    };
                    foreach (var subTaskNode in taskNode["subtasks"].AsArray())
                    {
                        var subTask = new SubTask()
                        {
                            Title = subTaskNode["title"].ToString(),
                            IsCompleted = subTaskNode["isCompleted"].GetValue<bool>()
                        };
                        mainTask.SubTasks.Add(subTask);
                    }

                    column.MainTasks.Add(mainTask);
                }

                board.Columns.Add(column);
            }

            boards.Add(board);
        }

        return boards;
    }
}