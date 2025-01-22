using System.Diagnostics.CodeAnalysis;
using Netconf.Netconf.Models;

namespace Netconf;

public readonly struct RpcResult
{
    public RpcResult(RpcErrorList errors)
    {
        this.Errors = errors;
    }
    public bool IsSuccess { get; private init; }
    public static RpcResult Success() => new() { IsSuccess = true };
    public static RpcResult<T> Success<T>(T value) where T : notnull => value;
    public RpcErrorList Errors { get; }

    public static implicit operator RpcResult(RpcErrorList errors) => new(errors);

    public bool Equals(RpcErrorList other) => this.Errors.Equals(other);
    public bool Equals(RpcResult other) => other.IsSuccess
        ? this.IsSuccess
        : !this.IsSuccess && this.Equals(other.Errors);
    public override bool Equals(object? obj) => obj switch
    {
        null => !this.IsSuccess,
        RpcResult other => this.Equals(other),
        RpcErrorList other => this.Equals(other),
        NetconfError other => this.Equals(new(other)),
        IReadOnlyList<NetconfError> other => this.Equals(new(other)),
        _ => false,
    };
    public override int GetHashCode() => HashCode.Combine(this.IsSuccess, this.Errors);
    public static bool operator ==(RpcResult left, RpcResult right) => left.Equals(right);
    public static bool operator !=(RpcResult left, RpcResult right) => !left.Equals(right);


    public override string ToString() => this.IsSuccess
        ? "RpcResult { IsSuccess = true }"
        : $"RpcResult {{ IsSuccess = false, Errors = {this.Errors.ToString()} }}";
}
public readonly struct RpcResult<T> : IEquatable<RpcResult<T>> where T : notnull
{
    public RpcResult(RpcErrorList errors)
    {
        this.Errors = errors;
    }
    public RpcResult(T value)
    {
        ThrowHelper.ThrowArgumentNullIfNull(value);
        this.ValueOrDefault = value;
        this.IsSuccess = true;
    }

    public T Value => this.IsSuccess ? this.ValueOrDefault : throw new InvalidOperationException("Cannot get the value of a failed result");
    public T? ValueOrDefault { get; }
    [MemberNotNullWhen(true, nameof(ValueOrDefault))]
    public bool IsSuccess { get; }

    public RpcErrorList Errors { get; }

    public static implicit operator RpcResult<T>(RpcErrorList errors) => new(errors);
    public static implicit operator RpcResult<T>(T value) => new(value);

    public static implicit operator RpcResult(RpcResult<T> value)
        => value.IsSuccess ? RpcResult.Success() : value.Errors;

    public bool Equals(RpcErrorList other) => this.Errors.Equals(other);
    public bool Equals(RpcResult<T> other) => other.IsSuccess
        ? this.Equals(other.Value)
        : this.Equals(other.Errors);
    public bool Equals(T? other) => other is not null
        ? this.IsSuccess && EqualityComparer<T>.Default.Equals(this.ValueOrDefault, other)
        : !this.IsSuccess;

    public override bool Equals(object? obj) => obj switch
    {
        null => !this.IsSuccess,
        T other => this.Equals(other),
        RpcResult<T> other => this.Equals(other),
        RpcErrorList other => this.Equals(other),
        IRpcError other => this.Equals(new RpcErrorList(other)),
        IReadOnlyList<NetconfError> other => this.Equals(new RpcErrorList(other)),
        _ => false,
    };
    public override int GetHashCode() => HashCode.Combine(this.IsSuccess, this.Value, this.Errors);
    public static bool operator ==(RpcResult<T> left, RpcResult<T> right) => left.Equals(right);
    public static bool operator !=(RpcResult<T> left, RpcResult<T> right) => !left.Equals(right);
    
    
    public override string ToString() => this.IsSuccess
        ? $"RpcResult<{typeof(T)}> {{ IsSuccess = true, Value = {this.Value.ToString()} }}"
        : $"RpcResult<{typeof(T)}> {{ IsSuccess = false, Errors = {this.Errors.ToString()} }}";

    public RpcResult<TOther> Map<TOther>(Func<T, TOther> mapper)
        where TOther : notnull
        => this.IsSuccess ? mapper(this.Value) : this.Errors;
    public RpcResult<TOther> Bind<TOther>(Func<T, RpcResult<TOther>> mapper)
        where TOther : notnull
        => this.IsSuccess ? mapper(this.Value) : this.Errors;
}