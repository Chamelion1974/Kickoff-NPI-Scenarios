using Microsoft.AspNetCore.SignalR;
using ShopStewardHub.DigitalTwin.Models.DTOs;

namespace ShopStewardHub.DigitalTwin.API.Hubs;

/// <summary>
/// SignalR hub for real-time data streaming to UE5 clients
/// </summary>
public class RealtimeDataHub : Hub
{
    private readonly ILogger<RealtimeDataHub> _logger;

    public RealtimeDataHub(ILogger<RealtimeDataHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Client subscribes to updates for a specific machine
    /// </summary>
    public async Task SubscribeToMachine(string machineId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"machine-{machineId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to machine {MachineId}",
            Context.ConnectionId, machineId);
    }

    /// <summary>
    /// Client unsubscribes from a specific machine
    /// </summary>
    public async Task UnsubscribeFromMachine(string machineId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"machine-{machineId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from machine {MachineId}",
            Context.ConnectionId, machineId);
    }

    /// <summary>
    /// Client subscribes to updates for a specific department
    /// </summary>
    public async Task SubscribeToDepartment(string departmentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"department-{departmentId}");
        _logger.LogInformation("Client {ConnectionId} subscribed to department {DepartmentId}",
            Context.ConnectionId, departmentId);
    }

    /// <summary>
    /// Client unsubscribes from a specific department
    /// </summary>
    public async Task UnsubscribeFromDepartment(string departmentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"department-{departmentId}");
        _logger.LogInformation("Client {ConnectionId} unsubscribed from department {DepartmentId}",
            Context.ConnectionId, departmentId);
    }

    /// <summary>
    /// Client subscribes to all shop updates
    /// </summary>
    public async Task SubscribeToShop()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "shop");
        _logger.LogInformation("Client {ConnectionId} subscribed to shop updates", Context.ConnectionId);
    }

    /// <summary>
    /// Client unsubscribes from all updates
    /// </summary>
    public async Task UnsubscribeAll()
    {
        // Note: SignalR doesn't have a built-in way to remove from all groups
        // In practice, disconnecting handles this
        _logger.LogInformation("Client {ConnectionId} unsubscribed from all", Context.ConnectionId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}, Exception: {Exception}",
            Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    // Methods to be called by server to push updates to clients

    /// <summary>
    /// Server method to broadcast machine status update
    /// </summary>
    public async Task SendMachineStatusUpdate(string machineId, MachineStatusUpdateMessage update)
    {
        await Clients.Group($"machine-{machineId}").SendAsync("MachineStatusUpdate", update);
        await Clients.Group("shop").SendAsync("MachineStatusUpdate", update);
    }

    /// <summary>
    /// Server method to broadcast job progress update
    /// </summary>
    public async Task SendJobProgressUpdate(JobProgressUpdateMessage update)
    {
        await Clients.Group("shop").SendAsync("JobProgressUpdate", update);
    }

    /// <summary>
    /// Server method to broadcast alarm event
    /// </summary>
    public async Task SendAlarmEvent(string machineId, AlarmEventMessage alarm)
    {
        await Clients.Group($"machine-{machineId}").SendAsync("AlarmEvent", alarm);
        await Clients.Group("shop").SendAsync("AlarmEvent", alarm);
    }
}
