using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly AppDbContext _context;

    public ToDoService(AppDbContext context)
    {
        _context = context;
    }

    public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext callContext)
    {
        if (request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid argument"));

        var toDoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description
        };

        await _context.AddAsync(toDoItem);
        await _context.SaveChangesAsync();

        return await Task.FromResult(new CreateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request, ServerCallContext callContext)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resource index must be greater than 0"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(p => p.Id == request.Id);

        if (toDoItem != null)
            return await Task.FromResult(new ReadToDoResponse
            {
                Id = toDoItem.Id,
                Title = toDoItem.Title,
                Description = toDoItem.Description,
                ToDoStatus = toDoItem.ToDoStatus
            });

        throw new RpcException(new Status(StatusCode.NotFound, $"No Task with id {request.Id}"));
    }

    public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext callContext)
    {
        var response = new GetAllResponse();

        var todoItems = await _context.ToDoItems.ToListAsync();

        foreach (var item in todoItems)
        {
            response.ToDo.Add(new ReadToDoResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                ToDoStatus = item.ToDoStatus
            });
        }

        return await Task.FromResult(response);
    }

    public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext callContext)
    {
        if (request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(p => p.Id == request.Id);

        if (toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"no item with id {request.Id}"));

        toDoItem.Title = request.Title;
        toDoItem.Description = request.Description;
        toDoItem.ToDoStatus = request.ToDoStatus;

        await _context.SaveChangesAsync();

        return await Task.FromResult(new UpdateToDoResponse
        {
            Id = toDoItem.Id
        });
    }

    public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToToRequest request, ServerCallContext callContext)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "resource index must be greather than 0"));

        var toDoItem = await _context.ToDoItems.FirstOrDefaultAsync(p => p.Id == request.Id);

        if (toDoItem == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"No Item with {request.Id}"));

        _context.Remove(toDoItem);
        await _context.SaveChangesAsync();

        return await Task.FromResult(new DeleteToDoResponse
        {
            Id = toDoItem.Id
        });
    }
}