namespace Monolith.BuildingBlocks.Application;

/// <summary>Marker interface for commands that return no result.</summary>
public interface ICommand;

/// <summary>Marker interface for commands that return a result.</summary>
public interface ICommand<TResult>;
